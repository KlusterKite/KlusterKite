// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigrationConnection.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the MigrationConnection type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.WebApi
{
    using System;

    using Akka.Actor;

    using ClusterKit.Data.CRUD;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.Security.Attributes;

    /// <summary>
    /// The cluster migration management
    /// </summary>
    public class MigrationConnection : Connection<Migration, int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationConnection"/> class.
        /// </summary>
        /// <param name="actorSystem">
        /// The actor system.
        /// </param>
        /// <param name="dataActorPath">
        /// The data actor path.
        /// </param>
        /// <param name="timeout">
        /// The timeout.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        public MigrationConnection(ActorSystem actorSystem, string dataActorPath, TimeSpan? timeout, RequestContext context)
            : base(actorSystem, dataActorPath, timeout, context)
        {
        }
    }
}
