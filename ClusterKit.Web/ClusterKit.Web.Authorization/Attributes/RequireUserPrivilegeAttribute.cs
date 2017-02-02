// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequireUserPrivilegeAttribute.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Checks session for authorization
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
    public class RequireUserPrivilegeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// The required privilege
        /// </summary>
        private readonly string privilege;

        /// <summary>
        /// This rule will be ignored if client authenticated on it's own behalf
        /// </summary>
        private readonly bool ignoreOnClientOwnBehalf;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequireUserPrivilegeAttribute"/> class.
        /// </summary>
        /// <param name="privilege">
        /// The required privilege
        /// </param>
        /// <param name="ignoreOnClientOwnBehalf">
        /// This rule will be ignored if client authenticated on it's own behalf
        /// </param>
        public RequireUserPrivilegeAttribute(string privilege, bool ignoreOnClientOwnBehalf = false)
        {
            this.privilege = privilege;
            this.ignoreOnClientOwnBehalf = ignoreOnClientOwnBehalf;
        }

        /// <inheritdoc />
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var session = actionContext.Request.GetOwinContext().GetSession();
            return session == null || (session.User == null && this.ignoreOnClientOwnBehalf)
                   || session.UserScope.Contains(this.privilege);
        }
    }
}
