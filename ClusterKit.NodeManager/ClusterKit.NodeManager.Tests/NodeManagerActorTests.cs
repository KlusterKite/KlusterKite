// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeManagerActorTests.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing node manager actor
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Configuration;
    using Akka.DI.Core;

    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ClusterKit.Core;
    using ClusterKit.Core.Data;
    using ClusterKit.Core.Data.TestKit;
    using ClusterKit.Core.EF;
    using ClusterKit.Core.EF.TestKit;
    using ClusterKit.Core.Ping;
    using ClusterKit.Core.TestKit;
    using ClusterKit.NodeManager.Client.Messages;
    using ClusterKit.NodeManager.ConfigurationSource;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Testing node manager actor
    /// </summary>
    public class NodeManagerActorTests : BaseActorTest<NodeManagerActorTests.Configurator>
    {
        /// <summary>
        /// Access to xunit output
        /// </summary>
        private ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeManagerActorTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public NodeManagerActorTests(ITestOutputHelper output)
            : base(output)
        {
            this.output = output;
        }

        /// <summary>
        /// Tests actor start
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Fact]
        public async Task ActorStartTest()
        {
            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>());
            var response = await testActor.Ask<PongMessage>(new PingMessage(), TimeSpan.FromSeconds(1));
            Assert.NotNull(response);
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
                var pluginInstallers = base.GetPluginInstallers();
                pluginInstallers.Add(new TestInstaller());
                return pluginInstallers;
            }
        }

        /// <summary>
        /// Replaces production datasources with the test ones
        /// </summary>
        public class TestInstaller : BaseInstaller
        {
            protected override decimal AkkaConfigLoadPriority => -1M;

            protected override Config GetAkkaConfig() => ConfigurationFactory.ParseString(@"
            {
                ClusterKit.NodeManager.ConfigurationDatabaseName = ""TestConfigurationDatabase""
            }");

            protected override void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store)
            {
                container.Register(Classes.FromAssemblyContaining<NodeManagerActor>().Where(t => t.IsSubclassOf(typeof(ActorBase))).LifestyleTransient());
                container.Register(Component.For<BaseConnectionManager>().Instance(new TestConnectionManager()).LifestyleSingleton());

                container.Register(Component.For<DataFactory<ConfigurationContext, NodeTemplate, int>>()
                    .Instance(new UniversalTestDataFactory<ConfigurationContext, NodeTemplate, int>(null, o => o.Id)).LifestyleSingleton());
                container.Register(Component.For<DataFactory<ConfigurationContext, NugetFeed, int>>()
                    .Instance(new UniversalTestDataFactory<ConfigurationContext, NugetFeed, int>(null, o => o.Id)).LifestyleSingleton());
                container.Register(Component.For<DataFactory<ConfigurationContext, SeedAddress, int>>()
                    .Instance(new UniversalTestDataFactory<ConfigurationContext, SeedAddress, int>(null, o => o.Id)).LifestyleSingleton());

                container.Register(Component.For<DataFactory<string, PackageDescription, string>>()
                    .Instance(new UniversalTestDataFactory<string, PackageDescription, string>(null, o => o.Id)).LifestyleSingleton());

                container.Register(Component.For<IContextFactory<ConfigurationContext>>().Instance(new TestContextFactory<ConfigurationContext>()).LifestyleSingleton());
            }
        }
    }
}