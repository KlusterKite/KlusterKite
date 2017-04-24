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
    using System.Threading.Tasks;

    using Akka.Actor;

    using ClusterKit.API.Attributes;
    using ClusterKit.API.Client;
    using ClusterKit.Data.CRUD;
    using ClusterKit.Data.CRUD.ActionMessages;
    using ClusterKit.NodeManager.Client;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.Messages;
    using ClusterKit.Security.Attributes;
    using ClusterKit.Web.Authorization.Attributes;

    using JetBrains.Annotations;

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

        /// <summary>
        /// Initiates cluster upgrade procedure. 
        /// </summary>
        /// <param name="id">The id of release that will be applied</param>
        /// <returns>The mutation result</returns>
        [UsedImplicitly]
        [RequireSession]
        [RequireUser]
        [RequireUserPrivilege(Privileges.ClusterUpdate)]
        [DeclareMutation("Initiates cluster upgrade procedure.")]
        public async Task<MutationResult<Migration>> UpdateCluster(
            [ApiDescription("The id of release that will be applied")] int id)
        {
            try
            {
                var response =
                    await this.System.ActorSelection(this.DataActorPath)
                        .Ask<CrudActionResponse<Migration>>(
                            new UpdateClusterRequest { Id = id, Context = this.Context, CurrentReleaseState = EnReleaseState.Obsolete },
                            this.Timeout);
                return CreateResponse(response);
            }
            catch (Exception exception)
            {
                this.System.Log.Error(exception, "{Type}: error on ClusterUpdate", this.GetType().Name);
                return new MutationResult<Migration> { Errors = new[] { new ErrorDescription(null, exception.Message) } };
            }
        }
    }
}
