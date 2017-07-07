// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestMessageRouter.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Mocked message router
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Core.TestKit
{
    using System;
    using System.Collections.Generic;
#if CORECLR
    using System.Reflection;
#endif
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
        /// The list of actors that would receive all messages to the virtual node as <see cref="TestMessage{T}"/>
        /// </summary>
        private readonly Dictionary<Address, IActorRef> registeredNodes = new Dictionary<Address, IActorRef>();

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
        /// Sends message to the specified actor path on the specified node. See also <see cref="Futures.Ask(Akka.Actor.ICanTell,object,System.Nullable{System.TimeSpan})"/>
        /// </summary>
        /// <typeparam name="T">The type of awaited response</typeparam>
        /// <param name="nodeAddress">The node address.</param>
        /// <param name="path">The recipient path.</param>
        /// <param name="message">The message.</param>
        ///  <param name="timeout">The maximum time to wait for response</param>
        /// <returns>The response</returns>
        public Task<T> Ask<T>(Address nodeAddress, string path, object message, TimeSpan timeout)
        {
            var forwardedMessage = CreateForwardedMessage(nodeAddress, path, message);

            IActorRef receiver;

            if (this.registeredNodes.TryGetValue(nodeAddress, out receiver))
            {
                return receiver.Ask<T>(forwardedMessage, timeout);
            }
            else
            {
                return this.testActor.Ask<T>(forwardedMessage, timeout);
            }
        }

        /// <summary>
        /// Registers virtual node as actor, that would receive all messages as <see cref="TestMessage{T}"/>
        /// </summary>
        /// <param name="address">The virtual node address</param>
        /// <param name="receiver">Virtual node representative</param>
        [UsedImplicitly]
        public void RegisterVirtualNode(Address address, IActorRef receiver)
        {
            this.registeredNodes[address] = receiver;
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
            var forwardedMessage = CreateForwardedMessage(nodeAddress, path, message);
            IActorRef receiver;
            if (this.registeredNodes.TryGetValue(nodeAddress, out receiver))
            {
                receiver.Tell(forwardedMessage, sender);
            }
            else
            {
                this.testActor.Tell(forwardedMessage, sender);
            }
        }

        /// <summary>
        ///  Create forwarded message with specified parameters
        /// </summary>
        /// <param name="nodeAddress">The node address.</param>
        /// <param name="path">The recipient path.</param>
        /// <param name="message">The message.</param>
        /// <returns>The message wrapped in <see cref="RemoteTestMessage{T}"/></returns>
        private static object CreateForwardedMessage(
            Address nodeAddress,
            string path,
            object message)
        {
            var returnType = typeof(RemoteTestMessage<>).MakeGenericType(message.GetType());
            var forwardedMessage = Activator.CreateInstance(returnType);
            returnType.GetProperty("Message")?.SetValue(forwardedMessage, message);
            returnType.GetProperty("ReceiverPath")?.SetValue(forwardedMessage, path);
            returnType.GetProperty("RecipientAddress")?.SetValue(forwardedMessage, nodeAddress);
            return forwardedMessage;
        }
    }
}