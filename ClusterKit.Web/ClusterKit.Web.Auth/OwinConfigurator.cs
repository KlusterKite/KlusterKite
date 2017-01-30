// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OwinConfigurator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the OwinConfigurator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Auth
{
    using System.Web.Http;

    using Castle.Windsor;

    using ClusterKit.Security.Client;

    using Microsoft.Owin;
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
            var options = new OAuthAuthorizationServerOptions
                              {
                                  AuthorizeEndpointPath = new PathString("/api/1.x/security/authorization"),
                                  TokenEndpointPath = new PathString("/api/1.x/security/token"),
                                  ApplicationCanDisplayErrors = true,
                                  AllowInsecureHttp = true, // internal communication is insecure
                                  Provider = new AuthorizationServerProvider(this.container.ResolveAll<IClientProvider>())
                              };

            appBuilder.UseOAuthAuthorizationServer(options);
        }
    }
}
