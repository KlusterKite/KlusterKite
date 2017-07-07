// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PingActor.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   General actor to resolve ping requests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Core.Ping
{
    using Akka.Actor;

    using JetBrains.Annotations;

    /// <summary>
    /// General actor to resolve ping requests.
    /// </summary>
    [UsedImplicitly]
    public class PingActor : ReceiveActor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PingActor"/> class.
        /// </summary>
        public PingActor()
        {
            this.Receive<PingMessage>(m => this.Sender.Tell(new PongMessage()));
        }
    }
}