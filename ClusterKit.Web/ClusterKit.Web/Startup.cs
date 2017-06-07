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
    using System.Threading.Tasks;

    using Autofac;
    using Autofac.Core;
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
        /// Gets source for service configuration waiter
        /// </summary>
        private static TaskCompletionSource<object> serviceConfigurationWaiter = new TaskCompletionSource<object>();

        /// <summary>
        /// Gets or sets the container builder
        /// </summary>
        internal static ContainerBuilder ContainerBuilder { get; set; }

        /// <summary>
        /// Gets the task to wait for container
        /// </summary>
        internal static TaskCompletionSource<IComponentContext> ContainerWaiter { get; } = new TaskCompletionSource<IComponentContext>();

        /// <summary>
        /// Gets the task to wait for container
        /// </summary>
        internal static Task<object> ServiceConfigurationWaiter => serviceConfigurationWaiter.Task;

        /// <summary>
        /// The services configuration
        /// </summary>
        /// <param name="services">The list of services</param>
        /// <returns>Service provider</returns>
        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();


            var builder = services.AddMvcCore();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                builder.AddApplicationPart(assembly);
            }
            
            builder.AddControllersAsServices();
            ContainerBuilder.Populate(services);
            serviceConfigurationWaiter.SetResult(null);
            var container = ContainerWaiter.Task.GetAwaiter().GetResult();
            var startupConfigurators = container.Resolve<IEnumerable<IWebHostingConfigurator>>().ToList();
            foreach (var configurator in startupConfigurators)
            {
                configurator.ConfigureServices(services);
            }

            return new AutofacServiceProvider(container);
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
            var container = ContainerWaiter.Task.GetAwaiter().GetResult();
            var startupConfigurators = container.Resolve<IEnumerable<IWebHostingConfigurator>>().ToList();
            foreach (var configurator in startupConfigurators)
            {
                appBuilder = configurator.ConfigureApplication(appBuilder);
            }

            appBuilder.UseMvc();
        }
    }
}