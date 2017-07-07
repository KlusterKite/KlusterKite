// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Installer.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Installing components from current library
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager
{
    using System.Collections.Generic;
    using System.Reflection;

    using Akka.Actor;
    using Akka.Configuration;

    using Autofac;

    using JetBrains.Annotations;

    using KlusterKite.Core;

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
        // ReSharper disable once ArrangeStaticMemberQualifier
        protected override decimal AkkaConfigLoadPriority => BaseInstaller.PriorityClusterRole;

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
            var connectionString = config.GetString(NodeManagerActor.ConfigConnectionStringPath);
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ConfigurationException($"{NodeManagerActor.ConfigConnectionStringPath} is not defined");
            }

            var databaseName = config.GetString(NodeManagerActor.ConfigDatabaseNamePath);
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ConfigurationException($"{NodeManagerActor.ConfigDatabaseNamePath} is not defined");
            }

            var packageRepository = config.GetString(NodeManagerActor.PackageRepositoryUrlPath);
            if (string.IsNullOrEmpty(packageRepository))
            {
                throw new ConfigurationException($"{NodeManagerActor.ConfigDatabaseNamePath} is not defined");
            }
        }

        /// <summary>
        /// Gets default akka configuration for current module
        /// </summary>
        /// <returns>Akka configuration</returns>
        protected override Config GetAkkaConfig() => ConfigurationFactory.ParseString(ReadTextResource(typeof(Installer).GetTypeInfo().Assembly, "KlusterKite.NodeManager.Resources.akka.hocon"));

        /// <summary>
        /// Gets list of roles, that would be assign to cluster node with this plugin installed.
        /// </summary>
        /// <returns>The list of roles</returns>
        protected override IEnumerable<string> GetRoles() => new[]
                                                                 {
                                                                     "NodeManager"
                                                                 };

        /// <inheritdoc />
        protected override void RegisterComponents(ContainerBuilder container, Config config)
        {
            container.RegisterAssemblyTypes(typeof(Installer).GetTypeInfo().Assembly).Where(t => t.GetTypeInfo().IsSubclassOf(typeof(ActorBase)));

            // container.RegisterType<NugetPackagesFactory>().As<DataFactory<string, IPackage, string>>();
            container.RegisterType<ApiProvider>().As<API.Provider.ApiProvider>();
            
            // var nugetUrl = config.GetString("KlusterKite.NodeManager.PackageRepository");
            // container.Register(c => PackageRepositoryFactory.Default.CreateRepository(nugetUrl)).As<IPackageRepository>();
        }
    }
}