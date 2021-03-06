﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiPublisherActorTests.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Testing the <see cref="ApiPublisherActor" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Configuration;

    using Autofac;

    using KlusterKite.API.Client.Messages;
    using KlusterKite.API.Endpoint;
    using KlusterKite.API.Provider;
    using KlusterKite.API.Tests.Mock;
    using KlusterKite.Core;
    using KlusterKite.Core.TestKit;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Testing the <see cref="ApiPublisherActor"/>
    /// </summary>
    public class ApiPublisherActorTests : BaseActorTest<ApiPublisherActorTests.Configurator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiPublisherActorTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public ApiPublisherActorTests(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Checking the discovery request
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task DiscoverTest()
        {
            this.Sys.StartNameSpaceActorsFromConfiguration();
            this.ExpectNoMsg();

            var descriptions =
                await this.Sys.ActorSelection("/user/KlusterKite/API/Publisher")
                    .Ask<List<ApiDiscoverResponse>>(new ApiDiscoverRequest(), TimeSpan.FromSeconds(5));

            Assert.NotNull(descriptions);
            Assert.Equal(1, descriptions.Count);

            var description = descriptions.First();

            Assert.NotNull(description);
            Assert.NotNull(description.Handler);
            Assert.NotNull(description.Description);
            Assert.Equal("TestApi", description.Description.ApiName);
            Assert.Equal("Tested API", description.Description.Description);
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
                                               new Core.Installer(),
                                               new Core.TestKit.Installer(),
                                               new Endpoint.Installer(),
                                               new TestInstaller()
                                           };
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
            protected override Config GetAkkaConfig() => ConfigurationFactory.Empty;

            /// <inheritdoc />
            protected override void RegisterComponents(ContainerBuilder builder, Config config)
            {
                builder.RegisterType<TestProvider>().As<ApiProvider>();
            }
        }
    }
}