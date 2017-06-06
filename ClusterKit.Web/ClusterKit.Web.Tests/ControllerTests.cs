// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ControllerTests.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Tests that ApiControllers are working
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Tests
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    using Akka.Configuration;

    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ClusterKit.Core;
    using ClusterKit.Core.TestKit;

    using RestSharp;
    
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Tests that ApiControllers are working
    /// </summary>
    public class ControllerTests : BaseActorTest<ControllerTests.Configurator>
    {
        /// <inheritdoc />
        public ControllerTests(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Current web server bind port
        /// </summary>
        private int Port => this.Sys.Settings.Config.GetInt("ClusterKit.Web.WebHostPort");

        /// <summary>
        /// Testing published controller method
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task RequestTest()
        {
            this.ExpectNoMsg();

            var client = new RestClient($"http://localhost:{this.Port}/testController") { Timeout = 5000 };
            var request = new RestRequest { Method = Method.GET, Resource = "method" };
            request.AddHeader("Accept", "application/json, text/json");
            var result = await client.ExecuteTaskAsync(request);

            Assert.Equal(ResponseStatus.Completed, result.ResponseStatus);
            this.Sys.Log.Info("Response: {Response}", result.Content);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("Hello world", result.Content);
        }

        /// <summary>
        /// The test configurator
        /// </summary>
        public class Configurator : TestConfigurator
        {
            /// <inheritdoc />
            public override bool RunPostStart => true;

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
                var listener = new TcpListener(IPAddress.Any, 0);
                listener.Start();
                var port = ((IPEndPoint)listener.LocalEndpoint).Port;
                listener.Stop();

                return ConfigurationFactory.ParseString(
                        $@"
                {{
                    ClusterKit {{
 		                Web {{
                            WebHostPort = {port},
 			                BindAddress = ""http://*:{port}"",
                            Debug.Trace = true
                        }}
                    }}
                }}")
                    .WithFallback(base.GetAkkaConfig(windsorContainer));
            }

            /// <summary>
            /// Gets list of all used plugin installers
            /// </summary>
            /// <returns>The list of installers</returns>
            public override List<BaseInstaller> GetPluginInstallers()
            {
                var installers = base.GetPluginInstallers();
                installers.Add(new Descriptor.Installer());
                installers.Add(new Web.Installer());
                installers.Add(new TestInstaller());
                return installers;
            }
        }

        /// <summary>
        /// The test installer to register components
        /// </summary> 
        public class TestInstaller : BaseInstaller
        {
            /// <inheritdoc />
            protected override decimal AkkaConfigLoadPriority => PriorityClusterRole;

            /// <inheritdoc />
            protected override Config GetAkkaConfig()
            {
                return Config.Empty;
            }

            /// <inheritdoc />
            protected override void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store)
            {
            }
        }
    }
}