// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Installer.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Installing components from current library
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource
{
    using System;

    using Akka.Configuration;

    using Autofac;

    using ClusterKit.Core;
    using ClusterKit.Data;
    using ClusterKit.NodeManager.Client.ORM;

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
        protected override decimal AkkaConfigLoadPriority => PrioritySharedLib;

        /// <summary>
        /// Gets default akka configuration for current module
        /// </summary>
        /// <returns>Akka configuration</returns>
        protected override Config GetAkkaConfig() => ConfigurationFactory.ParseString("{ClusterKit.NodeManager.ConfigurationSeederType = \"ClusterKit.NodeManager.ConfigurationSource.ConfigurationSeeder, ClusterKit.NodeManager.ConfigurationSource\"}");

        /// <inheritdoc />
        protected override void RegisterComponents(ContainerBuilder container, Config config)
        {
            container.RegisterType<RoleFactory>().As<DataFactory<ConfigurationContext, Role, Guid>>();
            container.RegisterType<UserFactoryByLogin>().As<DataFactory<ConfigurationContext, User, string>>();
            container.RegisterType<UserFactoryByUid>().As<DataFactory<ConfigurationContext, User, Guid>>();
            container.RegisterType<ReleaseDataFactory>().As<DataFactory<ConfigurationContext, Release, int>>();
            container.RegisterType<MigrationDataFactory>().As<DataFactory<ConfigurationContext, Migration, int>>();
        }
    }
}