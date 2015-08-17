// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseInstaller.cs" company="TaxiKit">
//   All rights reserved
// </copyright>
// <summary>
//   Base class to install TaxiKit plugin components
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TaxiKit.Core
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;

    using Akka.Configuration;
    using Akka.Configuration.Hocon;

    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    /// <summary>
    /// Base class to install TaxiKit plugin components
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
            var section = ConfigurationManager.GetSection("akka") as AkkaConfigurationSection;
            var config = section != null ? section.AkkaConfig : ConfigurationFactory.Empty;

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
        /// Performs the installation in the <see cref="T:Castle.Windsor.IWindsorContainer"/>.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="store">The configuration store.</param>
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
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
        /// Gets default akka configuration for current module
        /// </summary>
        /// <returns>Akka configuration</returns>
        protected abstract Config GetAkkaConfig();

        /// <summary>
        /// Gets list of roles, that would be assign to cluster node with this plugin installed.
        /// </summary>
        /// <returns>The list of roles</returns>
        protected IEnumerable<string> GetRoles()
        {
            return new string[0];
        }

        /// <summary>
        /// Registering DI components
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="store">The configuration store.</param>
        protected abstract void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store);
    }
}