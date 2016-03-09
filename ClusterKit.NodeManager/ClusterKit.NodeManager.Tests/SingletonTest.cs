// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SingletonTest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing akka singltones
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Tests
{
    using System;
    using System.Collections.Generic;

    using Akka.Actor;
    using Akka.Cluster.Tools.Singleton;
    using Akka.Configuration;
    using Akka.DI.Core;

    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ClusterKit.Core;
    using ClusterKit.Core.Ping;
    using ClusterKit.Core.TestKit;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Testing akka singletons
    /// </summary>
    public class SingletonTest : BaseActorTest<SingletonTest.Configurator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonTest"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public SingletonTest(ITestOutputHelper output)
                    : base(output)
        {
        }

        [Fact]
        public void SimpleTest()
        {
            var actor = this.Sys.ActorOf(this.Sys.DI().Props<ConfigurationDbWorker>(), "cdb");
            /*
            var singleTon =
                this.Sys.ActorOf(
                    ClusterSingletonManager.Props(
                        this.Sys.DI().Props<ConfigurationDbWorker>(),
                        new ClusterSingletonManagerSettings(
                            "nodemanager.dbworker",
                            "NodeManager",
                            TimeSpan.FromSeconds(1),
                            TimeSpan.FromSeconds(1))), "nodemanager.dbworker");

            var proxy =
                this.Sys.ActorOf(
                    ClusterSingletonProxy.Props("/user/nodemanager.dbworker",
                    new ClusterSingletonProxySettings("nodemanager.dbworker", "NodeManager", TimeSpan.FromSeconds(1), 1024)),
                    "nodemanager.dbworker.proxy");
*/
            var response = actor.Ask<PongMessage>(new PingMessage(), TimeSpan.FromMilliseconds(500)).Result;
            Assert.NotNull(response);
        }

        /// <summary>
        /// The current test configuration
        /// </summary>
        public class Configurator : TestConfigurator
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
            public override Config GetAkkaConfig(IWindsorContainer windsorContainer)
            {
                return ConfigurationFactory.ParseString(@"{
                  ClusterKit {
                    NodeManager {
                        ConfigurationDatabaseConnectionString = ""User ID=postgres;Host=192.168.99.100;Port=5432;Pooling=true""
                    }
                  }

                  akka {
                    remote {
                      helios {
                        tcp {
                          hostname = 127.0.0.1
                          port = 3091
                        }
                      }
                    }

                        cluster {
                      seed-nodes = [""akka.tcp://test@127.0.0.1:3091""]
                      min-nr-of-members = 1
                    }       }
    }").WithFallback(base.GetAkkaConfig(windsorContainer));
            }

            public override List<BaseInstaller> GetPluginInstallers()
            {
                var list = base.GetPluginInstallers();
                list.Add(new Core.EF.Installer());
                list.Add(new Core.EF.Npgsql.Installer());
                list.Add(new NodeManager.Installer());
                //list.Add(new Web.Installer());
                list.Add(new Installer());
                return list;
            }
        }

        public class Installer : BaseInstaller
        {
            protected override decimal AkkaConfigLoadPriority => PriorityClusterRole;

            protected override Config GetAkkaConfig()
            {
                return ConfigurationFactory.Empty;
            }

            protected override IEnumerable<string> GetRoles() => new[] { "NodeManager" };

            protected override void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store)
            {
            }
        }
    }
}