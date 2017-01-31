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

    using Akka.Actor;

    using Owin;

    /// <summary>
    /// Configures current web-api server to act as OAuth2 authorization server
    /// </summary>
    public class OwinConfigurator : IOwinStartupConfigurator
    {
        /// <summary>
        /// The current actor system
        /// </summary>
        private readonly ActorSystem system;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwinConfigurator"/> class.
        /// </summary>
        /// <param name="system">
        /// The system.
        /// </param>
        public OwinConfigurator(ActorSystem system)
        {
            this.system = system;
        }

        /// <inheritdoc />
        public void ConfigureApi(HttpConfiguration httpConfiguration)
        {
        }

        /// <inheritdoc />
        public void ConfigureApp(IAppBuilder appBuilder)
        {
            appBuilder.Use<Authorizer>(new Authorizer.AuthorizerOptions("Bearer", this.system));
        }
    }
}
