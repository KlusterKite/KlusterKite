// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnActorType.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Types of actors to generate by NameSpaceActor
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Core
{
    /// <summary>
    /// Types of actors to generate from <seealso cref="NameSpaceActor"/>
    /// </summary>
    public enum EnActorType
    {
        /// <summary>
        /// Just simple actor
        /// </summary>
        Simple,

        /// <summary>
        /// Cluster singleton actor
        /// </summary>
        Singleton,

        /// <summary>
        /// Cluster singleton proxy actor
        /// </summary>
        SingletonProxy,

        /// <summary>
        /// Cluster sharding manager
        /// </summary>
        Sharding,

        /// <summary>
        /// Cluster sharding proxy actor
        /// </summary>
        ShardingProxy
    }
}