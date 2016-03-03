// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OwinConfiguration.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   External additional owin configuration.
//   Should be registered in DI resolver
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Swagger.Monitor
{
    using System.Web.Http;

    using Owin;

    using Serilog;

    /// <summary>
    /// External additional owin configuration.
    /// Should be registered in DI resolver
    /// </summary>
    public class OwinConfigurator : IOwinStartupConfigurator
    {
        /// <summary>
        /// Add additional http configuration
        /// </summary>
        /// <param name="config">The configuration</param>
        public void ConfigureApi(HttpConfiguration config)
        {
        }

        /// <summary>
        /// Add additional owin configuration
        /// </summary>
        /// <param name="appBuilder">The builder</param>
        public void ConfigureApp(IAppBuilder appBuilder)
        {
        }
    }
}