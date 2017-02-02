// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OwinConfigurator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the OwinConfigurator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Authentication
{
    using System.Web.Http;

    using Akka.Configuration;

    using Castle.Windsor;

    using ClusterKit.Security.Client;

    using Microsoft.Owin;
    using Microsoft.Owin.Security.DataProtection;
    using Microsoft.Owin.Security.OAuth;

    using Owin;

    /// <summary>
    /// Configures current web-api server to act as OAuth2 authorization server
    /// </summary>
    public class OwinConfigurator : IOwinStartupConfigurator
    {
        /// <summary>
        /// The windsor container
        /// </summary>
        private readonly IWindsorContainer container;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwinConfigurator"/> class.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        public OwinConfigurator(IWindsorContainer container)
        {
            this.container = container;
        }

        /// <inheritdoc />
        public void ConfigureApi(HttpConfiguration httpConfiguration)
        {
        }

        /// <inheritdoc />
        public void ConfigureApp(IAppBuilder appBuilder)
        {
            Serilog.Log.Logger.Information("------------ Setting up authorization server -------------");
            var config = this.container.Resolve<Config>();

            var authorizeEndpointPath = config.GetString(
                "ClusterKit.Web.Authentication.AuthorizeEndpointPath",
                "/api/1.x/security/authorization");
            var tokenEndpointPath = config.GetString(
                "ClusterKit.Web.AuthenticationTokenEndpointPath",
                "/api/1.x/security/token");

            appBuilder.SetDataProtectionProvider(new DataNoProtectionProvider());
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
                                  AccessTokenProvider =
                                      new AuthenticationTokenProvider(
                                          this.container.Resolve<ITokenManager>()),
                              };

            appBuilder.UseOAuthAuthorizationServer(options);
        }
    }
}
