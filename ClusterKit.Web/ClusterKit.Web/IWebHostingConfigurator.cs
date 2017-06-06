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
        /// <param name="app">The application </param>
        /// <returns>The modified application</returns>
        IApplicationBuilder ConfigureApplication(IApplicationBuilder app);

        /// <summary>
        /// Configures services list
        /// </summary>
        /// <param name="services">The services list</param>
        void ConfigureServices(IServiceCollection services);
    }
}