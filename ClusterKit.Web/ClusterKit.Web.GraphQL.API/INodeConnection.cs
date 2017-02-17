// --------------------------------------------------------------------------------------------------------------------
// <copyright file="INodeConnection.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Describes node connection
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.API
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Describes node connection
    /// </summary>
    /// <typeparam name="T">The type of node</typeparam>
    /// <typeparam name="TId">The type of node id</typeparam>
    public interface INodeConnection<T, TId>
        where T : class, new()
    {
        /// <summary>
        /// Gets the node by id
        /// </summary>
        /// <param name="id">The node id</param>
        /// <returns>The node or null if nothing found</returns>
        Task<T> GetById(TId id);

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
        /// <returns>
        /// The nodes list
        /// </returns>
        Task<QueryResult<T>> Query(Expression<Func<T, bool>> filter, Expression<Func<IQueryable<T>, IOrderedQueryable<T>>> sort,  int limit, int offset);

        /// <summary>
        /// Creates a new node
        /// </summary>
        /// <param name="newNode">The new node data</param>
        /// <returns>The new node after creation</returns>
        Task<T> Create(T newNode);

        /// <summary>
        /// Updates a node
        /// </summary>
        /// <param name="id">
        /// The node id
        /// </param>
        /// <param name="newNode">
        /// The new node data
        /// </param>
        /// <param name="updatedFields">
        /// The list of an updated Fields.
        /// </param>
        /// <returns>
        /// The new node after update
        /// </returns>
        Task<T> Update(TId id, T newNode, List<string> updatedFields);

        /// <summary>
        /// Removes a node from the data store
        /// </summary>
        /// <param name="id">The node id</param>
        /// <returns>The old node data</returns>
        Task<T> Delete(TId id);
    }
}