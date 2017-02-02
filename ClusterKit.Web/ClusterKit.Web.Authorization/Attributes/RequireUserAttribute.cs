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
    public class RequireUserAttribute : Attribute, IAuthenticationFilter
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
            var userSession = context.Request.GetOwinContext().GetSession();
            if (userSession?.User == null)
            {
                context.Result = new UnauthorizedResult(new AuthenticationHeaderValue[0], context.Request);
            }

            return Task.CompletedTask;
        }
    }
}