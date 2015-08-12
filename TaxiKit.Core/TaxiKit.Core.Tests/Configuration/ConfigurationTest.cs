// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationTest.cs" company="TaxiKit">
//   All rights reserved
// </copyright>
// <summary>
//   Global configuration test
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TaxiKit.Core.Tests.Configuration
{
    using System;
    using System.Linq;

    using Akka.Actor;
    using Akka.Configuration;

    using Castle.MicroKernel.Registration;

    using Serilog;

    using TaxiKit.Core.Guarantee;
    using TaxiKit.Core.TestKit;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Global configuration test
    /// </summary>
    public class ConfigurationTest : BaseActorTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationTest"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public ConfigurationTest(ITestOutputHelper output)
            : base(output, GetConfig())
        {
        }

        /// <summary>
        /// Generates test akka configuration
        /// </summary>
        /// <returns>test akka configuration</returns>
        public static Config GetConfig()
        {
            return ConfigurationFactory.ParseString(@"{
                akka.actor.deployment {
                    /testNameSpace {
                        IsNameSpace = true
                        dispatcher = TaxiKit.test-dispatcher
                    }

                    /somethingElse {
                    }

                    /testNameSpace/forwarder {
                        type = ""TaxiKit.Core.TestKit.TestActorForwarder, TaxiKit.Core.TestKit""
                        dispatcher = TaxiKit.test-dispatcher
                    }
                 }
            }");
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
            this.WindsorContainer.Register(Classes.FromAssemblyContaining<NameSpaceActor>().Pick().LifestyleTransient());
            this.WindsorContainer.Register(Classes.FromAssemblyContaining<TestActorForwarder>().Pick().LifestyleTransient());
            this.WindsorContainer.Register(Component.For<IActorRef>().Instance(this.TestActor).Named("testActor"));

            this.Sys.StartNameSpaceActorsFromConfiguration();
            this.Sys.ActorSelection("/user/testNameSpace/forwarder").Tell("Hello world");
            Assert.Equal("Hello world", this.ExpectMsg<string>("/user/testNameSpace/forwarder"));
        }
    }
}