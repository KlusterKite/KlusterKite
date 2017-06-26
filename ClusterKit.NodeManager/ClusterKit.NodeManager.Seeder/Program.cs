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
    using System.Reflection;

    using Akka.Configuration;

    using Autofac;

    using ClusterKit.Core;
    using ClusterKit.NodeManager.Migrator;

    using JetBrains.Annotations;

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

            var builder = InitializeDependencies(config);

            var seederTypes = GetSeederTypes(seederConfig);
            if (seederTypes == null)
            {
                return;
            }

            foreach (var seederType in seederTypes)
            {
                builder.RegisterType(seederType);
            }

            var container = builder.Build();

            foreach (var seederType in seederTypes)
            {
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

            var invalidTypes = types.Where(t => !t.Type.GetTypeInfo().IsSubclassOf(typeof(BaseSeeder))).ToList();
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
        private static ContainerBuilder InitializeDependencies(Config config)
        {
            var container = new ContainerBuilder();
            container.RegisterInstallers();
            config = BaseInstaller.GetStackedConfig(container, config);
            container.RegisterInstance(config).As<Config>();
            BaseInstaller.RunComponentRegistration(container, config);
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
