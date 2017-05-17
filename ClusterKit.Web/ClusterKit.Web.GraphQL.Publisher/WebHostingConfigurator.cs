// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebHostingConfigurator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   External additional configuration.
//   Should be registered in DI resolver
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher
{
    using System.Web.Http;

    using ClusterKit.Web;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Cors.Infrastructure;
    using Microsoft.AspNetCore.Hosting;

    /// <summary>
    /// External additional configuration.
    /// Should be registered in DI resolver
    /// </summary>
    public class WebHostingConfigurator : IWebHostingConfigurator
    {
        /// <inheritdoc />
        public IWebHostBuilder ConfigureApp(IWebHostBuilder hostBuilder)
        {
            return hostBuilder;
        }

        /// <inheritdoc />
        public void Configure(IApplicationBuilder app)
        {
            app.UseCors(builder => { builder.AllowAnyOrigin(); });
        }
    }
}