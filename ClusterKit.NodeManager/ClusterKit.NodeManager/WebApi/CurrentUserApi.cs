// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CurrentUserApi.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Publishes access to the authenticated user information
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ClusterKit.API.Client;
    using ClusterKit.API.Client.Attributes;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.Security.Client;

    using JetBrains.Annotations;

    /// <summary>
    /// Publishes access to the authenticated user information
    /// </summary>
    [ApiDescription("Publishes access to the authenticated user information", Name = "CurrentUserApi")]
    public class CurrentUserApi
    {
        /// <summary>
        /// The context.
        /// </summary>
        private readonly RequestContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentUserApi"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public CurrentUserApi(RequestContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Gets the current user data
        /// </summary>
        [UsedImplicitly]
        [DeclareField("Authenticated user")]
        public UserDescription ClusterKitUser => this.context?.Authentication?.User as UserDescription;

        /// <summary>
        /// Gets the current user privileges
        /// </summary>
        [UsedImplicitly]
        [DeclareField("The current user privileges")]
        public List<string> ClusterKitUserPrivileges => this.context?.Authentication?.UserScope.ToList();

        /// <summary>
        /// Gets the current user privileges
        /// </summary>
        [UsedImplicitly]
        [DeclareField("The current application privileges")]
        public List<string> ClusterKitClientPrivileges => this.context?.Authentication?.ClientScope.ToList();

        /// <summary>
        /// Changes the current user password
        /// </summary>
        /// <param name="oldPassword">The user current password</param>
        /// <param name="newPassword">The user new password</param>
        /// <returns>The success of the operation</returns>
        [UsedImplicitly]
        [DeclareMutation("Changes the current user password")]
        public Task<MutationResult<bool>> ChangePassword(
            [ApiDescription("The user current password")] string oldPassword,
            [ApiDescription("The user current password")] string newPassword)
        {
            throw new NotImplementedException();
        }
    }
}
