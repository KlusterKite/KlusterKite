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
    using ClusterKit.Web;

    using Microsoft.AspNetCore.Builder;

    /// <summary>
    /// External additional configuration.
    /// Should be registered in DI resolver
    /// </summary>
    public class WebHostingConfigurator : BaseWebHostingConfigurator
    {
        /// <inheritdoc />
        public override IApplicationBuilder ConfigureApplication(IApplicationBuilder app)
        {
            return app.UseCors(builder => { builder.AllowAnyOrigin(); });
        }
    }
}