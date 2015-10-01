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
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Formatting;
    using System.Web.Http;

    using Akka.Util.Internal;

    using Castle.Windsor;

    using ClusterKit.Web.Client;

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
        public void Configuration(IAppBuilder appBuilder)
        {
            var owinStartupConfigurators = ServiceLocator.Current.GetAllInstances<IOwinStartupConfigurator>().ToList();

            // Configure Web API for self-host.
            var config = new HttpConfiguration();
            config.Formatters.Clear();
            config.Formatters.Add(new XmlMediaTypeFormatter { UseXmlSerializer = true });
            config.Formatters.Add(new JsonMediaTypeFormatter());

            config.MapHttpAttributeRoutes();

            owinStartupConfigurators.ForEach(c => c.ConfigureApi(config));
            var dependencyResolver = new WindsorDependencyResolver(ServiceLocator.Current.GetInstance<IWindsorContainer>());
            config.DependencyResolver = dependencyResolver;
            appBuilder.UseWebApi(config);

            owinStartupConfigurators.ForEach(c => c.ConfigureApp(appBuilder));
        }
    }
}