// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NameSpaceForwarder.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The namespace actor, that also forwards all it's messages to the test actor
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.TestKit
{
    using System;

    using Akka.Actor;
    using Akka.Configuration;
    using Akka.DI.Core;
    using Akka.Event;

    using Autofac;

    using JetBrains.Annotations;

    /// <summary>
    /// The namespace actor, that also forwards all it's messages to the test actor
    /// </summary>
    [UsedImplicitly]
    public class NameSpaceForwarder : NameSpaceActor
    {
        /// <summary>
        /// The link to the actual test actor
        /// </summary>
        private readonly IActorRef testActor;

        /// <summary>
        /// Initializes a new instance of the <see cref="NameSpaceForwarder"/> class.
        /// </summary>
        /// <param name="componentContext">The access to dependency injection</param>
        /// <param name="testActor">The test actor reference</param>
        public NameSpaceForwarder(IComponentContext componentContext, IActorRef testActor)
            : base(componentContext)
        {
            this.testActor = testActor;

            // ReSharper disable FormatStringProblem
            Context.GetLogger().Info("{Type}: started on {ActorPathString} with test actor path {TestActorPathString}", this.GetType().Name, this.Self.Path, this.testActor?.Path.ToString());
            // ReSharper restore FormatStringProblem
        }

        /// <summary>
        /// Creates simple actor from config
        /// </summary>
        /// <param name="context">Current actor context (will create child actor)</param>
        /// <param name="actorConfig">Configuration to create from</param>
        /// <param name="componentContext">Dependency resolver</param>
        /// <param name="currentPath">Parent (current) actor path</param>
        /// <param name="pathName">New actor's path name</param>
        protected override void CreateSimpleActor(
                    IActorContext context,
                    Config actorConfig,
                    IComponentContext componentContext,
                    string currentPath,
                    string pathName)
        {
            var childTypeName = actorConfig.GetString("type");
            if (string.IsNullOrWhiteSpace(childTypeName))
            {
                return;
            }

            var type = Type.GetType(childTypeName);
            if (type != null)
            {
                context.GetLogger()
                    .Info(
                        // ReSharper disable FormatStringProblem
                        "{Type}: {NameSpaceName} initializing {ActorType} on {PathString}",
                        // ReSharper restore FormatStringProblem
                        typeof(NameSpaceActor).Name,
                        currentPath,
                        type.Name,
                        pathName);

                if (type == typeof(NameSpaceActor) || type == typeof(NameSpaceForwarder))
                {
                    // this is done for tests, otherwise it would lead to CircularDependencyException
                    context.ActorOf(Props.Create(() => new NameSpaceForwarder(componentContext, this.testActor)), pathName);
                }
                else
                {
                    context.ActorOf(Context.System.DI().Props(type), pathName);
                }
            }
            else
            {
                context.GetLogger()
                    .Error(
                        // ReSharper disable FormatStringProblem
                        "{Type}: {ClassTypeString} was not found for actor {NameSpaceName}/{PathString}",
                        // ReSharper restore FormatStringProblem
                        typeof(NameSpaceActor).Name,
                        childTypeName,
                        currentPath,
                        pathName);
            }
        }

        /// <summary>
        /// Message forwarding to test actor
        /// </summary>
        /// <param name="obj">The original message</param>
        protected virtual void ForwardMessage(object obj)
        {
            var messageToSendType = typeof(TestMessage<>).MakeGenericType(obj.GetType());
            var messageToSend = Activator.CreateInstance(messageToSendType);
            messageToSendType.GetProperty("Message")?.SetValue(messageToSend, obj);
            messageToSendType.GetProperty("ReceiverPath")?.SetValue(messageToSend, this.Self.Path.ToString());
            this.testActor.Tell(messageToSend, Context.Sender);
        }

        /// <summary>
        /// To be implemented by concrete UntypedActor, this defines the behavior of the UntypedActor.
        /// This method is called for every message received by the actor.
        /// </summary>
        /// <param name="message">The message.</param>
        protected override void OnReceive(object message)
        {
            // ReSharper disable FormatStringProblem
            Context.GetLogger().Info("{Type}: received {MessageTypeName}", this.GetType().Name, message.GetType().Name);

            if (this.testActor == null)
            {
                Context.GetLogger().Error("{Type}: test actor was not defined", this.GetType().Name);
                return;
            }

            this.ForwardMessage(message);
            // ReSharper restore FormatStringProblem
            base.OnReceive(message);
        }
    }
}