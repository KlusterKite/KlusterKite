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
    using System.Runtime.Remoting;

    using Akka.Actor;
    using Akka.Configuration;
    using Akka.DI.Core;

    using ClusterKit.Core.TestKit;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Tests the <seealso cref="WebDescriptorActor"/>
    /// </summary>
    public class WebDescriptorTest : BaseActorTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDescriptorTest"/> class.
        /// </summary>
        /// <param name="output">
        /// The Xunit output.
        /// </param>
        public WebDescriptorTest(ITestOutputHelper output)
                    : base(output, GetConfig())
        {
        }

        /// <summary>
        /// Test actor for correct <seealso cref="WebDescriptorActor.WebDescriptionResponse"/>
        /// </summary>
        [Fact]
        public void ResponseTest()
        {
            var testDescriptor =
                this.ActorOfAsTestActorRef<WebDescriptorActor>(this.Sys.DI().Props<WebDescriptorActor>());

            var response =
                testDescriptor.Ask<WebDescriptorActor.WebDescriptionResponse>(
                    new WebDescriptorActor.WebDescriptionRequest(),
                    TimeSpan.FromMilliseconds(500)).Result;

            Assert.NotNull(response.ServiceNames);
            Assert.True(response.ServiceNames.ContainsKey("firstServiceRoot"));
            Assert.True(response.ServiceNames.ContainsKey("secondServiceRoot/secondServiceBranch"));
            Assert.True(response.ServiceNames.ContainsKey("thirdServiceRoot"));
            Assert.Equal(3, response.ServiceNames.Count);
            Assert.Equal("defaultHost", response.ServiceNames["firstServiceRoot"]);
            Assert.Equal("defaultHost", response.ServiceNames["secondServiceRoot/secondServiceBranch"]);
            Assert.Equal("otherHost", response.ServiceNames["thirdServiceRoot"]);
        }

        /// <summary>
        /// Generates test akka configuration
        /// </summary>
        /// <returns>test akka configuration</returns>
        private static Config GetConfig()
        {
            return ConfigurationFactory.ParseString(@"{
                ClusterKit {
 		            Web {
 			            OwinBindAddress = ""Http://*:8080""
                        Services {
                           firstServiceRoot = defaultHost
                           secondServiceRoot/secondServiceBranch = defaultHost
                           thirdServiceRoot = otherHost
                        }
                    }
                }
            }");
        }
    }
}