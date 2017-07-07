// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NugetPackagesFactory.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Data factory to read packages from nuget feed
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using KlusterKite.API.Client;
    using KlusterKite.Core.Monads;
    using KlusterKite.Data;
    using KlusterKite.Data.CRUD.ActionMessages;
    using KlusterKite.NodeManager.Launcher.Messages;
    using KlusterKite.NodeManager.Launcher.Utils;

    using JetBrains.Annotations;

    /// <summary>
    /// Data factory to read packages from nuget feed
    /// </summary>
    [UsedImplicitly]
    public class NugetPackagesFactory : DataFactory<IPackageRepository, PackageDescription, string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NugetPackagesFactory"/> class.
        /// </summary>
        /// <param name="context">
        /// Nuget server url
        /// </param>
        public NugetPackagesFactory(IPackageRepository context)
            : base(context)
        {
        }

        /// <inheritdoc />
        public override Task<Maybe<PackageDescription>> Delete(string id)
        {
            throw new InvalidOperationException();
        }

        /// <inheritdoc />
        public override Task<Maybe<PackageDescription>> Get(string id)
        {
            throw new InvalidOperationException();
        }

        /// <inheritdoc />
        public override string GetId(PackageDescription obj) => obj.Id;

        /// <inheritdoc />
        public override async Task<CollectionResponse<PackageDescription>> GetList(
            Expression<Func<PackageDescription, bool>> filter,
            List<SortingCondition> sort,
            int? skip,
            int? count,
            ApiRequest apiRequest)
        {
            var query = (await this.Context.SearchAsync(string.Empty, true))
                .Select(p => p.Identity)
                .GroupBy(p => p.Id)
                .Select(g => g.OrderByDescending(p => p.Version).First())
                .Select(p => new PackageDescription(p.Id, p.Version.ToString()))
                .AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var result = new CollectionResponse<PackageDescription> { Count = query.Count() };
            if (sort != null)
            {
                query = query.ApplySorting(sort);
            }

            if (skip.HasValue && query is IOrderedQueryable<PackageDescription>)
            {
                query = query.Skip(skip.Value);
            }

            if (count.HasValue)
            {
                query = query.Take(count.Value);
            }

            result.Items = query.ToList();

            return result;
        }

        /// <inheritdoc />
        public override Task Insert(PackageDescription obj)
        {
            throw new InvalidOperationException();
        }

        /// <inheritdoc />
        public override Task Update(PackageDescription newData, PackageDescription oldData)
        {
            throw new InvalidOperationException();
        }
    }
}