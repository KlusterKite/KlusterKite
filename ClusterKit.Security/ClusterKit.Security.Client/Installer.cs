// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Installer.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Installing components from current library
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ClusterKit.Security.Client
{
    using System.Collections.Generic;

    using Akka.Configuration;

    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ClusterKit.Core;

    /// <summary>
    /// Installing components from current library
    /// </summary>
    public class Installer : BaseInstaller
    {
        /// <inheritdoc />
        protected override decimal AkkaConfigLoadPriority => PrioritySharedLib;

        /// <inheritdoc />
        public override void PreCheck(Config config)
        {
            Utils.CreatePrivilegesCache();
        }

        /// <inheritdoc />
        protected override Config GetAkkaConfig() => Config.Empty;

        /// <inheritdoc />
        protected override IEnumerable<string> GetRoles() => new string[0];

        /// <inheritdoc />
        protected override void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store)
        {
        }
    }
}