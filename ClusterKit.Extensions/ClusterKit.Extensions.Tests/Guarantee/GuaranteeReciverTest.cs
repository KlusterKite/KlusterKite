// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GuaranteeReciverTest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
// Testing <seealso cref="GuaranteeRecieverActor"/> test
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Extensions.Tests.Guarantee
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using Akka.Configuration;
    using Akka.DI.Core;

    using Castle.Windsor;

    using ClusterKit.Core;
    using ClusterKit.Core.TestKit;
    using ClusterKit.Core.TestKit.Moq;
    using ClusterKit.Guarantee.Delivery;

    using StackExchange.Redis;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Testing <seealso cref="ClusterKit.Core.Guarantee.GuaranteeRecieverActor"/> test
    /// </summary>
    public class GuaranteeReciverTest : BaseActorTest<GuaranteeReciverTest.Configurator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GuaranteeReciverTest"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public GuaranteeReciverTest(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Testing correct actor configuration from config
        /// </summary>
        [Fact]
        public void GuaranteeReceiverInitializationTest()
        {
            var redis = new ConcurrentDictionary<string, object>();
            this.WinsorBind<IConnectionMultiplexer>(() => new RedisConnectionMoq(redis));

            var actor = this.ActorOfAsTestActorRef<GuaranteeRecieverActor>(
                this.Sys.DI().Props<GuaranteeRecieverActor>().WithDispatcher("ClusterKit.test-dispatcher"),
                "testreceiver");

            Assert.Equal("/user/test", actor.UnderlyingActor.EndReciever);
        }

        /// <summary>
        /// Testing correct message forward
        /// </summary>
        [Fact]
        public void GuaranteeReceiverNormalTest()
        {
            var redis = new ConcurrentDictionary<string, object>();
            this.WinsorBind<IConnectionMultiplexer>(() => new RedisConnectionMoq(redis));

            var actor =
                this.ActorOfAsTestActorRef<GuaranteeRecieverActor>(
                    this.Sys.DI().Props<GuaranteeRecieverActor>().WithDispatcher("ClusterKit.test-dispatcher"),
                    "testreceiver");

            var testActor = this.ActorOf(() => new TestActorForwarder(this.TestActor), "test");
            var envelope = new GuaranteeEnvelope
            {
                Message = "Hello world",
                MessageId = Guid.NewGuid(),
                Sender = testActor
            };

            redis[string.Format(GuaranteeEnvelope.RedisKeyFormat, envelope.MessageId)] = "1";
            actor.Tell(envelope);
            Assert.Equal("Hello world", this.ExpectMsg<string>("/user/test"));
            Assert.True(this.ExpectMsg<bool>());
            Assert.Equal(string.Empty, redis[string.Format((string)GuaranteeEnvelope.RedisKeyFormat, (object)envelope.MessageId)]);
        }

        /// <summary>
        /// Testing correct message forward
        /// </summary>
        [Fact]
        public void GuaranteeReceiverRepeatedMessageTest()
        {
            var redis = new ConcurrentDictionary<string, object>();
            this.WinsorBind<IConnectionMultiplexer>(() => new RedisConnectionMoq(redis));

            var actor =
                this.ActorOfAsTestActorRef<GuaranteeRecieverActor>(
                    this.Sys.DI().Props<GuaranteeRecieverActor>().WithDispatcher("ClusterKit.test-dispatcher"),
                    "testreceiver");

            var testActor = this.ActorOf(() => new TestActorForwarder(this.TestActor), "test");
            var envelope = new GuaranteeEnvelope
            {
                Message = "Hello world",
                MessageId = Guid.NewGuid(),
                Sender = testActor
            };

            redis[string.Format((string)GuaranteeEnvelope.RedisKeyFormat, (object)envelope.MessageId)] = string.Empty;
            actor.Tell(envelope);
            Assert.False(this.ExpectMsg<bool>());
            this.ExpectNoMsg();
            Assert.Equal(string.Empty, redis[string.Format((string)GuaranteeEnvelope.RedisKeyFormat, (object)envelope.MessageId)]);
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
                    /testreceiver {
                        endReciever = ""/user/test""
                        dispatcher = ClusterKit.test-dispatcher
                    }
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
                var pluginInstallers = base.GetPluginInstallers();
                pluginInstallers.Add(new ClusterKit.Guarantee.Installer());
                return pluginInstallers;
            }
        }
    }
}