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
    using System.Collections.Generic;
    using System.Linq;

    using ClusterKit.NodeManager.Client.ORM;

    /// <summary>
    /// Extending the work with <see cref="Release"/>
    /// </summary>
    public static class ReleaseExtensions
    {
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
        public static IEnumerable<CompatibleTemplate> GetCompatibleTemplates(this Release release, ConfigurationContext context)
        {
            if (release?.Configuration?.NodeTemplates == null)
            {
                yield break;
            }

            var currentRelease  =
                context.Releases.Include(nameof(Release.CompatibleTemplates))
                    .FirstOrDefault(r => r.State == Release.EnState.Active);

            if (currentRelease?.Configuration?.NodeTemplates == null)
            {
                yield break;
            }

            foreach (var template in currentRelease.Configuration.NodeTemplates)
            {
                var currentTemplate = release.Configuration.NodeTemplates.FirstOrDefault(t => t.Code == template.Code);
                if (currentTemplate != null)
                {
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
                        var oldVersion = release.Configuration.Packages.FirstOrDefault(p => p.Name == requirement.Id);
                        var newVersion = currentRelease.Configuration.Packages.FirstOrDefault(p => p.Name == requirement.Id);
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

                    yield return new CompatibleTemplate
                                     {
                                         CompatibleReleaseId = currentRelease.Id,
                                         ReleaseId = release.Id,
                                         TemplateCode = template.Code
                                     };

                    foreach (var compatible in currentRelease.CompatibleTemplates.Where(ct => ct.TemplateCode == template.Code))
                    {
                        yield return new CompatibleTemplate
                        {
                            CompatibleReleaseId = compatible.CompatibleReleaseId,
                            ReleaseId = release.Id,
                            TemplateCode = template.Code
                        };
                    }
                }
            }
        }
    }
}
