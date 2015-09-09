namespace TaxiKit.Core.Cluster
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Configuration;
    using Akka.DI.Core;
    using Akka.Event;
    using Akka.Routing;
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
    ///
    /// In case of network problems, messages to object can make dubles
    /// </remarks>
    public abstract class ClusterBusinessObjectActorSupervisor<T> : ReceiveActor, IWithUnboundedStash
        where T : ActorBase
    {
        /// <summary>
        /// Prefix to store all data in redis.
        /// </summary>
        [UsedImplicitly]
        public readonly string RedisPrefix;

        [UsedImplicitly]
        protected readonly Random Rnd = new Random();

        /// <summary>
        /// List of all active role nodes
        /// </summary>
        private readonly Dictionary<UniqueAddress, ICanTell> activeNodes = new Dictionary<UniqueAddress, ICanTell>();

        /// <summary>
        /// Loacal contact book to have addresses for every child actor
        /// </summary>
        private readonly Dictionary<string, FullChildRef> children = new Dictionary<string, FullChildRef>();

        /// <summary>
        /// Timeout to create child, before second attmept will be made
        /// </summary>
        private readonly TimeSpan createChildTimeout;

        private readonly Dictionary<IActorRef, string> localChildren = new Dictionary<IActorRef, string>();
        private readonly Dictionary<string, IActorRef> localChildrenById = new Dictionary<string, IActorRef>();

        /// <summary>
        /// Temprorary storage for message, that are coming, while child is creating
        /// </summary>
        private readonly Dictionary<string, List<Tuple<IMessageToBusinessObjectActor, IActorRef>>>
            messageToChildOnCreateQueue =
                new Dictionary<string, List<Tuple<IMessageToBusinessObjectActor, IActorRef>>>();

        /// <summary>
        /// REDIS connection object
        /// </summary>
        private readonly IConnectionMultiplexer redisConnection;

        private readonly IActorRef senders;

        protected ClusterBusinessObjectActorSupervisor(IConnectionMultiplexer redisConnection)
        {
            var lookup = Context.System.AsInstanceOf<ExtendedActorSystem>().Provider.Deployer.Lookup(this.Self.Path);
            var config = lookup != null ? lookup.Config : ConfigurationFactory.Empty;
            this.createChildTimeout = config.GetTimeSpan("createChildTimeout", TimeSpan.FromSeconds(5), false);

            var sendersCount = config.GetInt("sendersCount", 20);

            this.senders = Context.ActorOf(Context.System.DI().Props<SenderWorker>().WithRouter(new ConsistentHashingPool(sendersCount)), "senders");

            this.redisConnection = redisConnection;
            this.RedisPrefix = $"{string.Join(":", this.Self.Path.Elements)}:Mngmt";
            this.CurrentCluster = Cluster.Get(Context.System);

            this.UnClusteredMessageProccess();

            this.CurrentCluster.Subscribe(
                this.Self,
                ClusterEvent.InitialStateAsEvents,
                new[] { typeof(ClusterEvent.IMemberEvent), typeof(ClusterEvent.RoleLeaderChanged) });
        }

        public IStash Stash { get; set; }

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
        /// Selects one of the registered cluster nodes (among <see cref="activeNodes"/>) to create child node
        /// </summary>
        /// <param name="id">Identification string of object</param>
        /// <returns>The reference to supervisor to place a new child</returns>
        protected virtual ICanTell SelectNodeToPlaceChild(string id)
        {
            var memberNum = Rnd.Next(0, this.activeNodes.Count);
            var nodeToCreate = this.activeNodes.Skip(memberNum).First().Value;
            return nodeToCreate;
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
        /// Rules to proccess mesages in fully formed cluster
        /// </summary>
        private void ClusteredMessageProccess()
        {
            // ReSharper disable ConvertClosureToMethodGroup
            this.Receive<ClusterEvent.MemberUp>(m => m.Member.HasRole(this.ClusterRole), m => this.OnMemberUp(m));
            this.Receive<ClusterEvent.MemberRemoved>(m => m.Member.HasRole(this.ClusterRole), m => this.OnMemberDown(m));
            this.Receive<ClusterEvent.RoleLeaderChanged>(
                m => m.Role == this.ClusterRole,
                m => this.OnRoleLeaderChanged(m));
            this.Receive<CreateChildCommand>(m => this.CreateLocalChild(m));
            this.Receive<ChildCreated>(m => this.RegisterChildRef(m));
            this.Receive<ChildRemoved>(m => this.RemoveChildRef(m));
            this.Receive<EnvelopeToReceiver>(m => this.OnEnvelopeToReceiver(m));
            this.Receive<IMessageToBusinessObjectActor>(m => this.ForwardMessageToChild(m));
            this.Receive<Terminated>(m => this.OnChildTerminated(m));
            this.Receive<ResetChildren>(m => this.OnResetChildren());
            // ReSharper restore ConvertClosureToMethodGroup
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
            this.localChildrenById[message.Id] = child;

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

            db.HashSet(
                this.GetSupervisorHashKeyName(this.CurrentCluster.SelfUniqueAddress.Uid),
                new[] { new HashEntry(GetSafeChildId(message.Id), message.Id) });
            Context.Watch(child);
            this.BroadcastSupervisorMessage(
                new ChildCreated
                {
                    Id = message.Id,
                    NodeAddress = this.CurrentCluster.SelfUniqueAddress,
                    NodeUid = this.CurrentCluster.SelfUniqueAddress.Uid
                });
            // todo: duplicate stop may cause false child removal message
        }

        /// <summary>
        /// Forwarding message to object actor. If actor is not exists, it will be created.
        /// </summary>
        /// <param name="message">Message to child actor</param>
        /// <param name="sender">Override of original message sender</param>
        private void ForwardMessageToChild(IMessageToBusinessObjectActor message, IActorRef sender = null)
        {
            if (sender == null)
            {
                sender = this.Sender;
            }

            FullChildRef child;
            IActorRef localChild;
            if (this.localChildrenById.TryGetValue(message.Id, out localChild))
            {
                localChild.Tell(message, sender);
            }
            else if (this.children.TryGetValue(message.Id, out child) && this.activeNodes.ContainsKey(child.NodeAddress))
            {
                this.senders.Tell(new EnvelopeToSender { Message = message, Receiver = this.activeNodes[child.NodeAddress], Sender = sender });
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
                            this.messageToChildOnCreateQueue[message.Id] =
                                new EditableList<Tuple<IMessageToBusinessObjectActor, IActorRef>>();
                        }

                        this.messageToChildOnCreateQueue[message.Id].Add(
                            new Tuple<IMessageToBusinessObjectActor, IActorRef>(message, sender));

                        // For now, I'll just take random cluster member, but It could be more complex algorithm
                        var nodeToCreate = this.SelectNodeToPlaceChild(message.Id);
                        nodeToCreate.Tell(new CreateChildCommand { Id = message.Id }, this.Self);
                    }
                    else
                    {
                        // could not manage to acquire lock, so I should wait until it will be released to try one more time
                        Context.System.Scheduler.ScheduleTellOnce(
                            this.createChildTimeout,
                            this.Self,
                            message,
                            sender);
                    }
                }
                else
                {
                    this.senders.Tell(new EnvelopeToSender { Message = message, Receiver = this.RoleLeader, Sender = sender });
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
                this.localChildrenById.Remove(childId);
                this.children.Remove(childId);
                var db = this.redisConnection.GetDatabase();
                db.HashDelete(
                    this.GetSupervisorHashKeyName(this.CurrentCluster.SelfUniqueAddress.Uid),
                    GetSafeChildId(childId));
                this.BroadcastSupervisorMessage(
                    new ChildRemoved { Id = childId, NodeUid = this.CurrentCluster.SelfUniqueAddress.Uid });
            }
        }

        private void OnEnvelopeToReceiver(EnvelopeToReceiver envelopeToReceiver)
        {
            this.Sender.Tell(true);
            this.ForwardMessageToChild(envelopeToReceiver.Message, envelopeToReceiver.Sender);
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
            this.activeNodes.Remove(message.Member.UniqueAddress);
            if (this.IsLeader)
            {
                var db = this.redisConnection.GetDatabase();
                var hash = db.HashScan(this.GetSupervisorHashKeyName(message.Member.UniqueAddress.Uid));
                foreach (var hashEntry in hash)
                {
                    this.children.Remove(hashEntry.Name);
                    this.ForwardMessageToChild(new RestoreObject { Id = hashEntry.Name });
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
            this.activeNodes[message.Member.UniqueAddress] = node;
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
            this.localChildrenById.Clear();
        }

        /// <summary>
        /// Proccesses leader role leader position change notification
        /// </summary>
        /// <param name="message">The leader position change notification</param>
        private void OnRoleLeaderChanged(ClusterEvent.RoleLeaderChanged message)
        {
            if (message.Leader == null)
            {
                if (this.RoleLeader == null)
                {
                    return;
                }

                this.OnResetChildren();
                this.IsLeader = false;
                this.children.Clear();
                this.activeNodes.Clear();
                this.Become(this.UnClusteredMessageProccess);
            }
            else
            {
                // just joined th cluster
                if (this.RoleLeader == null)
                {
                    this.Become(this.ClusteredMessageProccess);
                    this.Stash.UnstashAll();
                }

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
        }

        /// <summary>
        /// Registering newly created child in local storage
        /// </summary>
        /// <param name="childCreated">The child created data</param>
        private void RegisterChildRef(ChildCreated childCreated)
        {
            ICanTell node;
            if (!this.activeNodes.TryGetValue(childCreated.NodeAddress, out node))
            {
                return;
            }

            this.children[childCreated.Id] = new FullChildRef
            {
                NodeAddress = childCreated.NodeAddress,
                NodeUid = childCreated.NodeUid
            };

            List<Tuple<IMessageToBusinessObjectActor, IActorRef>> messagesList;
            if (this.messageToChildOnCreateQueue.TryGetValue(childCreated.Id, out messagesList))
            {
                this.messageToChildOnCreateQueue.Remove(childCreated.Id);
                foreach (var message in messagesList)
                {
                    node.Tell(message.Item1, message.Item2);
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
        /// Rules to proccess messages before cluster completes it's forming
        /// </summary>
        private void UnClusteredMessageProccess()
        {
            // ReSharper disable ConvertClosureToMethodGroup
            this.Receive<ClusterEvent.MemberUp>(m => m.Member.HasRole(this.ClusterRole), m => this.OnMemberUp(m));
            this.Receive<ClusterEvent.MemberRemoved>(m => m.Member.HasRole(this.ClusterRole), m => this.OnMemberDown(m));
            this.Receive<ClusterEvent.RoleLeaderChanged>(
                m => m.Role == this.ClusterRole,
                m => this.OnRoleLeaderChanged(m));
            this.Receive<object>(m => this.Stash.Stash());
            // ReSharper restore ConvertClosureToMethodGroup
        }

        /// <summary>
        /// Notification, that new child actor spawned
        /// </summary>
        [UsedImplicitly]
        public class ChildCreated
        {
            public string Id { get; set; }
            public UniqueAddress NodeAddress { get; set; }
            public int NodeUid { get; set; }
        }

        /// <summary>
        /// The command, that is sent to node supervisor in order to notifiy that child was removed from cluster
        /// </summary>
        [UsedImplicitly]
        public class ChildRemoved
        {
            public string Id { get; set; }

            public int NodeUid { get; set; }
        }

        /// <summary>
        /// The command, that is sent to node supervisor in order to create child actor
        /// </summary>
        [UsedImplicitly]
        public class CreateChildCommand
        {
            public string Id { get; set; }
        }

        public class EnvelopeToReceiver
        {
            public IMessageToBusinessObjectActor Message { get; set; }
            public IActorRef Sender { get; set; }
        }

        public class EnvelopeToSender : IConsistentHashable
        {
            public object ConsistentHashKey => this.Message == null ? null : this.Message.Id;

            public IMessageToBusinessObjectActor Message { get; set; }
            public ICanTell Receiver { get; set; }
            public IActorRef Sender { get; set; }
        }

        public class FullChildRef
        {
            public UniqueAddress NodeAddress { get; set; }

            public int NodeUid { get; set; }
        }

        /// <summary>
        /// Command to all newly joined nodes to flush all data
        /// </summary>
        [UsedImplicitly]
        public class ResetChildren
        {
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
        /// Worker to send messages and verify there receive
        /// </summary>
        public class SenderWorker : ReceiveActor
        {
            private readonly TimeSpan nextAttmeptPause;
            private readonly TimeSpan sendTimeOut;
            private readonly ICanTell supervisor;

            public SenderWorker()
            {
                var lookup = Context.System.AsInstanceOf<ExtendedActorSystem>().Provider.Deployer.Lookup(this.Self.Path.Parent.Parent);
                var config = lookup != null ? lookup.Config : ConfigurationFactory.Empty;
                this.sendTimeOut = config.GetTimeSpan("sendTimeOut", TimeSpan.FromSeconds(1), false);
                this.nextAttmeptPause = config.GetTimeSpan("nextAttmeptPause", TimeSpan.FromSeconds(3), false);
                this.supervisor = Context.ActorSelection(this.Self.Path.Parent.Parent);
                this.Receive<EnvelopeToSender>(m => this.SendEnvelope(m));
            }

            private async Task SendEnvelope(EnvelopeToSender envelope)
            {
                try
                {
                    await envelope.Receiver.Ask(new EnvelopeToReceiver
                    {
                        Message = envelope.Message,
                        Sender = envelope.Sender
                    }, this.sendTimeOut);
                }
                catch (Exception)
                {
                    Context.System.Scheduler.ScheduleTellOnce(this.nextAttmeptPause, this.supervisor, envelope.Message, this.Self);
                    Context.GetLogger().Info("Message send failed, resending {message}...", envelope.Message.ToString());
                }
            }
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
    }
}