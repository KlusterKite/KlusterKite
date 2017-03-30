// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UsersConnection.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The <see cref="User" /> management
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
    /// The <see cref="User"/> management
    /// </summary>
    public class UsersConnection : Connection<User, Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UsersConnection"/> class.
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
        public UsersConnection(ActorSystem actorSystem, string dataActorPath, TimeSpan? timeout, RequestContext context)
            : base(actorSystem, dataActorPath, timeout, context)
        {
        }
    }
}
