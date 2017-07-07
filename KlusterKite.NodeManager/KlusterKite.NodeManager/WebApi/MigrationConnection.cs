// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigrationConnection.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the MigrationConnection type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.WebApi
{
    using System;

    using Akka.Actor;

    using KlusterKite.Data.CRUD;
    using KlusterKite.NodeManager.Client.ORM;
    using KlusterKite.Security.Attributes;

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
