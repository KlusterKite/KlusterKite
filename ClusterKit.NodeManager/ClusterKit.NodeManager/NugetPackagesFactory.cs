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
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;

    using ClusterKit.Core.Data;
    using ClusterKit.Core.Monads;
    using ClusterKit.NodeManager.Client.Messages;

    using JetBrains.Annotations;

    using NuGet;

    /// <summary>
    /// Data factory to read packages from nuget feed
    /// </summary>
    [UsedImplicitly]
    public class NugetPackagesFactory : DataFactory<string, PackageDescription, string>
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
        public override Task<Maybe<PackageDescription>> Delete(string id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets an object from datasource using it's identification
        /// </summary>
        /// <param name="id">The object's identification</param>
        /// <returns>
        /// Async execution task
        /// </returns>
        public override Task<Maybe<PackageDescription>> Get(string id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the object's identification from object
        /// </summary>
        /// <param name="obj">The data object</param>
        /// <returns>
        /// The object's identification
        /// </returns>
        public override string GetId(PackageDescription obj) => obj.Id;

        /// <summary>
        /// Gets a list of objects from datasource
        /// </summary>
        /// <param name="skip">The number of objects to skip from select</param><param name="count">The maximum number of objects to return. Returns all on null.</param>
        /// <returns>
        /// The list of objects from datasource
        /// </returns>
        public override async Task<List<PackageDescription>> GetList(int skip, int? count)
        {
            var nugetRepository = PackageRepositoryFactory.Default.CreateRepository(this.Context);

            return await Task.Run(() => nugetRepository
                .Search(string.Empty, true)
                .Where(p => p.IsLatestVersion)
                .ToList()
                .Select(p => new PackageDescription
                {
                    Id = p.Id,
                    Version = p.Version.Version.ToString()
                }).ToList());
        }

        /// <summary>
        /// Adds an object to datasource
        /// </summary>
        /// <param name="obj">The object to add</param>
        /// <returns>
        /// Async execution task
        /// </returns>
        public override Task Insert(PackageDescription obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates an object in datasource
        /// </summary>
        /// <param name="newData">The new object's data</param><param name="oldData">The old object's data</param>
        /// <returns>
        /// Async execution task
        /// </returns>
        public override Task Update(PackageDescription newData, PackageDescription oldData)
        {
            throw new NotImplementedException();
        }
    }
}