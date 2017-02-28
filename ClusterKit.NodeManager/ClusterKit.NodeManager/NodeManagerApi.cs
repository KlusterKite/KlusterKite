// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeManagerApi.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The node manager api
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager
{
    using Akka.Actor;

    using ClusterKit.API.Client.Attributes;
    using ClusterKit.Core;
    using ClusterKit.Data.CRUD;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.Security.Client;

    using JetBrains.Annotations;

    /// <summary>
    /// The node manager api
    /// </summary>
    public class NodeManagerApi
    {
        /// <summary>
        /// The actor system.
        /// </summary>
        private readonly ActorSystem actorSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeManagerApi"/> class.
        /// </summary>
        /// <param name="actorSystem">
        /// The actor system.
        /// </param>
        public NodeManagerApi(ActorSystem actorSystem)
        {
            this.actorSystem = actorSystem;
        }

        /// <summary>
        /// The connection to the <see cref="NodeTemplates"/>
        /// </summary>
        /// <param name="context">The request context</param>
        /// <returns>The data connection</returns>
        [UsedImplicitly]
        [DeclareConnection(CanCreate = true, CanDelete = true, CanUpdate = true, Description = "Node templates")]
        public Connection<NodeTemplate, int> NodeTemplates(RequestContext context)
        {
            this.actorSystem.Log.Info("{Type}: NodeTemplates accessed", this.GetType().Name);
            return new Connection<NodeTemplate, int>(
                this.actorSystem,
                "/user/NodeManager/NodeManagerProxy",
                ConfigurationUtils.GetRestTimeout(this.actorSystem),
                context);
        }
    }
}