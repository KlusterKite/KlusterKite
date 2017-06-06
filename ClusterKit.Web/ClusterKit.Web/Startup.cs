// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Startup.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The OWIN startup class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Autofac;
    using Autofac.Extensions.DependencyInjection;

    using JetBrains.Annotations;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Practices.ServiceLocation;

    using Serilog;

    /// <summary>
    /// The web hosting startup class.
    /// </summary>
    [UsedImplicitly]
    public class Startup
    {
        /// <summary>
        /// The list of startup configurators
        /// </summary>
        private List<IWebHostingConfigurator> startupConfigurators;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        public Startup()
        {
            var componentContext = ServiceLocator.Current.GetInstance<IComponentContext>();
            this.startupConfigurators = componentContext.Resolve<IEnumerable<IWebHostingConfigurator>>().ToList();
        }

        /// <summary>
        /// The services configuration
        /// </summary>
        /// <param name="services">The list of services</param>
        /// <returns>Service provider</returns>
        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            foreach (var configurator in this.startupConfigurators)
            {
                configurator.ConfigureServices(services);
            }

            var builder = services.AddMvcCore();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                builder.AddApplicationPart(assembly);
            }

            builder.AddControllersAsServices();

            var componentContext = ServiceLocator.Current.GetInstance<IComponentContext>();
            return new AutofacServiceProvider(componentContext);
        }

        /// <summary>
        /// The application configuration
        /// </summary>
        /// <param name="appBuilder">
        /// The builder
        /// </param>
        /// <param name="env">
        /// The env.
        /// </param>
        /// <param name="loggerFactory">
        /// The logger Factory.
        /// </param>
        [UsedImplicitly]
        public void Configure(IApplicationBuilder appBuilder, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddSerilog();
            foreach (var configurator in this.startupConfigurators)
            {
                appBuilder = configurator.ConfigureApplication(appBuilder);
            }

            appBuilder.UseMvc();
        }
    }
}