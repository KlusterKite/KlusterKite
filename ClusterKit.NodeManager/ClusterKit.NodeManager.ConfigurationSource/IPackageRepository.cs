// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageRepository.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Abstraction to work with packages
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

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
    }
}