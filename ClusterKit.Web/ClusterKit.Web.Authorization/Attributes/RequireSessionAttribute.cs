// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequireSessionAttribute.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the RequireSessionAttribute type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Authorization.Attributes
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Filters;
    using System.Web.Http.Results;

    using ClusterKit.Security.Client;

    /// <summary>
    /// Marks api method to check for authentication
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireSessionAttribute : Attribute, IAuthenticationFilter
    {
        /// <inheritdoc />
        public bool AllowMultiple => false;

        /// <summary>
        /// Gets or sets the severity of action
        /// </summary>
        public EnSeverity Severity { get; set; } = EnSeverity.Trivial;

        /// <inheritdoc />
        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            if (context.Request.GetOwinContext().GetSession() != null)
            {
                return Task.CompletedTask;
            }

            SecurityLog.CreateRecord(
                SecurityLog.EnType.OperationDenied,
                this.Severity,
                context.Request.GetOwinContext().GetRequestDescription(),
                "Attempt to access {ControllerName} action {ActionName} without authenticated session",
                context.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                context.ActionContext.ActionDescriptor.ActionName);
            context.Result = new UnauthorizedResult(new AuthenticationHeaderValue[0],  context.Request);

            return Task.CompletedTask;
        }
    }
}
