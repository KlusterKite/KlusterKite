// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseInstaller.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Base class to install ClusterKit plugin components
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Akka.Configuration;
    using Akka.Configuration.Hocon;

    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using JetBrains.Annotations;

    using Serilog;

    /// <summary>
    /// Base class to install ClusterKit plugin components
    /// </summary>
    public abstract class BaseInstaller : IWindsorInstaller
    {
        /// <summary>
        /// Predefined priority to load configuration for plugins, that handles node role functionality
        /// </summary>
        protected const decimal PriorityClasterRole = 100M;

        /// <summary>
        /// Predefined priority to load configuration for plugins, that handles other plugins functionality
        /// </summary>
        protected const decimal PrioritySharedLib = 10M;

        /// <summary>
        /// Predefined priority to load configuration for plugins, that handles unit tests
        /// </summary>
        protected const decimal PriorityTest = 100M;

        /// <summary>
        /// Every time <seealso cref="Install"/> called, installer register itself here
        /// </summary>
        private static readonly Dictionary<IWindsorContainer, List<BaseInstaller>> RegisteredInstallers
            = new Dictionary<IWindsorContainer, List<BaseInstaller>>();

        /// <summary>
        /// Gets priority for ordering akka configurations. Highest priority will override lower priority.
        /// </summary>
        /// <remarks>Consider using <seealso cref="BaseInstaller"/> integrated constants</remarks>
        protected abstract decimal AkkaConfigLoadPriority { get; }

        /// <summary>
        /// Gets the list of all registered installers
        /// </summary>
        /// <param name="container">
        /// The windsor container.
        /// </param>
        /// <returns>
        /// the list of all registered installers
        /// </returns>
        public static IList<BaseInstaller> GetRegisteredBaseInstallers(IWindsorContainer container)
        {
            List<BaseInstaller> list;
            if (!RegisteredInstallers.TryGetValue(container, out list))
            {
                return new List<BaseInstaller>();
            }

            return list;
        }

        /// <summary>
        /// Generates overall akka config from all registered modules (with respect to external provided configuration file)
        /// </summary>
        /// <param name="container">
        /// The windsor container.
        /// </param>
        /// <returns>
        /// Akka and system configuration
        /// </returns>
        public static Config GetStackedConfig(IWindsorContainer container)
        {
            Log.Information("ClusterKit starting plugin manager");
            var section = ConfigurationManager.GetSection("akka") as AkkaConfigurationSection;
            var config = section != null ? section.AkkaConfig : ConfigurationFactory.Empty;

            var hoconPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "akka.hocon");
            if (File.Exists(hoconPath))
            {
                var hoconConfig = File.ReadAllText(hoconPath);
                config = ConfigurationFactory.ParseString(hoconConfig).WithFallback(config);
            }
            else
            {
                Log.Warning("File {fileName} was not found", hoconPath);
            }

            var networkName = Environment.GetEnvironmentVariable("NETWORK_NAME");
            if (!string.IsNullOrEmpty(networkName))
            {
                config = ConfigurationFactory.ParseString($"{{ akka.remote.helios.tcp.hostname = \"{networkName.Replace("\"", "\\\"")}\" }}").WithFallback(config);
            }

            List<BaseInstaller> list;
            if (!RegisteredInstallers.TryGetValue(container, out list))
            {
                return config;
            }

            var rolesList =
                list.SelectMany(i => i.GetRoles())
                    .Distinct()
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .Select(r => $"\"{ r.Replace("\"", "\\\"") }\"").ToList();

            if (rolesList.Any())
            {
                config = config.WithFallback(ConfigurationFactory.ParseString($"akka.cluster.roles = [{string.Join(", ", rolesList)}]"));
            }

            return list.OrderByDescending(i => i.AkkaConfigLoadPriority)
                .Aggregate(config, (current, installer) => current.WithFallback(installer.GetAkkaConfig()));
        }

        /// <summary>
        /// Runs all registered installers <seealso cref="PostStart"/>
        /// </summary>
        /// <param name="container">
        /// The windsor container.
        /// </param>
        public static void RunPostStart(IWindsorContainer container)
        {
            List<BaseInstaller> list;
            if (!RegisteredInstallers.TryGetValue(container, out list))
            {
                return;
            }

            foreach (var installer in list.OrderBy(l => l.AkkaConfigLoadPriority))
            {
                installer.PostStart();
            }
        }

        /// <summary>
        /// Runs all registered installers <seealso cref="PreCheck"/>
        /// </summary>
        /// <param name="container">
        /// The windsor container.
        /// </param>
        /// <param name="config">Full akka config</param>
        public static void RunPrecheck(IWindsorContainer container, Config config)
        {
            List<BaseInstaller> list;
            if (!RegisteredInstallers.TryGetValue(container, out list))
            {
                return;
            }

            foreach (var installer in list.OrderBy(l => l.AkkaConfigLoadPriority))
            {
                installer.PreCheck(config);
            }
        }

        /// <summary>
        /// Performs the installation in the <see cref="T:Castle.Windsor.IWindsorContainer"/>.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="store">The configuration store.</param>
        public void Install([NotNull] IWindsorContainer container, IConfigurationStore store)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (!RegisteredInstallers.ContainsKey(container))
            {
                RegisteredInstallers[container] = new List<BaseInstaller>();
            }

            if (RegisteredInstallers[container].Contains(this))
            {
                return;
            }

            RegisteredInstallers[container].Add(this);
            this.RegisterWindsorComponents(container, store);
        }

        /// <summary>
        /// Should check the config and environment for possible erorrs.
        /// If any found, shod throw the exception to prevent node from starting.
        /// </summary>
        /// <param name="config">Full akka config</param>
        /// <exception cref="Exception">
        /// Thrown if there are error in configuration and/or environment
        /// </exception>
        public virtual void PreCheck(Config config)
        {
        }

        /// <summary>
        /// Gets default akka configuration for current module
        /// </summary>
        /// <returns>Akka configuration</returns>
        protected abstract Config GetAkkaConfig();

        /// <summary>
        /// Gets list of roles, that would be assign to cluster node with this plugin installed.
        /// </summary>
        /// <returns>The list of roles</returns>
        protected virtual IEnumerable<string> GetRoles()
        {
            return new string[0];
        }

        /// <summary>
        /// This method will be run after service start.
        /// Methods are run in <seealso cref="AkkaConfigLoadPriority"/> order.
        /// </summary>
        protected virtual void PostStart()
        {
        }

        /// <summary>
        /// Registering DI components
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="store">The configuration store.</param>
        protected abstract void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store);
    }
}