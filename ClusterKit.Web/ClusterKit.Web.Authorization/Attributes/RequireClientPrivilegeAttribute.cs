// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequireClientPrivilegeAttribute.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Checks session for action authorization
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Authorization.Attributes
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Controllers;

    /// <summary>
    /// Checks session for action authorization
    /// </summary>
    /// <remarks>The authentication should be required in separate attribute.</remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireClientPrivilegeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// The required privilege
        /// </summary>
        private readonly string privilege;

        /// <summary>
        /// This rule will be ignored if there is an authenticated user
        /// </summary>
        private readonly bool ignoreOnUserPresent;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequireClientPrivilegeAttribute"/> class.
        /// </summary>
        /// <param name="privilege">
        /// The required privilege
        /// </param>
        /// <param name="ignoreOnUserPresent">
        /// This rule will be ignored if there is an authenticated user
        /// </param>
        public RequireClientPrivilegeAttribute(string privilege, bool ignoreOnUserPresent = false)
        {
            this.privilege = privilege;
            this.ignoreOnUserPresent = ignoreOnUserPresent;
        }

        /// <inheritdoc />
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var session = actionContext.Request.GetOwinContext().GetSession();
            return session == null || (session.User != null && this.ignoreOnUserPresent)
                   || session.ClientScope.Contains(this.privilege);
        }
    }
}