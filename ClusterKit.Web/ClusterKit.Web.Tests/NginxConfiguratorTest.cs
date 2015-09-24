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
    using System.Collections.Generic;
    using System.Collections.Immutable;
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
            var configurator = this.ActorOfAsTestActorRef<NginxConfiguratorActor>("configurator");
            var webNamespace =
                this.ActorOfAsTestActorRef<NginxConfiguratorActor>(this.Sys.DI().Props<TestActorForwarder>(), "Web");
            var webDescriptor =
                webNamespace.Ask<IActorRef>(
                    new TestActorForwarder.CreateChildMessage()
                    {
                        Props = this.Sys.DI().Props<TestActorForwarder>(),
                        Name = "Descriptor"
                    }).Result;

            var address = Cluster.Get(this.Sys).SelfAddress;
            configurator.Tell(
                new ClusterEvent.MemberUp(
                    Member.Create(new UniqueAddress(address, 1), MemberStatus.Up, ImmutableHashSet.Create("Web"))));
            this.ExpectTestMsg<WebDescriptionRequest>();

            Assert.Equal(1, configurator.UnderlyingActor.KnownActiveNodes.Count);

            configurator.Tell(
                new WebDescriptionResponse
                {
                    ListeningPort = 8080,
                    ServiceNames = new Dictionary<string, string>
                                           {
                                               { "/TestWebService", "default" },
                                               { "/test/TestWebService2", "default" },
                                               { "/", "web" }
                                           }
                },
                webDescriptor);

            Assert.Equal(1, configurator.UnderlyingActor.NodePublishUrls.Count);
            Assert.Equal("127.0.0.1:8080", configurator.UnderlyingActor.NodePublishUrls.First().Value);
            Assert.Equal("127.0.0.1:8080", configurator.UnderlyingActor.ConfigDictionary["default"]["/TestWebService"][0]);
            Assert.Equal("127.0.0.1:8080", configurator.UnderlyingActor.ConfigDictionary["default"]["/test/TestWebService2"][0]);
            Assert.Equal("127.0.0.1:8080", configurator.UnderlyingActor.ConfigDictionary["web"]["/"][0]);
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
                                }
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