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

    /// <summary>
    /// Marks api method to check for user authentication
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireSessionAttribute : Attribute, IAuthenticationFilter
    {
        /// <inheritdoc />
        public bool AllowMultiple => false;

        /// <inheritdoc />
        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            if (context.Request.GetOwinContext().GetSession() == null)
            {
                context.Result = new UnauthorizedResult(new AuthenticationHeaderValue[0],  context.Request);
            }

            return Task.CompletedTask;
        }
    }
}
