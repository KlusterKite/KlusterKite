// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageRepositoryExtensions.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Extensions for <see cref="IPackageRepository" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Launcher.Utils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using NuGet.Packaging.Core;
    using NuGet.Protocol.Core.Types;
    using NuGet.Versioning;

    /// <summary>
    /// Extensions for <see cref="IPackageRepository"/>
    /// </summary>
    public static class PackageRepositoryExtensions
    {
        /// <summary>
        /// Installs needed packages and configures the service
        /// </summary>
        /// <param name="repository">
        /// The nuget repository
        /// </param>
        /// <param name="packages">
        /// The list of packages
        /// </param>
        /// <param name="frameworkName">
        /// The runtime framework name
        /// </param>
        /// <param name="outputDir">
        /// The path to install service
        /// </param>
        /// <param name="executionFileName">
        /// The service execution file name to provide configuration files
        /// </param>
        /// <returns>
        /// The async task
        /// </returns>
        public static async Task CreateServiceAsync(
            this IPackageRepository repository,
            IEnumerable<IPackageSearchMetadata> packages,
            string frameworkName,
            string outputDir,
            string executionFileName)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            var packageFiles = new Dictionary<PackageIdentity, List<string>>();
            try
            {
                foreach (var package in packages)
                {
                    var files = await repository.ExtractPackage(package, frameworkName, outputDir, tempDir);
                    packageFiles[package.Identity] = files.ToList();
                }

                ConfigurationUtils.FixAssemblyVersions(outputDir, executionFileName, packageFiles);
            }
            finally
            {
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch
                {
                    // ignore
                }
            }
        }


        /// <summary>
        /// Extracts the lib files to execution directory
        /// </summary>
        /// <param name="repository">The nuget repository</param>
        /// <param name="package">
        /// The package to extract
        /// </param>
        /// <param name="frameworkName">The runtime framework name</param>
        /// <param name="tmpDir">
        /// The temp directory to extract packages
        /// </param>
        /// <param name="executionDir">
        /// The execution directory to load packages
        /// </param>
        /// <returns>
        /// The async task
        /// </returns>
        private static async Task ExtractPackageAsync(
            this IPackageRepository repository,
            IPackageSearchMetadata package,
            string frameworkName,
            string tmpDir,
            string executionDir)
        {
            var files = await repository.ExtractPackage(package, frameworkName, executionDir, tmpDir);
            /*
            if (!files.Any())
            {
                Context.GetLogger().Warning(
                    "{Type}: Package {PackageId} does not contains compatible files",
                    this.GetType().Name,
                    package.Identity.ToString());
            }
            */ 
        }
    }
}
