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
    using ClusterKit.Web.GraphQL.Publisher;

    using Newtonsoft.Json;

    using RestSharp;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Testing authorization process
    /// </summary>
    public class GraphQlControllerTests : BaseActorTest<GraphQlControllerTests.Configurator>
    {
        /// <summary>
        /// The test output
        /// </summary>
        private ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQlControllerTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public GraphQlControllerTests(ITestOutputHelper output)
            : base(output)
        {
            this.output = output;
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
        public async Task SchemaUninitializedTest()
        {
            this.ExpectNoMsg();

            var client = new RestClient($"http://localhost:{this.OwinPort}/api/1.x/graphQL/") { Timeout = 5000 };

            var request = new RestRequest { Method = Method.POST };
            request.AddHeader("Accept", "application/json, text/json");

            var query = @"
            {                
                api {
                    asyncArrayOfScalarField,
                    asyncForwardedScalar,
                    nestedAsync {
                        asyncScalarField,
                        syncScalarField                        
                    },
                    asyncScalarField,
                    faultedSyncField,
                    forwardedArray,
                    syncArrayOfScalarField,
                    nestedSync {
                        asyncScalarField,
                        syncScalarField  
                    },
                    syncScalarField,
                    faultedASyncMethod {
                        asyncScalarField,
                        syncScalarField 
                    }
                }
            }";

            request.AddJsonBody(new EndpointController.QueryRequest { Query = query });

            var result = await client.ExecuteTaskAsync(request);

            Assert.Equal(ResponseStatus.Completed, result.ResponseStatus);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, result.StatusCode);
            this.Sys.Log.Info("Response {Response}", result.Content);
        }

        /// <summary>
        /// Just generic test
        /// </summary>
        /// <param name="variables">
        /// The raw variables representation.
        /// </param>
        /// <returns>
        /// The async task
        /// </returns>
        [Theory]
        [InlineData("null")]
        [InlineData("{}")]
        [InlineData("\"{}\"")]
        public async Task EmptyVariablesTest(string variables)
        {
            this.ExpectNoMsg();
            var internalApiProvider = new API.Tests.Mock.TestProvider();
            var publishingProvider = new ApiProviderPublishResolveIntegration.TestProvider(internalApiProvider, this.output);
            var schemaProvider = this.WindsorContainer.Resolve<SchemaProvider>();
            schemaProvider.CurrentSchema = SchemaGenerator.Generate(new List<ApiProvider> { publishingProvider });

            var client = new RestClient($"http://localhost:{this.OwinPort}/api/1.x/graphQL/") { Timeout = 5000 };

            var request = new RestRequest { Method = Method.POST };
            request.AddHeader("Accept", "application/json, text/json");
            
            var query = @"
            {                
                api {
                    syncScalarField
                }
            }";

            var body = $"{{\"query\": {JsonConvert.SerializeObject(query)}, \"variables\": {variables}}}";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            var result = await client.ExecuteTaskAsync(request);
            Assert.Equal(ResponseStatus.Completed, result.ResponseStatus);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            this.Sys.Log.Info("Response {Response}", result.Content);
        }

        /// <summary>
        /// Just generic test
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task SchemaInitializedTest()
        {
            this.ExpectNoMsg();
            var internalApiProvider = new API.Tests.Mock.TestProvider();
            var publishingProvider = new ApiProviderPublishResolveIntegration.TestProvider(internalApiProvider, this.output);
            var schemaProvider = this.WindsorContainer.Resolve<SchemaProvider>();
            schemaProvider.CurrentSchema = SchemaGenerator.Generate(new List<ApiProvider> { publishingProvider });

            var client = new RestClient($"http://localhost:{this.OwinPort}/api/1.x/graphQL/") { Timeout = 5000 };

            var request = new RestRequest { Method = Method.POST };
            request.AddHeader("Accept", "application/json, text/json");

            var query = @"
            {                
                api {
                    asyncArrayOfScalarField,
                    asyncForwardedScalar,
                    nestedAsync {
                        asyncScalarField,
                        syncScalarField                        
                    },
                    asyncScalarField,
                    faultedSyncField,
                    forwardedArray,
                    syncArrayOfScalarField,
                    nestedSync {
                        asyncScalarField,
                        syncScalarField  
                    },
                    syncScalarField,
                    faultedASyncMethod {
                        asyncScalarField,
                        syncScalarField 
                    }
                }
            }";

            request.AddJsonBody(new EndpointController.QueryRequest { Query = query });

            var result = await client.ExecuteTaskAsync(request);

            Assert.Equal(ResponseStatus.Completed, result.ResponseStatus);
            this.Sys.Log.Info("Response {Response}", result.Content);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            var expectedResult = @"
                        {
                          ""data"": {
                            ""api"": {
                              ""asyncArrayOfScalarField"": [
                                4.0,
                                5.0
                              ],
                              ""asyncForwardedScalar"": ""AsyncForwardedScalar"",
                              ""nestedAsync"": {
                                ""asyncScalarField"": ""AsyncScalarField"",
                                ""syncScalarField"": ""SyncScalarField""
                              },
                              ""asyncScalarField"": ""AsyncScalarField"",
                              ""faultedSyncField"": false,
                              ""forwardedArray"": [
                                5,
                                6,
                                7
                              ],
                              ""syncArrayOfScalarField"": [
                                1,
                                2,
                                3
                              ],
                              ""nestedSync"": {
                                ""asyncScalarField"": ""AsyncScalarField"",
                                ""syncScalarField"": ""SyncScalarField""
                              },
                              ""syncScalarField"": ""SyncScalarField"",
                              ""faultedASyncMethod"": {
                                ""asyncScalarField"": null,
                                ""syncScalarField"": null
                              }
                            }
                          }
                        }
                        ";
            Assert.Equal(
                ApiProviderPublishResolveIntegration.CleanResponse(expectedResult), 
                ApiProviderPublishResolveIntegration.CleanResponse(result.Content));
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