﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Startup.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The OWIN startup class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Formatting;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Routing;

    using Autofac;
    using Autofac.Integration.WebApi;

    using JetBrains.Annotations;
    using Microsoft.Practices.ServiceLocation;
    using Owin;

    /// <summary>
    /// The OWIN startup class.
    /// </summary>
    [UsedImplicitly]
    public class Startup
    {
        /// <summary>
        /// The Owin service configuration
        /// </summary>
        /// <param name="appBuilder">The builder</param>
        [UsedImplicitly]
        public void Configuration(IAppBuilder appBuilder)
        {
            var owinStartupConfigurators = ServiceLocator.Current.GetAllInstances<IOwinStartupConfigurator>().ToList();

            // Configure Web API for self-host.
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes(new CustomDirectRouteProvider());
            config.Formatters.Clear();
            config.Formatters.Add(new XmlMediaTypeFormatter { UseXmlSerializer = true });
            config.Formatters.Add(new JsonMediaTypeFormatter());
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            var lifetimeScope = ServiceLocator.Current.GetInstance<ILifetimeScope>();
            appBuilder.UseAutofacMiddleware(lifetimeScope);

            owinStartupConfigurators.ForEach(c => c.ConfigureApi(config));
            config.DependencyResolver = new AutofacWebApiDependencyResolver(lifetimeScope);

            config.EnsureInitialized();

            owinStartupConfigurators.ForEach(c => c.ConfigureApp(appBuilder));
            appBuilder.UseWebApi(config);
        }

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
    }
}