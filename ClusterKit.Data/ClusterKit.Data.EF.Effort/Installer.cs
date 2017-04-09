// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Installer.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Installing components from current library
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.EF.Effort
{
    using System;

    using Akka.Configuration;

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
        /// <exception cref="Exception">
        /// Thrown if there are error in configuration and/or environment
        /// </exception>
        public override void PreCheck(Config config)
        {
        }

        /// <summary>
        /// Gets default akka configuration for current module
        /// </summary>
        /// <returns>Akka configuration</returns>
        protected override Config GetAkkaConfig() => ConfigurationFactory.Empty;

        /// <summary>
        /// Registering DI components
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="store">The configuration store.</param>
        protected override void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store)
        {
        }
    }
}