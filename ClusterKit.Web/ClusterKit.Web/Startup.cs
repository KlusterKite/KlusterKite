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
    using System.Web.Http;

    using Castle.Windsor;
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
            // Configure Web API for self-host.
            var config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();
            /*
            config.Routes.MapHttpRoute(
                name: "Error404",
                routeTemplate: "{*url}",
                defaults: new { controller = "Error", action = "Handle404" });
                */
            var dependencyResolver = new WindsorDependencyResolver(ServiceLocator.Current.GetInstance<IWindsorContainer>());
            config.DependencyResolver = dependencyResolver;

            appBuilder.UseWebApi(config);
        }
    }
}