// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Installer.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Installing components from current library
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ClusterKit.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Akka.Actor;
    using Akka.Configuration;

    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ClusterKit.Core;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Installing components from current library
    /// </summary>
    public class Installer : BaseInstaller
    {
        /// <summary>
        /// Current windsor container
        /// </summary>
        private IWindsorContainer currentContainer;

        /// <summary>
        /// Gets priority for ordering akka configurations. Highest priority will override lower priority.
        /// </summary>
        /// <remarks>Consider using <seealso cref="BaseInstaller"/> integrated constants</remarks>
        protected override decimal AkkaConfigLoadPriority => PrioritySharedLib;

        /// <summary>
        /// Gets default akka configuration for current module
        /// </summary>
        /// <returns>Akka configuration</returns>
        protected override Config GetAkkaConfig() => ConfigurationFactory.ParseString(Configuration.AkkaConfig);

        /// <summary>
        /// Gets list of roles, that would be assign to cluster node with this plugin installed.
        /// </summary>
        /// <returns>The list of roles</returns>
        protected override IEnumerable<string> GetRoles() => new[]
                                                                 {
                                                                     "Web"
                                                                 };

        /// <summary>
        /// This method will be run after service start.
        /// Methods are run in <seealso cref="BaseInstaller.AkkaConfigLoadPriority"/> order.
        /// </summary>
        protected override void PostStart()
        {
            if (this.currentContainer == null)
            {
                throw new InvalidOperationException("There is no registered windsor container");
            }

            var registeredAssemblies =
                GetRegisteredBaseInstallers(this.currentContainer)
                    .Select(i => i.GetType().Assembly)
                    .Distinct();

            foreach (var registeredAssembly in registeredAssemblies)
            {
                this.currentContainer.Register(
                    Classes.FromAssembly(registeredAssembly).BasedOn<ApiController>().LifestyleScoped());
            }

            var system = this.currentContainer.Resolve<ActorSystem>();
            var bindUrl = GetWebHostingBindUrl(system.Settings.Config);

            var startupConfigurators = this.currentContainer.ResolveAll<IWebHostingConfigurator>().ToList();
            system.Log.Info("Starting web server on {Url}", bindUrl);
            try
            {
                var host = new WebHostBuilder()
                    .CaptureStartupErrors(true)
                    .UseUrls(bindUrl)
                    .UseKestrel();

                foreach (var configurator in startupConfigurators)
                {
                    host = configurator.ConfigureApp(host);
                }

                var server = host
                    .UseStartup<Startup>()
                    .Build();

                Task.Run(() =>
                {
                    try
                    {
                        server.Run();
                    }
                    catch (Exception exception)
                    {
                        system.Log.Error(exception, "Web server stopped");
                    }
                });
            }
            catch (Exception exception)
            {
                system.Log.Error(exception, "Could not start web server");
            }
        }

        /// <summary>
        /// Registering DI components
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="store">The configuration store.</param>
        protected override void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store)
        {
            this.currentContainer = container;
            container.Register(
                Classes.FromThisAssembly().Where(t => t.IsSubclassOf(typeof(ActorBase))).LifestyleTransient());

            container.Register(
                Component.For<IWebHostingConfigurator>().ImplementedBy<WebTracer>().LifestyleTransient());

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic))
            {
                container.Register(Classes.FromAssembly(assembly).Where(t => t.IsSubclassOf(typeof(Controller))).LifestyleTransient());
            }
        }

        /// <summary>
        /// Reads bind url from configuration
        /// </summary>
        /// <param name="config">The akka config</param>
        /// <returns>The Url to bind web hosting</returns>
        private static string GetWebHostingBindUrl(Config config)
        {
            return config.GetString("ClusterKit.Web.BindAddress", "http://*:80");
        }
    }
}