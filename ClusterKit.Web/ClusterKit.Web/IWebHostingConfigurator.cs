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
    using System.Web.Http;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;

    /// <summary>
    /// External additional web hosting configuration.
    /// Should be registered in DI resolver
    /// </summary>
    public interface IWebHostingConfigurator
    {
        /// <summary>
        /// Add additional host configuration
        /// </summary>
        /// <param name="hostBuilder">
        /// The builder
        /// </param>
        /// <returns>
        /// The <see cref="IWebHostBuilder"/>.
        /// </returns>
        IWebHostBuilder ConfigureApp(IWebHostBuilder hostBuilder);

        /// <summary>
        /// Add additional application configuration
        /// </summary>
        /// <param name="app">The application </param>
        void Configure(IApplicationBuilder app);
    }
}