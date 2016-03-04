// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GuaranteeSenderTests.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing <seealso cref="GuaranteeSenderActor" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Extensions.Tests.Guarantee
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using Akka.Actor;
    using Akka.Configuration;
    using Akka.DI.Core;
    using Akka.TestKit;

    using Castle.Windsor;

    using ClusterKit.Core;
    using ClusterKit.Core.TestKit;
    using ClusterKit.Extensions.Tests.Moq;
    using ClusterKit.Guarantee.Delivery;

    using StackExchange.Redis;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Testing <seealso cref="ClusterKit.Core.Guarantee.GuaranteeSenderActor"/>
    /// </summary>
    public class GuaranteeSenderTests : BaseActorTest<GuaranteeSenderTests.Configurator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GuaranteeSenderTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public GuaranteeSenderTests(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Testing correct actor initialization
        /// </summary>
        [Fact]
        public void GuaranteeRessendTest()
        {
            var redis = new ConcurrentDictionary<string, object>();
            this.WinsorBind<IConnectionMultiplexer>(() => new RedisConnectionMoq(redis));

            this.ActorOf(() => new TestActorForwarder(this.TestActor), "testReceiver");

            var actor = this.ActorOfAsTestActorRef<GuaranteeSenderActor>(
                this.Sys.DI().Props<GuaranteeSenderActor>().WithDispatcher("ClusterKit.test-dispatcher"),
                "testsender");

            int attemptCount = 0;

            this.SetAutoPilot(new DelegateAutoPilot(
                (sender, message) =>
                {
                    var envelope = message as TestMessage<GuaranteeEnvelope>;
                    if (envelope != null)
                    {
                        if (attemptCount >= 1)
                        {
                            sender.Tell(true, ActorRefs.NoSender);
                        }

                        attemptCount++;
                    }

                    return AutoPilot.KeepRunning;
                }));

            actor.Tell("Hello world");

            this.ExpectMsg<GuaranteeEnvelope>("/user/testReceiver");
            var rec = this.ExpectMsg<GuaranteeEnvelope>("/user/testReceiver", TimeSpan.FromMilliseconds(100));

            Assert.Equal("Hello world", rec.Message);
            Assert.True(redis.ContainsKey(string.Format(GuaranteeEnvelope.RedisKeyFormat, rec.MessageId)));
        }

        /// <summary>
        /// Testing correct actor initialization
        /// </summary>
        [Fact]
        public void GuaranteeSenderInitializationTest()
        {
            this.WinsorBind<IConnectionMultiplexer>(() => new RedisConnectionMoq());
            this.ActorOf(() => new TestActorForwarder(this.TestActor), "testReceiver");

            var sender = this.ActorOfAsTestActorRef<GuaranteeSenderActor>(
                this.Sys.DI().Props<GuaranteeSenderActor>(),
                "testsender");

            Assert.Equal(5, sender.UnderlyingActor.MaxAttempts);
            Assert.Equal(TimeSpan.FromMilliseconds(10), sender.UnderlyingActor.DeliveryTimeout);
            Assert.Equal(TimeSpan.FromMilliseconds(100), sender.UnderlyingActor.NextAttemptWait);
        }

        /// <summary>
        /// Testing correct actor initialization
        /// </summary>
        [Fact]
        public void GuaranteeSenderTest()
        {
            var redis = new ConcurrentDictionary<string, object>();
            this.WinsorBind<IConnectionMultiplexer>(() => new RedisConnectionMoq(redis));

            this.ActorOf(() => new TestActorForwarder(this.TestActor), "testReceiver");

            var actor = this.ActorOfAsTestActorRef<GuaranteeSenderActor>(
                this.Sys.DI().Props<GuaranteeSenderActor>(),
                "testsender");

            this.SetAutoPilot(new DelegateAutoPilot(
                (sender, message) =>
                    {
                        var envelope = message as TestMessage<GuaranteeEnvelope>;
                        if (envelope != null)
                        {
                            sender.Tell(true, ActorRefs.NoSender);
                        }

                        return AutoPilot.KeepRunning;
                    }));

            actor.Tell("Hello world");
            var rec = this.ExpectMsg<GuaranteeEnvelope>("/user/testReceiver");
            Assert.Equal("Hello world", rec.Message);
            Assert.True(redis.ContainsKey(string.Format((string)GuaranteeEnvelope.RedisKeyFormat, (object)rec.MessageId)));
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
                    /testsender {
                        maxAttempts = 5
                        deliveryTimeout = 10ms
                        nextAttemptWait = 100ms
                        dispatcher = ClusterKit.test-dispatcher
                    }
                    /testsender/workers {
                        router = round-robin-pool
                        nr-of-instances = 5
                        dispatcher = ClusterKit.test-dispatcher
                    }
                    ""/testsender/workers/*"" {
                        dispatcher = ClusterKit.test-dispatcher
                    }
                    /testsender/receiver {
                        router = random-group
                        routees.paths = [""/user/testReceiver""]
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