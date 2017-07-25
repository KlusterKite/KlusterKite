// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageRepository.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Abstraction to work with packages
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Launcher.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using NuGet.Packaging.Core;
    using NuGet.Protocol.Core.Types;
    using NuGet.Versioning;

    /// <summary>
    /// Abstraction to work with packages
    /// </summary>
    public interface IPackageRepository
    {
        /// <summary>
        /// Gets the latest version package description
        /// </summary>
        /// <param name="id">The package id</param>
        /// <returns>The package</returns>
        Task<IPackageSearchMetadata> GetAsync(string id);

        /// <summary>
        /// Gets the package description
        /// </summary>
        /// <param name="id">The package id</param>
        /// <param name="version">The package version</param>
        /// <returns>The package</returns>
        Task<IPackageSearchMetadata> GetAsync(string id, NuGetVersion version);

        /// <summary>
        /// Searches for the packages according to specified terms
        /// </summary>
        /// <param name="terms">
        /// The search terms.
        /// </param>
        /// <param name="includePreRelease">
        /// A value indicating whether results should include pre-release versions
        /// </param>
        /// <returns>
        /// The search result
        /// </returns>
        Task<IEnumerable<IPackageSearchMetadata>> SearchAsync(string terms, bool includePreRelease);

        /// <summary>
        /// Extracts package files to the specified destination
        /// </summary>
        /// <param name="packages">The list of packages to extract</param>
        /// <param name="runtime">The current runtime</param>
        /// <param name="frameworkName">The execution framework name</param>
        /// <param name="executionDir">The path to extract</param>
        /// <param name="tmpDir">The temporary directory name</param>
        /// <param name="logAction">The log writing action</param>
        /// <returns>The list of extracted files by package</returns>
        Task<Dictionary<PackageIdentity, IEnumerable<string>>> ExtractPackage(
            IEnumerable<PackageIdentity> packages,
            string runtime,
            string frameworkName,
            string executionDir,
            string tmpDir,
            Action<string> logAction = null);
    }
}