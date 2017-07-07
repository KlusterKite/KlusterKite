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
    using System.Linq;
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
        /// Checks the draft release if it can be moved to ready state.
        /// </summary>
        /// <param name="id">The id of release draft</param>
        /// <returns>The mutation result</returns>
        [DeclareMutation("checks the draft release if it can be moved to ready state.")]
        [UsedImplicitly]
        [RequireSession]
        [RequireUser]
        [RequireUserPrivilege(Privileges.ReleaseFinish)]
        public async Task<MutationResult<Release>> Check(int id)
        {
            try
            {
                var response =
                    await this.System.ActorSelection(this.DataActorPath)
                        .Ask<CrudActionResponse<Release>>(
                            new ReleaseCheckRequest { Id = id, Context = this.Context },
                            this.Timeout);
                return CreateResponse(response);
            }
            catch (Exception exception)
            {
                this.System.Log.Error(exception, "{Type}: error on check", this.GetType().Name);
                return new MutationResult<Release> { Errors = new[] { new ErrorDescription(null, exception.Message) }.ToList() };
            }
        }

        /// <summary>
        /// Mutation that moves <see cref="Release.State"/> from <see cref="EnReleaseState.Draft"/> to <see cref="EnReleaseState.Ready"/>
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
                return new MutationResult<Release> { Errors = new[] { new ErrorDescription(null, exception.Message) }.ToList() };
            }
        }

        /// <summary>
        /// Mutation that moves <see cref="Release.State"/> from <see cref="EnReleaseState.Ready"/> to <see cref="EnReleaseState.Obsolete"/>
        /// </summary>
        /// <param name="id">The id of release draft</param>
        /// <returns>The mutation result</returns>
        [DeclareMutation("moves release state from \"ready\" to \"obsolete\"")]
        [UsedImplicitly]
        [RequireSession]
        [RequireUser]
        [RequireUserPrivilege(Privileges.ReleaseFinish)]
        public async Task<MutationResult<Release>> SetObsolete(int id)
        {
            try
            {
                var response =
                    await this.System.ActorSelection(this.DataActorPath)
                        .Ask<CrudActionResponse<Release>>(
                            new ReleaseSetObsoleteRequest { Id = id, Context = this.Context },
                            this.Timeout);
                return CreateResponse(response);
            }
            catch (Exception exception)
            {
                this.System.Log.Error(exception, "{Type}: error on SetObsolete", this.GetType().Name);
                return new MutationResult<Release> { Errors = new[] { new ErrorDescription(null, exception.Message) }.ToList() };
            }
        }

        /// <summary>
        /// Mutation that sets the <see cref="Release.IsStable"/>
        /// </summary>
        /// <param name="id">The id of release draft</param>
        /// <param name="isStable">A value indicating the new <see cref="Release.IsStable"/> value</param>
        /// <returns>The mutation result</returns>-
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
                return new MutationResult<Release> { Errors = new[] { new ErrorDescription(null, exception.Message) }.ToList() };
            }
        }
    }
}