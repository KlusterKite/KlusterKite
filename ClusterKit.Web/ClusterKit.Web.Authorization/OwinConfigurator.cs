// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OwinConfigurator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the OwinConfigurator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Authorization
{
    using System.Web.Http;

    using ClusterKit.Security.Attributes;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;

    /// <summary>
    /// Configures current web-api server to act as OAuth2 authorization server
    /// </summary>
    public class OwinConfigurator : IWebHostingConfigurator
    {
        /// <summary>
        /// The token manager
        /// </summary>
        private readonly ITokenManager tokenManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwinConfigurator"/> class.
        /// </summary>
        /// <param name="tokenManager">
        /// The token Manager.
        /// </param>
        public OwinConfigurator(ITokenManager tokenManager)
        {
            this.tokenManager = tokenManager;
        }

        /// <inheritdoc />
        public void ConfigureApi(HttpConfiguration httpConfiguration)
        {
        }



        /// <inheritdoc />
        public IWebHostBuilder ConfigureApp(IWebHostBuilder hostBuilder)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void Configure(IApplicationBuilder app)
        {
            // app.UseMiddleware<Authorizer>(new Authorizer.AuthorizerOptions("Bearer", this.tokenManager));
            app.UseMiddleware<CheckTokenMiddleware>();
        }
    }
}
