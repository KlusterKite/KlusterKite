// --------------------------------------------------------------------------------------------------------------------
// <copyright file="INodeConnection.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Describes node connection
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    /// <summary>
    /// Describes node connection
    /// </summary>
    /// <typeparam name="T">The type of node</typeparam>
    public interface INodeConnection<T>
        where T : class, new()
    {
        /// <summary>
        /// Query the datasource for nodes
        /// </summary>
        /// <param name="filter">
        /// The filtering condition
        /// </param>
        /// <param name="sort">
        /// The sorting method
        /// </param>
        /// <param name="limit">
        /// The maximum number of objects to get
        /// </param>
        /// <param name="offset">
        /// The number of objects to skip
        /// </param>
        /// <param name="apiRequest">
        /// The original api request
        /// </param>
        /// <returns>
        /// The nodes list
        /// </returns>
        [UsedImplicitly]
        Task<QueryResult<T>> Query(
            Expression<Func<T, bool>> filter,
            IEnumerable<SortingCondition> sort,
            int? limit,
            int? offset,
            ApiRequest apiRequest);

        /// <summary>
        /// Creates a new node
        /// </summary>
        /// <param name="newNode">The new node data</param>
        /// <returns>The new node after creation</returns>
        [UsedImplicitly]
        Task<MutationResult<T>> Create(T newNode);

        /// <summary>
        /// Updates a node
        /// </summary>
        /// <param name="id">
        /// The node id
        /// </param>
        /// <param name="newNode">
        /// The new node data
        /// </param>
        /// <param name="request">
        /// The list of an updated Fields.
        /// </param>
        /// <returns>
        /// The new node after update
        /// </returns>
        [UsedImplicitly]
        Task<MutationResult<T>> Update(object id, T newNode, ApiRequest request);

        /// <summary>
        /// Removes a node from the data store
        /// </summary>
        /// <param name="id">The node id</param>
        /// <returns>The old node data</returns>
        [UsedImplicitly]
        Task<MutationResult<T>> Delete(object id);
    }
}