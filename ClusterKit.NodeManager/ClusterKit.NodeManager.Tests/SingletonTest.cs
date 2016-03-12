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
    using System.Threading;

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
            var proxy =
                this.Sys.ActorOf(
                    ClusterSingletonProxy.Props(
                        singletonManagerPath: "/user/singleton",
                        settings:
                            new ClusterSingletonProxySettings("dbWorker", "TimeSpan", TimeSpan.FromMilliseconds(200), 2048)),
                    name: "proxy");

            var singleTon =
                this.Sys.ActorOf(
                    ClusterSingletonManager.Props(
                        this.Sys.DI().Props(typeof(NodeManagerActor)),
                        new ClusterSingletonManagerSettings(
                            "dbWorker",
                            "NodeManager",
                            TimeSpan.FromMilliseconds(500),
                            TimeSpan.FromSeconds(1))),
                    "singleton");

            Thread.Sleep(TimeSpan.FromSeconds(10));
            this.Log.Warning("--------------------------------");

            singleTon.Tell(new PingMessage());
            try
            {
                singleTon.Ask<ActorIdentity>(new Identify(Guid.NewGuid()), TimeSpan.FromSeconds(1)).Wait();
            }
            catch (Exception e)
            {
                this.Log.Error(e, "Error on singleton manager ask");
            }

            try
            {
                this.Sys.ActorSelection("/user/singleton/dbWorker")
                    .Ask<PongMessage>(new PingMessage(), TimeSpan.FromSeconds(1))
                    .Wait();
            }
            catch (Exception e)
            {
                this.Log.Error(e, "Error on singleton ask");
            }

            Thread.Sleep(TimeSpan.FromSeconds(3));

            this.Log.Warning("Sending message to proxy");

            proxy.Ask<PongMessage>(new PingMessage(), TimeSpan.FromSeconds(5)).Wait();

            Thread.Sleep(TimeSpan.FromSeconds(3));

            proxy.Ask<PongMessage>(new PingMessage(), TimeSpan.FromSeconds(5)).Wait();
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
                 test-dispatcher {type : PinnedDispatcher
                    throughput: 100
                    throughput - deadline - time : 0ms
                }

            NodeManager {
                        ConfigurationDatabaseConnectionString = ""User ID=postgres;Host=192.168.99.100;Port=5432;Pooling=true""
                    }
                  }

                default-dispatcher {
                    type = PinnedDispatcher
                }

                  test : {
                  calling-thread-dispatcher : {
                    type : PinnedDispatcher
                    throughput: 2147483647
                  }
            test-actor : {
                    dispatcher : {
                      type : PinnedDispatcher
                      throughput : 2147483647
                    }
                    }
                }

                  akka {
                     actor {
                            deployment {
                            /singleton/dbWorker/workers {
                                    dispatcher = default-dispatcher
                                    router = consistent-hashing-pool
                                    nr-of-instances = 5
                            }

                            ""/singleton/*/*"" {
                                    dispatcher = default-dispatcher
                            }

                            ""/singleton/*"" {
                                    dispatcher = default-dispatcher
                            }

                            /singleton {
                                    dispatcher = default-dispatcher
                            }

                            /proxy {
                                    dispatcher = default-dispatcher
                            }
                        }
                    }

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
                      min-nr-of-members = 0
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