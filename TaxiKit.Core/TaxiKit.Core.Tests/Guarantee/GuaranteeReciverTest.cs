// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GuaranteeReciverTest.cs" company="TaxiKit">
//   All rights reserved
// </copyright>
// <summary>
// Testing <seealso cref="GuaranteeRecieverActor"/> test
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TaxiKit.Core.Tests.Guarantee
{
    using System;
    using System.Collections.Concurrent;

    using Akka.Actor;
    using Akka.Configuration;
    using Akka.DI.Core;

    using Castle.MicroKernel.Registration;

    using StackExchange.Redis;

    using TaxiKit.Core.Guarantee;
    using TaxiKit.Core.TestKit;
    using TaxiKit.Core.TestKit.Moq;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Testing <seealso cref="GuaranteeRecieverActor"/> test
    /// </summary>
    public class GuaranteeReciverTest : BaseActorTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GuaranteeReciverTest"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public GuaranteeReciverTest(ITestOutputHelper output)
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
                    /testreceiver {
                        endReciever = ""/user/test""
                        dispatcher = TaxiKit.test-dispatcher
                    }
                 }
            }");
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
                this.Sys.DI().Props<GuaranteeRecieverActor>().WithDispatcher("TaxiKit.test-dispatcher"),
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
                    this.Sys.DI().Props<GuaranteeRecieverActor>().WithDispatcher("TaxiKit.test-dispatcher"),
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
            Assert.True(this.ExpectMsg<bool>());
            Assert.Equal("Hello world", this.ExpectMsg<string>("/user/test"));
            Assert.Equal(string.Empty, redis[string.Format(GuaranteeEnvelope.RedisKeyFormat, envelope.MessageId)]);
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
                    this.Sys.DI().Props<GuaranteeRecieverActor>().WithDispatcher("TaxiKit.test-dispatcher"),
                    "testreceiver");

            var testActor = this.ActorOf(() => new TestActorForwarder(this.TestActor), "test");
            var envelope = new GuaranteeEnvelope
            {
                Message = "Hello world",
                MessageId = Guid.NewGuid(),
                Sender = testActor
            };

            redis[string.Format(GuaranteeEnvelope.RedisKeyFormat, envelope.MessageId)] = string.Empty;
            actor.Tell(envelope);
            Assert.False(this.ExpectMsg<bool>());
            this.ExpectNoMsg();
            Assert.Equal(string.Empty, redis[string.Format(GuaranteeEnvelope.RedisKeyFormat, envelope.MessageId)]);
        }
    }
}