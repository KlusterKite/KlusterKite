namespace ClusterKit.Core.TestKit
{
    using System;

    using Akka.Actor;
    using Akka.Configuration;
    using Akka.DI.Core;
    using Akka.Event;

    using Castle.Windsor;

    using JetBrains.Annotations;

    /// <summary>
    /// The namespace actor, that also forwards all it's messages to the test actor
    /// </summary>
    [UsedImplicitly]
    public class NameSpaceForwarder : NameSpaceActor
    {
        private readonly IActorRef testActor;

        /// <summary>
        /// Initializes a new instance of the <see cref="NameSpaceForwarder"/> class.
        /// </summary>
        /// <param name="windsorContainer">The access to dependency injection</param>
        /// <param name="testActor">The test actor reference</param>
        public NameSpaceForwarder(IWindsorContainer windsorContainer, IActorRef testActor)
            : base(windsorContainer)
        {
            this.testActor = testActor;

            // ReSharper disable FormatStringProblem
            Context.GetLogger().Info("{Type}: started on {ActorPathString}", this.GetType().Name, this.Self.Path);
            // ReSharper restore FormatStringProblem
        }

        /// <summary>
        /// To be implemented by concrete UntypedActor, this defines the behavior of the UntypedActor.
        /// This method is called for every message received by the actor.
        /// </summary>
        /// <param name="message">The message.</param>
        protected override void OnReceive(object message)
        {
            this.testActor.Forward(message);
            // ReSharper disable FormatStringProblem
            Context.GetLogger().Info("{Type}: received {MessageTypeName}", this.GetType().Name, message.GetType().Name);
            // ReSharper restore FormatStringProblem
            base.OnReceive(message);
        }

        protected override void CreateSimpleActor(
            IActorContext context,
            Config actorConfig,
            IWindsorContainer windsorContainer,
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
                    context.ActorOf(Props.Create(() => new NameSpaceForwarder(windsorContainer, this.testActor)), pathName);
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
    }
}
