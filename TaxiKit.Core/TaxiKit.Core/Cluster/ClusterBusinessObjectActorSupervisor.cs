// ReSharper disable FormatStringProblem
namespace TaxiKit.Core.Cluster
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

    using TaxiKit.Core.Cluster.Messages;
    using TaxiKit.Core.Utils;

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
        /// Local contact book to have addresses for every child actor
        /// </summary>
        private readonly Dictionary<string, UniqueAddress> children = new Dictionary<string, UniqueAddress>();

        private readonly Config config;

        /// <summary>
        /// Timeout to create child, before second attmept will be made
        /// </summary>
        private readonly TimeSpan createChildTimeout;

        private readonly Dictionary<IActorRef, string> localChildren = new Dictionary<IActorRef, string>();
        private readonly Dictionary<string, IActorRef> localChildrenById = new Dictionary<string, IActorRef>();

        /// <summary>
        /// Temprorary storage for message, that are coming, while child is creating
        /// </summary>
        private readonly Dictionary<string, List<EnvelopeToReceiver>>
            messageToChildOnCreateQueue =
                new Dictionary<string, List<EnvelopeToReceiver>>();

        /// <summary>
        /// REDIS connection object
        /// </summary>
        private readonly IConnectionMultiplexer redisConnection;

        private IActorRef senders;

        protected ClusterBusinessObjectActorSupervisor(IConnectionMultiplexer redisConnection)
        {
            var lookup = Context.System.AsInstanceOf<ExtendedActorSystem>().Provider.Deployer.Lookup(this.Self.Path);
            this.config = lookup != null ? lookup.Config : ConfigurationFactory.Empty;
            this.createChildTimeout = this.config.GetTimeSpan("createChildTimeout", TimeSpan.FromSeconds(5), false);

            this.redisConnection = redisConnection;
            this.RedisPrefix = $"{string.Join(":", this.Self.Path.Elements)}:Mngmt";
            this.CurrentCluster = Cluster.Get(Context.System);
        }

        /// <summary>
        /// Gets value, indincating, that current cluster state was recieved and cluster is in helathy state
        /// </summary>
        public bool IsClusterInitizlized { get; private set; }

        /// <summary>
        /// Gets value, indicating that all init data after start or restart was recieved and successfully proccessed
        /// </summary>
        public bool IsDataInitizlized { get; private set; }

        /// <summary>
        /// Gets or sets the stash. This will be automatically populated by the framework AFTER the constructor has been run.
        ///             Implement this as an auto property.
        /// </summary>
        /// <value>
        /// The stash.
        /// </value>
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
        /// Rules to proccess mesages in fully formed cluster
        /// </summary>
        protected virtual void ClusteredMessageProccess()
        {
            // ReSharper disable ConvertClosureToMethodGroup
            this.Receive<ClusterEvent.MemberUp>(m => m.Member.HasRole(this.ClusterRole), m => this.OnClusterMemberUp(m.Member));
            this.Receive<ClusterEvent.MemberRemoved>(m => m.Member.HasRole(this.ClusterRole), m => this.OnClusterMemberDown(m));
            this.Receive<ClusterEvent.RoleLeaderChanged>(
                m => m.Role == this.ClusterRole,
                m => this.OnClusterRoleLeaderChanged(m.Leader));
            this.Receive<CreateChildCommand>(m => this.CreateLocalChild(m, false));
            this.Receive<ChildCreated>(m => this.RegisterChildRef(m));
            this.Receive<ChildRemoved>(m => this.RemoveChildRef(m));
            this.Receive<EnvelopeToReceiver>(m => this.OnEnvelopeToReceiver(m));
            this.Receive<IMessageToBusinessObjectActor>(m => this.ForwardMessageToChild(m));
            this.Receive<Terminated>(m => this.OnChildTerminated(m));
            this.Receive<ResetChildren>(m => this.OnResetChildren());
            // ReSharper restore ConvertClosureToMethodGroup
        }

        /// <summary>
        /// Actor is sure that it joined the cluster and can start normal work
        /// </summary>
        protected virtual void ClusterJoined()
        {
            this.IsClusterInitizlized = true;
            this.TryStartNormalWork();
        }

        /// <summary>
        /// Actor have lost cluster connection and should terminate all work
        /// </summary>
        protected virtual void ClusterLost()
        {
            this.OnResetChildren();
            this.IsLeader = false;
            this.children.Clear();
            this.activeNodes.Clear();
            this.Become(this.UnClusteredMessageProccess);
            var db = this.redisConnection.GetDatabase();
            db.KeyDelete(this.GetSupervisorHashKeyName(this.CurrentCluster.SelfUniqueAddress.Uid));
        }

        /// <summary>
        /// Creates local child object actor
        /// </summary>
        /// <param name="message">Command to create child</param>
        /// <param name="isInititProccess">Value, indicates that method was called during actor initialization proccess</param>
        protected virtual void CreateLocalChild(CreateChildCommand message, bool isInititProccess)
        {
            var child = Context.ActorOf(Context.System.DI().Props<T>());
            child.Tell(new SetObjectId { Id = message.Id });
            this.localChildren[child] = message.Id;
            this.localChildrenById[message.Id] = child;

            Context.GetLogger().Info("Creating child {0}", message.Id);

            var db = this.redisConnection.GetDatabase();
            var keyName = this.GetChildAddressKeyName(message.Id);
            string oldChildAddressString = db.StringGetSet(keyName, child.SerializeToAkkaString(Context.System));
            if (!string.IsNullOrWhiteSpace(oldChildAddressString))
            {
                try
                {
                    var oldRef = oldChildAddressString.DeserializeFromAkkaString<IActorRef>(Context.System);
                    if (!Equals(oldRef, child))
                    {
                        oldRef.Tell(PoisonPill.Instance);
                    }
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

            if (!isInititProccess)
            {
                this.BroadcastSupervisorMessage(
                    new ChildCreated { Id = message.Id, NodeAddress = this.CurrentCluster.SelfUniqueAddress });
            }
            // todo: duplicate stop may cause false child removal message
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

        /// <summary>
        /// Forwarding message to object actor. If actor is not exists, it will be created.
        /// </summary>
        /// <param name="message">Message to child actor</param>
        /// <param name="sender">Override of original message sender</param>
        protected virtual void ForwardMessageToChild(IMessageToBusinessObjectActor message, IActorRef sender = null)
        {
            if (string.IsNullOrWhiteSpace(message.Id))
            {
                return;
            }

            if (sender == null)
            {
                sender = this.Sender;
            }

            UniqueAddress nodeAddress;
            IActorRef localChild;
            if (this.localChildrenById.TryGetValue(message.Id, out localChild))
            {
                localChild.Tell(message, sender);
            }
            else if (this.children.TryGetValue(message.Id, out nodeAddress) && this.activeNodes.ContainsKey(nodeAddress))
            {
                this.senders.Tell(
                    new EnvelopeToSender
                    {
                        Message = message,
                        Receiver = this.activeNodes[nodeAddress],
                        Sender = sender
                    });
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
                            this.messageToChildOnCreateQueue[message.Id] = new EditableList<EnvelopeToReceiver>();
                        }

                        this.messageToChildOnCreateQueue[message.Id].Add(
                            new EnvelopeToReceiver { Message = message, Sender = sender });

                        // For now, I'll just take random cluster member, but It could be more complex algorithm
                        Context.GetLogger().Info("Ordering to create child {0} because of {1}", message.Id, message);
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
                    this.senders.Tell(
                        new EnvelopeToSender { Message = message, Receiver = this.RoleLeader, Sender = sender });
                }
            }
        }

        protected virtual string GetChildAddressKeyName(string childId)
        {
            return $"{this.RedisPrefix}:{GetSafeChildId(childId)}:ChildAddress";
        }

        protected virtual string GetChildCreationLockName(string childId)
        {
            return $"{this.RedisPrefix}:{GetSafeChildId(childId)}:CreationLock";
        }

        protected virtual string GetSupervisorHashKeyName(int uid)
        {
            return $"{this.RedisPrefix}:Supervisor:{uid}:Children";
        }

        protected virtual void OnChildTerminated(Terminated terminated)
        {
            string childId;
            if (this.localChildren.TryGetValue(terminated.ActorRef, out childId))
            {
                Context.GetLogger().Info("!! Child {child} terminated, {0}", childId, this.localChildren.Count);

                this.localChildren.Remove(terminated.ActorRef);
                this.localChildrenById.Remove(childId);

                Context.GetLogger().Info("!! Child {child} removed, {0}", childId, this.localChildren.Count);
                this.children.Remove(childId);
                var db = this.redisConnection.GetDatabase();
                db.HashDelete(
                    this.GetSupervisorHashKeyName(this.CurrentCluster.SelfUniqueAddress.Uid),
                    GetSafeChildId(childId));
                this.BroadcastSupervisorMessage(
                    new ChildRemoved { Id = childId, NodeAddress = this.CurrentCluster.SelfUniqueAddress });
            }
        }

        /// <summary>
        /// Registers lost role member
        /// </summary>
        /// <param name="message">Member status change notification</param>
        protected virtual void OnClusterMemberDown(ClusterEvent.MemberRemoved message, bool isClusterInit = false)
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
        /// <param name="member">The member</param>
        /// <param name="isClusterInit">Was called as part of startup initialization process</param>
        protected virtual void OnClusterMemberUp(Member member, bool isClusterInit = false)
        {
            var node = this.CreateSupervisorICanTell(member.Address);
            this.activeNodes[member.UniqueAddress] = node;

            if (isClusterInit)
            {
                return;
            }

            if (this.IsLeader)
            {
                node.Tell(new ResetChildren(), this.Self);
            }

            foreach (var child in this.localChildrenById.Keys)
            {
                node.Tell(new ChildCreated
                {
                    Id = child,
                    NodeAddress = this.CurrentCluster.SelfUniqueAddress
                }, this.Self);
            }
        }

        /// <summary>
        /// Proccesses leader role leader position change notification
        /// </summary>
        /// <param name="leaderAddress">The new leader address</param>
        protected virtual void OnClusterRoleLeaderChanged(Address leaderAddress)
        {
            if (leaderAddress == null)
            {
                if (this.RoleLeader == null)
                {
                    return;
                }

                this.ClusterLost();
            }
            else
            {
                // just joined the cluster
                if (this.RoleLeader == null)
                {
                    this.ClusterJoined();
                }

                bool wasLeader = this.IsLeader;
                this.IsLeader = leaderAddress == this.CurrentCluster.SelfAddress;
                this.RoleLeader = this.CreateSupervisorICanTell(leaderAddress);

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

        protected virtual void OnClusterState(ClusterEvent.CurrentClusterState currentClusterState)
        {
            foreach (var member in currentClusterState.Members.Where(m => m.Roles.Any(r => r == this.ClusterRole)))
            {
                this.OnClusterMemberUp(member, true);
            }

            this.OnClusterRoleLeaderChanged(currentClusterState.RoleLeader(this.ClusterRole));
        }

        protected virtual void OnEnvelopeToReceiver(EnvelopeToReceiver envelopeToReceiver)
        {
            this.Sender.Tell(true);
            this.ForwardMessageToChild(envelopeToReceiver.Message, envelopeToReceiver.Sender);
        }

        protected virtual void OnInitializationDataReceived(InitializationData initializationData)
        {
            if (initializationData.AddressBook != null && initializationData.LocalChildren != null)
            {
                this.IsDataInitizlized = true;
                this.children.Clear();
                this.localChildren.Clear();
                this.localChildrenById.Clear();

                initializationData.LocalChildren.ForEach(
                    c =>
                        {
                            this.localChildren.Add(c.Value, c.Key);
                            this.localChildrenById.Add(c.Key, c.Value);
                        });

                initializationData.AddressBook.ForEach(c => this.children.Add(c.Key, c.Value));

                this.TryStartNormalWork();
            }
        }

        /// <summary>
        /// Proccessing event of becoming role leader
        /// </summary>
        protected virtual void OnLeaderPositionAcquired()
        {
            // todo: check if some actors should be created
        }

        /// <summary>
        /// Proccessing event of loosing role leader position
        /// </summary>
        protected virtual void OnLeaderPositionLost()
        {
            foreach (var tuple in this.messageToChildOnCreateQueue.SelectMany(pair => pair.Value))
            {
                this.Sender.Tell(
                    new EnvelopeToSender { Message = tuple.Message, Sender = tuple.Sender, Receiver = this.RoleLeader });
            }

            this.messageToChildOnCreateQueue.Clear();
        }

        protected virtual void OnResetChildren()
        {
            foreach (var localChild in this.localChildren)
            {
                localChild.Key.Tell(PoisonPill.Instance);
            }

            this.localChildren.Clear();
            this.localChildrenById.Clear();
        }

        /// <summary>
        /// User overridable callback: By default it calls `preStart()`.
        ///                 <p/>
        ///                 Is called right AFTER restart on the newly created Actor to allow reinitialization after an Actor crash.
        /// </summary>
        /// <param name="reason">the Exception that caused the restart to happen.</param>
        protected override void PostRestart(Exception reason)
        {
            this.senders = Context.Child("senders");
            this.Become(this.UnClusteredMessageProccess);
            this.CurrentCluster.Unsubscribe(this.Self);
        }

        protected override void PreRestart(Exception reason, object message)
        {
            this.Self.Tell(new InitializationData
            {
                AddressBook = this.children,
                LocalChildren = this.localChildrenById
            });

            // base.PreRestart(reason, message);
        }

        /// <summary>
        /// User overridable callback.
        ///                 <p/>
        ///                 Is called when an Actor is started.
        ///                 Actors are automatically started asynchronously when created.
        ///                 Empty default implementation.
        /// </summary>
        protected override void PreStart()
        {
            base.PreStart();

            this.CurrentCluster.Subscribe(
                this.Self,
                ClusterEvent.InitialStateAsSnapshot,
                new[] { typeof(ClusterEvent.IMemberEvent), typeof(ClusterEvent.RoleLeaderChanged) });

            var sendersCount = this.config.GetInt("sendersCount", 20);
            this.senders =
                Context.ActorOf(
                    Props.Create<SenderWorker>().WithRouter(new ConsistentHashingPool(sendersCount)),
                    "senders");
            this.Become(this.UnClusteredMessageProccess);

            this.OnInitializationDataReceived(
                new InitializationData
                {
                    AddressBook = new Dictionary<string, UniqueAddress>(),
                    LocalChildren = new Dictionary<string, IActorRef>()
                });
        }

        /// <summary>
        /// Registering newly created child in local storage
        /// </summary>
        /// <param name="childCreated">The child created data</param>
        protected virtual void RegisterChildRef(ChildCreated childCreated)
        {
            ICanTell node;
            if (!this.activeNodes.TryGetValue(childCreated.NodeAddress, out node))
            {
                return;
            }

            this.children[childCreated.Id] = childCreated.NodeAddress;

            List<EnvelopeToReceiver> messagesList;
            if (this.messageToChildOnCreateQueue.TryGetValue(childCreated.Id, out messagesList))
            {
                this.messageToChildOnCreateQueue.Remove(childCreated.Id);
                foreach (var message in messagesList)
                {
                    this.senders.Tell(new EnvelopeToSender
                    {
                        Message = message.Message,
                        Sender = message.Sender,
                        Receiver = node
                    });
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

        protected virtual void RemoveChildRef(ChildRemoved message)
        {
            UniqueAddress childRef;
            if (this.children.TryGetValue(message.Id, out childRef) && childRef == message.NodeAddress)
            {
                this.children.Remove(message.Id);
            }
        }

        /// <summary>
        /// Selects one of the registered cluster nodes (among <see cref="activeNodes"/>) to create child node
        /// </summary>
        /// <param name="id">Identification string of object</param>
        /// <returns>The reference to supervisor to place a new child</returns>
        protected virtual ICanTell SelectNodeToPlaceChild(string id)
        {
            var memberNum = this.Rnd.Next(0, this.activeNodes.Count);
            var nodeToCreate = this.activeNodes.Skip(memberNum).First().Value;
            return nodeToCreate;
        }

        /// <summary>
        /// Rules to proccess messages before cluster completes it's forming
        /// </summary>
        protected virtual void UnClusteredMessageProccess()
        {
            // ReSharper disable ConvertClosureToMethodGroup
            this.Receive<InitializationData>(m => this.OnInitializationDataReceived(m));
            this.Receive<ClusterEvent.CurrentClusterState>(m => this.OnClusterState(m));
            this.Receive<ClusterEvent.MemberUp>(m => m.Member.HasRole(this.ClusterRole), m => this.OnClusterMemberUp(m.Member));
            this.Receive<ClusterEvent.MemberRemoved>(m => m.Member.HasRole(this.ClusterRole), m => this.OnClusterMemberDown(m));
            this.Receive<ClusterEvent.RoleLeaderChanged>(
                m => m.Role == this.ClusterRole,
                m => this.OnClusterRoleLeaderChanged(m.Leader));
            this.Receive<object>(m => this.Stash.Stash());
            // ReSharper restore ConvertClosureToMethodGroup
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
        /// Converts Id string to string that could be safely used in Redis key names and actor names
        /// </summary>
        /// <param name="id">Original Id string</param>
        /// <returns>Safe id string</returns>
        private static string GetSafeChildId([NotNull] string id)
        {
            return id.Replace('/', '-').Replace(':', '-');
        }

        private void TryStartNormalWork()
        {
            if (this.IsDataInitizlized && this.IsClusterInitizlized)
            {
                this.Become(this.ClusteredMessageProccess);
                this.Stash.UnstashAll();
            }
        }

        /// <summary>
        /// Message to restore cuurent data in case of supervisor restart
        /// </summary>
        protected class InitializationData
        {
            public Dictionary<string, UniqueAddress> AddressBook { get; set; }
            public Dictionary<string, IActorRef> LocalChildren { get; set; }
        }

        /// <summary>
        /// Worker to send messages and verify there receive
        /// </summary>
        [UsedImplicitly]
        protected class SenderWorker : ReceiveActor
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
    }
}