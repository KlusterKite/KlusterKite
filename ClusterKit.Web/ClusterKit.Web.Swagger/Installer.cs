// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Installer.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Installing components from current library
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Swagger
{
    using System.Collections.Generic;

    using Akka.Actor;
    using Akka.Configuration;

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
        protected override decimal AkkaConfigLoadPriority => PrioritySharedLib;

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
            var configSection = config.GetConfig("ClusterKit.Web.Swagger.Publish");
            if (configSection == null)
            {
                throw new ConfigurationException("ClusterKit.Web.Swagger.Publish is not defined");
            }

            if (string.IsNullOrEmpty(configSection.GetString("publishDocPath")))
            {
                throw new ConfigurationException("ClusterKit.Web.Swagger.Publish.publishDocPath is not defined");
            }

            if (string.IsNullOrEmpty(configSection.GetString("publishUiPath")))
            {
                throw new ConfigurationException("ClusterKit.Web.Swagger.Publish.publishUiPath is not defined");
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
        protected override IEnumerable<string> GetRoles() => new[]
                                                                 {
                                                                     "Web.Swagger.Publish"
                                                                 };

        /// <summary>
        /// Registering DI components
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="store">The configuration store.</param>
        protected override void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IWebHostingConfigurator>().ImplementedBy<OwinConfigurator>());
            container.Register(
                Classes.FromThisAssembly().Where(t => t.IsSubclassOf(typeof(ActorBase))).LifestyleTransient());
        }
    }
}