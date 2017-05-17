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
        /// The services configuration
        /// </summary>
        /// <param name="services">The list of services</param>
        [UsedImplicitly]
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IControllerFactory, ControllerFactory>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
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
            var configurators = ServiceLocator.Current.GetAllInstances<IWebHostingConfigurator>().ToList();


            // Configure Web API for self-host.
            /*
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes(new CustomDirectRouteProvider());
            config.Formatters.Clear();
            config.Formatters.Add(new XmlMediaTypeFormatter { UseXmlSerializer = true });
            config.Formatters.Add(new JsonMediaTypeFormatter());
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            */
            
            appBuilder.UseMvc();

            foreach (var configurator in configurators)
            {
                configurator.Configure(appBuilder);
            }
        }

        /// <summary>
        /// The controller factory
        /// </summary>
        private class ControllerFactory : IControllerFactory
        {
            /// <summary>
            /// The windsor container
            /// </summary>
            private IWindsorContainer windsorContainer;

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


        /*
        /// <summary>
        /// Workaround to handle inherited routes
        /// </summary>
        private class CustomDirectRouteProvider : DefaultDirectRouteProvider
        {
            /// <summary>
            /// Gets a set of route factories for the given action descriptor.
            /// </summary>
            /// <returns>
            /// A set of route factories.
            /// </returns>
            /// <param name="actionDescriptor">The action descriptor.</param>
            protected override IReadOnlyList<IDirectRouteFactory> GetActionRouteFactories(HttpActionDescriptor actionDescriptor)
            {
                // inherit route attributes decorated on base class controller's actions
                return actionDescriptor.GetCustomAttributes<IDirectRouteFactory>(inherit: true);
            }
        }
        */
    }
}