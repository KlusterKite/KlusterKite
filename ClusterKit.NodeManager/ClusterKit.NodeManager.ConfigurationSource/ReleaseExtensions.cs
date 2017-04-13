// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReleaseExtensions.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Extending the work with <see cref="Release" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Akka.Configuration;

    using ClusterKit.API.Client;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.Launcher.Messages;

    using JetBrains.Annotations;

    using NuGet;

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
        public static IEnumerable<ErrorDescription> CheckAll(
            this Release release,
            ConfigurationContext context,
            IPackageRepository nugetRepository,
            List<string> supportedFrameworks)
        {
            release.CompatibleTemplates = release.GetCompatibleTemplates(context).ToList();
            foreach (var error in release.SetPackagesDescriptionsForTemplates(nugetRepository, supportedFrameworks))
            {
                yield return error;
            }

            foreach (var error in release.CheckTemplatesConfigurations())
            {
                yield return error;
            }
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

            if (release?.Configuration?.NugetFeeds == null || release.Configuration.NugetFeeds.Count == 0)
            {
                yield return new ErrorDescription(
                    "configuration.nugetFeeds",
                    "Nuget feeds list is empty");
            }

            if (release?.Configuration?.NodeTemplates == null || release.Configuration.NodeTemplates.Count == 0)
            {
                yield break;
            }

            foreach (var template in release.Configuration.NodeTemplates)
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

                yield return
                    new ErrorDescription(
                        $"configuration.nodeTemplates[\"{template.Code}\"].configuration",
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
                context.Releases.Include(nameof(Release.CompatibleTemplates))
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
                    var compatible in currentRelease.CompatibleTemplates.Where(ct => ct.TemplateCode == template.Code))
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
        /// Checks the <see cref="Release"/> data and sets the <see cref="Template.PackagesToInstall"/>
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
        public static IEnumerable<ErrorDescription> SetPackagesDescriptionsForTemplates(
            this Release release,
            IPackageRepository nugetRepository,
            List<string> supportedFrameworks)
        {
            if (release?.Configuration?.NodeTemplates == null || release.Configuration.NodeTemplates.Count == 0)
            {
                yield return new ErrorDescription("configuration.nodeTemplates", "Node templates are not initialized");
                yield break;
            }

            if (release.Configuration.Packages == null || release.Configuration.Packages.Count == 0)
            {
                yield return new ErrorDescription("configuration.packages", "Packages are not initialized");
                yield break;
            }

            Dictionary<string, IPackage> definedPackages = new Dictionary<string, IPackage>();
            foreach (var errorDescription in
                CheckPackages(release, supportedFrameworks, nugetRepository, definedPackages))
            {
                yield return errorDescription;
            }

            foreach (var errorDescription1 in
                CheckTemplatesPackages(release, supportedFrameworks, definedPackages, nugetRepository))
            {
                yield return errorDescription1;
            }
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
        /// <param name="definedPackagesToFill">
        /// The dictionary to fill.
        /// </param>
        /// <returns>
        /// The list of possible errors
        /// </returns>
        private static IEnumerable<ErrorDescription> CheckPackages(
            Release release,
            List<string> supportedFrameworks,
            IPackageRepository nugetRepository,
            Dictionary<string, IPackage> definedPackagesToFill)
        {
            foreach (var description in release.Configuration.Packages)
            {
                var packageField = $"configuration.packages[\"{description.Id}\"]";
                SemanticVersion requiredVersion = null;
                try
                {
                    requiredVersion = SemanticVersion.Parse(description.Version);
                }
                catch
                {
                    // set null
                }

                if (requiredVersion == null)
                {
                    yield return new ErrorDescription(packageField, "Package version could not be parsed");
                    continue;
                }

                var package =
                    nugetRepository.Search(description.Id, true)
                        .ToList()
                        .FirstOrDefault(p => p.Id == description.Id && p.Version == requiredVersion);

                if (package == null)
                {
                    yield return
                        new ErrorDescription(
                            packageField,
                            "Package of specified version could not be found in the nuget repository");
                    continue;
                }

                definedPackagesToFill[description.Id] = package;
            }

            foreach (var package in definedPackagesToFill.Values)
            {
                var packageField = $"configuration.packages[\"{package.Id}\"]";
                foreach (var dependencySet in
                    package.DependencySets.Where(
                        s => s.SupportedFrameworks.Any(f => supportedFrameworks.Contains(f.FullName))))
                {
                    foreach (var dependency in dependencySet.Dependencies)
                    {
                        IPackage dependentPackage;
                        if (!definedPackagesToFill.TryGetValue(dependency.Id, out dependentPackage))
                        {
                            yield return
                                new ErrorDescription(
                                    packageField,
                                    $"Package dependency for {dependencySet.TargetFramework.FullName} {dependency.Id} is not defined");
                            continue;
                        }

                        if (!dependency.VersionSpec.Satisfies(dependentPackage.Version))
                        {
                            yield return
                                new ErrorDescription(
                                    packageField,
                                    $"Package dependency for {dependencySet.TargetFramework.FullName} {dependency.Id} doesn't satisify version requirements");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks package requirements in release templates and fills the <see cref="Template.PackagesToInstall"/> field in templates
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
        private static IEnumerable<ErrorDescription> CheckTemplatesPackages(
            [NotNull] Release release,
            [NotNull] List<string> supportedFrameworks,
            [NotNull] Dictionary<string, IPackage> definedPackages,
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

            foreach (var template in release.Configuration.NodeTemplates)
            {
               if (template == null)
                {
                    continue;
                }

                var templateField = $"configuration.nodeTemplates[\"{template.Code}\"]";

                if (template.PackageRequirements == null || template.PackageRequirements.Count == 0)
                {
                    yield return new ErrorDescription(templateField, "Package requirements are not set");
                    continue;
                }

                Dictionary<string, IPackage> directPackages = new Dictionary<string, IPackage>();
                foreach (var errorDescription in GetTemplateDirectPackages(
                    definedPackages,
                    nugetRepository,
                    template,
                    templateField,
                    directPackages))
                {
                    yield return errorDescription;
                }

                template.PackagesToInstall = new Dictionary<string, List<PackageDescription>>();
                foreach (var supportedFramework in supportedFrameworks)
                {
                    var packagesToInstall = new List<IPackage>();
                    foreach (var package in directPackages.Values)
                    {
                        if (package == null)
                        {
                            continue;
                        }

                        packagesToInstall.Add(package);
                        var requirementField = $"{templateField}.packageRequirements[\"{package.Id}\"]";
                        var dependencySet = package.DependencySets?.FirstOrDefault(
                            s => s.TargetFramework == null
                                 || s.SupportedFrameworks.Any(f => f.FullName == supportedFramework));
                        if (dependencySet?.Dependencies == null)
                        {
                            continue;
                        }

                        foreach (var dependency in dependencySet.Dependencies)
                        {
                            if (dependency?.VersionSpec == null)
                            {
                                yield return new ErrorDescription(
                                    requirementField,
                                    $"Package dependency for {supportedFramework} {dependency?.Id} is undefined of corrupted");
                                continue;
                            }

                            IPackage dependentPackage;
                            if ((!directPackages.TryGetValue(dependency.Id, out dependentPackage)
                                && !definedPackages.TryGetValue(dependency.Id, out dependentPackage)) || dependentPackage == null)
                            {
                                yield return new ErrorDescription(
                                    requirementField,
                                    $"Package dependency for {dependencySet.TargetFramework?.FullName} {dependency.Id} is not defined");
                                continue;
                            }

                            if (!dependency.VersionSpec.Satisfies(dependentPackage.Version))
                            {
                                yield return new ErrorDescription(
                                    requirementField,
                                    $"Package dependency for {supportedFramework} {dependency.Id} doesn't satisfy version requirements");
                            }

                            if (packagesToInstall.All(p => p.Id != dependentPackage.Id))
                            {
                                packagesToInstall.Add(dependentPackage);
                            }
                        }
                    }

                    template.PackagesToInstall[supportedFramework] = packagesToInstall
                        .Select(p => new PackageDescription { Id = p.Id, Version = p.Version?.ToString() })
                        .ToList();
                }
            }
        }

        /// <summary>
        /// Fills the provided dictionary with packages directly linked in template requirements
        /// </summary>
        /// <param name="definedPackages">
        /// The list defined packages defined in release.
        /// </param>
        /// <param name="nugetRepository">
        /// The nuget repository.
        /// </param>
        /// <param name="template">
        /// The template.
        /// </param>
        /// <param name="templateField">
        /// The prefix for error field name.
        /// </param>
        /// <param name="directPackagesToFill">
        /// The dictionary to fill with data
        /// </param>
        /// <returns>
        ///  The list of possible errors
        /// </returns>
        private static IEnumerable<ErrorDescription> GetTemplateDirectPackages(
            Dictionary<string, IPackage> definedPackages,
            IPackageRepository nugetRepository,
            Template template,
            string templateField,
            Dictionary<string, IPackage> directPackagesToFill)
        {
            foreach (var requirement in template.PackageRequirements)
            {
                var requirementField = $"{templateField}.packageRequirements[\"{requirement.Id}\"]";
                IPackage package;
                if (requirement.SpecificVersion == null)
                {
                    if (!definedPackages.TryGetValue(requirement.Id, out package))
                    {
                        yield return
                            new ErrorDescription(
                                requirementField,
                                "Package requirement is not defined in release packages");
                        continue;
                    }

                    directPackagesToFill[package.Id] = package;
                    continue;
                }

                SemanticVersion requiredVersion = null;
                try
                {
                    requiredVersion = SemanticVersion.Parse(requirement.SpecificVersion);
                }
                catch
                {
                    // set null
                }

                if (requiredVersion == null)
                {
                    yield return new ErrorDescription(requirementField, "Package version could not be parsed");
                    continue;
                }

                package =
                    nugetRepository.Search(requirement.Id, true)
                        .ToList()
                        .FirstOrDefault(p => p.Id == requirement.Id && p.Version == requiredVersion);

                if (package == null)
                {
                    yield return
                        new ErrorDescription(requirementField, "Package could not be found in nuget repository");
                    continue;
                }

                directPackagesToFill[package.Id] = package;
            }
        }
    }
}