// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Service main entry point
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Seeder
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Akka.Configuration;

    using Castle.Facilities.TypedFactory;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.Resolvers.SpecializedResolvers;
    using Castle.Windsor;

    using ClusterKit.Core;
    using ClusterKit.NodeManager.Migrator;

    using CommonServiceLocator.WindsorAdapter;

    using JetBrains.Annotations;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// Service main entry point
    /// </summary>
    [UsedImplicitly]
    public class Program
    {
        /// <summary>
        /// Service main entry point
        /// </summary>
        /// <param name="args">
        /// Startup parameters
        /// </param>
        public static void Main(string[] args)
        {
            Config config;
            Config seederConfig;
            if (!ReadConfig(args, out config, out seederConfig))
            {
                return;
            }

            var container = InitializeDependencies(config);

            var seederTypes = GetSeederTypes(seederConfig);
            if (seederTypes == null)
            {
                return;
            }

            foreach (var seederType in seederTypes)
            {
                container.Register(Component.For(seederType));
                var seeder = (BaseSeeder)container.Resolve(seederType);
                Console.WriteLine($@"Running {seederType.FullName}");
                seeder.Seed();
            }
        }

        /// <summary>
        /// Gets the seeder types
        /// </summary>
        /// <param name="seederConfig">The seeder configuration</param>
        /// <returns>The list of seeder types</returns>
        private static List<Type> GetSeederTypes(Config seederConfig)
        {
            var seeders = seederConfig.GetStringList("Seeders");
            var types = seeders.Select(t => new { Name = t, Type = Type.GetType(t, false) }).ToList();
            var lostTypes = types.Where(t => t.Type == null).ToList();
            foreach (var lostType in lostTypes)
            {
                Console.WriteLine($@"{lostType.Name} was not found");
            }

            if (lostTypes.Any())
            {
                return null;
            }

            var invalidTypes = types.Where(t => !t.Type.IsSubclassOf(typeof(BaseSeeder))).ToList();
            foreach (var type in invalidTypes)
            {
                Console.WriteLine($@"{type.Name} is not a BaseSeeder");
            }

            if (invalidTypes.Any())
            {
                return null;
            }

            var seederTypes = types.Select(t => t.Type).ToList();
            return seederTypes;
        }

        /// <summary>
        /// Performs the dependencies initialization
        /// </summary>
        /// <param name="config">The seeder configuration</param>
        /// <returns>The windsor container</returns>
        private static WindsorContainer InitializeDependencies(Config config)
        {
            var container = new WindsorContainer();
            container.AddFacility<TypedFactoryFacility>();
            container.Kernel.Resolver.AddSubResolver(new ArrayResolver(container.Kernel, true));
            container.Register(Component.For<IWindsorContainer>().Instance(container));
            container.RegisterWindsorInstallers();
            config = BaseInstaller.GetStackedConfig(container, config);
            container.Register(Component.For<Config>().Instance(config));
            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));
            return container;
        }

        /// <summary>
        /// Parses the configuration
        /// </summary>
        /// <param name="args">The command line arguments</param>
        /// <param name="config">The overall config</param>
        /// <param name="seederConfig">The current seeder config</param>
        /// <returns>The success of the operation</returns>
        private static bool ReadConfig(string[] args, out Config config, out Config seederConfig)
        {
            config = null;
            seederConfig = null;

            if (!File.Exists("seeder.hocon"))
            {
                Console.WriteLine(@"Could not find the config");
                return false;
            }

            config = ConfigurationFactory.ParseString(File.ReadAllText("seeder.hocon"));

            if (args == null || args.Length != 1)
            {
                Console.WriteLine(@"Seeder configuration is undefined");
                return false;
            }

            seederConfig = config.GetConfig(args[0]);
            if (seederConfig == null)
            {
                Console.WriteLine(@"Specified seeder configuration was not found in config file");
                return false;
            }

            return true;
        }
    }
}
