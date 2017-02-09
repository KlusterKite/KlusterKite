// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GraphQlControllerTests.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing authorization process
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ClusterKit.Web.Tests.GraphQL
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    using Akka.Configuration;

    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ClusterKit.Core;
    using ClusterKit.Core.TestKit;
    using ClusterKit.Security.Client;

    using RestSharp;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Testing authorization process
    /// </summary>
    public class GraphQlControllerTests : BaseActorTest<GraphQlControllerTests.Configurator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQlControllerTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public GraphQlControllerTests(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Current owin bind port
        /// </summary>
        private int OwinPort => this.Sys.Settings.Config.GetInt("ClusterKit.Web.OwinPort");

        /// <summary>
        /// Just generic test
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task CheckRequest()  
        {
            this.ExpectNoMsg();

            var client = new RestClient($"http://localhost:{this.OwinPort}/api/1.x/graphQL/") { Timeout = 5000 };
            
            var request = new RestRequest { Method = Method.GET, Resource = "test" };
            request.AddHeader("Accept", "application/json, text/json");
            var result = await client.ExecuteTaskAsync(request);

            Assert.Equal(ResponseStatus.Completed, result.ResponseStatus);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            this.Sys.Log.Info("Response {Response}", result.Content);
        }

        /// <summary>
        /// The test configurator
        /// </summary>
        public class Configurator : TestConfigurator
        {
            /// <inheritdoc />
            public override bool RunPostStart => true;

            /// <summary>
            /// Gets list of all used plugin installers
            /// </summary>
            /// <returns>The list of installers</returns>
            public override List<BaseInstaller> GetPluginInstallers()
            {
                var installers = base.GetPluginInstallers();
                installers.Add(new Descriptor.Installer());
                installers.Add(new Web.Installer());
                installers.Add(new Authentication.Installer());
                installers.Add(new Web.GraphQL.Publisher.Installer());
                installers.Add(new TestInstaller());
                return installers;
            }

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

                return ConfigurationFactory.ParseString($@"
                {{
                    ClusterKit {{
 		                Web {{
                            OwinPort = {port},
 			                OwinBindAddress = ""http://*:{port}"",
                            Debug.Trace = true
                        }}
                    }}
                }}").WithFallback(base.GetAkkaConfig(windsorContainer));
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
                container.Register(Component.For<ITokenManager>().ImplementedBy<MoqTokenManager>().LifestyleSingleton());
            }
        }
    }
}