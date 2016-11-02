// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShardedMessage.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The message that can be used for sharded distribution
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core
{
    /// <summary>
    /// The message that can be used for sharded distribution
    /// </summary>
    public interface IShardedMessage
    {
        /// <summary>
        /// Gets the entity id
        /// </summary>
        string EntityId { get; }

        /// <summary>
        /// Gets the shard id
        /// </summary>
        string ShardId { get; }
    }
}
