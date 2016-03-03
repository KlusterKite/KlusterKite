using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterKit.Web.Tests
{
    using System.Collections.Immutable;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Configuration;

    using Castle.Windsor;

    using ClusterKit.Core;
    using ClusterKit.Core.TestKit;
    using ClusterKit.Web.Swagger;
    using ClusterKit.Web.Swagger.Messages;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Bundle of tests for swagger descriptor
    /// </summary>
    public class SwaggerDescriptorTest : BaseActorTest<SwaggerDescriptorTest.Configurator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerDescriptorTest"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public SwaggerDescriptorTest(ITestOutputHelper output)
                    : base(output)
        {
        }

        /// <summary>
        /// Testing <seealso cref="SwaggerDescriptionActor"/>
        /// </summary>
        [Fact]
        public void SwaggerDescriptorActorTest()
        {
            this.Sys.StartNameSpaceActorsFromConfiguration();
            var descriptor = this.ActorOfAsTestActorRef<SwaggerDescriptionActor>("descriptor");

            var description = descriptor.Ask<SwaggerPublishDescription>(new SwaggerPublishDescriptionRequest(), TimeSpan.FromMilliseconds(200)).Result;
            Assert.Equal("/swagger/doc", description.DocUrl);
            Assert.Equal("/swagger/ui", description.Url);

            this.ExpectNoMsg();
            this.ExpectNoMsg();
            descriptor
                .Tell(
                    new ClusterEvent.MemberUp(
                        Member.Create(
                            Cluster.Get(this.Sys).SelfUniqueAddress,
                            MemberStatus.Up,
                            ImmutableHashSet.Create("Web.Swagger.Monitor"))));
            this.ExpectMsg<SwaggerPublishDescription>("/user/Web/Swagger/Monitor");
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
                        Web.Swagger.Publish {
                                publishDocPath = ""/swagger/doc""
                                publishUiPath = ""/swagger/ui""
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

                        /Web {
                            IsNameSpace = true
                        }
 		                 /Web/Swagger {
                            type = ""ClusterKit.Core.NameSpaceActor, ClusterKit.Core""
                            dispatcher = ClusterKit.test-dispatcher
                        }

                        /Web/Swagger/Monitor {
                            type = ""ClusterKit.Core.TestKit.TestActorForwarder, ClusterKit.Core.TestKit""
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