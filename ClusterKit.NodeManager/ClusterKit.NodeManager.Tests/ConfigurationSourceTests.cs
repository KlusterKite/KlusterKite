using System.Collections.Generic;

namespace ClusterKit.NodeManager.Tests
{
    using Akka.Configuration;

    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ClusterKit.Core;
    using ClusterKit.Core.TestKit;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Tests for ClusterKit.NodeManager.ConfigurationSource
    /// </summary>
    public class ConfigurationSourceTests : BaseActorTest<ConfigurationSourceTests.Configurator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationSourceTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public ConfigurationSourceTests(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Tests that plugins can override seeder type
        /// </summary>
        [Fact]
        public void SeederOverrideTest()
        {
            Assert.Equal("Test.NodeTemplateSeeder, Test", this.Sys.Settings.Config.GetString("ClusterKit.NodeManager.ConfigurationSeederType"));
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
                var pluginInstallers = new List<BaseInstaller> { new Core.TestKit.Installer(), new TestInstaller(), new Core.Installer(), new NodeManager.ConfigurationSource.Installer() };
                return pluginInstallers;
            }
        }

        /// <summary>
        /// Replaces production datasources with the test ones
        /// </summary>
        private class TestInstaller : BaseInstaller
        {
            protected override decimal AkkaConfigLoadPriority => PriorityClusterRole;

            protected override Config GetAkkaConfig() => ConfigurationFactory.ParseString(@"
            {
                ClusterKit.NodeManager.ConfigurationSeederType = ""Test.NodeTemplateSeeder, Test""
            }");

            protected override void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store)
            {
            }
        }
    }
}