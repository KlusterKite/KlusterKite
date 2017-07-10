// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemotePackageRepository.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The repository to work with real remote nuget server
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Launcher.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using NuGet.Frameworks;
    using NuGet.Packaging.Core;
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
        public async Task<IPackageSearchMetadata> GetAsync(string id)
        {
            var packages = await PackageUtils.Search(this.url, id);
            return packages.Where(p => p.Identity.Id == id).OrderByDescending(p => p.Identity.Version).FirstOrDefault();
        }

        /// <inheritdoc />
        public Task<IPackageSearchMetadata> GetAsync(string id, NuGetVersion version)
        {
            return PackageUtils.Search(this.url, id, version);
        }

        /// <inheritdoc />
        public Task<IEnumerable<IPackageSearchMetadata>> SearchAsync(string terms, bool includePreRelease)
        {
            return PackageUtils.Search(this.url, terms).ContinueWith(t => (IEnumerable<IPackageSearchMetadata>)t.Result);
        }

        /// <inheritdoc />
        public Task<Dictionary<PackageIdentity, IEnumerable<string>>> ExtractPackage(
            IEnumerable<PackageIdentity> packages,
            string runtime,
            string frameworkName,
            string executionDir,
            string tmpDir,
            Action<string> logAction = null)
        {
            var framework = NuGetFramework.ParseFrameworkName(frameworkName, DefaultFrameworkNameProvider.Instance);
            Console.WriteLine(framework.ToString());
            return packages.Install(
                runtime,
                framework,
                this.url,
                executionDir,
                logAction);
        }
    }
}
