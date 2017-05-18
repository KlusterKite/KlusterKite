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

    using Castle.Windsor;

    using JetBrains.Annotations;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
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
            var windsorContainer = ServiceLocator.Current.GetInstance<IWindsorContainer>();
            this.startupConfigurators = windsorContainer.ResolveAll<IWebHostingConfigurator>().ToList();
        }

        /// <summary>
        /// The services configuration
        /// </summary>
        /// <param name="services">The list of services</param>
        [UsedImplicitly]
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IControllerFactory, ControllerFactory>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            foreach (var configurator in this.startupConfigurators)
            {
                configurator.ConfigureServices(services);
            }

            var builder = services.AddMvc();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                builder.AddApplicationPart(assembly);
            }

            builder.AddControllersAsServices();
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

        /// <summary>
        /// The controller factory
        /// </summary>
        [UsedImplicitly]
        private class ControllerFactory : IControllerFactory
        {
            /// <summary>
            /// The windsor container
            /// </summary>
            private readonly IWindsorContainer windsorContainer;

            /// <inheritdoc />
            public ControllerFactory()
            {
                this.windsorContainer = ServiceLocator.Current.GetInstance<IWindsorContainer>();
            }

            /// <inheritdoc />
            public object CreateController(ControllerContext context)
            {
                var controllerType = context.ActionDescriptor.ControllerTypeInfo.AsType();
                var controller = this.windsorContainer.Resolve(controllerType) as Controller;
                if (controller == null)
                {
                    return null;
                }

                controller.ControllerContext = context;
                return controller;
            }

            /// <inheritdoc />
            public void ReleaseController(ControllerContext context, object controller)
            {
                var disposable = controller as IDisposable;
                disposable?.Dispose();
            }
        }
    }
}