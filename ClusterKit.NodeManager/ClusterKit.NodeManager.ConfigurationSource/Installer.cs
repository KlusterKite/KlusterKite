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
    using Akka.Configuration;

    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ClusterKit.Core;
    using ClusterKit.Data;

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
            container.Register(Component.For<DataFactory<ConfigurationContext, NodeTemplate, int>>()
                .ImplementedBy<NodeTemplateFactory>().LifestyleTransient());
            container.Register(Component.For<DataFactory<ConfigurationContext, NugetFeed, int>>()
                .ImplementedBy<NugetFeedFactory>().LifestyleTransient());
            container.Register(Component.For<DataFactory<ConfigurationContext, SeedAddress, int>>()
                .ImplementedBy<SeedAddressFactorycs>().LifestyleTransient());

            container.Register(
                Component.For<IContextFactory<ConfigurationContext>>()
                    .ImplementedBy<ConfigurationContextFactory>()
                    .LifestyleTransient());
        }
    }
}