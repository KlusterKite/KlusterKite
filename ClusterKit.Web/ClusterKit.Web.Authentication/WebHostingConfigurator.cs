// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebHostingConfigurator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the WebHostingConfigurator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Authentication
{
    using System.Web.Http;

    using Akka.Configuration;

    using Castle.Windsor;

    using ClusterKit.Security.Attributes;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;

    /// <summary>
    /// Configures current web-api server to act as OAuth2 authorization server
    /// </summary>
    public class WebHostingConfigurator : IWebHostingConfigurator
    {
        /// <summary>
        /// The windsor container
        /// </summary>
        private readonly IWindsorContainer container;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHostingConfigurator"/> class.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        public WebHostingConfigurator(IWindsorContainer container)
        {
            this.container = container;
        }

        /// <inheritdoc />
        public void ConfigureApi(HttpConfiguration httpConfiguration)
        {
        }

        /*
        /// <inheritdoc />
        public void ConfigureApp(IAppBuilder appBuilder)
        {
            appBuilder.UseCors(CorsOptions.AllowAll);
            Serilog.Log.Logger.Information("------------ Setting up authorization server -------------");
            var config = this.container.Resolve<Config>();

            var authorizeEndpointPath = config.GetString(
                "ClusterKit.Web.Authentication.AuthorizeEndpointPath",
                "/api/1.x/security/authorization");
            var tokenEndpointPath = config.GetString(
                "ClusterKit.Web.AuthenticationTokenEndpointPath",
                "/api/1.x/security/token");

            appBuilder.SetDataProtectionProvider(new DataNoProtectionProvider());
            var tokenProvider = this.container.Resolve<ITokenManager>();

            var options = new OAuthAuthorizationServerOptions
                              {
                                  AuthorizeEndpointPath =
                                      new PathString(authorizeEndpointPath),
                                  TokenEndpointPath = new PathString(tokenEndpointPath),
                                  ApplicationCanDisplayErrors = true,

                                  // internal communication is insecure
                                  AllowInsecureHttp = true,
                                  Provider =
                                      new AuthorizationServerProvider(
                                          this.container.ResolveAll<IClientProvider>()),
                                  RefreshTokenProvider = new RefreshTokenProvider(tokenProvider),
                                  AccessTokenProvider = new AccessTokenProvider(tokenProvider)
                              };

            appBuilder.UseOAuthAuthorizationServer(options);
        }
        */

        /// <inheritdoc />
        public IWebHostBuilder ConfigureApp(IWebHostBuilder hostBuilder)
        {
            return hostBuilder;
        }

        /// <inheritdoc />
        public void Configure(IApplicationBuilder app)
        {
            app.UseCors(builder => { builder.AllowAnyOrigin(); });
            Serilog.Log.Logger.Information("------------ Setting up authorization server -------------");
            /*
            var config = this.container.Resolve<Config>();

            var authorizeEndpointPath = config.GetString(
                "ClusterKit.Web.Authentication.AuthorizeEndpointPath",
                "/api/1.x/security/authorization");
            var tokenEndpointPath = config.GetString(
                "ClusterKit.Web.AuthenticationTokenEndpointPath",
                "/api/1.x/security/token");

            var tokenProvider = this.container.Resolve<ITokenManager>();
            */
        }
    }
}
