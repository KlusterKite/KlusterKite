// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseWebHostingConfigurator.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Base abstract class to implement <see cref="IWebHostingConfigurator" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Web
{
    using Akka.Configuration;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Base abstract class to implement <see cref="IWebHostingConfigurator"/>
    /// </summary>
    public abstract class BaseWebHostingConfigurator : IWebHostingConfigurator
    {
        /// <inheritdoc />
        public virtual IApplicationBuilder ConfigureApplication(IApplicationBuilder app, Config config)
        {
            return app;
        }

        /// <inheritdoc />
        public virtual void ConfigureServices(IServiceCollection services, Config config)
        {
        }
    }
}