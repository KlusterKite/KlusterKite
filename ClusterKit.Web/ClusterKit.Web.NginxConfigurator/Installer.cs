// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Installer.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Installing components from current library
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.NginxConfigurator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;

    using Akka.Actor;
    using Akka.Configuration;

    using Autofac;

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
        protected override decimal AkkaConfigLoadPriority => PriorityClusterRole;

        /// <summary>
        /// Should check the config and environment for possible errors.
        /// If any found, shod throw the exception to prevent node from starting.
        /// </summary>
        /// <param name="config">Full akka config</param>
        /// <exception cref="System.Exception">
        /// Thrown if there are error in configuration and/or environment
        /// </exception>
        public override void PreCheck(Config config)
        {
            if (config.GetConfig("ClusterKit.Web.Nginx.Configuration") == null)
            {
                throw new ConfigurationException("ClusterKit.Web.Nginx.Configuration is not defined");
            }

            CheckNginxConfigAccess(config);
            CheckNginxReloadCommandAccess(config);
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

        /// <inheritdoc />
        protected override void RegisterComponents(ContainerBuilder container, Config config)
        {
            container.RegisterAssemblyTypes(typeof(Installer).Assembly).Where(t => t.IsSubclassOf(typeof(ActorBase)));
        }

        /// <summary>
        /// Checks access to file
        /// </summary>
        /// <param name="filePath">Path to file for check</param>
        /// <param name="permissionAccess">Permission to check</param>
        private static void CheckFileAccess(string filePath, FileIOPermissionAccess permissionAccess)
        {
            if (!Path.IsPathRooted(filePath))
            {
                var currentExecutablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrWhiteSpace(currentExecutablePath))
                {
                    throw new ConfigurationException("Failed to determine current executable path");
                }

                filePath = Path.Combine(currentExecutablePath, filePath);
            }

            var configDirectory = Path.GetDirectoryName(filePath);
            if (string.IsNullOrWhiteSpace(configDirectory))
            {
                throw new ConfigurationException("ClusterKit.Web.Nginx.PathToConfig has no defined directory");
            }

            if (!Directory.Exists(configDirectory))
            {
                throw new ConfigurationException($"{configDirectory} does not exists");
            }

            var path = File.Exists(filePath) ? filePath : configDirectory;
            var permission = new FileIOPermission(permissionAccess, path);
            var permissionSet = new PermissionSet(PermissionState.None);
            if (!permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet))
            {
                throw new ConfigurationException($"Cannot access {path} for writing");
            }
        }

        /// <summary>
        /// Checks that service has access to nginx config file
        /// </summary>
        /// <param name="config">Akka configuration</param>
        private static void CheckNginxConfigAccess(Config config)
        {
            var configPath = config.GetString("ClusterKit.Web.Nginx.PathToConfig");
            if (string.IsNullOrWhiteSpace(configPath))
            {
                throw new ConfigurationException("ClusterKit.Web.Nginx.PathToConfig is not defined");
            }

            CheckFileAccess(configPath, FileIOPermissionAccess.Write);
        }

        /// <summary>
        /// Checks that service has access to nginx config file
        /// </summary>
        /// <param name="config">Akka configuration</param>
        private static void CheckNginxReloadCommandAccess(Config config)
        {
            var reloadCommandConfig = config.GetConfig("ClusterKit.Web.Nginx.ReloadCommand");
            if (reloadCommandConfig != null)
            {
                var commandPath = reloadCommandConfig.GetString("Command");
                if (string.IsNullOrWhiteSpace(commandPath))
                {
                    throw new ConfigurationException("ClusterKit.Web.Nginx.ReloadCommand.Command is not defined");
                }

                // todo: actualy need to check Execute access
                CheckFileAccess(commandPath, FileIOPermissionAccess.Read);
            }
        }
    }
}