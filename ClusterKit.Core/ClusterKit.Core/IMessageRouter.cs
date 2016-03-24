// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMessageRouter.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Base class to routing messages.
//   Used to give possibility to create routing mocks
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core
{
    using System;
    using System.Threading.Tasks;

    using Akka.Actor;

    /// <summary>
    /// Base class to route messages.
    /// Used to give possibility to create routing mocks
    /// </summary>
    public interface IMessageRouter
    {
        /// <summary>
        /// Sends message to the specified actor path on the specified node. See also <see cref="Futures.Ask"/>
        /// </summary>
        /// <typeparam name="T">The type of awaited response</typeparam>
        /// <param name="nodeAddress">The node address.</param>
        /// <param name="path">The recipient path.</param>
        /// <param name="message">The message.</param>
        ///  <param name="timeout">The maximum time to wait for response</param>
        /// <returns>The response</returns>
        Task<T> Ask<T>(Address nodeAddress, string path, object message, TimeSpan timeout);

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
        void Tell(Address nodeAddress, string path, object message, IActorRef sender = null);
    }
}