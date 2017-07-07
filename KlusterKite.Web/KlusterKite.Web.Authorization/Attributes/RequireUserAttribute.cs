// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequireUserAttribute.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Marks api method to check for user authentication
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Authorization.Attributes
{
    using System;

    using ClusterKit.Security.Attributes;
    using ClusterKit.Security.Client;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Filters;

    /// <summary>
    /// Marks api method to check for user authentication
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireUserAttribute : Attribute, IAuthorizationFilter
    {
        /// <summary>
        /// Gets or sets the severity of action
        /// </summary>
        public EnSeverity Severity { get; set; } = EnSeverity.Trivial;

        /// <inheritdoc />
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var session = context.HttpContext.GetSession();
            if (session?.User == null)
            {
                SecurityLog.CreateRecord(
                    EnSecurityLogType.OperationDenied,
                    this.Severity,
                    context.HttpContext.GetRequestDescription(),
                    "Attempt to access {ControllerName} action {ActionName} without authenticated user",
                    (context.ActionDescriptor as ControllerActionDescriptor)?.ActionName,
                    (context.ActionDescriptor as ControllerActionDescriptor)?.ControllerName);
                context.Result = new StatusCodeResult(401);
            }
        }
    }
}