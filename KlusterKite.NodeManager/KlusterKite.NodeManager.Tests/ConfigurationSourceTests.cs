﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationSourceTests.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Tests for KlusterKite.NodeManager.ConfigurationSource
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Tests
{
    using System.Collections.Generic;

    using Akka.Configuration;

    using Autofac;

    using KlusterKite.Core;
    using KlusterKite.Core.TestKit;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Tests for KlusterKite.NodeManager.ConfigurationSource
    /// </summary>
    [Collection("KlusterKite.NodeManager.Tests.ConfigurationContext")]
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
            Assert.Equal("Test.NodeTemplateSeeder, Test", this.Sys.Settings.Config.GetString("KlusterKite.NodeManager.ConfigurationSeederType"));
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
                var pluginInstallers = new List<BaseInstaller>
                                           {
                                               new Core.TestKit.Installer(),
                                               new TestInstaller(),
                                               new Core.Installer(),
                                               new ConfigurationSource.Installer()
                                           };
                return pluginInstallers;
            }
        }

        /// <summary>
        /// Replaces production data sources with the test ones
        /// </summary>
        private class TestInstaller : BaseInstaller
        {
            /// <inheritdoc />
            protected override decimal AkkaConfigLoadPriority => PriorityClusterRole;

            /// <inheritdoc />
            protected override Config GetAkkaConfig() => ConfigurationFactory.ParseString(@"
            {
                KlusterKite.NodeManager.ConfigurationSeederType = ""Test.NodeTemplateSeeder, Test""
            }");

            /// <inheritdoc />
            protected override void RegisterComponents(ContainerBuilder container, Config config)
            {
            }
        }
    }
}