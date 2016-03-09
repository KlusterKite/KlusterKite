namespace ClusterKit.NodeManager
{
    using System.Collections.Generic;

    using Akka.Actor;
    using Akka.Configuration;

    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ClusterKit.Core;

    using JetBrains.Annotations;

    /// <summary>
    /// Installing components from current library
    /// </summary>
    [UsedImplicitly]
    public class Installer : BaseInstaller
    {
        /// <summary>
        /// Gets priority for ordering akka configurations. Highest priority will override lower priority.
        /// </summary>
        /// <remarks>Consider using <seealso cref="BaseInstaller"/> integrated constants</remarks>
        protected override decimal AkkaConfigLoadPriority => PriorityClusterRole;

        /// <summary>
        /// Should check the config and environment for possible erorrs.
        /// If any found, shod throw the exception to prevent node from starting.
        /// </summary>
        /// <param name="config">Full akka config</param>
        /// <exception cref="System.Exception">
        /// Thrown if there are error in configuration and/or environment
        /// </exception>
        public override void PreCheck(Config config)
        {
            var connectionString = config.GetString(ConfigurationDbWorker.ConfigConnectionStringPath);
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ConfigurationException($"{ConfigurationDbWorker.ConfigConnectionStringPath} is not defined");
            }

            var databaseName = config.GetString(ConfigurationDbWorker.ConfigDatabaseNamePath);
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ConfigurationException($"{ConfigurationDbWorker.ConfigDatabaseNamePath} is not defined");
            }
        }

        /// <summary>
        /// Gets default akka configuration for current module
        /// </summary>
        /// <returns>Akka configuration</returns>
        protected override Config GetAkkaConfig() => ConfigurationFactory.ParseString(Configuration.AkkaConfig);

        /// <summary>
        /// Gets list of roles, that would be assign to cluster node with this plugin installed.
        /// </summary>
        /// <returns>The list of roles</returns>
        protected override IEnumerable<string> GetRoles() => new[] { "NodeManager" };

        /// <summary>
        /// Registering DI components
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="store">The configuration store.</param>
        protected override void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Classes.FromThisAssembly().Where(t => t.IsSubclassOf(typeof(ActorBase))).LifestyleTransient());
        }
    }
}