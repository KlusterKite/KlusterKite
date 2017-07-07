// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiBrowserActorTest.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Testing the <see cref="ApiBrowserActor" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Web.Tests.GraphQL
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Reflection;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Configuration;
    using Akka.DI.Core;
    using Akka.TestKit;
    using Akka.Util;

    using Autofac;

    using KlusterKite.API.Client.Messages;
    using KlusterKite.Core;
    using KlusterKite.Core.TestKit;
    using KlusterKite.Web.GraphQL.Publisher;

    using JetBrains.Annotations;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Testing the <see cref="ApiBrowserActor"/>
    /// </summary>
    public class ApiBrowserActorTest : BaseActorTest<ApiBrowserActorTest.Configurator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiBrowserActorTest"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public ApiBrowserActorTest(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// The test scheduler
        /// </summary>
        private TestScheduler TestScheduler => (TestScheduler)this.Sys.Scheduler;

        /// <summary>
        /// Testing the new api node register process
        /// </summary>
        [Fact]
        public void SimpleNodeLifeCycleTest()
        {
            var schemaProvider = this.Container.Resolve<SchemaProvider>();
            Assert.Null(schemaProvider.CurrentSchema);
            var browser = this.ActorOf(this.Sys.DI().Props<ApiBrowserActor>());
            this.ExpectNoMsg();
            Assert.Null(schemaProvider.CurrentSchema);

            var nodeAddress = new UniqueAddress(new Address("akka.tcp", "KlusterKite", "testNode1", 1), 1);
            var remoteActorRef =
                new FakeActorRef(
                    ActorPath.Parse($"{nodeAddress.Address}/user/KlusterKite/API/Publisher"),
                    this.TestActor);

            browser.Tell(
                new ClusterEvent.MemberUp(
                    ClusterExtensions.MemberCreate(nodeAddress, 1, MemberStatus.Up, ImmutableHashSet<string>.Empty)));

            // node role check
            this.ExpectNoMsg();
            browser.Tell(
                new ClusterEvent.MemberUp(
                    ClusterExtensions.MemberCreate(
                        nodeAddress,
                        1,
                        MemberStatus.Up,
                        ImmutableHashSet.Create("KlusterKite.API.Endpoint"))));

            var discoverRequest = this.ExpectMsg<RemoteTestMessage<ApiDiscoverRequest>>();
            Assert.Equal("/user/KlusterKite/API/Publisher", discoverRequest.ReceiverPath);
            Assert.Equal(nodeAddress.Address, discoverRequest.RecipientAddress);
            this.ExpectNoMsg();
            Assert.Null(schemaProvider.CurrentSchema);

            this.TestScheduler.Advance(TimeSpan.FromSeconds(30));
            discoverRequest = this.ExpectMsg<RemoteTestMessage<ApiDiscoverRequest>>();
            Assert.Equal("/user/KlusterKite/API/Publisher", discoverRequest.ReceiverPath);
            Assert.Equal(nodeAddress.Address, discoverRequest.RecipientAddress);
            this.ExpectNoMsg();
            Assert.Null(schemaProvider.CurrentSchema);

            var internalApiProvider = new API.Tests.Mock.TestProvider();
            var discoverResponse = new ApiDiscoverResponse
                                       {
                                           Description = internalApiProvider.ApiDescription,
                                           Handler = this.TestActor
                                       };

            browser.Tell(new List<ApiDiscoverResponse> { discoverResponse }, remoteActorRef);
            this.ExpectNoMsg();

            // The new api node now launched
            Assert.NotNull(schemaProvider.CurrentSchema);
            this.TestScheduler.Advance(TimeSpan.FromSeconds(30));
            this.ExpectNoMsg();

            browser.Tell(
                new ClusterEvent.MemberRemoved(
                    ClusterExtensions.MemberCreate(
                        nodeAddress,
                        1,
                        MemberStatus.Removed,
                        ImmutableHashSet.Create("KlusterKite.API.Endpoint")),
                    MemberStatus.Up));

            this.ExpectNoMsg();
            Assert.Null(schemaProvider.CurrentSchema);
        }

        /// <summary>
        /// Configures current test system
        /// </summary>
        public class Configurator : TestConfigurator
        {
            /// <summary>
            /// Gets list of all used plugin installers
            /// </summary>
            /// <returns>The list of installers</returns>
            public override List<BaseInstaller> GetPluginInstallers()
            {
                var pluginInstallers = new List<BaseInstaller> { new Core.TestKit.Installer(), new TestInstaller() };
                return pluginInstallers;
            }
        }

        /// <summary>
        /// Some fake actor ref to reproduce remote node answer
        /// </summary>
        private class FakeActorRef : IActorRef
        {
            /// <summary>
            /// The test actor reference
            /// </summary>
            private readonly IActorRef testActor;

            /// <summary>
            /// Initializes a new instance of the <see cref="FakeActorRef"/> class.
            /// </summary>
            /// <param name="path">
            /// The path.
            /// </param>
            /// <param name="testActor">
            /// The test actor.
            /// </param>
            public FakeActorRef(ActorPath path, IActorRef testActor)
            {
                this.testActor = testActor;
                this.Path = path;
            }

            /// <inheritdoc />
            public ActorPath Path { get; }

            /// <inheritdoc />
            public int CompareTo(IActorRef other)
            {
                throw new InvalidOperationException();
            }

            /// <inheritdoc />
            public int CompareTo(object obj)
            {
                throw new InvalidOperationException();
            }

            /// <inheritdoc />
            public bool Equals(IActorRef other)
            {
                return ReferenceEquals(this, other);
            }

            /// <inheritdoc />
            public void Tell(object message, IActorRef sender)
            {
                var warapped = new UntypedForwardedMessage
                                   {
                                       Message = message,
                                       ReceiverPath = this.Path,
                                       Receiver = this
                                   };

                this.testActor.Tell(warapped, sender);
            }

            /// <inheritdoc />
            public ISurrogate ToSurrogate(ActorSystem system)
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// The message wrap from <see cref="FakeActorRef"/>
        /// </summary>
        private class UntypedForwardedMessage
        {
            /// <summary>
            /// Gets or sets the original message
            /// </summary>
            [UsedImplicitly]
            public object Message { get; set; }

            /// <summary>
            /// Gets or sets the original receiver path
            /// </summary>
            [UsedImplicitly]
            public ActorPath ReceiverPath { get; set; }

            /// <summary>
            /// Gets or sets the original receiver
            /// </summary>
            [UsedImplicitly]
            public IActorRef Receiver { get; set; }
        }

        /// <summary>
        /// Replaces production data sources with the test ones
        /// </summary>
        private class TestInstaller : BaseInstaller
        {
            /// <summary>
            /// Gets priority for ordering akka configurations. Highest priority will override lower priority.
            /// </summary>
            /// <remarks>Consider using <seealso cref="BaseInstaller"/> integrated constants</remarks>
            protected override decimal AkkaConfigLoadPriority => -1M;

            /// <summary>
            /// Gets default akka configuration for current module
            /// </summary>
            /// <returns>Akka configuration</returns>
            protected override Config GetAkkaConfig() => ConfigurationFactory.ParseString(@"
            {
                KlusterKite.NodeManager.ConfigurationDatabaseName = ""TestConfigurationDatabase""

                akka : {
                  actor: {
                    provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
                   
                    serializers {
		                hyperion = ""Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion""
                    }

                    serialization-bindings {
                        ""System.Object"" = hyperion
                    }
                  }

                 remote : {
                        helios.tcp : {
                          hostname = 127.0.0.1
                          port = 0
                        }
                      }

                      cluster: {
                        auto-down-unreachable-after = 15s
		                min-nr-of-members = 3
                        seed-nodes = []
                        singleton {
                            # The number of retries are derived from hand-over-retry-interval and
                            # akka.cluster.down-removal-margin (or ClusterSingletonManagerSettings.removalMargin),
                            # but it will never be less than this property.
                            min-number-of-hand-over-retries = 10
                        }
                      }
                }
            }");

            /// <inheritdoc />
            protected override void RegisterComponents(ContainerBuilder container, Config config)
            {
                container.RegisterAssemblyTypes(typeof(ApiBrowserActor).GetTypeInfo().Assembly).Where(t => t.GetTypeInfo().IsSubclassOf(typeof(ActorBase)));
                container.RegisterType<TestMessageRouter>().As<IMessageRouter>().SingleInstance();
                container.RegisterType<SchemaProvider>().SingleInstance();
            }
        }
    }
}