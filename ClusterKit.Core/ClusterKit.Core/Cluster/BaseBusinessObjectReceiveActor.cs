using System;

namespace ClusterKit.Core.Cluster
{
    using Akka.Actor;

    using ClusterKit.Core.Cluster.Messages;

    /// <summary>
    /// Base class for simple business object actors.
    /// </summary>
    public abstract class BaseBusinessObjectReceiveActor : ReceiveActor, IWithUnboundedStash
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseBusinessObjectReceiveActor"/> class.
        /// </summary>
        protected BaseBusinessObjectReceiveActor()
        {
            this.Receive<SetObjectId>(
                delegate (SetObjectId id)
                    {
                        this.Id = id.Id;
                        this.Stash.UnstashAll();
                        this.Become(this.SubscribeToMessages);
                    });
            this.Receive<object>(m => this.Stash.Stash());
        }

        /// <summary>
        /// Gets business object identification
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets or sets the stash. This will be automatically populated by the framework AFTER the constructor has been run.
        ///             Implement this as an auto property.
        /// </summary>
        /// <value>
        /// The stash.
        /// </value>
        public IStash Stash { get; set; }

        /// <summary>
        /// Perform all usual <seealso cref="Receive"/>, that usualy done in constructor.
        /// Also any additional initialization can be made here.
        /// </summary>
        public abstract void SubscribeToMessages();

        /// <summary>
        /// User overridable callback: '''By default it disposes of all children and then calls `postStop()`.'''
        ///                 <p/>
        ///                 Is called on a crashed Actor right BEFORE it is restarted to allow clean
        ///                 up of resources before Actor is terminated.
        /// </summary>
        /// <param name="reason">the Exception that caused the restart to happen.</param><param name="message">optionally the current message the actor processed when failing, if applicable.</param>
        protected override void PreRestart(Exception reason, object message)
        {
            this.Self.Tell(new SetObjectId { Id = this.Id });
            base.PreRestart(reason, message);
        }
    }
}