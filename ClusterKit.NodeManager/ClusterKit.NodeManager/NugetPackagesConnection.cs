 // --------------------------------------------------------------------------------------------------------------------
// <copyright file="NugetPackagesConnection.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The connection to the nuget server
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using ClusterKit.API.Client;
    using ClusterKit.NodeManager.Client.ApiSurrogates;

    using NuGet;

    /// <summary>
    /// The connection to the nuget server
    /// </summary>
    public class NugetPackagesConnection : INodeConnection<PackageDescriptionSurrogate>
    {
        /// <summary>
        /// The nuget server url
        /// </summary>
        private string nugetServerUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="NugetPackagesConnection"/> class.
        /// </summary>
        /// <param name="nugetServerUrl">
        /// The nuget server url.
        /// </param>
        public NugetPackagesConnection(string nugetServerUrl)
        {
            this.nugetServerUrl = nugetServerUrl;
        }

        /// <inheritdoc />
        public Task<QueryResult<PackageDescriptionSurrogate>> Query(
            Expression<Func<PackageDescriptionSurrogate, bool>> filter,
            IEnumerable<SortingCondition> sort,
            int? limit,
            int? offset,
            ApiRequest apiRequest)
        {
            var nugetRepository = PackageRepositoryFactory.Default.CreateRepository(this.nugetServerUrl);

            IQueryable<PackageDescriptionSurrogate> query;
            if (apiRequest.Fields.Where(f => f.FieldName == "items").SelectMany(f => f.Fields).Any(f => f.FieldName == "availableVersions"))
            {
                query = nugetRepository.Search(string.Empty, true).ToList().GroupBy(p => p.Id).Select(
                    g =>
                        {
                            var package = g.FirstOrDefault(p => p.IsLatestVersion) ?? g.First();
                            return new PackageDescriptionSurrogate
                                       {
                                           Name = package.Id,
                                           Version = package.Version.ToString(),
                                           AvailableVersions =
                                               g.OrderByDescending(p => p.Version)
                                                   .Select(p => p.Version.ToString())
                                                   .ToList()
                                       };
                        }).AsQueryable();
            }
            else
            {
                query =
                    nugetRepository.Search(string.Empty, true)
                        .Where(p => p.IsLatestVersion)
                        .ToList()
                        .Select(p => new PackageDescriptionSurrogate { Name = p.Id, Version = p.Version.ToString() })
                        .AsQueryable();
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

            return Task.FromResult(new QueryResult<PackageDescriptionSurrogate> { Count = count, Items = query });
        }

        /// <inheritdoc />
        public Task<MutationResult<PackageDescriptionSurrogate>> Create(PackageDescriptionSurrogate newNode)
        {
            throw new InvalidOperationException();
        }

        /// <inheritdoc />
        public Task<MutationResult<PackageDescriptionSurrogate>> Update(object id, PackageDescriptionSurrogate newNode, ApiRequest request)
        {
            throw new InvalidOperationException();
        }

        /// <inheritdoc />
        public Task<MutationResult<PackageDescriptionSurrogate>> Delete(object id)
        {
            throw new InvalidOperationException();
        }
    }
}
