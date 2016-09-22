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

    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ClusterKit.Core;
    using ClusterKit.Core.TestKit;
    using ClusterKit.LargeObjects.Client;

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

            var receivedPayload = await notification.Receive(this.Sys) as string;
            Assert.Equal(payload, receivedPayload);

            // checking that you cannot receive parcel twice
            try
            {
                receivedPayload = await notification.Receive(this.Sys) as string;
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
                var receivedPayload = await notification.Receive(this.Sys) as string;
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
                        serializers {
                            wire = ""Akka.Serialization.WireSerializer, Akka.Serialization.Wire""
                        }
                        serialization-bindings {
                            ""System.Object"" = wire
                        }
                    }


                   }");

            /// <summary>
            /// Registering DI components
            /// </summary>
            /// <param name="container">The container.</param>
            /// <param name="store">The configuration store.</param>
            protected override void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store)
            {
                container.Register(Classes.FromAssemblyContaining<ParcelManagerActor>().Where(t => t.IsSubclassOf(typeof(ActorBase))).LifestyleTransient());
                container.Register(Classes.FromAssemblyContaining<Core.Installer>().Where(t => t.IsSubclassOf(typeof(ActorBase))).LifestyleTransient());
            }
        }
    }
}