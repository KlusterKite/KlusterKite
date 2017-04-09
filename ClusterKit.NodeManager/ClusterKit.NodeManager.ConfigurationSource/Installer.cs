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

    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

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

        /// <summary>
        /// Registering DI components
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="store">The configuration store.</param>
        protected override void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<DataFactory<ConfigurationContext, Role, Guid>>().ImplementedBy<RoleFactory>().LifestyleTransient());
            container.Register(Component.For<DataFactory<ConfigurationContext, User, string>>().ImplementedBy<UserFactoryByLogin>().LifestyleTransient());
            container.Register(Component.For<DataFactory<ConfigurationContext, User, Guid>>().ImplementedBy<UserFactoryByUid>().LifestyleTransient());
            container.Register(Component.For<DataFactory<ConfigurationContext, Release, int>>().ImplementedBy<ReleaseDataFactory>().LifestyleTransient());

            container.Register(
                Component.For<IContextFactory<ConfigurationContext>>()
                    .ImplementedBy<ConfigurationContextFactory>()
                    .LifestyleTransient());
        }
    }
}