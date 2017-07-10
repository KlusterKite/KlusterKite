// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CheckTokenMiddleware.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Checks request for the invalid token to return proper result
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Web.Authorization
{
    using System;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using KlusterKite.Security.Attributes;

    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Checks request for the invalid token to return proper result
    /// </summary>
    public class CheckTokenMiddleware
    {
        /// <summary>
        /// The next request processor in the request processing pipeline
        /// </summary>
        private readonly RequestDelegate next;

        /// <summary>
        /// The token manager.
        /// </summary>
        private readonly ITokenManager tokenManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckTokenMiddleware"/> class.
        /// </summary>
        /// <param name="next">
        /// The next request processor in the request processing pipeline
        /// </param>
        /// <param name="tokenManager">
        /// The token Manager.
        /// </param>
        public CheckTokenMiddleware(RequestDelegate next, ITokenManager tokenManager)
        {
            this.next = next;
            this.tokenManager = tokenManager;
        }

        /// <summary>Process an individual request.</summary>
        /// <param name="context">The request context</param>
        /// <returns>The async process task</returns>
        [UsedImplicitly]
        public async Task Invoke(HttpContext context)
        {
            var header = context.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrWhiteSpace(header)
                && header.IndexOf("bearer ", StringComparison.OrdinalIgnoreCase) == 0)
            {
                var token = header.Substring(7);
                var ticket = await this.tokenManager.ValidateAccessToken(token);
                if (ticket == null)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Invalid token");
                    return;
                }

                context.Items["AccessTicket"] = ticket;
            }
           
            await this.next.Invoke(context);
        }
    }
}