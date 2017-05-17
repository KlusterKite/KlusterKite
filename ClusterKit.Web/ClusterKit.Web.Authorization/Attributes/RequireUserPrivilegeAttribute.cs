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
    using System.Web.Http.Controllers;

    using ClusterKit.Security.Attributes;
    using ClusterKit.Security.Client;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Filters;

    /// <summary>
    /// Checks session for action authorization
    /// </summary>
    /// <remarks>The authentication should be required in separate attribute.</remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireUserPrivilegeAttribute : Attribute, IAuthorizationFilter
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
        /// Gets or sets the severity of action
        /// </summary>
        public EnSeverity Severity { get; set; } = EnSeverity.Trivial;

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
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var controllerContext = context.ActionDescriptor as ControllerActionDescriptor;
            var requiredPrivilege = this.CombinePrivilegeWithActionName
                                        ? $"{this.privilege}.{controllerContext?.ActionName}"
                                        : this.privilege;

            var session = context.HttpContext.GetSession();
            var isAuthorized = session == null
                               || (session.User == null && this.IgnoreOnClientOwnBehalf)
                               || session.UserScope.Contains(requiredPrivilege);

            if (!isAuthorized)
            {
                SecurityLog.CreateRecord(
                    EnSecurityLogType.OperationDenied,
                    this.Severity,
                    context.HttpContext.GetRequestDescription(),
                    "Attempt to access {ControllerName} action {ActionName} without required user privilege {Privilege}",
                    controllerContext?.ControllerName,
                    controllerContext?.ActionName,
                    this.privilege);
                context.Result = new StatusCodeResult(401);
            }
        }
    }
}