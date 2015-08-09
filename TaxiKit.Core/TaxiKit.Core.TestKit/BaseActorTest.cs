// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseActorTest.cs" company="Taxi@SMS">
//   All rights reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace TaxiKit.Core.TestKit
{
    using System;
    using System.Configuration;
    using System.Linq.Expressions;

    using Akka.Actor;
    using Akka.Configuration;
    using Akka.Configuration.Hocon;
    using Akka.Logger.Serilog;
    using Akka.TestKit;
    using Akka.TestKit.Xunit2;

    using Serilog;

    using TaxiKit.Core.Utils;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// <seealso cref="TestKit"/> extension class
    /// </summary>
    public class BaseActorTest : TestKit
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseActorTest"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public BaseActorTest(ITestOutputHelper output)
            : base(CreateTestActorSystem(output))
        {
            this.Initialize();
        }

        /// <summary>
        /// Create a new actor as child of <see cref="Sys"/> and returns it as <see cref="TestActorRef{TActor}"/>
        /// to enable access to the underlying actor instance via <see cref="TestActorRefBase{TActor}.UnderlyingActor"/>.
        /// Uses an expression that calls the constructor of <typeparamref name="TActor"/>.
        /// <example>
        /// <code>
        /// ActorOf&lt;MyActor&gt;(()=&gt;new MyActor("value", 4711), "test-actor")
        /// </code>
        /// </example>
        /// </summary>
        /// <typeparam name="TActor">
        /// The type of the actor.
        /// </typeparam>
        /// <param name="factory">
        /// An expression that calls the constructor of <typeparamref name="TActor"/>
        /// </param>
        /// <param name="name">
        /// Optional: The name.
        /// </param>
        /// <returns>
        /// The actor ref
        /// </returns>
        public new TestActorRef<TActor> ActorOfAsTestActorRef<TActor>(Expression<Func<TActor>> factory, string name = null) where TActor : ActorBase
        {
            return new TestActorRef<TActor>(this.Sys, Props.Create(factory).WithDispatcher("TaxiKit.test-dispatcher"), null, name);
        }

        /// <summary>
        /// Cleanup after test run
        /// </summary>
        public virtual void Cleanup()
        {
            DateTimeWrapper.NowGetter = null;
            CallingThreadDispatcher.ConcurrentMode = false;
        }

        /// <summary>
        /// Actor system initialization
        /// </summary>
        public virtual void Initialize()
        {
            DateTimeWrapper.NowGetter = () => TimeMachineScheduler.GetCurrentLocation().DateTime;
            TimeMachineScheduler.Reset();
            CallingThreadDispatcher.ConcurrentMode = false;

            // RootActors.Start(this.GetRootActors(), this.Sys, null);
        }

        /// <summary>
        /// Waiting for message
        /// <seealso cref="TestActorForwarder"/>
        /// that was sent to the specified address
        /// </summary>
        /// <param name="path">
        /// specified address
        /// </param>
        /// <typeparam name="T">
        /// The original message type
        /// </typeparam>
        /// <returns>
        /// The original message
        /// </returns>
        protected T ExpectMsg<T>(string path)
        {
            var message = this.ExpectMsg<TestMessage<T>>();
            Assert.Equal(path, message.ReceiverPathRooted);
            return message.Message;
        }

        /// <summary>
        /// Waiting for message
        /// <seealso cref="TestActorForwarder"/>
        /// that was sent to the specified address on specified time
        /// </summary>
        /// <param name="path">
        /// specified address
        /// </param>
        /// <param name="timeout">
        /// Timespan when message should arrive
        /// </param>
        /// <typeparam name="T">
        /// The original message type
        /// </typeparam>
        /// <remarks>
        /// It is supposed that bitween now and timespan they will be now messagres, and message should arrive exactly at specified time
        /// </remarks>
        /// <returns>
        /// The original message
        /// </returns>
        protected T ExpectMsg<T>(string path, TimeSpan timeout)
        {
            var message = this.ExpectTestMsg<T>(timeout);
            Assert.Equal(path, message.ReceiverPathRooted);
            return message.Message;
        }

        /// <summary>
        /// Expecting that ther is no new messages
        /// </summary>
        protected void ExpectNoTestMsg()
        {
            // CallingThreadDispatcher.WaitForAllDone();
            if (this.HasMessages)
            {
                var message = this.ExpectMsg<object>();
                if (message.GetType().IsGenericType && message.GetType().GetGenericTypeDefinition() == typeof(TestMessage<>))
                {
                    var path = (string)message.GetType().GetProperty("RecieverPathRooted").GetValue(message);
                    var type = message.GetType().GenericTypeArguments[0];
                    Assert.False(true, $"Expected no messages, but got message of type {type.FullName} to {path}");
                }
                else
                {
                    Assert.False(true, $"Expected no messages, but got nonforwarded message {message.GetType().FullName}");
                }
            }
        }

        /// <summary>
        /// Expects test message <seealso cref="TestActorForwarder"/>
        /// </summary>
        /// <typeparam name="T">
        /// The original message type
        /// </typeparam>
        /// <returns>
        /// The <see cref="TestMessage{T}"/>.
        /// </returns>
        protected TestMessage<T> ExpectTestMsg<T>()
        {
            return this.ExpectMsg<TestMessage<T>>();
        }

        /// <summary>
        /// Waiting for message
        /// <seealso cref="TestActorForwarder"/>
        /// that was sent on specified time
        /// </summary>
        /// <param name="timeout">
        /// Timespan when message should arrive
        /// </param>
        /// <typeparam name="T">
        /// The original message type
        /// </typeparam>
        /// <returns>
        /// The <see cref="TestMessage{T}"/>.
        /// </returns>
        /// <remarks>
        /// It is supposed that bitween now and timespan they will be now messagres, and message should arrive exactly at specified time
        /// </remarks>
        protected TestMessage<T> ExpectTestMsg<T>(TimeSpan timeout)
        {
            this.ExpectNoTestMsg();
            TimeMachineScheduler.JumpJustBefore(timeout);
            this.ExpectNoTestMsg();
            TimeMachineScheduler.JumpAfter();
            return this.ExpectTestMsg<T>();
        }

        /// <summary>
        /// Creating actor system for test
        /// </summary>
        /// <param name="output">
        /// Xunit output
        /// </param>
        /// <returns>
        /// actor system for test
        /// </returns>
        private static ActorSystem CreateTestActorSystem(ITestOutputHelper output)
        {
            var loggerConfig =
                new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.TextWriter(new XunitOutputWriter(output));
            Serilog.Log.Logger = loggerConfig.CreateLogger();
            var section = ConfigurationManager.GetSection("akka") as AkkaConfigurationSection;
            var config = (section?.AkkaConfig ?? ConfigurationFactory.Empty)
                .WithFallback(ConfigurationFactory.ParseString(Configuration.AkkaConfig));

            return ActorSystem.Create("test", config);
        }

        /*
        /// <summary>
        /// Формирует список адресов по каторым расставляем тестовые акторы
        /// </summary>
        /// <returns>список адресов</returns>
        public virtual List<RootActors.RootActorDescription> GetRootActors()
        {
            var rootActors = RootActors.GetRootActors();
            foreach (var rootActor in rootActors)
            {
                rootActor.Props = Props.Create(() => new TestActorForwarder(this.TestActor));
            }

            return rootActors;
        }
        */
    }
}