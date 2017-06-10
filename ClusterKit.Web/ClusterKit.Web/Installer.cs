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
    using System.Reflection;
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
        protected override Config GetAkkaConfig() => ConfigurationFactory.ParseString(ReadTextResource(typeof(Installer).GetTypeInfo().Assembly, "ClusterKit.Web.Resources.akka.hocon"));

        /// <summary>
        /// Gets list of roles, that would be assign to cluster node with this plugin installed.
        /// </summary>
        /// <returns>The list of roles</returns>
        protected override IEnumerable<string> GetRoles() => new[]
                                                                 {
                                                                     "Web"
                                                                 };

        /// <inheritdoc />
        protected override void RegisterComponents(ContainerBuilder container, Config config)
        {
            container.RegisterAssemblyTypes(typeof(Installer).GetTypeInfo().Assembly).Where(t => t.GetTypeInfo().IsSubclassOf(typeof(ActorBase)));
            container.RegisterType<WebTracer>().As<IWebHostingConfigurator>();

            var registeredAssemblies =
                GetRegisteredBaseInstallers(container)
                    .Select(i => i.GetType().GetTypeInfo().Assembly)
                    .Distinct();

            foreach (var registeredAssembly in registeredAssemblies)
            {
                container.RegisterAssemblyTypes(registeredAssembly).Where(t => t.GetTypeInfo().IsSubclassOf(typeof(Controller)));
            }

            Startup.ContainerBuilder = container;
            Startup.Config = config;
            var bindUrl = GetWebHostingBindUrl(config);
            var host = new WebHostBuilder()
                .CaptureStartupErrors(true)
                .UseUrls(bindUrl)
                .UseKestrel();

            Task.Run(
                () =>
                    {
                        var server = host.UseStartup<Startup>().Build();
                        var system = Startup.ContainerWaiter.Task.Result.Resolve<ActorSystem>();
                        try
                        {
                            system.Log.Info("Starting web server...");
                            server.Run();
                        }
                        catch (Exception exception)
                        {
                            system.Log.Error(exception, "Web server stopped");
                        }
                    });
            Startup.ServiceConfigurationWaiter.Wait();
        }

        /// <inheritdoc />
        protected override void PostStart(IComponentContext context)
        {
            context.Resolve<ActorSystem>().Log.Info("{Type}: post start started", this.GetType().FullName);
            Startup.ContainerWaiter.SetResult(context);
            Startup.ServiceStartWaiter.Wait();
            context.Resolve<ActorSystem>().Log.Info("{Type}: post start completed", this.GetType().FullName);
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