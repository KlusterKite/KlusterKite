// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CurrentUserApi.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Publishes access to the authenticated user information
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager
{
    using System.Collections.Generic;
    using System.Linq;

    using ClusterKit.API.Client.Attributes;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.Security.Client;

    using JetBrains.Annotations;

    /// <summary>
    /// Publishes access to the authenticated user information
    /// </summary>
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
        [DeclareField(Description = "Authenticated user")]
        public UserDescription ClusterKitUser => this.context?.Authentication?.User as UserDescription;

        /// <summary>
        /// Gets the current user privileges
        /// </summary>
        [UsedImplicitly]
        [DeclareField(Description = "The current user privileges")]
        public List<string> ClusterKitUserPrivileges => this.context?.Authentication?.UserScope.ToList();

        /// <summary>
        /// Gets the current user privileges
        /// </summary>
        [UsedImplicitly]
        [DeclareField(Description = "The current application privileges")]
        public List<string> ClusterKitClientPrivileges => this.context?.Authentication?.ClientScope.ToList();
    }
}
