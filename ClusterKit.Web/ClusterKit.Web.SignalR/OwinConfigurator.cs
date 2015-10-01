// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OwinConfigurator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   External additional owin configuration.
//   Should be registered in DI resolver
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.SignalR
{
    using ClusterKit.Web;
    using Microsoft.Owin.Cors;
    using Owin;

    /// <summary>
    /// External additional owin configuration.
    /// Should be registered in DI resolver
    /// </summary>
    public class OwinConfigurator : IOwinStartupConfigurator
    {
        /// <summary>
        /// Add additional owin configuration
        /// </summary>
        /// <param name="appBuilder">The builder</param>
        public void Configure(IAppBuilder appBuilder)
        {
            appBuilder.UseCors(CorsOptions.AllowAll);
            appBuilder.MapSignalR();
        }
    }
}