// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestMessageRouter.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Mocked message router
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.TestKit
{
    using System;
    using System.Threading.Tasks;

    using Akka.Actor;

    using JetBrains.Annotations;

    /// <summary>
    /// Mocked message router
    /// </summary>
    [UsedImplicitly]
    public class TestMessageRouter : IMessageRouter
    {
        /// <summary>
        /// Reference to the test actor
        /// </summary>
        private readonly IActorRef testActor;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestMessageRouter"/> class.
        /// </summary>
        /// <param name="testActor">
        /// The test actor.
        /// </param>
        public TestMessageRouter(IActorRef testActor)
        {
            this.testActor = testActor;
        }

        /// <summary>
        /// Sends message to the specified actor path on the specified node. See also <see cref="Futures.Ask"/>
        /// </summary>
        /// <typeparam name="T">The type of awaited response</typeparam>
        /// <param name="nodeAddress">The node address.</param>
        /// <param name="path">The recipient path.</param>
        /// <param name="message">The message.</param>
        ///  <param name="timeout">The maximum time to wait for response</param>
        /// <returns>The response</returns>
        public Task<T> Ask<T>(Address nodeAddress, string path, object message, TimeSpan timeout)
        {
            return this.testActor.Ask<T>(
                new RemoteTestMessage<object>
                {
                    Message = message,
                    ReceiverPath = path,
                    RecipientAddress = nodeAddress
                },
            timeout);
        }

        /// <summary>
        /// Sends message to the specified actor path on the specified node
        /// </summary>
        /// <param name="nodeAddress">
        /// The node address.
        /// </param>
        /// <param name="path">
        /// The recipient path.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="sender">
        /// The sender.
        /// </param>
        public void Tell(Address nodeAddress, string path, object message, IActorRef sender = null)
        {
            this.testActor.Tell(
                new RemoteTestMessage<object>
                {
                    Message = message,
                    ReceiverPath = path,
                    RecipientAddress = nodeAddress
                },
            sender);
        }
    }
}