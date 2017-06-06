// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebHostingConfigurator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the WebHostingConfigurator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Authorization
{
    using ClusterKit.Security.Attributes;

    using Microsoft.AspNetCore.Builder;

    /// <summary>
    /// Configures current web-api server to act as OAuth2 authorization server
    /// </summary>
    public class WebHostingConfigurator : BaseWebHostingConfigurator
    {
        /// <summary>
        /// The token manager
        /// </summary>
        private readonly ITokenManager tokenManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHostingConfigurator"/> class.
        /// </summary>
        /// <param name="tokenManager">
        /// The token Manager.
        /// </param>
        public WebHostingConfigurator(ITokenManager tokenManager)
        {
            this.tokenManager = tokenManager;
        }

        /// <inheritdoc />
        public override IApplicationBuilder ConfigureApplication(IApplicationBuilder app)
        {
            return app.UseMiddleware<CheckTokenMiddleware>(this.tokenManager);
        }
    }
}
