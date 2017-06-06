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
    
    using Akka.Actor;
    using Akka.Configuration;

    using Autofac;

    using ClusterKit.Core;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Installing components from current library
    /// </summary>
    public class Installer : BaseInstaller
    {
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

        /// <inheritdoc />
        protected override void RegisterComponents(ContainerBuilder container)
        {
            container.RegisterAssemblyTypes(typeof(Installer).Assembly).Where(t => t.IsSubclassOf(typeof(ActorBase)));
            container.RegisterType<WebTracer>().As<IWebHostingConfigurator>();

            var registeredAssemblies =
                GetRegisteredBaseInstallers(container)
                    .Select(i => i.GetType().Assembly)
                    .Distinct();

            foreach (var registeredAssembly in registeredAssemblies)
            {
                container.RegisterAssemblyTypes(registeredAssembly).Where(t => t.IsSubclassOf(typeof(Controller)));
            }
        }

        /// <inheritdoc />
        protected override void PostStart(IComponentContext context)
        {
            var system = context.Resolve<ActorSystem>();
            var bindUrl = GetWebHostingBindUrl(system.Settings.Config);
            system.Log.Info("Starting web server on {Url}", bindUrl);
            try
            {
                var host = new WebHostBuilder()
                    .CaptureStartupErrors(true)
                    .UseUrls(bindUrl)
                    .UseKestrel();

                var server = host.UseStartup<Startup>().Build();
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