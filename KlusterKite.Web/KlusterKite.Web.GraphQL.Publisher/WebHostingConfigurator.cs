// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebHostingConfigurator.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   External additional configuration.
//   Should be registered in DI resolver
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Web.GraphQL.Publisher
{
    using Akka.Configuration;

    using KlusterKite.Web;

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