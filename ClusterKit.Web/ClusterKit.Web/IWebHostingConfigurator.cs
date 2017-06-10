// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IWebHostingConfigurator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   External additional web hosting configuration.
//   Should be registered in DI resolver
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web
{
    using Akka.Configuration;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// External additional web hosting configuration.
    /// Should be registered in DI resolver
    /// </summary>
    public interface IWebHostingConfigurator
    {
        /// <summary>
        /// Add additional application configuration
        /// </summary>
        /// <param name="app">
        /// The application 
        /// </param>
        /// <param name="config">
        /// The config.
        /// </param>
        /// <returns>
        /// The modified application
        /// </returns>
        IApplicationBuilder ConfigureApplication(IApplicationBuilder app, Config config);

        /// <summary>
        /// Configures services list
        /// </summary>
        /// <param name="services">
        /// The services list
        /// </param>
        /// <param name="config">
        /// The config.
        /// </param>
        void ConfigureServices(IServiceCollection services, Config config);
    }
}