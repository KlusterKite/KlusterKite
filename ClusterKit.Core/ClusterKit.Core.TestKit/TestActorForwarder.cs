// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestActorForwarder.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Actor that envelopes all recieved messages in <seealso cref="TestMessage{T}" /> and forwards to test actor
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.TestKit
{
    using System;

    using Akka.Actor;
    using Akka.TestKit;

    /// <summary>
    /// Actor that envelopes all received messages in <seealso cref="TestMessage{T}"/> and forwards to test actor
    /// </summary>
    public class TestActorForwarder : ReceiveActor
    {
        /// <summary>
        /// The test actor reference.
        /// </summary>
        protected readonly IActorRef TestActor;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestActorForwarder"/> class.
        /// </summary>
        /// <param name="testActor">
        /// The test actor reference.
        /// </param>
        public TestActorForwarder(IActorRef testActor)
        {
            this.TestActor = testActor;
            this.Receive<CreateChildMessage>(m => this.CreateChild(m));
            this.Receive<object>(o => this.ForwardMessage(o));
        }

        /// <summary>
        /// Gets self path related to actor system root
        /// </summary>
        /// <param name="path">
        /// Full actor path
        /// </param>
        /// <returns>
        /// path related to actor system root
        /// </returns>
        public static string RootedPath(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                var index = path.IndexOf("akka://test/", StringComparison.InvariantCulture);
                return index >= 0 ? path.Substring(index + 11) : path;
            }

            return path;
        }

        /// <summary>
        /// Creates child actor for actor hierarchy testing
        /// </summary>
        /// <param name="createChild">
        /// Child creation instructions
        /// </param>
        public void CreateChild(CreateChildMessage createChild)
        {
            var returnType = typeof(TestActorRef<>).MakeGenericType(createChild.Props.Type);

            var actorRef = Activator.CreateInstance(
                returnType,
                Context.System,
                createChild.Props,
                this.Self,
                createChild.Name);
            this.Sender.Tell(actorRef);
        }

        /// <summary>
        /// Message forwarding to test actor
        /// </summary>
        /// <param name="obj">The original message</param>
        protected virtual void ForwardMessage(object obj)
        {
            var messsageToSendType = typeof(TestMessage<>).MakeGenericType(obj.GetType());
            var messageToSend = Activator.CreateInstance(messsageToSendType);
            messsageToSendType.GetProperty("Message").SetValue(messageToSend, obj);
            messsageToSendType.GetProperty("ReceiverPath").SetValue(messageToSend, this.Self.Path.ToString());
            this.TestActor.Tell(messageToSend, Context.Sender);
        }

        /// <summary>
        /// Child creation instructions
        /// </summary>
        public class CreateChildMessage
        {
            /// <summary>
            /// Gets or sets the name of newborn.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the props.
            /// </summary>
            public Props Props { get; set; }
        }
    }
}