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
    using System.Linq;

    using Akka.Configuration;

    using Autofac;

    using JetBrains.Annotations;

    /// <summary>
    /// Base class to install ClusterKit plugin components
    /// </summary>
    [UsedImplicitly]
    public abstract class BaseInstaller
    {
        /// <summary>
        /// Predefined priority to load configuration for plugins, that handles node role functionality
        /// </summary>
        [UsedImplicitly]
        protected const decimal PriorityClusterRole = 100M;

        /// <summary>
        /// Predefined priority to load configuration for plugins, that handles other plugins functionality
        /// </summary>
        [UsedImplicitly]
        protected const decimal PrioritySharedLib = 10M;

        /// <summary>
        /// Predefined priority to load configuration for plugins, that handles unit tests
        /// </summary>
        [UsedImplicitly]
        protected const decimal PriorityTest = 100M;

        /// <summary>
        /// Every time <seealso cref="Install"/> called, installer register itself here
        /// </summary>
        private static readonly Dictionary<ContainerBuilder, List<BaseInstaller>> RegisteredInstallers =
            new Dictionary<ContainerBuilder, List<BaseInstaller>>();

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
        public static IList<BaseInstaller> GetRegisteredBaseInstallers(ContainerBuilder container)
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
        /// <param name="config">
        /// Top level config
        /// </param>
        /// <returns>
        /// Akka and system configuration
        /// </returns>
        public static Config GetStackedConfig(ContainerBuilder container, Config config)
        {
            Serilog.Log.Information("ClusterKit starting plugin manager");

            List<BaseInstaller> list;
            if (!RegisteredInstallers.TryGetValue(container, out list))
            {
                return config;
            }

            var rolesList =
                list.SelectMany(i => i.GetRoles())
                    .Distinct()
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .Select(r => $"\"{r.Replace("\"", "\\\"")}\"")
                    .ToList();

            if (rolesList.Any())
            {
                config =
                    config.WithFallback(
                        ConfigurationFactory.ParseString($"akka.cluster.roles = [{string.Join(", ", rolesList)}]"));
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
        public static void RunPostStart(ContainerBuilder container)
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
        public static void RunPreCheck(ContainerBuilder container, Config config)
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
        public void Install([NotNull] ContainerBuilder container)
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
            this.RegisterComponents(container);
        }

        /// <summary>
        /// Should check the config and environment for possible errors.
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
        protected abstract void RegisterComponents(ContainerBuilder container);
    }
}