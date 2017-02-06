// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IOwinStartupConfigurator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   External additional owin configuration.
//   Should be registered in DI resolver
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web
{
    using System.Web.Http;

    using Owin;

    /// <summary>
    /// External additional owin configuration.
    /// Should be registered in DI resolver
    /// </summary>
    public interface IOwinStartupConfigurator
    {
        /// <summary>
        /// Add additional http configuration
        /// </summary>
        /// <param name="httpConfiguration">The configuration</param>
        void ConfigureApi(HttpConfiguration httpConfiguration);

        /// <summary>
        /// Add additional owin configuration
        /// </summary>
        /// <param name="appBuilder">The builder</param>
        void ConfigureApp(IAppBuilder appBuilder);
    }
}