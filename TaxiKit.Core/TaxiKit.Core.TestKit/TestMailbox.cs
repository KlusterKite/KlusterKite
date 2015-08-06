// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestMailbox.cs" company="TaxiKit">
//   All rights reserved
// </copyright>
// <summary>
//   Override of standard mailbox for synchronous message delivery
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TaxiKit.Core.TestKit
{
    using System;

    using Akka.Actor;
    using Akka.Dispatch;

    /// <summary>
    /// Override of standard mailbox for synchronous message delivery
    /// </summary>
    public class TestMailbox : UnboundedMailbox
    {
        /// <summary>
        /// Posts the specified envelope.
        /// </summary>
        /// <param name="receiver">
        /// The message receiver
        /// </param>
        /// <param name="envelope">The envelope. </param>
        public override void Post(IActorRef receiver, Envelope envelope)
        {
            if (receiver.Path.ToString().IndexOf("akka://test/system/", StringComparison.InvariantCulture) != 0)
            {
                CallingThreadDispatcher.RiseBlock();
            }

            base.Post(receiver, envelope);
        }
    }
}