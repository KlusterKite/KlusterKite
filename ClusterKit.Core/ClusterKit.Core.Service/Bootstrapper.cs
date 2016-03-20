// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Bootstrapper.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Dependency injection configuration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.Service
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Akka.Actor;
    using Akka.Configuration;
    using Akka.Configuration.Hocon;
    using Akka.DI.CastleWindsor;
    using Akka.DI.Core;

    using Castle.Facilities.TypedFactory;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.Resolvers.SpecializedResolvers;
    using Castle.Windsor;

    using CommonServiceLocator.WindsorAdapter;

    using JetBrains.Annotations;

    using Microsoft.Practices.ServiceLocation;

    using Serilog;

    /// <summary>
    /// Dependency injection configuration
    /// </summary>
    [UsedImplicitly]
    public class Bootstrapper
    {
        /// <summary>
        ///  Dependency injection configuration
        /// </summary>
        /// <param name="container">Dependency injection container</param>
        /// <param name="args">
        /// Startup parameters
        /// </param>
        public static void Configure(IWindsorContainer container, string[] args)
        {
            Console.WriteLine(@"Starting bootstrapper");

            container.AddFacility<TypedFactoryFacility>();
            container.Kernel.Resolver.AddSubResolver(new ArrayResolver(container.Kernel, true));
            container.Register(Component.For<Controller>().LifestyleTransient());
            container.Register(Component.For<IWindsorContainer>().Instance(container));

            container.RegisterWindsorInstallers();

            Console.WriteLine(@"Preparing config");
            var config = BaseInstaller.GetStackedConfig(container, CreateTopLevelConfig(args));
            container.Register(Component.For<Config>().Instance(config));
            Console.WriteLine(@"Config created");

            // performing prestart checks
            BaseInstaller.RunPrecheck(container, config);

            // starting akka system
            Console.WriteLine(@"starting akka system");
            var actorSystem = ActorSystem.Create("ClusterKit", config);
            actorSystem.AddDependencyResolver(new WindsorDependencyResolver(container, actorSystem));

            container.Register(Component.For<ActorSystem>().Instance(actorSystem).LifestyleSingleton());
            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));
            Console.WriteLine(@"Bootstrapper start finished");
        }

        /// <summary>
        /// Creates configuration file from environment variables, start-up parameters and local files
        /// </summary>
        /// <param name="args">Start up parameters</param>
        /// <returns>Top level configuration</returns>
        /// <remarks>
        /// Load priority:
        ///     * List of files in start up parameters in order of parameters
        ///     * environment variables
        ///     * akka.hocon file in local application directory
        ///     * Akka config section in application configuration file
        /// </remarks>
        private static Config CreateTopLevelConfig(string[] args)
        {
            var config = ConfigurationFactory.Empty;
            if (args != null)
            {
                Console.WriteLine($"loading config from command line parameters");
                config = args
                    .Where(File.Exists)
                    .Select(File.ReadAllText)
                    .Aggregate(config, (current, configText) => current.WithFallback(ConfigurationFactory.ParseString(configText)));
            }

            Console.WriteLine($"loading config from environment");
            var networkName = Environment.GetEnvironmentVariable("NETWORK_NAME");
            if (!string.IsNullOrEmpty(networkName))
            {
                config = config.WithFallback(ConfigurationFactory.ParseString($"{{ akka.remote.helios.tcp.hostname = \"{networkName.Replace("\"", "\\\"")}\" }}"));
            }

            var hoconPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "akka.hocon");
            Console.WriteLine($"loading akka.hocon ({hoconPath})");
            if (File.Exists(hoconPath))
            {
                Console.WriteLine($"reading file ({hoconPath})");
                var hoconConfig = File.ReadAllText(hoconPath);
                Console.WriteLine($"file loaded in memory");
                config = config.WithFallback(ConfigurationFactory.ParseString(hoconConfig));
            }
            else
            {
                Log.Warning("File {fileName} was not found", hoconPath);
            }

            Console.WriteLine($"loading aplication configuration");
            var section = ConfigurationManager.GetSection("akka") as AkkaConfigurationSection;
            if (section?.AkkaConfig != null)
            {
                config = config.WithFallback(section.AkkaConfig);
            }

            return config;
        }
    }
}