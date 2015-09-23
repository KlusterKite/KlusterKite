// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestConfigurator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Configures base data for tests. Such as Akka config and list of used WindsorInstallers
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.TestKit
{
    using System.Collections.Generic;

    using Akka.Configuration;

    using Castle.Windsor;

    /// <summary>
    /// Configures base data for tests. Such as Akka config and list of used WindsorInstallers
    /// </summary>
    public class TestConfigurator
    {
        /// <summary>
        /// Gets the akka system config
        /// </summary>
        /// <param name="windsorContainer">
        /// The windsor Container.
        /// </param>
        /// <returns>
        /// The config
        /// </returns>
        public virtual Config GetAkkaConfig(IWindsorContainer windsorContainer)
        {
            return BaseInstaller.GetStackedConfig(windsorContainer);
        }

        /// <summary>
        /// Gets list of all used plugin installers
        /// </summary>
        /// <returns>The list of installers</returns>
        public virtual List<BaseInstaller> GetPluginInstallers()
        {
            return new List<BaseInstaller> { new Core.Installer(), new Installer() };
        }
    }
}