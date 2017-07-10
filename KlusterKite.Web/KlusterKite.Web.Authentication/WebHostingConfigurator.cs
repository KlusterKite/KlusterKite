// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebHostingConfigurator.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the WebHostingConfigurator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Web.Authentication
{
    using Akka.Configuration;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Configures current web-api server to act as OAuth2 authorization server
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
