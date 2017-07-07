// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteTestMessage.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Message wrapper to include original receiver actor path and receiver node address
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.TestKit
{
    using Akka.Actor;

    /// <summary>
    /// Message wrapper to include original receiver actor path and receiver node address
    /// </summary>
    /// <typeparam name="T">Original message type</typeparam>
    public class RemoteTestMessage<T> : TestMessage<T>
    {
        /// <summary>
        /// Gets or sets the receiver node address
        /// </summary>
        public Address RecipientAddress { get; set; }
    }
}