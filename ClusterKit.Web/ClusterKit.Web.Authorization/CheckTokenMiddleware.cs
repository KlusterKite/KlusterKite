// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CheckTokenMiddleware.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Checks request for the invalid token to return proper result
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Authorization
{
    using System.Threading.Tasks;

    using Microsoft.Owin;

    /// <summary>
    /// Checks request for the invalid token to return proper result
    /// </summary>
    public class CheckTokenMiddleware : OwinMiddleware
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckTokenMiddleware"/> class.
        /// </summary>
        /// <param name="next">
        /// The next.
        /// </param>
        public CheckTokenMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        /// <inheritdoc />
        public override Task Invoke(IOwinContext context)
        {
            if (context.Get<bool>("InvalidToken"))
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            }

            return this.Next.Invoke(context);
        }
    }
}