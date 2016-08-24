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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Akka.Actor;
    using Akka.Configuration;
    using Akka.Configuration.Hocon;
    using Akka.DI.CastleWindsor;
    using Akka.DI.Core;
    using Akka.Util.Internal;

    using Castle.Facilities.TypedFactory;
    using Castle.MicroKernel.Resolvers.SpecializedResolvers;
    using Castle.Windsor;

    using CommonServiceLocator.WindsorAdapter;

    using JetBrains.Annotations;

    using Microsoft.Practices.ServiceLocation;

    using Serilog;
    using Serilog.Events;

    using Component = Castle.MicroKernel.Registration.Component;

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

            Log.Debug($"Cluster configuration: seed-nodes { string.Join(", ", config.GetStringList("akka.cluster.seed-nodes") ?? new List<string>())}");
            Log.Debug($"Cluster configuration: min-nr-of-members { config.GetInt("akka.cluster.min-nr-of-members")}");
            Log.Debug($"Cluster configuration: roles { string.Join(", ", config.GetStringList("akka.cluster.roles") ?? new List<string>())}");
            Log.Debug($"Cluster node hostname: { config.GetString("akka.remote.helios.tcp.hostname") }");
            var publicHostName = config.GetString("akka.remote.helios.tcp.public-hostname");
            if (!string.IsNullOrWhiteSpace(publicHostName))
            {
                Log.Debug($"Cluster node public hostname: { publicHostName }");
            }

            container.Register(Component.For<Config>().Instance(config));
            Console.WriteLine(@"Config created");

            // log configuration
            LogEventLevel level;
            if (!Enum.TryParse(config.GetString("ClusterKit.Log.minimumLevel"), true, out level))
            {
                level = LogEventLevel.Information;
            }

            var loggerConfig = new LoggerConfiguration().MinimumLevel.Is(level);
            var configurators = container.ResolveAll<ILoggerConfigurator>();

            configurators.ForEach(c => Log.Information("Using log configurator {TypeName}", c.GetType().FullName));

            loggerConfig = configurators.Aggregate(
                loggerConfig,
                (current, loggerConfigurator) => loggerConfigurator.Configure(current, config));

            var logger = loggerConfig.CreateLogger();
            Log.Logger = logger;
            // log configuration finished


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
                Console.WriteLine(@"loading config from command line parameters");
                config = args
                    .Where(File.Exists)
                    .Select(File.ReadAllText)
                    .Aggregate(config, (current, configText) => current.WithFallback(ConfigurationFactory.ParseString(configText)));
            }

            Console.WriteLine(@"loading config from environment");
            var networkName = Environment.GetEnvironmentVariable("NETWORK_NAME");
            if (!string.IsNullOrEmpty(networkName))
            {
                config = config.WithFallback(ConfigurationFactory.ParseString($"{{ akka.remote.helios.tcp.public-hostname = \"{networkName.Replace("\"", "\\\"")}\" }}"));
            }

            // ReSharper disable once AssignNullToNotNullAttribute
            var hoconPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "akka.hocon");
            Console.WriteLine($@"loading akka.hocon ({hoconPath})");
            if (File.Exists(hoconPath))
            {
                Console.WriteLine($@"reading file ({hoconPath})");
                var hoconConfig = File.ReadAllText(hoconPath);
                Console.WriteLine(@"file loaded in memory");
                config = config.WithFallback(ConfigurationFactory.ParseString(hoconConfig));
            }
            else
            {
                Log.Warning("File {fileName} was not found", hoconPath);
            }

            Console.WriteLine(@"loading aplication configuration");
            var section = ConfigurationManager.GetSection("akka") as AkkaConfigurationSection;
            if (section?.AkkaConfig != null)
            {
                config = config.WithFallback(section.AkkaConfig);
            }

            return config;
        }
    }
}