// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NugetPackagesFactory.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Data factory to read packages from nuget feed
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
    using ClusterKit.Core.Monads;
    using ClusterKit.Data;
    using ClusterKit.Data.CRUD.ActionMessages;

    using JetBrains.Annotations;

    using NuGet;

    /// <summary>
    /// Data factory to read packages from nuget feed
    /// </summary>
    [UsedImplicitly]
    public class NugetPackagesFactory : DataFactory<string, IPackage, string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NugetPackagesFactory"/> class.
        /// </summary>
        /// <param name="context">
        /// Nuget server url
        /// </param>
        public NugetPackagesFactory(string context)
            : base(context)
        {
        }

        /// <summary>
        /// Deletes object from datasource
        /// </summary>
        /// <param name="id">Objects identification</param>
        /// <returns>
        /// Removed objects data
        /// </returns>
        public override Task<Maybe<IPackage>> Delete(string id)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Gets an object from datasource using it's identification
        /// </summary>
        /// <param name="id">The object's identification</param>
        /// <returns>
        /// Async execution task
        /// </returns>
        public override Task<Maybe<IPackage>> Get(string id)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Gets the object's identification from object
        /// </summary>
        /// <param name="obj">The data object</param>
        /// <returns>
        /// The object's identification
        /// </returns>
        public override string GetId(IPackage obj) => obj.Id;

        /// <inheritdoc />
        public override Task<CollectionResponse<IPackage>> GetList(
            Expression<Func<IPackage, bool>> filter,
            List<SortingCondition> sort,
            int? skip,
            int? count)
        {
            var nugetRepository = PackageRepositoryFactory.Default.CreateRepository(this.Context);
            var query = nugetRepository.Search(string.Empty, true).AsQueryable().Where(p => p.IsLatestVersion);

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var result = new CollectionResponse<IPackage> { Count = query.Count() };
            if (sort != null)
            {
                query = query.ApplySorting(sort);
            }

            if (skip.HasValue && query is IOrderedQueryable<IPackage>)
            {
                query = query.Skip(skip.Value);
            }

            if (count.HasValue)
            {
                query = query.Take(count.Value);
            }

            result.Items = query.ToList();

            return Task.FromResult(result);
        }

        /// <summary>
        /// Adds an object to datasource
        /// </summary>
        /// <param name="obj">The object to add</param>
        /// <returns>
        /// Async execution task
        /// </returns>
        public override Task Insert(IPackage obj)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Updates an object in datasource
        /// </summary>
        /// <param name="newData">The new object's data</param><param name="oldData">The old object's data</param>
        /// <returns>
        /// Async execution task
        /// </returns>
        public override Task Update(IPackage newData, IPackage oldData)
        {
            throw new InvalidOperationException();
        }
    }
}