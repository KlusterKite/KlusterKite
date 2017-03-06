// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IConnectionResolver.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The connection resolver public methods
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Provider.Resolvers
{
    using System.Threading.Tasks;

    /// <summary>
    /// The connection resolver public methods
    /// </summary>
    public interface IConnectionResolver
    {
        /// <summary>
        /// Gets the node object resolver
        /// </summary>
        IResolver NodeResolver { get; }

        /// <summary>
        /// Gets node object from node connection by it's id
        /// </summary>
        /// <param name="nodeConnection">The node connection object</param>
        /// <param name="id">The object's serialized id</param>
        /// <returns>The node</returns>
        Task<object> GetNodeById(object nodeConnection, string id);
    }
}