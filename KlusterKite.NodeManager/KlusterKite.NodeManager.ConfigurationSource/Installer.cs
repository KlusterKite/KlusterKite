// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Installer.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Installing components from current library
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.ConfigurationSource
{
    using System;

    using Akka.Configuration;

    using Autofac;

    using JetBrains.Annotations;

    using KlusterKite.Core;
    using KlusterKite.Data;
    using KlusterKite.NodeManager.Client.ORM;
    using KlusterKite.NodeManager.Launcher.Utils;

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
        protected override Config GetAkkaConfig() => ConfigurationFactory.ParseString("{KlusterKite.NodeManager.ConfigurationSeederType = \"KlusterKite.NodeManager.ConfigurationSource.ConfigurationSeeder, KlusterKite.NodeManager.ConfigurationSource\"}");

        /// <inheritdoc />
        protected override void RegisterComponents(ContainerBuilder container, Config config)
        {
            container.RegisterType<RoleFactory>().As<DataFactory<ConfigurationContext, Role, Guid>>();
            container.RegisterType<UserFactoryByLogin>().As<DataFactory<ConfigurationContext, User, string>>();
            container.RegisterType<UserFactoryByUid>().As<DataFactory<ConfigurationContext, User, Guid>>();
            container.RegisterType<ConfigurationDataFactory>().As<DataFactory<ConfigurationContext, Configuration, int>>();
            container.RegisterType<MigrationDataFactory>().As<DataFactory<ConfigurationContext, Migration, int>>();

            var nugetUrl = config.GetString("KlusterKite.NodeManager.PackageRepository");
            if (config.GetBoolean("KlusterKite.NodeManager.RegisterNuget", true)
                && !string.IsNullOrWhiteSpace(nugetUrl))
            {
                container.RegisterInstance(new RemotePackageRepository(nugetUrl)).As<IPackageRepository>();
            }
        }
    }
}