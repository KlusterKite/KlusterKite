// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryResult.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The result of <see cref="INodeConnection{T,TId}.Query" /> method
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.API
{
    using System.Collections.Generic;

    /// <summary>
    /// The result of <see cref="INodeConnection{T,TId}.Query"/> method
    /// </summary>
    /// <typeparam name="T">The type of node</typeparam>
    public class QueryResult<T>
    {
        /// <summary>
        /// Gets or sets the total count of selected objects
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the items limited by <see cref="INodeConnection{T,TId}.Query"/> limit and offset parameters
        /// </summary>
        public IEnumerable<T> Items { get; set; }
    }
}