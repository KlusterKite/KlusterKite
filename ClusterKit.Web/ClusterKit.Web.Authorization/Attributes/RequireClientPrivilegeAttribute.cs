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

    using ClusterKit.Security.Attributes;
    using ClusterKit.Security.Client;

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
        /// Initializes a new instance of the <see cref="RequireClientPrivilegeAttribute"/> class.
        /// </summary>
        /// <param name="privilege">
        /// The required privilege
        /// </param>
        public RequireClientPrivilegeAttribute(string privilege)
        {
            this.privilege = privilege;
        }

        /// <summary>
        /// Gets or sets the severity of action
        /// </summary>
        public EnSeverity Severity { get; set; } = EnSeverity.Trivial;

        /// <summary>
        /// Gets or sets a value indicating whether this rule will be ignored if there is an authenticated user
        /// </summary>
        public bool IgnoreOnUserPresent { get; set; }

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

            /*
            var session = actionContext.Request.GetSession();
            var isAuthorized = session == null || (session.User != null && this.IgnoreOnUserPresent)
                               || session.ClientScope.Contains(requiredPrivilege);

            if (!isAuthorized)
            {
                SecurityLog.CreateRecord(
                    EnSecurityLogType.OperationDenied,
                    this.Severity,
                    actionContext.Request.GetOwinContext().GetRequestDescription(),
                    "Attempt to access {ControllerName} action {ActionName} without required client privilege {Privilege}",
                    actionContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                    actionContext.ActionDescriptor.ActionName,
                    this.privilege);
            }

            return isAuthorized;
            */
            return false;
        }
    }
}