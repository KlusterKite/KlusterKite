// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReleaseConnection.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the ReleaseConnection type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Akka;
    using Akka.Actor;

    using ClusterKit.API.Client;
    using ClusterKit.API.Client.Attributes;
    using ClusterKit.Data.CRUD;
    using ClusterKit.Data.CRUD.ActionMessages;
    using ClusterKit.Data.CRUD.Exceptions;
    using ClusterKit.NodeManager.Client;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.Messages;
    using ClusterKit.Security.Client;
    using ClusterKit.Web.Authorization.Attributes;

    using JetBrains.Annotations;

    /// <summary>
    /// The release management
    /// </summary>
    public class ReleaseConnection : Connection<Release, int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseConnection"/> class.
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
        public ReleaseConnection(
            ActorSystem actorSystem,
            string dataActorPath,
            TimeSpan? timeout,
            RequestContext context)
            : base(actorSystem, dataActorPath, timeout, context)
        {
        }

        /// <summary>
        /// Initiates cluster upgrade procedure. The previous active release will be marked as <see cref="Release.EnState.Obsolete"/>
        /// </summary>
        /// <param name="id">The id of release that will be applied</param>
        /// <returns>The mutation result</returns>
        [UsedImplicitly]
        [RequireSession]
        [RequireUser]
        [RequireUserPrivilege(Privileges.ClusterUpdate)]
        [DeclareMutation("Initiates cluster upgrade procedure. The previous active release will be marked as failed")]
        public async Task<MutationResult<Release>> RollbackCluster(
            [ApiDescription(Description = "The id of release that will be applied")] int id)
        {
            try
            {
                var response =
                    await this.System.ActorSelection(this.DataActorPath)
                        .Ask<CrudActionResponse<Release>>(
                            new UpdateClusterRequest { Id = id, Context = this.Context, CurrentReleaseState = Release.EnState.Faulted },
                            this.Timeout);
                return CreateResponse(response);
            }
            catch (Exception exception)
            {
                this.System.Log.Error(exception, "{Type}: error on RollbackCluster", this.GetType().Name);
                return new MutationResult<Release> { Errors = new[] { new ErrorDescription(null, exception.Message) } };
            }
        }

        /// <summary>
        /// Mutation that moves <see cref="Release.State"/> from <see cref="Release.EnState.Draft"/> to <see cref="Release.EnState.Ready"/>
        /// </summary>
        /// <param name="id">The id of release draft</param>
        /// <returns>The mutation result</returns>
        [DeclareMutation("moves release state from \"draft\" to \"ready\"")]
        [UsedImplicitly]
        [RequireSession]
        [RequireUser]
        [RequireUserPrivilege(Privileges.ReleaseFinish)]
        public async Task<MutationResult<Release>> SetReady(int id)
        {
            try
            {
                var response =
                    await this.System.ActorSelection(this.DataActorPath)
                        .Ask<CrudActionResponse<Release>>(
                            new ReleaseSetReadyRequest { Id = id, Context = this.Context },
                            this.Timeout);
                return CreateResponse(response);
            }
            catch (Exception exception)
            {
                this.System.Log.Error(exception, "{Type}: error on SetReady", this.GetType().Name);
                return new MutationResult<Release> { Errors = new[] { new ErrorDescription(null, exception.Message) } };
            }
        }

        /// <summary>
        /// Mutation that sets the <see cref="Release.IsStable"/>
        /// </summary>
        /// <param name="id">The id of release draft</param>
        /// <param name="isStable">A value indicating the new <see cref="Release.IsStable"/> value</param>
        /// <returns>The mutation result</returns>
        [UsedImplicitly]
        [RequireSession]
        [RequireUser]
        [RequireUserPrivilege(Privileges.ClusterUpdate)]
        [DeclareMutation("moves release state from \"draft\" to \"ready\"")]
        public async Task<MutationResult<Release>> SetStable(int id, bool isStable)
        {
            try
            {
                var response =
                    await this.System.ActorSelection(this.DataActorPath)
                        .Ask<CrudActionResponse<Release>>(
                            new ReleaseSetStableRequest { Id = id, Context = this.Context, IsStable = isStable },
                            this.Timeout);
                return CreateResponse(response);
            }
            catch (Exception exception)
            {
                this.System.Log.Error(exception, "{Type}: error on SetStable", this.GetType().Name);
                return new MutationResult<Release> { Errors = new[] { new ErrorDescription(null, exception.Message) } };
            }
        }

        /// <summary>
        /// Initiates cluster upgrade procedure. The previous active release will be marked as <see cref="Release.EnState.Obsolete"/>
        /// </summary>
        /// <param name="id">The id of release that will be applied</param>
        /// <returns>The mutation result</returns>
        [UsedImplicitly]
        [RequireSession]
        [RequireUser]
        [RequireUserPrivilege(Privileges.ClusterUpdate)]
        [DeclareMutation("Initiates cluster upgrade procedure. The previous active release will be marked as obsolete")]
        public async Task<MutationResult<Release>> UpdateCluster(
            [ApiDescription("The id of release that will be applied")] int id)
        {
            try
            {
                var response =
                    await this.System.ActorSelection(this.DataActorPath)
                        .Ask<CrudActionResponse<Release>>(
                            new UpdateClusterRequest { Id = id, Context = this.Context, CurrentReleaseState = Release.EnState.Obsolete },
                            this.Timeout);
                return CreateResponse(response);
            }
            catch (Exception exception)
            {
                this.System.Log.Error(exception, "{Type}: error on ClusterUpdate", this.GetType().Name);
                return new MutationResult<Release> { Errors = new[] { new ErrorDescription(null, exception.Message) } };
            }
        }

        /// <summary>
        /// Creates mutation response from actor response
        /// </summary>
        /// <param name="response">The actor response</param>
        /// <returns>The mutation response</returns>
        private static MutationResult<Release> CreateResponse(CrudActionResponse<Release> response)
        {
            if (response.Data != null)
            {
                return new MutationResult<Release> { Result = response.Data };
            }

            var errors =
                response.Exception.Match<List<ErrorDescription>>()
                    .With<EntityNotFoundException>(
                        e => new List<ErrorDescription> { new ErrorDescription("releaseId", "not found") })
                    .With<MutationException>(e => e.Errors)
                    .ResultOrDefault(
                        e =>
                            new List<ErrorDescription>
                                {
                                    new ErrorDescription(
                                        "null",
                                        ((Exception)e)?.Message ?? "unknown error")
                                });

            return new MutationResult<Release> { Errors = errors };
        }
    }
}