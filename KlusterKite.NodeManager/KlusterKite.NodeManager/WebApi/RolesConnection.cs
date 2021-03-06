﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RolesConnection.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The <see cref="Role" /> management
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.WebApi
{
    using System;
    using System.Threading.Tasks;

    using Akka.Actor;

    using JetBrains.Annotations;

    using KlusterKite.API.Attributes;
    using KlusterKite.API.Client;
    using KlusterKite.Data.CRUD;
    using KlusterKite.Data.CRUD.ActionMessages;
    using KlusterKite.NodeManager.Client;
    using KlusterKite.NodeManager.Client.ORM;
    using KlusterKite.NodeManager.Messages;
    using KlusterKite.Security.Attributes;
    using KlusterKite.Web.Authorization.Attributes;

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
        public async Task<MutationResult<Role>> GrantToUser(Guid userUid, Guid roleUid)
        {
            var result =
                await this.System.ActorSelection(this.DataActorPath)
                    .Ask<CrudActionResponse<Role>>(
                        new UserRoleAddRequest { UserUid = userUid, RoleUid = roleUid, ReturnUser = false },
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
        public async Task<MutationResult<Role>> WithdrawFromUser(Guid userUid, Guid roleUid)
        {
            var result =
                await this.System.ActorSelection(this.DataActorPath)
                    .Ask<CrudActionResponse<Role>>(
                        new UserRoleRemoveRequest { UserUid = userUid, RoleUid = roleUid, ReturnUser = false },
                        this.Timeout);

            return CreateResponse(result);
        }
    }
}
