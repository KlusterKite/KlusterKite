// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReleaseExtensions.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Extending the work with <see cref="Release" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.ConfigurationSource
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Akka.Configuration;

    using JetBrains.Annotations;

    using KlusterKite.API.Client;
    using KlusterKite.NodeManager.Client.ORM;
    using KlusterKite.NodeManager.Launcher.Messages;
    using KlusterKite.NodeManager.Launcher.Utils;

    using Microsoft.EntityFrameworkCore;

    using NuGet.Frameworks;
    using NuGet.Protocol.Core.Types;
    using NuGet.Versioning;

    /// <summary>
    /// Extending the work with <see cref="Release"/>
    /// </summary>
    public static class ReleaseExtensions
    {
        /// <summary>
        /// Creates full release check and fills it values with actual data
        /// </summary>
        /// <param name="release">The release</param>
        /// <param name="context">The data context</param>
        /// <param name="nugetRepository">The access to the nuget repository</param>
        /// <param name="supportedFrameworks">The list of supported frameworks</param>
        /// <returns>The list of possible errors</returns>
        public static async Task<List<ErrorDescription>> CheckAll(
            this Release release,
            ConfigurationContext context,
            IPackageRepository nugetRepository,
            List<string> supportedFrameworks)
        {
            release.CompatibleTemplatesBackward = release.GetCompatibleTemplates(context).ToList();
            var errors = new List<ErrorDescription>();
            errors.AddRange(await release.SetPackagesDescriptionsForTemplates(nugetRepository, supportedFrameworks));
            errors.AddRange(release.CheckTemplatesConfigurations());
            return errors;
        }
         
        /// <summary>
        /// Checks release node templates for correct configuration sections
        /// </summary>
        /// <param name="release">The release</param>
        /// <returns>The list of errors</returns>
        public static IEnumerable<ErrorDescription> CheckTemplatesConfigurations(this Release release)
        {
            if (release?.Configuration?.SeedAddresses == null || release.Configuration.SeedAddresses.Count == 0)
            {
                yield return new ErrorDescription(
                    "configuration.seedAddresses",
                    "Seeds addresses is empty");
            }

            if (string.IsNullOrWhiteSpace(release?.Configuration?.NugetFeed))
            {
                yield return new ErrorDescription(
                    "configuration.nugetFeed",
                    "Nuget feed is empty");
            }

            if (release?.Configuration?.NodeTemplates == null 
                || release.Configuration.NodeTemplates.Count == 0
                || release.Configuration.MigratorTemplates == null
                || release.Configuration.MigratorTemplates.Count == 0)
            {
                yield break;
            }

            foreach (var template in release.Configuration.GetAllTemplates())
            {
                try
                {
                    ConfigurationFactory.ParseString(template.Configuration);
                    continue;
                }
                catch
                {
                    // ignored
                }

                var fieldName = template is NodeTemplate 
                    ? $"configuration.nodeTemplates[\"{template.Code}\"].configuration"
                    : $"configuration.migratorTemplates[\"{template.Code}\"].configuration";
                yield return
                    new ErrorDescription(
                        fieldName,
                        "Configuration could not be parsed");
            }
        }

        /// <summary>
        /// Gets the list of compatible templates
        /// </summary>
        /// <param name="release">
        /// The release to check
        /// </param>
        /// <param name="context">
        /// The configuration data context
        /// </param>
        /// <returns>
        /// The list of compatible templates
        /// </returns> 
        public static IEnumerable<CompatibleTemplate> GetCompatibleTemplates(
            this Release release,
            ConfigurationContext context)
        {
            if (release?.Configuration?.NodeTemplates == null)
            {
                yield break;
            }

            var currentRelease =
                context.Releases.Include(nameof(Release.CompatibleTemplatesBackward))
                    .FirstOrDefault(r => r.State == EnReleaseState.Active);

            if (currentRelease?.Configuration?.NodeTemplates == null)
            {
                yield break;
            }

            foreach (var template in currentRelease.Configuration.NodeTemplates)
            {
                var currentTemplate = release.Configuration.NodeTemplates.FirstOrDefault(t => t.Code == template.Code);
                if (currentTemplate == null)
                {
                    continue;
                }

                if (currentTemplate.Configuration != template.Configuration)
                {
                    continue;
                }

                var oldRequirements = string.Join(
                    "; ",
                    template.PackageRequirements.OrderBy(p => p.Id).Select(p => $"{p.Id} {p.SpecificVersion}"));
                var newRequirements = string.Join(
                    "; ",
                    template.PackageRequirements.OrderBy(p => p.Id).Select(p => $"{p.Id} {p.SpecificVersion}"));

                if (oldRequirements != newRequirements)
                {
                    continue;
                }

                var needPackageUpdate = false;
                foreach (var requirement in currentTemplate.PackageRequirements.Where(r => r.SpecificVersion == null))
                {
                    var oldVersion = release.Configuration.Packages.FirstOrDefault(p => p.Id == requirement.Id);
                    var newVersion = currentRelease.Configuration.Packages.FirstOrDefault(p => p.Id == requirement.Id);
                    if (newVersion == null || oldVersion?.Version != newVersion.Version)
                    {
                        needPackageUpdate = true;
                        break;
                    }
                }

                if (needPackageUpdate)
                {
                    continue;
                }

                yield return
                    new CompatibleTemplate
                        {
                            CompatibleReleaseId = currentRelease.Id,
                            ReleaseId = release.Id,
                            TemplateCode = template.Code
                        };

                foreach (
                    var compatible in currentRelease.CompatibleTemplatesBackward.Where(ct => ct.TemplateCode == template.Code))
                {
                    yield return
                        new CompatibleTemplate
                            {
                                CompatibleReleaseId = compatible.CompatibleReleaseId,
                                ReleaseId = release.Id,
                                TemplateCode = template.Code
                            };
                }
            }
        }

        /// <summary>
        /// Checks the <see cref="Release"/> data and sets the <see cref="NodeTemplate.PackagesToInstall"/>
        /// </summary>
        /// <param name="release">
        /// The object to update
        /// </param>
        /// <param name="nugetRepository">
        /// The nuget repository.
        /// </param>
        /// <param name="supportedFrameworks">
        /// The list of supported Frameworks.
        /// </param>
        /// <returns>
        /// The list of possible errors
        /// </returns>
        public static async Task<List<ErrorDescription>> SetPackagesDescriptionsForTemplates(
            this Release release,
            IPackageRepository nugetRepository,
            List<string> supportedFrameworks)
        {
            var errors = new List<ErrorDescription>();
            if (release?.Configuration?.NodeTemplates == null || release.Configuration.NodeTemplates.Count == 0)
            {
                errors.Add(new ErrorDescription("configuration.nodeTemplates", "Node templates are not initialized"));
                return errors;
            }

            if (release.Configuration?.MigratorTemplates == null || release.Configuration.MigratorTemplates.Count == 0)
            {
                errors.Add(new ErrorDescription("configuration.migratorTemplates", "Migrator templates are not initialized"));
                return errors;
            }

            if (release.Configuration.Packages == null || release.Configuration.Packages.Count == 0)
            {
                errors.Add(new ErrorDescription("configuration.packages", "Packages are not initialized"));
                return errors;
            }

            var(packages, packagesErrors) = await CheckPackages(release, supportedFrameworks, nugetRepository);
            errors.AddRange(packagesErrors);
            errors.AddRange(await CheckTemplatesPackages(release, supportedFrameworks, packages, nugetRepository));
            return errors;
        }

        /// <summary>
        /// Checks the list of defined packages in the release and fills provided dictionary with precise package data
        /// </summary>
        /// <param name="release">
        /// The release.
        /// </param>
        /// <param name="supportedFrameworks">
        /// The list of supported frameworks.
        /// </param>
        /// <param name="nugetRepository">
        /// The nuget repository.
        /// </param>
        /// <returns>
        /// The list of defined packages along with list of possible errors
        /// </returns>
        private static async Task<Tuple<Dictionary<string, IPackageSearchMetadata>, List<ErrorDescription>>>
            CheckPackages(Release release, List<string> supportedFrameworks, IPackageRepository nugetRepository)
        {
            var definedPackages = new Dictionary<string, IPackageSearchMetadata>();
            var errors = new List<ErrorDescription>();
            var packages = await Task.WhenAll(
                               release.Configuration.Packages
                               .Select(d => new { Description = d, Version = NuGetVersion.TryParse(d.Version, out var v) ? v : null })
                               .Select(
                                       async p => new
                                                      {
                                                          p.Description, p.Version,
                                                          Package = p.Version != null 
                                                              ? await nugetRepository.GetAsync(p.Description.Id, p.Version) 
                                                              : null
                                                      }));

            foreach (var package in packages)
            {
                if (package.Version == null)
                {
                    errors.Add(
                        new ErrorDescription(
                            $"configuration.packages[\"{package.Description.Id}\"]",
                            "Package version could not be parsed"));
                }
                else if (package.Package == null)
                {
                    errors.Add(
                        new ErrorDescription(
                            $"configuration.packages[\"{package.Description.Id}\"]",
                            "Package of specified version could not be found in the nuget repository"));
                }
                else
                {
                    definedPackages[package.Description.Id] = package.Package;
                }
            }

            var frameworks = supportedFrameworks.Select(
                f => NuGetFramework.ParseFrameworkName(f, new DefaultFrameworkNameProvider()))
                .ToList();

            errors.AddRange(definedPackages.Values
                .SelectMany(
                    p => p.DependencySets.Where(
                        s => frameworks.Any(
                            f => NuGetFrameworkUtility.IsCompatibleWithFallbackCheck(f, s.TargetFramework))))
                .SelectMany(s => s.Packages).GroupBy(p => p.Id).Select(
                    r =>
                        {
                            var packageField = $"configuration.packages[\"{r.Key}\"]";
                            IPackageSearchMetadata definedPackage;
                            if (!definedPackages.TryGetValue(r.Key, out definedPackage))
                            {
                                return new ErrorDescription(
                                    packageField,
                                    "Package is not defined");
                            }

                            if (r.Any(rc => !rc.VersionRange.Satisfies(definedPackage.Identity.Version)))
                            {
                                return new ErrorDescription(
                                    packageField,
                                    "Package doesn't satisfy other packages version requirements");
                            }

                            return null;
                        })
                 .Where(e => e != null));
            return Tuple.Create(definedPackages, errors);
        }

        /// <summary>
        /// Checks package requirements in release templates and fills the <see cref="NodeTemplate.PackagesToInstall"/> field in templates
        /// </summary>
        /// <param name="release">
        /// The release.
        /// </param>
        /// <param name="supportedFrameworks">
        /// The list of supported frameworks.
        /// </param>
        /// <param name="definedPackages">
        /// The list of defined packages in the release.
        /// </param>
        /// <param name="nugetRepository">
        /// The nuget repository.
        /// </param>
        /// <returns>
        /// The list of possible errors
        /// </returns>
        private static async Task<List<ErrorDescription>> CheckTemplatesPackages(
            [NotNull] Release release,
            [NotNull] List<string> supportedFrameworks,
            [NotNull] Dictionary<string, IPackageSearchMetadata> definedPackages,
            [NotNull] IPackageRepository nugetRepository)
        {
            if (release == null)
            {
                throw new ArgumentNullException(nameof(release));
            }

            if (release.Configuration == null)
            {
                throw new ArgumentNullException(nameof(release.Configuration));
            }

            if (release.Configuration.NodeTemplates == null)
            {
                throw new ArgumentNullException(nameof(release.Configuration.NodeTemplates));
            }

            if (supportedFrameworks == null)
            {
                throw new ArgumentNullException(nameof(supportedFrameworks));
            }

            if (definedPackages == null)
            {
                throw new ArgumentNullException(nameof(definedPackages));
            }

            if (nugetRepository == null)
            {
                throw new ArgumentNullException(nameof(nugetRepository));
            }

            var errors = new List<ErrorDescription>();

            foreach (var template in release.Configuration.GetAllTemplates())
            {
                if (template == null)
                {
                    continue;
                }

                var templateField = template is NodeTemplate
                                        ? $"configuration.nodeTemplates[\"{template.Code}\"]"
                                        : $"configuration.migratorTemplates[\"{template.Code}\"]";

                if (template.PackageRequirements == null || template.PackageRequirements.Count == 0)
                {
                    errors.Add(new ErrorDescription(templateField, "Package requirements are not set"));
                    continue;
                }

                if (template is MigratorTemplate)
                {
                    // Every MigratorTemplate should contain this package for execution
                    if (template.PackageRequirements.All(p => p.Id != "KlusterKite.NodeManager.Migrator.Executor"))
                    {
                        template.PackageRequirements.Add(new NodeTemplate.PackageRequirement("KlusterKite.NodeManager.Migrator.Executor", null));
                    }
                }

                var(templatePackages, templateErrors) =
                    await GetTemplateDirectPackages(definedPackages, nugetRepository, template, templateField);

                errors.AddRange(templateErrors);

                template.PackagesToInstall = new Dictionary<string, List<PackageDescription>>();
                foreach (var supportedFramework in supportedFrameworks.Select(
                    f => NuGetFramework.ParseFrameworkName(f, new DefaultFrameworkNameProvider())))
                {
                    var packagesToInstall = new Dictionary<string, IPackageSearchMetadata>();
                    foreach (var package in templatePackages)
                    {
                        packagesToInstall.Add(package.Key, package.Value);
                    }

                    var queue = new Queue<IPackageSearchMetadata>(templatePackages.Values);

                    while (queue.Count > 0)
                    {
                        var package = queue.Dequeue();
                        if (!packagesToInstall.ContainsKey(package.Identity.Id))
                        {
                            packagesToInstall.Add(package.Identity.Id, package);
                        }

                        var requirementField = $"{templateField}.packageRequirements[\"{package.Identity.Id}\"]";
                        var dependencySet =
                            NuGetFrameworkUtility.GetNearest(package.DependencySets, supportedFramework);
                        if (dependencySet == null || !NuGetFrameworkUtility.IsCompatibleWithFallbackCheck(
                                supportedFramework,
                                dependencySet.TargetFramework))
                        {
                            continue;
                        }

                        foreach (var dependency in dependencySet.Packages)
                        {
                            IPackageSearchMetadata packageToInstall;
                            if (!packagesToInstall.TryGetValue(dependency.Id, out packageToInstall))
                            {
                                if (!definedPackages.TryGetValue(dependency.Id, out packageToInstall))
                                {
                                    errors.Add(
                                        new ErrorDescription(
                                            requirementField,
                                            $"Package dependency for {supportedFramework.DotNetFrameworkName} {dependency.Id} {dependency.VersionRange} is missing"));
                                    continue;
                                }

                                if (queue.All(p => p.Identity.Id != packageToInstall.Identity.Id))
                                {
                                    queue.Enqueue(packageToInstall);
                                }
                            }

                            if (!dependency.VersionRange.Satisfies(packageToInstall.Identity.Version))
                            {
                                errors.Add(
                                    new ErrorDescription(
                                        requirementField,
                                        $"Package dependency for {supportedFramework} {packageToInstall.Identity} doesn't satisfy version requirements {dependency.VersionRange}."));
                            }
                        }
                    }

                    template.PackagesToInstall[supportedFramework.DotNetFrameworkName] = packagesToInstall.Values
                        .Select(
                            p => new PackageDescription { Id = p.Identity.Id, Version = p.Identity.Version.ToString() })
                        .ToList();
                }
            }

            return errors;
        }

        /// <summary>
        /// Fills the provided dictionary with packages directly linked in nodeTemplate requirements
        /// </summary>
        /// <param name="definedPackages">
        /// The list defined packages defined in release.
        /// </param>
        /// <param name="nugetRepository">
        /// The nuget repository.
        /// </param>
        /// <param name="nodeTemplate">
        /// The nodeTemplate.
        /// </param>
        /// <param name="templateField">
        /// The prefix for error field name.
        /// </param>
        /// <returns>
        ///  The list of possible errors
        /// </returns>
        private static async Task<Tuple<Dictionary<string, IPackageSearchMetadata>, List<ErrorDescription>>> GetTemplateDirectPackages(
            Dictionary<string, IPackageSearchMetadata> definedPackages,
            IPackageRepository nugetRepository,
            ITemplate nodeTemplate,
            string templateField)
        {
            var errors = new List<ErrorDescription>();
            var directPackagesToFill = new Dictionary<string, IPackageSearchMetadata>();

            foreach (var requirement in nodeTemplate.PackageRequirements.Where(r => r.SpecificVersion == null))
            {
                IPackageSearchMetadata package;
                if (!definedPackages.TryGetValue(requirement.Id, out package))
                {
                    var requirementField = $"{templateField}.packageRequirements[\"{requirement.Id}\"]";
                    errors.Add(
                        new ErrorDescription(
                            requirementField,
                            "Package requirement is not defined in release packages"));
                }
                else
                {
                    directPackagesToFill[package.Identity.Id] = package;
                }
            }

            var searchResults = await Task.WhenAll(nodeTemplate.PackageRequirements.Where(r => r.SpecificVersion != null)
                .Select(r => new { Requirement = r, Version = NuGetVersion.TryParse(r.SpecificVersion, out var v) ? v : null })
                .Select(
                async r => new
                               {
                                   r.Requirement,
                                   r.Version,
                                   Package = r.Version != null ? await nugetRepository.GetAsync(
                                                 r.Requirement.Id,
                                                 r.Version) : null
                               }));

            foreach (var result in searchResults)
            {
                var requirementField = $"{templateField}.packageRequirements[\"{result.Requirement.Id}\"]";
                if (result.Version == null)
                {
                    errors.Add(
                        new ErrorDescription(requirementField, "Package version could not be parsed"));
                }
                else if (result.Package == null)
                {
                    errors.Add(
                        new ErrorDescription(requirementField, "Package could not be found in nuget repository"));
                }
                else
                {
                    directPackagesToFill[result.Requirement.Id] = result.Package;
                }
            }

            return Tuple.Create(directPackagesToFill, errors);
        }
    }
}