// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemotePackageRepository.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The repository to work with real remote nuget server
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ClusterKit.NodeManager.Launcher.Utils;

    using NuGet.Protocol.Core.Types;
    using NuGet.Versioning;

    /// <summary>
    /// The repository to work with real remote nuget server
    /// </summary>
    public class RemotePackageRepository : IPackageRepository
    {
        /// <summary>
        /// The server url
        /// </summary>
        private readonly string url;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemotePackageRepository"/> class.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        public RemotePackageRepository(string url)
        {
            this.url = url;
        }

        /// <inheritdoc />
        public Task<IPackageSearchMetadata> GetAsync(string id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<IPackageSearchMetadata> GetAsync(string id, NuGetVersion version)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<IEnumerable<IPackageSearchMetadata>> SearchAsync(string terms, bool includePreRelease)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<IEnumerable<string>> ExtractPackage(IPackageSearchMetadata package, string frameworkName, string executionDir, string tmpDir)
        {
            throw new NotImplementedException();
        }
    }
}
