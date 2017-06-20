// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockPackageRepository.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The mock package repository
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Tests.Mock
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ClusterKit.NodeManager.ConfigurationSource;
    using ClusterKit.NodeManager.Launcher.Utils;

    using NuGet.Protocol.Core.Types;
    using NuGet.Versioning;

    /// <summary>
    /// The mock package repository
    /// </summary>
    public class MockPackageRepository : IPackageRepository
    {
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
