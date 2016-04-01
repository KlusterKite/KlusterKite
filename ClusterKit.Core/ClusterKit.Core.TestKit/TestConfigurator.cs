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
            var config = ConfigurationFactory.ParseString(
                @"
                akka.actor.serialize-messages = on
                akka.actor.serialize-creators = on
                akka.actor.serializers.wire = ""Akka.Serialization.WireSerializer, Akka.Serialization.Wire""
                akka.actor.serialization-bindings {
                  ""System.Object"" = wire
                }

                ClusterKit : {
                    test-dispatcher {
					    type : ""ClusterKit.Core.TestKit.CallingThreadDispatcherConfigurator, ClusterKit.Core.TestKit""
                        throughput: 100
                        throughput - deadline - time : 0ms
                    }
                }

                    akka.actor.deployment {
                        ""/*"" {
                           dispatcher = ClusterKit.test-dispatcher
                        }
                        ""/*/*"" {
                           dispatcher = ClusterKit.test-dispatcher
                        }
                        ""/*/*/*"" {
                           dispatcher = ClusterKit.test-dispatcher
                        }
                }");

            return BaseInstaller.GetStackedConfig(windsorContainer, config);
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