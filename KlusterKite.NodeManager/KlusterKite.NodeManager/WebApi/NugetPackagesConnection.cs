 // --------------------------------------------------------------------------------------------------------------------
// <copyright file="NugetPackagesConnection.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The connection to the nuget server
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using KlusterKite.API.Client;
    using KlusterKite.NodeManager.Client.ApiSurrogates;
    using KlusterKite.NodeManager.Launcher.Utils;

    /// <summary>
    /// The connection to the nuget server
    /// </summary>
    public class NugetPackagesConnection : INodeConnection<PackageFamily>
    {
        /// <summary>
        /// The packages repository
        /// </summary>
        private readonly IPackageRepository packageRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="NugetPackagesConnection"/> class.
        /// </summary>
        /// <param name="packageRepository">
        /// The package Repository.
        /// </param>
        public NugetPackagesConnection(IPackageRepository packageRepository)
        {
            this.packageRepository = packageRepository;
        }

        /// <inheritdoc />
        public async Task<QueryResult<PackageFamily>> Query(
            Expression<Func<PackageFamily, bool>> filter,
            IEnumerable<SortingCondition> sort,
            int? limit,
            int? offset,
            ApiRequest apiRequest)
        {
            IQueryable<PackageFamily> query;
            if (apiRequest.Fields.Where(f => f.FieldName == "items").SelectMany(f => f.Fields)
                .Any(f => f.FieldName == "availableVersions"))
            {
                query = (await this.packageRepository.SearchAsync(string.Empty, true)).Select(p => p.Identity)
                    .GroupBy(p => p.Id).Select(
                        g =>
                            {
                                var package = g.OrderByDescending(p => p.Version).First();
                                return new PackageFamily
                                           {
                                               Name = package.Id,
                                               Version = package.Version.ToString(),
                                               AvailableVersions = g.OrderByDescending(p => p.Version)
                                                   .Select(p => p.Version.ToString()).ToList()
                                           };
                            }).AsQueryable();
            }
            else
            {
                query = (await this.packageRepository.SearchAsync(string.Empty, true)).Select(p => p.Identity)
                    .GroupBy(p => p.Id).Select(g => g.OrderByDescending(p => p.Version).First())
                    .Select(p => new PackageFamily { Name = p.Id, Version = p.Version.ToString() }).AsQueryable();
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var count = query.Count();

            if (sort != null)
            {
                query = query.ApplySorting(sort);
            }

            if (offset != null)
            {
                query = query.Skip(offset.Value);
            }

            if (limit != null)
            {
                query = query.Take(limit.Value);
            }

            return new QueryResult<PackageFamily> { Count = count, Items = query };
        }

        /// <inheritdoc />
        public Task<MutationResult<PackageFamily>> Create(PackageFamily newNode)
        {
            throw new InvalidOperationException();
        }

        /// <inheritdoc />
        public Task<MutationResult<PackageFamily>> Update(object id, PackageFamily newNode, ApiRequest request)
        {
            throw new InvalidOperationException();
        }

        /// <inheritdoc />
        public Task<MutationResult<PackageFamily>> Delete(object id)
        {
            throw new InvalidOperationException();
        }
    }
}
