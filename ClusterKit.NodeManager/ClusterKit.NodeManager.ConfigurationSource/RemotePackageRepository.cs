using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace ClusterKit.NodeManager.ConfigurationSource
{
    public class RemotePackageRepository : IPackageRepository
    {
        public Task<IPackageSearchMetadata> GetAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IPackageSearchMetadata> GetAsync(string id, NuGetVersion version)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IPackageSearchMetadata>> SearchAsync(string terms, bool includePreRelease)
        {
            throw new NotImplementedException();
        }
    }
}
