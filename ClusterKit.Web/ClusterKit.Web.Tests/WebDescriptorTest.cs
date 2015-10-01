// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebDescriptorTest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Tests the <seealso cref="WebDescriptorActor" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Tests
{
    using System;
    using System.Collections.Generic;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Configuration;
    using Akka.DI.Core;

    using Castle.Windsor;

    using ClusterKit.Core;
    using ClusterKit.Core.TestKit;
    using ClusterKit.Web.Client;
    using ClusterKit.Web.Client.Messages;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Tests the <seealso cref="WebDescriptorActor"/>
    /// </summary>
    public class WebDescriptorTest : BaseActorTest<WebDescriptorTest.Configurator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDescriptorTest"/> class.
        /// </summary>
        /// <param name="output">
        /// The Xunit output.
        /// </param>
        public WebDescriptorTest(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Test actor for correct <seealso cref="WebDescriptionResponse"/>
        /// </summary>
        [Fact]
        public void ResponseTest()
        {
            /*
            var testDescriptor =
                this.ActorOfAsTestActorRef<WebDescriptorActor>(this.Sys.DI().Props<WebDescriptorActor>());
            */
            this.Sys.StartNameSpaceActorsFromConfiguration();

            var response =
                this.Sys.GetWebDescriptor(Cluster.Get(this.Sys).SelfAddress)
                    .Ask<WebDescriptionResponse>(new WebDescriptionRequest(), TimeSpan.FromMilliseconds(500))
                    .Result;

            Assert.NotNull(response.ServiceNames);
            Assert.True(response.ServiceNames.ContainsKey("firstServiceRoot"));
            Assert.True(response.ServiceNames.ContainsKey("secondServiceRoot/secondServiceBranch"));
            Assert.True(response.ServiceNames.ContainsKey("thirdServiceRoot"));
            Assert.Equal(3, response.ServiceNames.Count);
            Assert.Equal("defaultHost", response.ServiceNames["firstServiceRoot"]);
            Assert.Equal("defaultHost", response.ServiceNames["secondServiceRoot/secondServiceBranch"]);
            Assert.Equal("otherHost", response.ServiceNames["thirdServiceRoot"]);
            Assert.Equal(8085, response.ListeningPort);
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
 			                OwinBindAddress = ""http://*:8085""
                            Services {
                               firstServiceRoot = defaultHost
                               secondServiceRoot/secondServiceBranch = defaultHost
                               thirdServiceRoot = otherHost
                            }
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
                installers.Add(new Web.Installer());
                return installers;
            }
        }
    }
}