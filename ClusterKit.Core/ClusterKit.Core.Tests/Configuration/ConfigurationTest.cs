// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationTest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace ClusterKit.Core.Tests.Configuration
{
    using System.Collections.Generic;
    using System.Linq;

    using Akka.Actor;
    using Akka.Cluster.Sharding;
    using Akka.Configuration;

    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ClusterKit.Core.TestKit;

    using JetBrains.Annotations;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Global configuration test
    /// </summary>
    public class ConfigurationTest : BaseActorTest<ConfigurationTest.Configurator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationTest"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public ConfigurationTest(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Testing that hocon config stacks as expected
        /// </summary>
        [Fact]
        public void CheckConfigStacking()
        {
            var config1 = @"{
                akka.actor.deployment {
                    /testRoot1 {
                        autoDeploy = on
                    }
                 }
            }";

            var config2 = @"{
                akka.actor.deployment {
                    /testRoot2 {
                        autoDeploy = on
                    }
                 }
            }";

            var endConfig = ConfigurationFactory.Empty
                .WithFallback(config1)
                .WithFallback(config2);

            var deployConfig = endConfig.GetConfig("akka.actor.deployment");
            Assert.Equal(2, deployConfig.AsEnumerable().Count());
            Assert.True(endConfig.GetBoolean("akka.actor.deployment./testRoot1.autoDeploy"));
            Assert.True(endConfig.GetBoolean("akka.actor.deployment./testRoot2.autoDeploy"));
        }

        /// <summary>
        /// Testing correct <seealso cref="NameSpaceActor"/> work
        /// </summary>
        [Fact]
        public void NameSpaceActorTest()
        {
            // todo: fix test cluster joining and add all types of actors to test
            /*
            var cluster = Akka.Cluster.Cluster.Get(this.Sys);
            cluster.Join(cluster.SelfAddress);
            */

            this.Sys.StartNameSpaceActorsFromConfiguration();
            this.Sys.ActorSelection("/user/testNameSpace/forwarder").Tell("Hello world");
            Assert.Equal("Hello world", this.ExpectMsg<string>("/user/testNameSpace/forwarder"));
            this.Sys.ActorSelection("/user/testNameSpace/second/forwarder").Tell("Hello world");
            Assert.Equal("Hello world", this.ExpectMsg<string>("/user/testNameSpace/second/forwarder"));

            /*
            var shardingActor = this.Sys.ActorSelection("/user/testNameSpace/sharding");
            var shardingState = await shardingActor.Ask<ClusterShardingStats>(
                new GetClusterShardingStats(TimeSpan.FromMilliseconds(100)),
                TimeSpan.FromMilliseconds(2000));

            shardingActor.Tell("Hello sharding world");
            var shardMessage = this.ExpectMsg<TestMessage<string>>();
            Assert.Equal("Hello sharding world", shardMessage.Message);
            this.Sys.Log.Info(shardMessage.ReceiverPath);
            */
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
                return ConfigurationFactory.ParseString(@"{
                akka.actor.deployment {
                    /testNameSpace {
                        IsNameSpace = true
                    }

                    /somethingElse {
                    }

                    /testNameSpace/forwarder {
                        type = ""ClusterKit.Core.TestKit.TestActorForwarder, ClusterKit.Core.TestKit""
                    }

                    /testNameSpace/second {
                        type = ""ClusterKit.Core.NameSpaceActor, ClusterKit.Core""
                    }

                    /testNameSpace/second/forwarder {
                        type = ""ClusterKit.Core.TestKit.TestActorForwarder, ClusterKit.Core.TestKit""
                    }

                    /testNameSpace/sharding {
                        actor-type = Sharding
                        type-name = test-shard
                        type = ""ClusterKit.Core.TestKit.TestActorForwarder, ClusterKit.Core.TestKit""
                        role = test
                        message-extractor = ""ClusterKit.Core.Tests.Configuration.ConfigurationTest+TestMessageExtractor, ClusterKit.Core.Tests""
                    }
                 }
            }").WithFallback(base.GetAkkaConfig(windsorContainer));
            }

            /// <summary>
            /// Gets list of all used plugin installers
            /// </summary>
            /// <returns>The list of installers</returns>
            public override List<BaseInstaller> GetPluginInstallers()
            {
                var pluginInstallers = base.GetPluginInstallers();
                pluginInstallers.Add(new TestInstaller());
                return pluginInstallers;
            }
        }

        public class TestInstaller : BaseInstaller
        {
            protected override decimal AkkaConfigLoadPriority => -1M;

            protected override Config GetAkkaConfig() => ConfigurationFactory.Empty;

            protected override IEnumerable<string> GetRoles() => new[] { "test" };

            protected override void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store)
            {
                container.Register(Classes.FromThisAssembly().Where(t => t.IsSubclassOf(typeof(ActorBase))).LifestyleTransient());
                container.Register(Classes.From(typeof(TestMessageExtractor)).Where(t => true).LifestyleTransient());
            }
        }

        /// <summary>
        /// Test message extractor
        /// </summary>
        [UsedImplicitly]
        public class TestMessageExtractor : IMessageExtractor
        {
            /// <summary>
            /// Extract the entity id from an incoming <paramref name="message"/>.
            ///             If `null` is returned the message will be `unhandled`, i.e. posted as `Unhandled`
            ///              messages on the event stream
            /// </summary>
            public string EntityId(object message) => message.GetType().Name;

            /// <summary>
            /// Extract the message to send to the entity from an incoming <paramref name="message"/>.
            ///             Note that the extracted message does not have to be the same as the incoming
            ///             message to support wrapping in message envelope that is unwrapped before
            ///             sending to the entity actor.
            /// </summary>
            public object EntityMessage(object message) => message;

            /// <summary>
            /// Extract the entity id from an incoming <paramref name="message"/>. Only messages that
            ///             passed the <see cref="M:Akka.Cluster.Sharding.IMessageExtractor.EntityId(System.Object)"/> method will be used as input to this method.
            /// </summary>
            public string ShardId(object message) => message.GetType().Assembly.FullName;
        }
    }
}