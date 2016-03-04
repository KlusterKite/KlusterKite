namespace ClusterKit.Extensions.Tests.BusinessObjects
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Threading;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Configuration;
    using Akka.DI.Core;
    using Akka.Event;
    using Akka.TestKit;
    using Akka.Util.Internal;

    using Castle.MicroKernel.Registration;
    using Castle.Windsor;

    using ClusterKit.BusinessObjects;
    using ClusterKit.BusinessObjects.Messages;
    using ClusterKit.Core;
    using ClusterKit.Core.TestKit;
    using ClusterKit.Core.Utils;
    using ClusterKit.Extensions.Tests.Moq;

    using JetBrains.Annotations;

    using StackExchange.Redis;

    using Xunit;
    using Xunit.Abstractions;

    public class ClusterBusinessObjectActorSupervisorTest : BaseActorTest<ClusterBusinessObjectActorSupervisorTest.Configurator>
    {
        private readonly ConcurrentBag<EchoMessage> recievedEchoMessages = new ConcurrentBag<EchoMessage>();

        public ClusterBusinessObjectActorSupervisorTest(ITestOutputHelper output)
                    : base(output)
        {
            this.WindsorContainer.Register(Component.For<TestSupervisorActor>().LifestyleTransient());
            this.WindsorContainer.Register(Component.For<TestObjectActor>().LifestyleTransient());
            this.WindsorContainer.Register(Component.For<ITestOutputHelper>().Instance(output));
            this.WindsorContainer.Register(Component.For<ConcurrentBag<EchoMessage>>().Instance(this.recievedEchoMessages));
        }

        [Fact]
        public void ChildIsDeadAndRestoreTest()
        {
            var redis = new ConcurrentDictionary<string, object>();
            this.WinsorBind<IConnectionMultiplexer>(() => new RedisConnectionMoq(redis));

            this.SetAutoPilot(
                new DelegateAutoPilot(
                    delegate (IActorRef sender, object message)
                        {
                            if (message is EnvelopeToReceiver)
                            {
                                sender.Tell(true);
                            }

                            return AutoPilot.KeepRunning;
                        }));

            var superVisor = this.ActorOfAsTestActorRef<TestSupervisorActor>(Akka.DI.Core.Extensions.DI((ActorSystem)this.Sys).Props<TestSupervisorActor>());

            superVisor.Tell(
                new ClusterEvent.MemberUp(
                    Member.Create(
                        Cluster.Get(this.Sys).SelfUniqueAddress,
                        MemberStatus.Up,
                        ImmutableHashSet.Create("test"))));
            superVisor.Tell(new ClusterEvent.RoleLeaderChanged("test", Cluster.Get(this.Sys).SelfAddress));

            superVisor.Tell(
                new ClusterEvent.MemberUp(
                    Member.Create(
                        new UniqueAddress(
                            Cluster.Get(this.Sys).SelfAddress.WithPort(Cluster.Get(this.Sys).SelfAddress.Port + 1),
                            1),
                        MemberStatus.Up,
                        ImmutableHashSet.Create("test"))));

            this.ExpectMsg<ResetChildren>();

            var echoMessage = new EchoMessage { Id = "1", Text = "Hello 1-1" };
            superVisor.Tell(echoMessage);
            var created = this.ExpectMsg<string>();
            Assert.Equal((string)"Created 1", (string)created);
            Assert.Equal("1", this.ExpectMsg<ChildCreated>().Id);
            var response = this.ExpectMsg<EchoMessage>();
            Assert.Equal(echoMessage, response);
            Assert.True((bool)response.FromObjectActor);

            superVisor.Tell(new SniperShot { Id = "1" });
            this.ExpectMsg<ChildRemoved>(TimeSpan.FromMilliseconds(150));

            echoMessage = new EchoMessage { Id = "1", Text = "Hello 1-2" };
            superVisor.Tell(echoMessage);
            created = this.ExpectMsg<string>();
            Assert.Equal((string)"Created 1", (string)created);
            Assert.Equal("1", this.ExpectMsg<ChildCreated>().Id);
            response = this.ExpectMsg<EchoMessage>();
            Assert.Equal(echoMessage, response);
            Assert.True((bool)response.FromObjectActor);
        }

        [Fact]
        public void ChildMigrationTest()
        {
            var redis = new ConcurrentDictionary<string, object>();
            this.WinsorBind<IConnectionMultiplexer>(() => new RedisConnectionMoq(redis));

            var superVisor = this.ActorOfAsTestActorRef<TestSupervisorActor>(this.Sys.DI().Props<TestSupervisorActor>());

            superVisor.Tell(
                new ClusterEvent.MemberUp(
                    Member.Create(
                        Cluster.Get(this.Sys).SelfUniqueAddress,
                        MemberStatus.Up,
                        ImmutableHashSet.Create("test"))));
            superVisor.Tell(new ClusterEvent.RoleLeaderChanged("test", Cluster.Get(this.Sys).SelfAddress));

            var secondNodeAddress =
                new UniqueAddress(
                    Cluster.Get(this.Sys).SelfAddress.WithPort(Cluster.Get(this.Sys).SelfAddress.Port + 1),
                    1);
            superVisor.Tell(
                new ClusterEvent.MemberUp(
                    Member.Create(secondNodeAddress, MemberStatus.Up, ImmutableHashSet.Create("test"))));

            this.ExpectMsg<ResetChildren>();

            superVisor.UnderlyingActor.SelectNodeToPlaceChildOverride = s => this.TestActor;

            var echoMessage = new EchoMessage { Id = "1", Text = "Hello 1-1" };
            superVisor.Tell(echoMessage);

            Assert.Equal(
                "1",
                this.ExpectMsg<CreateChildCommand>().Id);
            redis[$"{superVisor.UnderlyingActor.RedisPrefix}:Supervisor:1:Children"] =
                new Dictionary<string, string> { { "1", this.TestActor.SerializeToAkkaString(this.Sys) } };
            superVisor.Tell(
                new ChildCreated
                {
                    NodeAddress = secondNodeAddress,
                    Id = "1"
                });

            var response = this.ExpectMsg<EnvelopeToReceiver>();
            Assert.Equal(echoMessage, response.Message);
            Assert.False(response.Message.AsInstanceOf<EchoMessage>().FromObjectActor);

            superVisor.Tell(
                new ClusterEvent.MemberRemoved(
                    Member.Create(secondNodeAddress, MemberStatus.Removed, ImmutableHashSet.Create("test")),
                    MemberStatus.Up));

            Assert.Equal(
                "1",
                this.ExpectMsg<CreateChildCommand>().Id);
        }

        [Fact]
        public void ChildRestartTest()
        {
            var redis = new ConcurrentDictionary<string, object>();
            this.WinsorBind<IConnectionMultiplexer>(() => new RedisConnectionMoq(redis));

            this.SetAutoPilot(
                new DelegateAutoPilot(
                    delegate (IActorRef sender, object message)
                    {
                        if (message is EnvelopeToReceiver)
                        {
                            sender.Tell(true);
                        }

                        return AutoPilot.KeepRunning;
                    }));

            var superVisor = this.ActorOfAsTestActorRef<TestSupervisorActor>(this.Sys.DI().Props<TestSupervisorActor>());

            superVisor.Tell(
                new ClusterEvent.MemberUp(
                    Member.Create(
                        Cluster.Get(this.Sys).SelfUniqueAddress,
                        MemberStatus.Up,
                        ImmutableHashSet.Create("test"))));
            superVisor.Tell(new ClusterEvent.RoleLeaderChanged("test", Cluster.Get(this.Sys).SelfAddress));

            superVisor.Tell(
                new ClusterEvent.MemberUp(
                    Member.Create(
                        new UniqueAddress(
                            Cluster.Get(this.Sys).SelfAddress.WithPort(Cluster.Get(this.Sys).SelfAddress.Port + 1),
                            1),
                        MemberStatus.Up,
                        ImmutableHashSet.Create("test"))));

            this.ExpectMsg<ResetChildren>();

            var echoMessage = new EchoMessage { Id = "1", Text = "Hello 1-1" };
            superVisor.Tell(echoMessage);
            var created = this.ExpectMsg<string>();
            Assert.Equal((string)"Created 1", (string)created);
            Assert.Equal("1", this.ExpectMsg<ChildCreated>().Id);
            var response = this.ExpectMsg<EchoMessage>();
            Assert.Equal(echoMessage, response);
            Assert.True((bool)response.FromObjectActor);

            superVisor.Tell(new Vomitive { Id = "1" });
            created = this.ExpectMsg<string>();
            Assert.Equal((string)"Created 1", (string)created);

            echoMessage = new EchoMessage { Id = "1", Text = "Hello 1-2" };
            superVisor.Tell(echoMessage);
            response = this.ExpectMsg<EchoMessage>();
            Assert.Equal(echoMessage, response);
            Assert.True((bool)response.FromObjectActor);
        }

        [Fact]
        public void ClusterDownTest()
        {
            var redis = new ConcurrentDictionary<string, object>();
            this.WinsorBind<IConnectionMultiplexer>(() => new RedisConnectionMoq(redis));

            var superVisor = this.ActorOfAsTestActorRef<TestSupervisorActor>(this.Sys.DI().Props<TestSupervisorActor>());

            this.SetAutoPilot(new DelegateAutoPilot(
                delegate (IActorRef sender, object message)
                {
                    if (message is EnvelopeToReceiver)
                    {
                        sender.Tell(true);
                    }

                    return AutoPilot.KeepRunning;
                }));

            superVisor.Tell(
                new ClusterEvent.MemberUp(
                    Member.Create(
                        Cluster.Get(this.Sys).SelfUniqueAddress,
                        MemberStatus.Up,
                        ImmutableHashSet.Create("test"))));
            var address = new UniqueAddress(
                Cluster.Get(this.Sys).SelfAddress.WithPort(Cluster.Get(this.Sys).SelfAddress.Port + 1),
                1);

            superVisor.Tell(
                new ClusterEvent.MemberUp(
                    Member.Create(
                        address,
                        MemberStatus.Up,
                        ImmutableHashSet.Create("test"))));

            superVisor.Tell(new ClusterEvent.RoleLeaderChanged("test", address.Address));
            superVisor.Tell(new ResetChildren());
            superVisor.Tell(new ChildCreated { Id = "Id", NodeAddress = address });

            var echoMessage = new EchoMessage { Id = "1", Text = "Hello 1-1" };
            superVisor.Tell(echoMessage);
            var response = this.ExpectMsg<EnvelopeToReceiver>();
            Assert.Equal(echoMessage, (EchoMessage)response.Message);
            Assert.False(response.Message.AsInstanceOf<EchoMessage>().FromObjectActor);

            superVisor.Tell(new ClusterEvent.RoleLeaderChanged("test", null));
            echoMessage = new EchoMessage { Id = "1", Text = "Hello 1-2" };
            superVisor.Tell(echoMessage);
            this.ExpectNoMsg();
            superVisor.Tell(new ClusterEvent.RoleLeaderChanged("test", address.Address));
            this.ExpectNoMsg();
            superVisor.Tell(
                new ClusterEvent.MemberUp(
                    Member.Create(
                        address,
                        MemberStatus.Up,
                        ImmutableHashSet.Create("test"))));
            this.ExpectMsg<EnvelopeToReceiver>();
        }

        [Fact]
        public void CreateChildLockBlockedTest()
        {
            var redis = new ConcurrentDictionary<string, object>();
            this.WinsorBind<IConnectionMultiplexer>(() => new RedisConnectionMoq(redis));

            var superVisor = this.ActorOfAsTestActorRef<TestSupervisorActor>(this.Sys.DI().Props<TestSupervisorActor>(), "supervisor");

            superVisor.Tell(
                new ClusterEvent.MemberUp(
                    Member.Create(
                        Cluster.Get(this.Sys).SelfUniqueAddress,
                        MemberStatus.Up,
                        ImmutableHashSet.Create("test"))));
            superVisor.Tell(new ClusterEvent.RoleLeaderChanged("test", Cluster.Get(this.Sys).SelfAddress));

            superVisor.Tell(
                new ClusterEvent.MemberUp(
                    Member.Create(
                        new UniqueAddress(
                            Cluster.Get(this.Sys).SelfAddress.WithPort(Cluster.Get(this.Sys).SelfAddress.Port + 1),
                            1),
                        MemberStatus.Up,
                        ImmutableHashSet.Create("test"))));

            this.ExpectMsg<ResetChildren>();

            redis["user:supervisor:Mngmt:1:CreationLock"] = "true";

            var echoMessage = new EchoMessage { Id = "1", Text = "Hello 1-1" };
            superVisor.Tell(echoMessage);
            this.ExpectNoMsg();
            object val;
            redis.TryRemove("user:supervisor:Mngmt:1:CreationLock", out val);
            TimeMachineScheduler.JumpJustBefore(TimeSpan.FromSeconds(1));
            this.ExpectNoMsg();
            TimeMachineScheduler.JumpAfter();

            var created = this.ExpectMsg<string>();
            Assert.Equal((string)"Created 1", (string)created);
            Assert.Equal("1", this.ExpectMsg<ChildCreated>().Id);
            var response = this.ExpectMsg<EchoMessage>();
            Assert.Equal(echoMessage, response);
            Assert.True((bool)response.FromObjectActor);

            echoMessage = new EchoMessage { Id = "1", Text = "Hello 1-2" };
            superVisor.Tell(echoMessage);
            response = this.ExpectMsg<EchoMessage>();
            Assert.Equal(echoMessage, response);
            Assert.True((bool)response.FromObjectActor);
        }

        [Fact]
        public void ForeignNodeDownTest()
        {
            var redis = new ConcurrentDictionary<string, object>();
            this.WinsorBind<IConnectionMultiplexer>(() => new RedisConnectionMoq(redis));

            var superVisor = this.ActorOfAsTestActorRef<TestSupervisorActor>(
                this.Sys.DI().Props<TestSupervisorActor>(),
                "supervisor");

            this.SetAutoPilot(
                new DelegateAutoPilot(
                    delegate (IActorRef sender, object message)
                        {
                            if (message is EnvelopeToReceiver)
                            {
                                sender.Tell(true);
                            }

                            return AutoPilot.KeepRunning;
                        }));

            var address =
                new UniqueAddress(
                    Cluster.Get(this.Sys).SelfAddress.WithPort(Cluster.Get(this.Sys).SelfAddress.Port + 1),
                    1);

            var address2 =
                new UniqueAddress(
                    Cluster.Get(this.Sys).SelfAddress.WithPort(Cluster.Get(this.Sys).SelfAddress.Port + 1),
                    2);

            superVisor.Tell(
                new ClusterEvent.MemberUp(
                    Member.Create(
                        Cluster.Get(this.Sys).SelfUniqueAddress,
                        MemberStatus.Up,
                        ImmutableHashSet.Create("test"))));

            superVisor.Tell(
                new ClusterEvent.MemberUp(Member.Create(address, MemberStatus.Up, ImmutableHashSet.Create("test"))));

            superVisor.Tell(
                new ClusterEvent.MemberUp(Member.Create(address2, MemberStatus.Up, ImmutableHashSet.Create("test"))));

            superVisor.Tell(new ClusterEvent.RoleLeaderChanged("test", address.Address));
            superVisor.Tell(new ResetChildren());
            superVisor.Tell(new ChildCreated { Id = "1", NodeAddress = address });

            var echoMessage = new EchoMessage { Id = "1", Text = "Hello 1-1" };
            superVisor.Tell(echoMessage);

            this.ExpectMsg<EnvelopeToReceiver>();

            redis["user:supervisor:Mngmt:1:ChildAddress"] = ActorRefs.Nobody.SerializeToAkkaString(this.Sys);
            redis["user:supervisor:Mngmt:Supervisor:1:Children"] = new Dictionary<string, string> { { "1", "1" } };

            superVisor.Tell(new ClusterEvent.RoleLeaderChanged("test", Cluster.Get(this.Sys).SelfAddress));
            superVisor.Tell(
                new ClusterEvent.MemberRemoved(
                    Member.Create(address, MemberStatus.Removed, ImmutableHashSet.Create("test")),
                    MemberStatus.Up));
            var created = this.ExpectMsg<string>();
            Assert.Equal((string)"Created 1", (string)created);
            Assert.Equal("1", this.ExpectMsg<ChildCreated>().Id);

            echoMessage = new EchoMessage { Id = "1", Text = "Hello 1-2" };
            superVisor.Tell(echoMessage);
            this.ExpectMsg<EchoMessage>();
        }

        [Fact]
        public void ForeignNodeDownTest2()
        {
            var redis = new ConcurrentDictionary<string, object>();
            this.WinsorBind<IConnectionMultiplexer>(() => new RedisConnectionMoq(redis));

            var superVisor = this.ActorOfAsTestActorRef<TestSupervisorActor>(
                this.Sys.DI().Props<TestSupervisorActor>(),
                "supervisor");

            this.SetAutoPilot(
                new DelegateAutoPilot(
                    delegate (IActorRef sender, object message)
                    {
                        if (message is EnvelopeToReceiver)
                        {
                            sender.Tell(true);
                        }

                        return AutoPilot.KeepRunning;
                    }));

            var address =
                new UniqueAddress(
                    Cluster.Get(this.Sys).SelfAddress.WithPort(Cluster.Get(this.Sys).SelfAddress.Port + 1),
                    1);

            var address2 =
                new UniqueAddress(
                    Cluster.Get(this.Sys).SelfAddress.WithPort(Cluster.Get(this.Sys).SelfAddress.Port + 1),
                    2);

            superVisor.Tell(
                new ClusterEvent.MemberUp(
                    Member.Create(
                        Cluster.Get(this.Sys).SelfUniqueAddress,
                        MemberStatus.Up,
                        ImmutableHashSet.Create("test"))));

            superVisor.Tell(
                new ClusterEvent.MemberUp(Member.Create(address, MemberStatus.Up, ImmutableHashSet.Create("test"))));

            superVisor.Tell(
                new ClusterEvent.MemberUp(Member.Create(address2, MemberStatus.Up, ImmutableHashSet.Create("test"))));

            superVisor.Tell(new ClusterEvent.RoleLeaderChanged("test", address.Address));
            superVisor.Tell(new ResetChildren());
            superVisor.Tell(new ChildCreated { Id = "1", NodeAddress = address });

            var echoMessage = new EchoMessage { Id = "1", Text = "Hello 1-1" };
            superVisor.Tell(echoMessage);

            this.ExpectMsg<EnvelopeToReceiver>();

            redis["user:supervisor:Mngmt:1:ChildAddress"] = ActorRefs.Nobody.SerializeToAkkaString(this.Sys);
            redis["user:supervisor:Mngmt:Supervisor:1:Children"] = new Dictionary<string, string> { { "1", "1" } };

            superVisor.Tell(
                new ClusterEvent.MemberRemoved(
                    Member.Create(address, MemberStatus.Removed, ImmutableHashSet.Create("test")),
                    MemberStatus.Up));
            superVisor.Tell(new ClusterEvent.RoleLeaderChanged("test", Cluster.Get(this.Sys).SelfAddress));

            var created = this.ExpectMsg<string>();
            Assert.Equal((string)"Created 1", (string)created);
            Assert.Equal("1", this.ExpectMsg<ChildCreated>().Id);

            echoMessage = new EchoMessage { Id = "1", Text = "Hello 1-2" };
            superVisor.Tell(echoMessage);
            this.ExpectMsg<EchoMessage>();
        }

        [Fact]
        public void MessageBeforeClusterStartedTest()
        {
            var redis = new ConcurrentDictionary<string, object>();
            this.WinsorBind<IConnectionMultiplexer>(() => new RedisConnectionMoq(redis));

            var superVisor = this.ActorOfAsTestActorRef<TestSupervisorActor>(this.Sys.DI().Props<TestSupervisorActor>());

            var echoMessage = new EchoMessage { Id = "1", Text = "Hello 1-1" };
            superVisor.Tell(echoMessage);
            this.ExpectNoMsg();

            superVisor.Tell(
                new ClusterEvent.MemberUp(
                    Member.Create(
                        Cluster.Get(this.Sys).SelfUniqueAddress,
                        MemberStatus.Up,
                        ImmutableHashSet.Create("test"))));

            this.ExpectNoMsg();

            superVisor.Tell(
                new ClusterEvent.MemberUp(
                    Member.Create(
                        new UniqueAddress(
                            Cluster.Get(this.Sys).SelfAddress.WithPort(Cluster.Get(this.Sys).SelfAddress.Port + 1),
                            1),
                        MemberStatus.Up,
                        ImmutableHashSet.Create("test"))));

            this.ExpectNoMsg();

            superVisor.Tell(new ClusterEvent.RoleLeaderChanged("test", Cluster.Get(this.Sys).SelfAddress));

            var created = this.ExpectMsg<string>();
            Assert.Equal((string)"Created 1", (string)created);
            Assert.Equal("1", this.ExpectMsg<ChildCreated>().Id);
            var response = this.ExpectMsg<EchoMessage>();
            Assert.Equal(echoMessage, response);
            Assert.True((bool)response.FromObjectActor);
        }

        [Fact]
        public void NetworkMessageSendTest()
        {
            var redis = new ConcurrentDictionary<string, object>();
            this.WinsorBind<IConnectionMultiplexer>(() => new RedisConnectionMoq(redis));

            var superVisor = this.ActorOfAsTestActorRef<TestSupervisorActor>(this.Sys.DI().Props<TestSupervisorActor>(), "supervisor");

            superVisor.Tell(
                new ClusterEvent.MemberUp(
                    Member.Create(
                        Cluster.Get(this.Sys).SelfUniqueAddress,
                        MemberStatus.Up,
                        ImmutableHashSet.Create("test"))));
            superVisor.Tell(new ClusterEvent.RoleLeaderChanged("test", Cluster.Get(this.Sys).SelfAddress));

            var address =
                new UniqueAddress(
                    Cluster.Get(this.Sys).SelfAddress.WithPort(Cluster.Get(this.Sys).SelfAddress.Port + 1),
                    1);
            superVisor.Tell(
                new ClusterEvent.MemberUp(Member.Create(address, MemberStatus.Up, ImmutableHashSet.Create("test"))));

            this.ExpectMsg<ResetChildren>();
            superVisor.UnderlyingActor.SelectNodeToPlaceChildOverride = s => this.TestActor;

            int messagesRecieved = 0;
            this.SetAutoPilot(
                new DelegateAutoPilot(
                    delegate (IActorRef sender, object message)
                        {
                            if (message is EnvelopeToReceiver)
                            {
                                messagesRecieved++;
                                if (messagesRecieved == 1 || messagesRecieved == 3)
                                {
                                    sender.Tell(true);
                                }
                            }

                            return AutoPilot.KeepRunning;
                        }));

            var echoMessage = new EchoMessage { Id = "1", Text = "Hello 1-1" };
            superVisor.Tell(echoMessage);

            this.ExpectMsg<CreateChildCommand>();
            this.ExpectNoTestMsg();
            superVisor.Tell(new ChildCreated { Id = "1", NodeAddress = address });
            this.ExpectMsg<EnvelopeToReceiver>();

            echoMessage = new EchoMessage { Id = "1", Text = "Hello 1-2" };
            superVisor.Tell(echoMessage);
            this.ExpectMsg<EnvelopeToReceiver>();
            Thread.Sleep(TimeSpan.FromMilliseconds(150));
            TimeMachineScheduler.Jump(TimeSpan.FromSeconds(1));
            TimeMachineScheduler.JumpAfter();
            this.ExpectMsg<EnvelopeToReceiver>();
            Thread.Sleep(TimeSpan.FromMilliseconds(110));
            TimeMachineScheduler.Jump(TimeSpan.FromSeconds(1));
            TimeMachineScheduler.JumpAfter();
            this.ExpectNoMsg();
        }

        [Fact]
        public void NotALeaderTest()
        {
            var redis = new ConcurrentDictionary<string, object>();
            this.WinsorBind<IConnectionMultiplexer>(() => new RedisConnectionMoq(redis));

            var superVisor = this.ActorOfAsTestActorRef<TestSupervisorActor>(this.Sys.DI().Props<TestSupervisorActor>());

            this.SetAutoPilot(new DelegateAutoPilot(
                delegate (IActorRef sender, object message)
                    {
                        if (message is EnvelopeToReceiver)
                        {
                            sender.Tell(true);
                        }

                        return AutoPilot.KeepRunning;
                    }));

            superVisor.Tell(
                new ClusterEvent.MemberUp(
                    Member.Create(
                        Cluster.Get(this.Sys).SelfUniqueAddress,
                        MemberStatus.Up,
                        ImmutableHashSet.Create("test"))));
            var address = new UniqueAddress(
                Cluster.Get(this.Sys).SelfAddress.WithPort(Cluster.Get(this.Sys).SelfAddress.Port + 1),
                1);

            superVisor.Tell(
                new ClusterEvent.MemberUp(
                    Member.Create(
                        address,
                        MemberStatus.Up,
                        ImmutableHashSet.Create("test"))));

            superVisor.Tell(new ClusterEvent.RoleLeaderChanged("test", address.Address));
            superVisor.Tell(new ResetChildren());

            var echoMessage = new EchoMessage { Id = "1", Text = "Hello 1-1" };
            superVisor.Tell(echoMessage);
            var response = this.ExpectMsg<EnvelopeToReceiver>();
            Assert.Equal(echoMessage, (EchoMessage)response.Message);
            Assert.False(response.Message.AsInstanceOf<EchoMessage>().FromObjectActor);
        }

        [Fact]
        public void SupervisorRestartTest()
        {
            var redis = new ConcurrentDictionary<string, object>();
            this.WinsorBind<IConnectionMultiplexer>(() => new RedisConnectionMoq(redis));

            this.SetAutoPilot(
                new DelegateAutoPilot(
                    delegate (IActorRef sender, object message)
                        {
                            if (message is EnvelopeToReceiver)
                            {
                                sender.Tell(true);
                            }

                            return AutoPilot.KeepRunning;
                        }));

            var uniqueAddress =
                new UniqueAddress(
                    Cluster.Get(this.Sys).SelfAddress.WithPort(Cluster.Get(this.Sys).SelfAddress.Port + 1),
                    1);

            var superVisor = this.ActorOfAsTestActorRef<TestSupervisorActor>(this.Sys.DI().Props<TestSupervisorActor>(), "supervisor");

            superVisor.Tell(
                new ClusterEvent.MemberUp(
                    Member.Create(
                        Cluster.Get(this.Sys).SelfUniqueAddress,
                        MemberStatus.Up,
                        ImmutableHashSet.Create("test"))));
            superVisor.Tell(new ClusterEvent.RoleLeaderChanged("test", Cluster.Get(this.Sys).SelfAddress));

            superVisor.Tell(
                new ClusterEvent.MemberUp(
                    Member.Create(uniqueAddress, MemberStatus.Up, ImmutableHashSet.Create("test"))));

            this.ExpectMsg<ResetChildren>();

            var echoMessage = new EchoMessage { Id = "1", Text = "Hello 1-1" };
            superVisor.Tell(echoMessage);
            var created = this.ExpectMsg<string>();
            Assert.Equal((string)"Created 1", (string)created);
            Assert.Equal("1", this.ExpectMsg<ChildCreated>().Id);
            var response = this.ExpectMsg<EchoMessage>();
            Assert.Equal(echoMessage, response);
            Assert.True((bool)response.FromObjectActor);

            superVisor.Tell(new SupervisorVomitive());
            echoMessage = new EchoMessage { Id = "1", Text = "Hello 1-2" };
            superVisor.Tell(echoMessage);

            var members = ImmutableSortedSet<Member>.Empty;
            var builder = members.ToBuilder();
            builder.Add(Member.Create(Cluster.Get(this.Sys).SelfUniqueAddress, MemberStatus.Up, ImmutableHashSet.Create("test")));
            builder.Add(Member.Create(uniqueAddress, MemberStatus.Up, ImmutableHashSet.Create("test")));
            members = builder.ToImmutable();

            var roleLeaders = ImmutableDictionary<string, Address>.Empty;

            var currentClusterState = new ClusterEvent.CurrentClusterState(
                members,
                ImmutableHashSet<Member>.Empty,
                ImmutableHashSet<Address>.Empty,
                uniqueAddress.Address,
                roleLeaders);

            superVisor.Tell(currentClusterState);

            superVisor.Tell(new ClusterEvent.RoleLeaderChanged("test", Cluster.Get(this.Sys).SelfAddress));
            created = this.ExpectMsg<string>();
            Assert.Equal((string)"Created 1", (string)created);

            response = this.ExpectMsg<EchoMessage>();

            Assert.Equal(echoMessage, response);
            Assert.True((bool)response.FromObjectActor);
        }

        [Fact]
        public void SupervisorStartTest()
        {
            var redis = new ConcurrentDictionary<string, object>();
            this.WinsorBind<IConnectionMultiplexer>(() => new RedisConnectionMoq(redis));

            var superVisor = this.ActorOfAsTestActorRef<TestSupervisorActor>(this.Sys.DI().Props<TestSupervisorActor>(), "supervisor");

            superVisor.Tell(
                new ClusterEvent.MemberUp(
                    Member.Create(
                        Cluster.Get(this.Sys).SelfUniqueAddress,
                        MemberStatus.Up,
                        ImmutableHashSet.Create("test"))));
            superVisor.Tell(new ClusterEvent.RoleLeaderChanged("test", Cluster.Get(this.Sys).SelfAddress));

            superVisor.Tell(
                new ClusterEvent.MemberUp(
                    Member.Create(
                        new UniqueAddress(
                            Cluster.Get(this.Sys).SelfAddress.WithPort(Cluster.Get(this.Sys).SelfAddress.Port + 1),
                            1),
                        MemberStatus.Up,
                        ImmutableHashSet.Create("test"))));

            this.ExpectMsg<ResetChildren>();

            var echoMessage = new EchoMessage { Id = "1", Text = "Hello 1-1" };
            superVisor.Tell(echoMessage);
            var created = this.ExpectMsg<string>();
            Assert.Equal((string)"Created 1", (string)created);
            Assert.Equal("1", this.ExpectMsg<ChildCreated>().Id);
            var response = this.ExpectMsg<EchoMessage>();
            Assert.Equal(echoMessage, response);
            Assert.True((bool)response.FromObjectActor);

            echoMessage = new EchoMessage { Id = "1", Text = "Hello 1-2" };
            superVisor.Tell(echoMessage);
            response = this.ExpectMsg<EchoMessage>();
            Assert.Equal(echoMessage, response);
            Assert.True((bool)response.FromObjectActor);

            echoMessage = new EchoMessage { Id = "2", Text = "Hello 2-1" };
            superVisor.Tell(echoMessage);

            Assert.Equal("2", this.ExpectMsg<ChildCreated>().Id);
            created = this.ExpectMsg<string>();
            Assert.Equal((string)"Created 2", (string)created);

            response = this.ExpectMsg<EchoMessage>();
            Assert.Equal(echoMessage, response);
            Assert.True((bool)response.FromObjectActor);
        }

        /// <summary>
        /// The current test configuration
        /// </summary>
        public class Configurator : TestConfigurator
        {
            /// <summary>
            /// Gets the akka system config
            /// </summary>
            /// <param name="windsorContainer">
            /// The windsor Container.
            /// </param>
            /// <returns>
            /// The config
            /// </returns>
            public override Config GetAkkaConfig(IWindsorContainer windsorContainer)
            {
                return ConfigurationFactory.ParseString(@"
                    akka.actor.deployment {
                        /supervisor {
                            createChildTimeout = 1s
                            sendTimeOut = 100ms
                            nextAttmeptPause = 1s
                            sendersCount = 1
                            dispatcher = ClusterKit.test-dispatcher
                        }

                        ""/*"" {
                           dispatcher = ClusterKit.test-dispatcher
                        }
                        ""/*/*"" {
                           dispatcher = ClusterKit.test-dispatcher
                        }
                        ""/*/*/*"" {
                           dispatcher = ClusterKit.test-dispatcher
                        }
                    }
")
                .WithFallback(
                    ConfigurationFactory.ParseString("akka.remote.helios.tcp.hostname = 127.0.0.1"))
                    .WithFallback(ConfigurationFactory.ParseString("akka.remote.helios.tcp.port = 0"))
                    .WithFallback(
                        ConfigurationFactory.ParseString(
                            "akka.cluster.roles = [\"test\"]"))
                    .WithFallback(
                        ConfigurationFactory.ParseString(
                            "akka.cluster.seed-nodes = []"))
                .WithFallback(
                    ConfigurationFactory.ParseString(
                        "akka.actor.provider = \"Akka.Cluster.ClusterActorRefProvider, Akka.Cluster\""))
                    .WithFallback(
                        ConfigurationFactory.ParseString(
                            "akka.loggers = [\"Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog\"]"))
                    .WithFallback(ConfigurationFactory.ParseString("min-nr-of-members = 1"))
                    .WithFallback(ConfigurationFactory.ParseString("akka.loglevel = INFO"))
                    .WithFallback(ConfigurationFactory.ParseString("akka.cluster.auto-down-unreachable-after = 1s"))
                    .WithFallback(base.GetAkkaConfig(windsorContainer));
            }

            /// <summary>
            /// Gets list of all used plugin installers
            /// </summary>
            /// <returns>The list of installers</returns>
            public override List<BaseInstaller> GetPluginInstallers()
            {
                var pluginInstallers = base.GetPluginInstallers();
                pluginInstallers.Add(new ClusterKit.BusinessObjects.Installer());
                return pluginInstallers;
            }
        }
    }

    public class EchoMessage : IMessageToBusinessObjectActor
    {
        public bool FromObjectActor { get; set; }
        public string Id { get; set; }
        public string Text { get; set; }

        public static bool operator !=(EchoMessage left, EchoMessage right)
        {
            return !Equals(left, right);
        }

        public static bool operator ==(EchoMessage left, EchoMessage right)
        {
            return Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return this.Equals((EchoMessage)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.Id != null ? this.Id.GetHashCode() : 0) * 397) ^ (this.Text != null ? this.Text.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return $"Object {this.Id}: {this.Text}";
        }

        protected bool Equals(EchoMessage other)
        {
            return string.Equals(this.Id, other.Id) && string.Equals(this.Text, other.Text);
        }
    }

    public class SniperShot : IMessageToBusinessObjectActor
    {
        public string Id { get; set; }
    }

    public class SupervisorVomitive
    {
    }

    [UsedImplicitly]
    public class TestObjectActor : BaseBusinessObjectReceiveActor
    {
        private readonly ITestOutputHelper output;

        private readonly ConcurrentBag<EchoMessage> recievedEchoMessages;

        private readonly IActorRef testActor;

        public TestObjectActor(IActorRef testActor, ITestOutputHelper output, ConcurrentBag<EchoMessage> recievedEchoMessages)
        {
            this.testActor = testActor;
            this.output = output;
            this.recievedEchoMessages = recievedEchoMessages;
        }

        public override void SubscribeToMessages()
        {
            this.Receive<EchoMessage>(m => this.OnEchoMessage(m));
            this.Receive<SniperShot>(m => this.OnSniperShot());
            this.Receive<Vomitive>(m => this.OnVomitive());
            Context.GetLogger().Info($"TestObjectActor created {this.Id}");
            this.testActor.Tell($"Created {this.Id}");
        }

        protected override void PostRestart(Exception reason)
        {
            Context.GetLogger().Info("{Id}: PostRestart", this.Id);
            base.PostRestart(reason);
        }

        protected override void PostStop()
        {
            Context.GetLogger().Info("{Id}: PostStop", this.Id);
            base.PostStop();
        }

        protected override void PreRestart(Exception reason, object message)
        {
            Context.GetLogger().Info("{Id}: PreRestart", this.Id);
            base.PreRestart(reason, message);
        }

        protected override void PreStart()
        {
            Context.GetLogger().Info("{Id}: PreStart", this.Id);
            base.PreStart();
        }

        private void OnEchoMessage(EchoMessage message)
        {
            var echoMessage = new EchoMessage { Id = this.Id, Text = message.Text, FromObjectActor = true };
            Context.GetLogger().Info($"{Cluster.Get(Context.System).SelfUniqueAddress.Uid}: {echoMessage}");
            this.recievedEchoMessages.Add(echoMessage);
            this.testActor.Tell(echoMessage);
        }

        private void OnSniperShot()
        {
            Context.GetLogger().Info("{Id}: got sniper shot... I'll better take poison pill", this.Id);
            this.Self.Tell(PoisonPill.Instance);
        }

        private void OnVomitive()
        {
            throw new Exception("Vomit");
        }
    }

    [UsedImplicitly]
    public class TestSupervisorActor : ClusterBusinessObjectActorSupervisor<TestObjectActor>
    {
        private readonly IActorRef testActor;

        public TestSupervisorActor(IConnectionMultiplexer redisConnection, IActorRef testActor)
            : base(redisConnection)
        {
            this.testActor = testActor;
            this.SelectNodeToPlaceChildOverride = s => this.Self;
        }

        public Func<string, ICanTell> SelectNodeToPlaceChildOverride { get; set; }

        protected override string ClusterRole => "test";

        protected override void ClusteredMessageProccess()
        {
            base.ClusteredMessageProccess();
            this.Receive<SupervisorVomitive>(m => this.OnSupervisorVomitive());
        }

        protected override ICanTell CreateSupervisorICanTell(Address nodeAddress)
        {
            return nodeAddress == this.CurrentCluster.SelfAddress ? this.Self : this.testActor;
        }

        protected override void ForwardMessageToChild(IMessageToBusinessObjectActor message, IActorRef sender = null)
        {
            Context.GetLogger().Info("Forwarding message to child");
            base.ForwardMessageToChild(message, sender);
        }

        protected override void OnClusterRoleLeaderChanged(Address leaderAddress)
        {
            base.OnClusterRoleLeaderChanged(leaderAddress);
            Context.GetLogger().Info("Leader change received, {0}, {1}", this.IsClusterInitizlized, this.IsDataInitizlized);
        }

        protected override void OnClusterState(ClusterEvent.CurrentClusterState currentClusterState)
        {
            base.OnClusterState(currentClusterState);
            Context.GetLogger().Info("Cluser state received, {0}, {1}", this.IsClusterInitizlized, this.IsDataInitizlized);
        }

        protected override void OnInitializationDataReceived(InitializationData initializationData)
        {
            base.OnInitializationDataReceived(initializationData);
            Context.GetLogger().Info("Initialization Data Received, {0}, {1}", this.IsClusterInitizlized, this.IsDataInitizlized);
        }

        protected override void PostRestart(Exception reason)
        {
            Context.GetLogger().Info("...Restarting...");
            base.PostRestart(reason);
            Context.GetLogger().Info("...restarted");
        }

        protected override void PreRestart(Exception reason, object message)
        {
            base.PreRestart(reason, message);
            Context.GetLogger().Info("Fault...");
        }

        protected override void PreStart()
        {
            Context.GetLogger().Info("Starting...");
            base.PreStart();
        }

        protected override ICanTell SelectNodeToPlaceChild(string id)
        {
            return this.SelectNodeToPlaceChildOverride(id);
        }

        private void OnSupervisorVomitive()
        {
            throw new Exception("Vomit");
        }
    }

    public class Vomitive : IMessageToBusinessObjectActor
    {
        public string Id { get; set; }
    }
}