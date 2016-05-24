// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SwaggerMonitorTestscs.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Bundle of tests for swagger monitor
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Configuration;

    using Castle.Windsor;

    using ClusterKit.Core;
    using ClusterKit.Core.TestKit;
    using ClusterKit.Web.Swagger.Messages;
    using ClusterKit.Web.Swagger.Monitor;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Bundle of tests for swagger monitor
    /// </summary>
    public class SwaggerMonitorTests : BaseActorTest<SwaggerMonitorTests.Configurator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerMonitorTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public SwaggerMonitorTests(ITestOutputHelper output)
                    : base(output)
        {
        }

        /// <summary>
        /// Testing swagger collector
        /// </summary>
        [Fact]
        public void SwaggerCollectorTest()
        {
            this.Sys.StartNameSpaceActorsFromConfiguration();
            var collector = this.ActorOfAsTestActorRef<SwaggerCollectorActor>("collector");
            var response = collector.Ask<IReadOnlyCollection<string>>(new SwaggerCollectorActor.SwaggerListRequest(), TimeSpan.FromMilliseconds(200)).Result;
            Assert.NotNull(response);
            Assert.Equal(0, response.Count);

            collector.Tell(new SwaggerPublishDescription { Url = "test", DocUrl = "docTest" });
            response = collector.Ask<IReadOnlyCollection<string>>(new SwaggerCollectorActor.SwaggerListRequest(), TimeSpan.FromMilliseconds(200)).Result;
            Assert.NotNull(response);
            Assert.Equal(1, response.Count);
            Assert.Equal("test", response.First());

            collector.Tell(new SwaggerPublishDescription { Url = "test2", DocUrl = "docTest" });
            response = collector.Ask<IReadOnlyCollection<string>>(new SwaggerCollectorActor.SwaggerListRequest(), TimeSpan.FromMilliseconds(200)).Result;
            Assert.NotNull(response);
            Assert.Equal(1, response.Count);
            Assert.Equal("test2", response.First());

            this.ExpectNoMsg();
            this.ActorSelection("/user/collector/$b")
                .Tell(
                    new ClusterEvent.MemberUp(
                        Member.Create(
                            Cluster.Get(this.Sys).SelfUniqueAddress,
                            1,
                            MemberStatus.Up,
                            ImmutableHashSet.Create("Web.Swagger.Publish"))));
            this.ExpectMsg<SwaggerPublishDescriptionRequest>("/user/Web/Swagger/Descriptor");
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
                        }

                    akka.actor.deployment {
                     

                        /collector/workers {
                             router = round-robin-pool
                             nr-of-instances = 5
                        }

                        /Web {
                            IsNameSpace = true
                        }
 		                 /Web/Swagger {
                            type = ""ClusterKit.Core.NameSpaceActor, ClusterKit.Core""
                        }

                        /Web/Swagger/Descriptor {
                            type = ""ClusterKit.Core.TestKit.TestActorForwarder, ClusterKit.Core.TestKit""
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