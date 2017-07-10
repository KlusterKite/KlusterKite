// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Installer.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Installing components from current library
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KlusterKite.Security.Client
{
    using System.Collections.Generic;

    using Akka.Configuration;

    using Autofac;

    using KlusterKite.Core;
    using KlusterKite.Security.Attributes;

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
        protected override void RegisterComponents(ContainerBuilder container, Config config)
        {
        }
    }
}