namespace ClusterKit.Web.NginxConfigurator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;

    using Akka.Actor;
    using Akka.Configuration;

    using Castle.Core.Internal;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ClusterKit.Core;

    /// <summary>
    /// Installing components from current library
    /// </summary>
    public class Installer : BaseInstaller
    {
        /// <summary>
        /// Gets priority for ordering akka configurations. Highest priority will override lower priority.
        /// </summary>
        /// <remarks>Consider using <seealso cref="BaseInstaller"/> integrated constants</remarks>
        protected override decimal AkkaConfigLoadPriority => BaseInstaller.PriorityClasterRole;

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
            var configPath = config.GetString("ClusterKit.Web.Nginx.PathToConfig");
            if (string.IsNullOrWhiteSpace(configPath))
            {
                throw new ConfigurationException("ClusterKit.Web.Nginx.PathToConfig is not defined");
            }

            if (!Path.IsPathRooted(configPath))
            {
                var currentExecutablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrWhiteSpace(currentExecutablePath))
                {
                    throw new ConfigurationException("Failed to determine current executable path");
                }

                configPath = Path.Combine(currentExecutablePath, configPath);
            }

            var configDirectory = Path.GetDirectoryName(configPath);
            if (string.IsNullOrWhiteSpace(configDirectory))
            {
                throw new ConfigurationException("ClusterKit.Web.Nginx.PathToConfig has no defined directory");
            }

            if (!Directory.Exists(configDirectory))
            {
                throw new ConfigurationException($"{configDirectory} does not exists");
            }

            var path = File.Exists(configPath) ? configPath : configDirectory;
            var permission = new FileIOPermission(FileIOPermissionAccess.Write, path);
            if (!permission.IsGranted())
            {
                throw new ConfigurationException($"Cannot access {path} for writing");
            }

            if (config.GetConfig("ClusterKit.Web.Nginx.ServicesHost") == null)
            {
                throw new ConfigurationException("ClusterKit.Web.Nginx.ServiceHost is not defined");
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
        protected override IEnumerable<string> GetRoles()
        {
            return new[] { "Web.Nginx" };
        }

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