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
        /// Initializes a new instance of the <see cref="RequireUserPrivilegeAttribute"/> class.
        /// </summary>
        /// <param name="privilege">
        /// The required privilege
        /// </param>
        public RequireUserPrivilegeAttribute(string privilege)
        {
            this.privilege = privilege;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this rule will be ignored if client authenticated on it's own behalf
        /// </summary>
        public bool IgnoreOnClientOwnBehalf { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the resulting privilege test will be run against combined string of specified privilege and action method name.
        /// </summary>
        /// <example>
        /// For example specified privilege is "ClusterKit.Object" and controller action called is "Get", then user will require privilege "ClusterKit.Object.Get" in order to call this method
        /// </example>
        public bool CombinePrivilegeWithActionName { get; set; }

        /// <inheritdoc />
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var requiredPrivilege = this.CombinePrivilegeWithActionName
                                        ? $"{this.privilege}.{actionContext.ActionDescriptor.ActionName}"
                                        : this.privilege;

            var session = actionContext.Request.GetOwinContext().GetSession();
            return session == null || (session.User == null && this.IgnoreOnClientOwnBehalf)
                   || session.UserScope.Contains(requiredPrivilege);
        }
    }
}