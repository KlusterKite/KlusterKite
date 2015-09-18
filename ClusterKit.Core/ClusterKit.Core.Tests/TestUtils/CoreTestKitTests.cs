// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActorSystemTest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   ClusterKit TestKit tests
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.Tests.TestUtils
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Akka.Actor;

    using ClusterKit.Core.TestKit;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// ClusterKit TestKit tests
    /// </summary>
    public class CoreTestKitTests : BaseActorTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestKitTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public CoreTestKitTests(ITestOutputHelper output)
                    : base(output)
        {
        }

        /// <summary>
        /// <seealso cref="CallingThreadDispatcher"/> testing
        /// </summary>
        [Fact]
        public void CallingThreadDispatcherTest()
        {
            Assert.True(CallingThreadDispatcher.WaitForAllDone(TimeSpan.FromMilliseconds(10)));
            var testActor = this.ActorOfAsTestActorRef(() => new DelayedForwarder(this.TestActor));

            CallingThreadDispatcher.RiseBlock();
            Task.Run(() => testActor.Tell("hello world"));
            Assert.False(this.HasMessages);
            Assert.True(CallingThreadDispatcher.WaitForAllDone(TimeSpan.FromMilliseconds(110)));
            Assert.True(this.HasMessages);
        }

        /// <summary>
        /// Single threading and <seealso cref="Akka.TestKit.TestKitBase.HasMessages"/> test
        /// </summary>
        [Fact]
        public void ExpectHasMessagesTest()
        {
            var test = this.ActorOfAsTestActorRef(() => new DelayedForwarder(this.TestActor), "test-1");
            test.Tell("Hello world");
            Assert.True(this.HasMessages);
        }

        /// <summary>
        /// Single threading and <seealso cref="BaseActorTest.ExpectNoTestMsg"/> test
        /// </summary>
        [Fact]
        public void ExpectNoMsgTest()
        {
            var test = this.ActorOfAsTestActorRef(() => new DelayedForwarder(this.TestActor), "test-1");
            bool recieved = false;
            test.Tell("Hello world");

            try
            {
                this.ExpectNoTestMsg();
            }
            catch (Exception)
            {
                recieved = true;
            }

            Assert.True(recieved, "Everything is ok. We've managed to recieve message in time.");
        }

        /// <summary>
        /// Actor System Initialization Test
        /// </summary>
        [Fact]
        public void TestTestKit()
        {
            this.ActorOfAsTestActorRef(() => new TestActorForwarder(this.TestActor), "test-1");

            this.Sys.ActorSelection("/user/test-1").Tell("hello world");
            var message = this.ExpectMsg<TestMessage<string>>();
            Assert.Equal("hello world", message.Message);
            Assert.Equal("/user/test-1", message.ReceiverPathRooted);
            Assert.Equal(typeof(TimeMachineScheduler), this.Sys.Scheduler.GetType());
        }

        /// <summary>
        /// Time machine work test
        /// </summary>
        [Fact]
        public void TimeMachineTest()
        {
            var jump = TimeSpan.FromSeconds(1000);
            this.Sys.Scheduler.ScheduleTellOnce(jump, this.TestActor, new TestMessage<string> { Message = "hello world" }, this.TestActor);
            this.ExpectTestMsg<string>(jump);
            this.ExpectNoTestMsg();

            jump = TimeSpan.FromDays(1000);
            this.Sys.Scheduler.ScheduleTellOnce(jump, this.TestActor, new TestMessage<string> { Message = "hello world" }, this.TestActor);
            this.ExpectTestMsg<string>(jump);
            this.ExpectNoTestMsg();

            jump = TimeSpan.FromMilliseconds(1);
            this.Sys.Scheduler.ScheduleTellOnce(jump, this.TestActor, new TestMessage<string> { Message = "hello world" }, this.TestActor);
            this.ExpectTestMsg<string>(jump);
            this.ExpectNoTestMsg();
        }

        /// <summary>
        /// TestActorForwarder modification, that forwards messages with some delay
        /// </summary>
        private class DelayedForwarder : TestActorForwarder
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DelayedForwarder"/> class.
            /// </summary>
            /// <param name="testActor">
            /// The test actor.
            /// </param>
            public DelayedForwarder(IActorRef testActor)
                : base(testActor)
            {
            }

            /// <summary>
            /// Message forwarding to test actor
            /// </summary>
            /// <param name="obj">The original message</param>
            protected override void ForwardMessage(object obj)
            {
                Thread.Sleep(100);
                base.ForwardMessage(obj);
            }
        }
    }
}