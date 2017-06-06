// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParcelsTest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing <see cref="ParcelManagerActor" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.LargeObjects.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Configuration;
    using Akka.DI.Core;
    using Akka.TestKit;

    using Autofac;

    using ClusterKit.Core;
    using ClusterKit.Core.TestKit;
    using ClusterKit.LargeObjects.Client;

    using JetBrains.Annotations;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Testing <see cref="Parcel"/> sending and receiving
    /// </summary>
    public class ParcelsTest : BaseActorTest<ParcelsTest.Configurator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParcelsTest"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public ParcelsTest(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Just testing that everything is ok
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Fact(DisplayName = "ParcelManagerActor simple test")]
        public async Task NormalFlowTest()
        {
            var actor = this.Sys.ActorOf(this.Sys.DI().Props<ParcelManagerActor>(), "parcelManager");
            this.ExpectNoMsg(TimeSpan.FromSeconds(1));

            var payload = "Hello world";
            actor.Tell(new Parcel { Payload = payload, Recipient = this.TestActor });

            var notification = this.ExpectMsg<ParcelNotification>();
            Assert.Equal(typeof(string), notification.GetPayloadType());
            this.ExpectNoMsg(TimeSpan.FromSeconds(1));

            var receivedPayload = await notification.Receive(this.Sys) as string;
            Assert.Equal(payload, receivedPayload);

            // checking that you cannot receive parcel twice
            try
            {
                await notification.Receive(this.Sys);
                throw new Exception("Expected ParcelNotFoundException exception");
            }
            catch (ParcelNotFoundException)
            {
            }
        }

        /// <summary>
        /// Just testing that everything is ok
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Fact(DisplayName = "ParcelManagerActor test of envelopers")]
        public async Task EnveloperFlowTest()
        {
            var actor = this.Sys.ActorOf(this.Sys.DI().Props<ParcelManagerActor>(), "parcelManager");
            this.ExpectNoMsg(TimeSpan.FromSeconds(1));

            object payload = 5;
            actor.Tell(new Parcel { Payload = payload, Recipient = this.TestActor });

            var envelope = this.ExpectMsg<NotificationEnvelope>();
            Assert.Equal("int", envelope.Tag);
            var notification = envelope.Notification;
            Assert.Equal(typeof(int), notification.GetPayloadType());
            this.ExpectNoMsg(TimeSpan.FromSeconds(1));

            object receivedPayload = (int)await notification.Receive(this.Sys);
            Assert.Equal(payload, receivedPayload);

            payload = 5.5;
            actor.Tell(new Parcel { Payload = payload, Recipient = this.TestActor });

            envelope = this.ExpectMsg<NotificationEnvelope>();
            Assert.Equal("double", envelope.Tag);
            notification = envelope.Notification;
            Assert.Equal(typeof(double), notification.GetPayloadType());
            this.ExpectNoMsg(TimeSpan.FromSeconds(1));

            receivedPayload = (double)await notification.Receive(this.Sys);
            Assert.Equal(payload, receivedPayload);
        }

        /// <summary>
        /// Just testing that everything is ok
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Fact(DisplayName = "ParcelManagerActor testing that abandoned parcels are removed from store")]
        public async Task CleanUpTest()
        {
            var actor = this.Sys.ActorOf(this.Sys.DI().Props<ParcelManagerActor>(), "parcelManager");
            this.ExpectNoMsg(TimeSpan.FromSeconds(1));

            var payload = "Hello world";
            actor.Tell(new Parcel { Payload = payload, Recipient = this.TestActor });

            var notification = this.ExpectMsg<ParcelNotification>();
            Assert.Equal(typeof(string), notification.GetPayloadType());
            ((TestScheduler)this.Sys.Scheduler).Advance(TimeSpan.FromMinutes(6));

            try
            {
                await notification.Receive(this.Sys);
                throw new Exception("Expected ParcelNotFoundException exception");
            }
            catch (ParcelNotFoundException)
            {
            }
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
            protected override Config GetAkkaConfig() => ConfigurationFactory.ParseString(
                @"{
                    akka.actor {

                         serialization-identifiers {
                          ""ClusterKit.Core.Serializers.WindsorContainerSerializer, ClusterKit.Core"" = 101
                          ""ClusterKit.Core.Serializers.WindsorDependencyResolverSerializer, ClusterKit.Core"" = 102
                        }
                        serializers {
                          hyperion = ""Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion""
                          windsorContainerSerializer = ""ClusterKit.Core.Serializers.WindsorContainerSerializer, ClusterKit.Core""
                          windsorDependencyResolver = ""ClusterKit.Core.Serializers.WindsorDependencyResolverSerializer, ClusterKit.Core""
                          json = ""Akka.Serialization.NewtonSoftJsonSerializer""
                        }

                        serialization-bindings {
                           ""System.Object"" = ""hyperion""           
                           ""Castle.Windsor.IWindsorContainer, Castle.Windsor"" = windsorContainerSerializer
                           ""Castle.Windsor.WindsorContainer, Castle.Windsor"" = windsorContainerSerializer
                           ""Akka.DI.CastleWindsor.WindsorDependencyResolver, Akka.DI.CastleWindsor"" = windsorDependencyResolver           
                        } 

                    }


                   }");

            /// <inheritdoc />
            protected override void RegisterComponents(ContainerBuilder container)
            {
                container.RegisterAssemblyTypes(typeof(ParcelManagerActor).Assembly).Where(t => t.IsSubclassOf(typeof(ActorBase)));
                container.RegisterAssemblyTypes(typeof(Core.Installer).Assembly).Where(t => t.IsSubclassOf(typeof(ActorBase)));
                container.RegisterType<TestIntEnveloper>().As<INotificationEnveloper>();
                container.RegisterType<TestInt2Enveloper>().As<INotificationEnveloper>();
                container.RegisterType<TestDoubleEnveloper>().As<INotificationEnveloper>();
            }
        }

        /// <summary>
        /// The test notification envelope
        /// </summary>
        private class NotificationEnvelope
        {
            /// <summary>
            /// Gets or sets the notification
            /// </summary>
            public ParcelNotification Notification { get; set; }

            /// <summary>
            /// Gets or sets the mark of enveloper
            /// </summary>
            public string Tag { get; set; }
        }

        /// <summary>
        /// The test notification enveloper
        /// </summary>
        [UsedImplicitly]
        private class TestIntEnveloper : INotificationEnveloper
        {
            /// <inheritdoc />
            public decimal Priority => 1M;

            /// <inheritdoc />
            public object Envelope(Parcel parcel, ParcelNotification notification)
            {
                return parcel.Payload is int 
                    ? new NotificationEnvelope { Notification = notification, Tag = "int" } 
                    : null;
            }
        }

        /// <summary>
        /// The test notification enveloper
        /// </summary>
        [UsedImplicitly]
        private class TestInt2Enveloper : INotificationEnveloper
        {
            /// <inheritdoc />
            public decimal Priority => 0M;

            /// <inheritdoc />
            public object Envelope(Parcel parcel, ParcelNotification notification)
            {
                return parcel.Payload is int
                    ? new NotificationEnvelope { Notification = notification, Tag = "int2" }
                    : null;
            }
        }

        /// <summary>
        /// The test notification enveloper
        /// </summary>
        [UsedImplicitly]
        private class TestDoubleEnveloper : INotificationEnveloper
        {
            /// <inheritdoc />
            public decimal Priority => 0M;

            /// <inheritdoc />
            public object Envelope(Parcel parcel, ParcelNotification notification)
            {
                return parcel.Payload is double
                    ? new NotificationEnvelope { Notification = notification, Tag = "double" }
                    : null;
            }
        }
    }
}