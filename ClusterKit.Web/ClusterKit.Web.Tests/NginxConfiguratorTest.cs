// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NginxConfiguratorTest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing work of <seealso cref="NginxConfiguratorActor" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Configuration;
    using Akka.DI.Core;
    using Akka.TestKit;

    using Castle.Windsor;

    using ClusterKit.Core;
    using ClusterKit.Core.TestKit;
    using ClusterKit.Web.Client.Messages;
    using ClusterKit.Web.NginxConfigurator;
    using ClusterKit.Web.Swagger.Monitor;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Testing work of <seealso cref="NginxConfiguratorActor"/>
    /// </summary>
    public class NginxConfiguratorTest : BaseActorTest<NginxConfiguratorTest.Configurator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NginxConfiguratorTest"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public NginxConfiguratorTest(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Testing generation of nginx.config
        /// </summary>
        [Fact]
        public void ServiceConfigGenerationTest()
        {
            BaseInstaller.RunPreCheck(this.WindsorContainer, this.Sys.Settings.Config);
            var configurator = this.ActorOfAsTestActorRef<NginxConfiguratorActor>("configurator");

            this.ActorOfAsTestActorRef<NameSpaceActor>(this.Sys.DI().Props<NameSpaceActor>(), "Web");

            var address = Cluster.Get(this.Sys).SelfAddress;
            configurator.Tell(
                new ClusterEvent.MemberUp(
                    ClusterExtensions.MemberCreate(new UniqueAddress(address, 1), 1, MemberStatus.Up, ImmutableHashSet.Create("Web"))));
            this.ExpectTestMsg<WebDescriptionRequest>();

            Assert.Equal(1, configurator.UnderlyingActor.KnownActiveNodes.Count);

            configurator.Tell(
                new WebDescriptionResponse
                {
                    Services =
                            new List<ServiceDescription>
                                {
                                    new ServiceDescription
                                        {
                                            ListeningPort = 8080,
                                            PublicHostName = "web1",
                                            Route = "/TestWebService"
                                        },
                                    new ServiceDescription
                                        {
                                            ListeningPort = 8080,
                                            PublicHostName = "web1",
                                            Route = "/test/TestWebService2"
                                        },
                                    new ServiceDescription
                                        {
                                            ListeningPort = 8080,
                                            PublicHostName = "web2",
                                            Route = "/Api"
                                        }
                                }
                            .AsReadOnly()
                });

            Assert.Equal(1, configurator.UnderlyingActor.NodePublishUrls.Count);
            Assert.Equal(
                "0.0.0.0:8080",
                configurator.UnderlyingActor.Configuration["web1"]["/TestWebService"].ActiveNodes[0].NodeUrl);
            Assert.Equal(
                "0.0.0.0:8080",
                configurator.UnderlyingActor.Configuration["web1"]["/test/TestWebService2"].ActiveNodes[0].NodeUrl);
            Assert.Equal(
                "0.0.0.0:8080",
                configurator.UnderlyingActor.Configuration["web2"]["/Api"].ActiveNodes[0].NodeUrl);

            var config = File.ReadAllText("./nginx.conf");
            this.Sys.Log.Info(config);
        }

        /// <summary>
        /// The test configurator
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
                return ConfigurationFactory.ParseString(@"
                {
                    ClusterKit {
	 		                    Web {
	 			                    Nginx {
	 				                    PathToConfig = ""./nginx.conf""
                                        Configuration {
                                            web1 {
                                               listen: 8081
                                               ""location /api/1.x/external"" {
                                                    proxy_pass = ""http://external/api/1.x/external""
                                               }
                                            }
                                            web2 {
                                               listen: 8082
                                               server_name: ""www.example.com""
                                               ""location /"" {
                                                         root = /var/www/example/
                                                }
                                            }
                                           ""web.3"" {
                                             server_name: ""www.example2.com""
                                           }
                                        }
                                }
                            }
                        }

            akka.actor {
                        serializers {
                            hyperion = ""Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion""
                        }
                        serialization-bindings {
                            ""System.Object"" = hyperion
                        }
                    }

            akka.actor.deployment {
                    /Web {
                        IsNameSpace = true
                        dispatcher = akka.test.calling-thread-dispatcher
                    }

                    /Web/Descriptor {
                        type = ""ClusterKit.Core.TestKit.TestActorForwarder, ClusterKit.Core.TestKit""
                        dispatcher = akka.test.calling-thread-dispatcher
                    }
                }

                }").WithFallback(base.GetAkkaConfig(windsorContainer));
            }

            /// <summary>
            /// Gets list of all used plugin installers
            /// </summary>
            /// <returns>The list of installers</returns>
            public override List<BaseInstaller> GetPluginInstallers()
            {
                var installers = base.GetPluginInstallers();
                installers.Add(new NginxConfigurator.Installer());
                return installers;
            }
        }
    }
}