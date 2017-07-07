// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryResult.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The result of <see cref="INodeConnection{T,TId}.Query" /> method
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Client
{
    using System.Collections.Generic;

    /// <summary>
    /// The result of <see cref="INodeConnection{T}.Query"/> method
    /// </summary>
    /// <typeparam name="T">The type of node</typeparam>
    public class QueryResult<T>
    {
        /// <summary>
        /// Gets or sets the total count of selected objects
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the items limited by <see cref="INodeConnection{T}.Query"/> limit and offset parameters
        /// </summary>
        public IEnumerable<T> Items { get; set; }
    }
}