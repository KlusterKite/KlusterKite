// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RolesConnection.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The <see cref="Role" /> management
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.WebApi
{
    using System;

    using Akka.Actor;

    using ClusterKit.Data.CRUD;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.Security.Client;

    /// <summary>
    /// The <see cref="Role"/> management
    /// </summary>
    public class RolesConnection : Connection<Role, Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RolesConnection"/> class.
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
        public RolesConnection(ActorSystem actorSystem, string dataActorPath, TimeSpan? timeout, RequestContext context)
            : base(actorSystem, dataActorPath, timeout, context)
        {
        }
    }
}
