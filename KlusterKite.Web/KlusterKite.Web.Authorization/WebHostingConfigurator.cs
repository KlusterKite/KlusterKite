// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebHostingConfigurator.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the WebHostingConfigurator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Web.Authorization
{
    using Akka.Configuration;

    using Microsoft.AspNetCore.Builder;

    /// <summary>
    /// Configures current web-api server to act as OAuth2 authorization server
    /// </summary>
    public class WebHostingConfigurator : BaseWebHostingConfigurator
    {
        /// <inheritdoc />
        public override IApplicationBuilder ConfigureApplication(IApplicationBuilder app, Config config)
        {
            return app.UseMiddleware<CheckTokenMiddleware>();
        }
    }
}
