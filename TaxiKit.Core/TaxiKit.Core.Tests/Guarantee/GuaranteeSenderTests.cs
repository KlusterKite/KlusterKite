// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GuaranteeSenderTests.cs" company="TaxiKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing <seealso cref="GuaranteeSenderActor" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TaxiKit.Core.Tests.Guarantee
{
    using System;
    using System.Collections.Concurrent;

    using Akka.Actor;
    using Akka.Configuration;
    using Akka.DI.Core;
    using Akka.TestKit;

    using Castle.MicroKernel.Registration;

    using StackExchange.Redis;

    using TaxiKit.Core.Guarantee;
    using TaxiKit.Core.TestKit;
    using TaxiKit.Core.TestKit.Moq;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Testing <seealso cref="GuaranteeSenderActor"/>
    /// </summary>
    public class GuaranteeSenderTests : BaseActorTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GuaranteeSenderTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public GuaranteeSenderTests(ITestOutputHelper output)
            : base(output, GetGuaranteeSenderConfig())
        {
        }

        /// <summary>
        /// Generates <seealso cref="GuaranteeSenderActor"/> initialization config
        /// </summary>
        /// <returns>The initialization config</returns>
        public static Config GetGuaranteeSenderConfig()
        {
            return ConfigurationFactory.ParseString(@"{
            akka.actor.deployment {
                    /testsender {
                        maxAttempts = 5
                        deliveryTimeout = 10ms
                        nextAttemptWait = 100ms
                        dispatcher = TaxiKit.test-dispatcher
                    }
                    /testsender/workers {
                        router = round-robin-pool
                        nr-of-instances = 5
                        dispatcher = TaxiKit.test-dispatcher
                    }
                    ""/testsender/workers/*"" {
                        dispatcher = TaxiKit.test-dispatcher
                    }
                    /testsender/receiver {
                        router = random-group
                        routees.paths = [""/user/testReceiver""]
                        dispatcher = TaxiKit.test-dispatcher
                    }
                }
            }");
        }

        /// <summary>
        /// Testing correct actor initialization
        /// </summary>
        [Fact]
        public void GuaranteeRessendTest()
        {
            var redis = new ConcurrentDictionary<string, object>();
            this.WindsorContainer.Register(
                Component.For<GuaranteeSenderActor>().Named("GuaranteeSenderActor").LifestyleTransient());
            this.WinsorBind<IConnectionMultiplexer>(() => new RedisConnectionMoq(redis));

            this.ActorOf(() => new TestActorForwarder(this.TestActor), "testReceiver");

            var actor = this.ActorOfAsTestActorRef<GuaranteeSenderActor>(
                this.Sys.DI().Props<GuaranteeSenderActor>().WithDispatcher("TaxiKit.test-dispatcher"),
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
            this.WindsorContainer.Register(
                Component.For<GuaranteeSenderActor>().Named("GuaranteeSenderActor").LifestyleTransient());
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
            this.WindsorContainer.Register(
                Component.For<GuaranteeSenderActor>().Named("GuaranteeSenderActor").LifestyleTransient());
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
            Assert.True(redis.ContainsKey(string.Format(GuaranteeEnvelope.RedisKeyFormat, rec.MessageId)));
        }
    }
}