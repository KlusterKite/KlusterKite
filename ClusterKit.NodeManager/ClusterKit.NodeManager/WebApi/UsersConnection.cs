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
    using System.Threading.Tasks;

    using Akka.Actor;

    using ClusterKit.API.Client;
    using ClusterKit.API.Client.Attributes;
    using ClusterKit.Data.CRUD;
    using ClusterKit.Data.CRUD.ActionMessages;
    using ClusterKit.NodeManager.Client;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.Messages;
    using ClusterKit.Security.Client;
    using ClusterKit.Web.Authorization.Attributes;

    using JetBrains.Annotations;

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

        /// <summary>
        /// Resets the user password
        /// </summary>
        /// <param name="id">The user uid</param>
        /// <param name="password">The new user password</param>
        /// <returns>The mutation result</returns>
        [UsedImplicitly]
        [DeclareMutation("Resets the user password")]
        [RequireUser]
        [RequireSession]
        [RequireUserPrivilege(Privileges.UserResetPassword)]
        public async Task<MutationResult<User>> ResetPassword(Guid id, string password)
        {
            var result =
                await this.System.ActorSelection(this.DataActorPath)
                    .Ask<CrudActionResponse<User>>(
                        new UserResetPasswordRequest { UserUid = id, NewPassword = password },
                        this.Timeout);

            return CreateResponse(result);
        }

        /// <summary>
        /// Grants a new role to the user
        /// </summary>
        /// <param name="userUid">
        /// The user Uid.
        /// </param>
        /// <param name="roleUid">
        /// The role Uid.
        /// </param>
        /// <returns>
        /// The mutation result
        /// </returns>
        [UsedImplicitly]
        [DeclareMutation("Grants a new role to the user")]
        [RequireUser]
        [RequireSession]
        [RequireUserPrivilege(Privileges.UserRole)]
        public async Task<MutationResult<User>> GrantRole(Guid userUid, Guid roleUid)
        {
            var result =
                await this.System.ActorSelection(this.DataActorPath)
                    .Ask<CrudActionResponse<User>>(
                        new UserRoleAddRequest { UserUid = userUid, RoleUid = roleUid, ReturnUser = true },
                        this.Timeout);

            return CreateResponse(result);
        }

        /// <summary>
        /// Withdraws the role from the user
        /// </summary>
        /// <param name="userUid">
        /// The user Uid.
        /// </param>
        /// <param name="roleUid">
        /// The role Uid.
        /// </param>
        /// <returns>
        /// The mutation result
        /// </returns>
        [UsedImplicitly]
        [DeclareMutation("Withdraws the role from the user")]
        [RequireUser]
        [RequireSession]
        [RequireUserPrivilege(Privileges.UserRole)]
        public async Task<MutationResult<User>> WithdrawRole(Guid userUid, Guid roleUid)
        {
            var result =
                await this.System.ActorSelection(this.DataActorPath)
                    .Ask<CrudActionResponse<User>>(
                        new UserRoleAddRequest { UserUid = userUid, RoleUid = roleUid, ReturnUser = true },
                        this.Timeout);

            return CreateResponse(result);
        }
    }
}
