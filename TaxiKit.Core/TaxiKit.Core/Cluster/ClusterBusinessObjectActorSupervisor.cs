namespace TaxiKit.Core.Cluster
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.DI.Core;
    using Akka.Event;
    using Akka.Util.Internal;

    using Castle.Components.DictionaryAdapter;

    using JetBrains.Annotations;

    using StackExchange.Redis;

    using TaxiKit.Core.Utils;

    public interface IMessageToBusinessObjectActor
    {
        string Id { get; }
    }

    /// <summary>
    /// Base class to manage child actors that impersonates som business objects, assuming that there should be only one actor per object across the cluster.
    /// </summary>
    /// <typeparam name="T">Type of child actors</typeparam>
    /// <remarks>
    /// All actor implementation should be located on same path in every role node.
    /// All child actors are initialized with dependency injection
    /// </remarks>
    public abstract class ClusterBusinessObjectActorSupervisor<T> : ReceiveActor where T : ActorBase
    {
        /// <summary>
        /// Prefix to store all data in redis.
        /// </summary>
        [UsedImplicitly]
        protected readonly string RedisPrefix;

        /// <summary>
        /// List of all active role nodes
        /// </summary>
        private readonly Dictionary<Address, ICanTell> activeNodes = new Dictionary<Address, ICanTell>();

        /// <summary>
        /// Loacal contact book to have addresses for every child actor
        /// </summary>
        private readonly Dictionary<string, FullChildRef> children = new Dictionary<string, FullChildRef>();

        /// <summary>
        /// Timeout to create child, before second attmept will be made
        /// </summary>
        private readonly TimeSpan createChildTimeout;

        private readonly Dictionary<IActorRef, string> localChildren = new Dictionary<IActorRef, string>();

        /// <summary>
        /// Temprorary storage for message, that are coming, while child is creating
        /// </summary>
        private readonly Dictionary<string, List<Tuple<IMessageToBusinessObjectActor, IActorRef>>> messageToChildOnCreateQueue
            = new Dictionary<string, List<Tuple<IMessageToBusinessObjectActor, IActorRef>>>();

        /// <summary>
        /// REDIS connection object
        /// </summary>
        private readonly IConnectionMultiplexer redisConnection;

        protected ClusterBusinessObjectActorSupervisor(IConnectionMultiplexer redisConnection)
        {
            var config =
                Context.System.AsInstanceOf<ExtendedActorSystem>().Provider.Deployer.Lookup(this.Self.Path).Config;
            this.createChildTimeout = config.GetTimeSpan("createChildTimeout", TimeSpan.FromSeconds(5), false);

            this.redisConnection = redisConnection;
            this.RedisPrefix = $"{string.Join(":", this.Self.Path.Elements)}:Mngmt";
            this.CurrentCluster = Cluster.Get(Context.System);

            // ReSharper disable ConvertClosureToMethodGroup
            this.Receive<ClusterEvent.MemberUp>(m => m.Member.HasRole(this.ClusterRole), m => this.OnMemberUp(m));
            this.Receive<ClusterEvent.MemberRemoved>(m => m.Member.HasRole(this.ClusterRole), m => this.OnMemberDown(m));
            this.Receive<ClusterEvent.RoleLeaderChanged>(m => m.Role == this.ClusterRole, m => this.OnRoleLeaderChanged(m));
            this.Receive<CreateChildCommand>(m => this.CreateLocalChild(m));
            this.Receive<ChildCreated>(m => this.RegisterChildRef(m));
            this.Receive<ChildRemoved>(m => this.RemoveChildRef(m));
            this.Receive<IMessageToBusinessObjectActor>(m => this.ForwardMessageToChild(m));
            this.Receive<Terminated>(m => this.OnChildTerminated(m));
            this.Receive<DeadLetter>(m => this.OnDeadLetter(m));
            this.Receive<ResetChildren>(m => this.OnResetChildren());

            // ReSharper restore ConvertClosureToMethodGroup

            this.CurrentCluster.Subscribe(this.Self,
                ClusterEvent.InitialStateAsEvents,
                new[] { typeof(ClusterEvent.IMemberEvent), typeof(ClusterEvent.RoleLeaderChanged) });
            Context.System.EventStream.Subscribe(this.Self, typeof(DeadLetter));

            this.CurrentCluster.RegisterOnMemberUp(
                () =>
                    {
                        // todo: do something after cluster initialization
                    });
        }

        /// <summary>
        /// Cluster node role name, that handles such objects.
        /// </summary>
        protected abstract string ClusterRole { get; }

        /// <summary>
        /// Gets the currents cluster.
        /// </summary>
        [UsedImplicitly]
        protected Cluster CurrentCluster { get; }

        /// <summary>
        /// Gets value, indicating that current node is role leader for now
        /// </summary>
        [UsedImplicitly]
        protected bool IsLeader { get; private set; }

        /// <summary>
        /// Gets address of role leader node
        /// </summary>
        [UsedImplicitly]
        protected ICanTell RoleLeader { get; private set; }

        /// <summary>
        /// Sends message to all registered supervisors. Even to myself.
        /// </summary>
        /// <param name="message">The message to send</param>
        [UsedImplicitly]
        protected void BroadcastSupervisorMessage(object message)
        {
            foreach (var activeNode in this.activeNodes.Select(n => n.Value))
            {
                activeNode.Tell(message, this.Self);
            }
        }

        /// <summary>
        /// Creates link to brother supervisor with known node address
        /// </summary>
        /// <param name="nodeAddress">The node address</param>
        /// <returns>The link to supervisor actor</returns>
        /// <remarks>
        /// Can be overriden in test purposes
        /// </remarks>
        [UsedImplicitly]
        protected virtual ICanTell CreateSupervisorICanTell(Address nodeAddress)
        {
            return Context.System.ActorSelection($"{nodeAddress}/{string.Join("/", this.Self.Path.Elements)}");
        }

        protected override void PostRestart(Exception reason)
        {
            base.PostRestart(reason);
            // todo: need to restart all child actors on this node
        }

        /// <summary>
        /// Is called when a message isn't handled by the current behavior of the actor
        ///             by default it fails with either a <see cref="T:Akka.Actor.DeathPactException"/> (in
        ///             case of an unhandled <see cref="T:Akka.Actor.Terminated"/> message) or publishes an <see cref="T:Akka.Event.UnhandledMessage"/>
        ///             to the actor's system's <see cref="T:Akka.Event.EventStream"/>
        /// </summary>
        /// <param name="message">The unhandled message.</param>
        protected override void Unhandled(object message)
        {
            if (message is ClusterEvent.IClusterDomainEvent)
            {
                return;
            }

            base.Unhandled(message);
        }

        /// <summary>
        /// Cpnverts Id string to string that could be safely used in Redis key names and actor names
        /// </summary>
        /// <param name="id">Original Id string</param>
        /// <returns>Safe id string</returns>
        private static string GetSafeChildId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(@"Id should be not empty or null", nameof(id));
            }
            return id.Replace('/', '-').Replace(':', '-');
        }

        /// <summary>
        /// Creates local child object actor
        /// </summary>
        /// <param name="message">Command to create child</param>
        private void CreateLocalChild(CreateChildCommand message)
        {
            var child = Context.ActorOf(Context.System.DI().Props<T>(), message.Id);
            child.Tell(new SetObjectId { Id = message.Id });
            this.localChildren[child] = message.Id;

            var db = this.redisConnection.GetDatabase();
            var keyName = this.GetChildAddressKeyName(message.Id);
            string oldChildAddressString = db.StringGetSet(keyName, child.SerializeToAkkaString(Context.System));
            if (!string.IsNullOrWhiteSpace(oldChildAddressString))
            {
                try
                {
                    var oldRef = oldChildAddressString.DeserializeFromAkkaString<IActorRef>(Context.System);
                    oldRef.Tell(PoisonPill.Instance);
                }
                catch (Exception exception)
                {
                    // ReSharper disable once FormatStringProblem
                    Context.GetLogger()
                        .Error(exception, "{Type}: error while retrieving old child address", this.GetType().Name);
                }
            }

            db.HashSet(this.GetSupervisorHashKeyName(this.CurrentCluster.SelfUniqueAddress.Uid), new[] { new HashEntry(GetSafeChildId(message.Id), message.Id) });
            Context.Watch(child);
            this.BroadcastSupervisorMessage(new ChildCreated { Id = message.Id, ChildRef = child, NodeUid = this.CurrentCluster.SelfUniqueAddress.Uid });
            // todo: duplicate stop may cause false child removal message
        }

        /// <summary>
        /// Forwarding message to object actor. If actor is not exists, it will be created.
        /// </summary>
        /// <param name="message">Message to child actor</param>
        private void ForwardMessageToChild(IMessageToBusinessObjectActor message)
        {
            FullChildRef child;
            if (this.children.TryGetValue(message.Id, out child))
            {
                child.Child.Forward(message);
            }
            else
            {
                if (this.IsLeader)
                {
                    var childCreationLockName = this.GetChildCreationLockName(message.Id);
                    var db = this.redisConnection.GetDatabase();

                    var lockAcquired = db.StringSet(
                        childCreationLockName,
                        true,
                        this.createChildTimeout,
                        When.NotExists);

                    if (lockAcquired)
                    {
                        if (!this.messageToChildOnCreateQueue.ContainsKey(message.Id))
                        {
                            this.messageToChildOnCreateQueue[message.Id] = new EditableList<Tuple<IMessageToBusinessObjectActor, IActorRef>>();
                        }

                        this.messageToChildOnCreateQueue[message.Id].Add(new Tuple<IMessageToBusinessObjectActor, IActorRef>(message, this.Sender));

                        // For now, I'll just take random cluster member, but It could be more complex algorithm
                        var rnd = new Random();
                        var memberNum = rnd.Next(0, this.activeNodes.Count);
                        var nodeToCreate = this.activeNodes.Skip(memberNum).First().Value;
                        nodeToCreate.Tell(new CreateChildCommand { Id = message.Id }, this.Self);
                    }
                    else
                    {
                        // could not manage to acquire lock, so I should wait until it will be released to try one more time
                        Context.System.Scheduler.ScheduleTellOnce(this.createChildTimeout, this.Self, message, this.Sender);
                    }
                }
                else
                {
                    this.RoleLeader.Tell(message, this.Sender);
                }
            }
        }

        private string GetChildAddressKeyName(string childId)
        {
            return $"{this.RedisPrefix}:{GetSafeChildId(childId)}:ChildAddress";
        }

        private string GetChildCreationLockName(string childId)
        {
            return $"{this.RedisPrefix}:{GetSafeChildId(childId)}:CreationLock";
        }

        private string GetSupervisorHashKeyName(int uid)
        {
            return $"{this.RedisPrefix}:Supervisor:{uid}:Children";
        }

        private void OnChildTerminated(Terminated terminated)
        {
            string childId;
            if (this.localChildren.TryGetValue(terminated.ActorRef, out childId))
            {
                this.localChildren.Remove(terminated.ActorRef);
                this.children.Remove(childId);
                var db = this.redisConnection.GetDatabase();
                db.HashDelete(
                    this.GetSupervisorHashKeyName(this.CurrentCluster.SelfUniqueAddress.Uid),
                    GetSafeChildId(childId));
                this.BroadcastSupervisorMessage(new ChildRemoved { Id = childId, NodeUid = this.CurrentCluster.SelfUniqueAddress.Uid });
            }
        }

        private void OnDeadLetter(DeadLetter deadLetter)
        {
            if (!Equals(deadLetter.Sender, this.Self))
            {
                return;
            }

            var messageToObject = deadLetter.Message as IMessageToBusinessObjectActor;

            if (messageToObject != null)
            {
                // todo: something strange happened. We need to resend this message properly
            }
        }

        /// <summary>
        /// Proccessing event of becoming role leader
        /// </summary>
        private void OnLeaderPositionAcquired()
        {
            // todo: check if some actors should be created
        }

        /// <summary>
        /// Proccessing event of loosing role leader position
        /// </summary>
        private void OnLeaderPositionLost()
        {
            foreach (var pair in this.messageToChildOnCreateQueue)
                foreach (var tuple in pair.Value)
                {
                    this.RoleLeader.Tell(tuple.Item1, tuple.Item2);
                }

            this.messageToChildOnCreateQueue.Clear();
        }

        /// <summary>
        /// Registers lost role member
        /// </summary>
        /// <param name="message">Member status change notification</param>
        private void OnMemberDown(ClusterEvent.MemberRemoved message)
        {
            this.activeNodes.Remove(message.Member.Address);
            if (this.IsLeader)
            {
                var db = this.redisConnection.GetDatabase();
                var hash = db.HashScan(this.GetSupervisorHashKeyName(message.Member.UniqueAddress.Uid));
                foreach (var hashEntry in hash)
                {
                    this.ForwardMessageToChild(new RestoreObject { Id = hashEntry.Value });
                }
            }
        }

        /// <summary>
        /// Registers new active role member
        /// </summary>
        /// <param name="message">Member status change notification</param>
        private void OnMemberUp(ClusterEvent.MemberUp message)
        {
            var node = this.CreateSupervisorICanTell(message.Member.Address);
            this.activeNodes[message.Member.Address] = node;
            if (this.IsLeader)
            {
                node.Tell(new ResetChildren(), this.Self);
            }
        }

        private void OnResetChildren()
        {
            foreach (var localChild in this.localChildren)
            {
                localChild.Key.Tell(PoisonPill.Instance);
            }

            this.localChildren.Clear();
        }

        /// <summary>
        /// Proccesses leader role leader position change notification
        /// </summary>
        /// <param name="message">The leader position change notification</param>
        private void OnRoleLeaderChanged(ClusterEvent.RoleLeaderChanged message)
        {
            bool wasLeader = this.IsLeader;
            this.IsLeader = message.Leader == this.CurrentCluster.SelfAddress;
            this.RoleLeader = this.CreateSupervisorICanTell(message.Leader);

            if (this.IsLeader)
            {
                this.OnLeaderPositionAcquired();
            }
            else if (wasLeader)
            {
                this.OnLeaderPositionLost();
            }
        }

        /// <summary>
        /// Registering newly created child in local storage
        /// </summary>
        /// <param name="childCreated">The child created data</param>
        private void RegisterChildRef(ChildCreated childCreated)
        {
            this.children[childCreated.Id] = new FullChildRef
            {
                Child = childCreated.ChildRef,
                NodeUid = childCreated.NodeUid
            };

            List<Tuple<IMessageToBusinessObjectActor, IActorRef>> messagesList;
            if (this.messageToChildOnCreateQueue.TryGetValue(childCreated.Id, out messagesList))
            {
                this.messageToChildOnCreateQueue.Remove(childCreated.Id);
                foreach (var message in messagesList)
                {
                    childCreated.ChildRef.Tell(message.Item1, message.Item2);
                }
            }

            if (this.IsLeader)
            {
                // removing child creation lock
                var db = this.redisConnection.GetDatabase();
                var childCreationLockName = this.GetChildCreationLockName(childCreated.Id);
                db.KeyDelete(childCreationLockName);
            }
        }

        private void RemoveChildRef(ChildRemoved message)
        {
            FullChildRef childRef;
            if (this.children.TryGetValue(message.Id, out childRef) && childRef.NodeUid == message.NodeUid)
            {
                this.children.Remove(message.Id);
            }
        }

        /// <summary>
        /// The command, that is sent to child actor to initialize it with id.
        /// </summary>
        [UsedImplicitly]
        public class RestoreObject : IMessageToBusinessObjectActor
        {
            [UsedImplicitly]
            public string Id { get; set; }
        }

        /// <summary>
        /// The command, that is sent to child actor to initialize it with id.
        /// </summary>
        [UsedImplicitly]
        public class SetObjectId
        {
            [UsedImplicitly]
            public string Id { get; set; }
        }

        /// <summary>
        /// Notification, that new child actor spawned
        /// </summary>
        [UsedImplicitly]
        protected class ChildCreated
        {
            public IActorRef ChildRef { get; set; }
            public string Id { get; set; }
            public int NodeUid { get; set; }
        }

        /// <summary>
        /// The command, that is sent to node supervisor in order to notifiy that child was removed from cluster
        /// </summary>
        [UsedImplicitly]
        protected class ChildRemoved
        {
            public string Id { get; set; }
            public int NodeUid { get; set; }
        }

        /// <summary>
        /// The command, that is sent to node supervisor in order to create child actor
        /// </summary>
        [UsedImplicitly]
        protected class CreateChildCommand
        {
            public string Id { get; set; }
        }

        protected class FullChildRef
        {
            public IActorRef Child { get; set; }
            public int NodeUid { get; set; }
        }

        /// <summary>
        /// Command to all newly joined nodes to flush all data
        /// </summary>
        [UsedImplicitly]
        protected class ResetChildren
        {
        }
    }
}