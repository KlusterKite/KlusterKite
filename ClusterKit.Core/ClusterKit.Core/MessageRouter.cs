// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageRouter.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Base class to route messages.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core
{
    using System;
    using System.Threading.Tasks;

    using Akka.Actor;

    using JetBrains.Annotations;

    /// <summary>
    /// Base class to route messages.
    /// </summary>
    [UsedImplicitly]
    public class MessageRouter : IMessageRouter
    {
        /// <summary>
        /// Current actor system
        /// </summary>
        private readonly ActorSystem actorSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageRouter"/> class.
        /// </summary>
        /// <param name="actorSystem">
        /// The actor system.
        /// </param>
        public MessageRouter(ActorSystem actorSystem)
        {
            this.actorSystem = actorSystem;
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
        public async Task<T> Ask<T>(Address nodeAddress, string path, object message, TimeSpan timeout)
        {
            return await this.actorSystem.ActorSelection($"{nodeAddress}{path}").Ask<T>(message, timeout);
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
            this.actorSystem.ActorSelection($"{nodeAddress}{path}").Tell(message, sender);
        }
    }
}