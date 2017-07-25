// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestMessage.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Message wrapper to include original receiver address
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Core.TestKit
{
    /// <summary>
    /// Message wrapper to include original receiver address
    /// </summary>
    /// <typeparam name="T">Original message type</typeparam>
    public class TestMessage<T> : IMessageWithPath
    {
        /// <summary>
        /// Gets or sets the original message.
        /// </summary>
        public T Message { get; set; }

        /// <summary>
        /// Gets or sets the receiver address path.
        /// </summary>
        public string ReceiverPath { get; set; }

        /// <summary>
        /// Gets the receiver address path from actor system root.
        /// </summary>
        public string ReceiverPathRooted => TestActorForwarder.RootedPath(this.ReceiverPath);
    }
}