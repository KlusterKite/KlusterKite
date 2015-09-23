// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationTest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace ClusterKit.Core.Tests.Configuration
{
    using System.Linq;

    using Akka.Configuration;

    using Castle.Windsor;

    using ClusterKit.Core.TestKit;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Global configuration test
    /// </summary>
    public class ConfigurationTest : BaseActorTest<ConfigurationTest.Configurator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationTest"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public ConfigurationTest(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Testing that hocon config stacks as expected
        /// </summary>
        [Fact]
        public void CheckConfigStacking()
        {
            var config1 = @"{
                akka.actor.deployment {
                    /testRoot1 {
                        autoDeploy = on
                    }
                 }
            }";

            var config2 = @"{
                akka.actor.deployment {
                    /testRoot2 {
                        autoDeploy = on
                    }
                 }
            }";

            var endConfig = ConfigurationFactory.Empty
                .WithFallback(config1)
                .WithFallback(config2);

            var deployConfig = endConfig.GetConfig("akka.actor.deployment");
            Assert.Equal(2, deployConfig.AsEnumerable().Count());
            Assert.True(endConfig.GetBoolean("akka.actor.deployment./testRoot1.autoDeploy"));
            Assert.True(endConfig.GetBoolean("akka.actor.deployment./testRoot2.autoDeploy"));
        }

        /// <summary>
        /// Testing correct <seealso cref="NameSpaceActor"/> work
        /// </summary>
        [Fact]
        public void NameSpaceActorTest()
        {
            this.Sys.StartNameSpaceActorsFromConfiguration();
            this.Sys.ActorSelection("/user/testNameSpace/forwarder").Tell("Hello world");
            Assert.Equal("Hello world", this.ExpectMsg<string>("/user/testNameSpace/forwarder"));
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
                akka.actor.deployment {
                    /testNameSpace {
                        IsNameSpace = true
                        dispatcher = ClusterKit.test-dispatcher
                    }

                    /somethingElse {
                    }

                    /testNameSpace/forwarder {
                        type = ""ClusterKit.Core.TestKit.TestActorForwarder, ClusterKit.Core.TestKit""
                        dispatcher = ClusterKit.test-dispatcher
                    }
                 }
            }").WithFallback(base.GetAkkaConfig(windsorContainer));
            }
        }
    }
}