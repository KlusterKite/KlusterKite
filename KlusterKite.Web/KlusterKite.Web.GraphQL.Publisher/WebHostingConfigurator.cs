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
    using Akka.Configuration;

    using ClusterKit.Web;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// External additional configuration.
    /// Should be registered in DI resolver
    /// </summary>
    public class WebHostingConfigurator : BaseWebHostingConfigurator
    {
        /// <inheritdoc />
        public override IApplicationBuilder ConfigureApplication(IApplicationBuilder app, Config config)
        {
            return app.UseCors(builder => { builder.AllowAnyOrigin(); });
        }

        /// <inheritdoc />
        public override void ConfigureServices(IServiceCollection services, Config config)
        {
            services.AddCors();
        }
    }
}