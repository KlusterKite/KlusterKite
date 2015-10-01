// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IOwinStartupConfigurator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   External additional owin configuration.
//   Shoud be registered in DI resolver
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web
{
    using Owin;

    /// <summary>
    /// External additional owin configuration.
    /// Should be registered in DI resolver
    /// </summary>
    public interface IOwinStartupConfigurator
    {
        /// <summary>
        /// Add additional owin configuration
        /// </summary>
        /// <param name="appBuilder">The builder</param>
        void Configure(IAppBuilder appBuilder);
    }
}