// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseWebHostingConfigurator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Base abstract class to implement <see cref="IWebHostingConfigurator" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Base abstract class to implement <see cref="IWebHostingConfigurator"/>
    /// </summary>
    public abstract class BaseWebHostingConfigurator : IWebHostingConfigurator
    {
        /// <inheritdoc />
        public virtual IApplicationBuilder ConfigureApplication(IApplicationBuilder app)
        {
            return app;
        }

        /// <inheritdoc />
        public virtual void ConfigureServices(IServiceCollection services)
        {
        }
    }
}