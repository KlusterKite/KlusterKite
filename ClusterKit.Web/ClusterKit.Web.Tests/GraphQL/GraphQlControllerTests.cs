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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    using Akka.Configuration;

    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ClusterKit.API.Tests.Mock;
    using ClusterKit.Core;
    using ClusterKit.Core.TestKit;
    using ClusterKit.Security.Attributes;
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
        private readonly ITestOutputHelper output;

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
        /// Current bind port
        /// </summary>
        private int OwinPort => this.Sys.Settings.Config.GetInt("ClusterKit.Web.WebHostPort");

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
            var internalApiProvider = new TestProvider();
            var publishingProvider = new DirectProvider(internalApiProvider, this.output.WriteLine) { UseJsonRepack = true };
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
        /// Some load testing
        /// </summary>
        [Fact(Skip = "Just for profiling")]
        public void LoadTest()
        {
            this.ExpectNoMsg();
            var initialObjects = new List<TestObject>
                                     {
                                         new TestObject
                                             {
                                                 Id =
                                                     Guid.Parse(
                                                         "{3BEEE369-11DF-4A30-BF11-1D8465C87110}"),
                                                 Name = "1-test",
                                                 Value = 100m
                                             },
                                         new TestObject
                                             {
                                                 Id =
                                                     Guid.Parse(
                                                         "{B500CA20-F649-4DCD-BDA8-1FA5031ECDD3}"),
                                                 Name = "2-test",
                                                 Value = 50m
                                             },
                                         new TestObject
                                             {
                                                 Id =
                                                     Guid.Parse(
                                                         "{67885BA0-B284-438F-8393-EE9A9EB299D1}"),
                                                 Name = "3-test",
                                                 Value = 50m
                                             },
                                         new TestObject
                                             {
                                                 Id =
                                                     Guid.Parse(
                                                         "{3AF2C973-D985-4F95-A0C7-AA928D276881}"),
                                                 Name = "4-test",
                                                 Value = 70m
                                             },
                                         new TestObject
                                             {
                                                 Id =
                                                     Guid.Parse(
                                                         "{F0607502-5B77-4A3C-9142-E6197A7EE61E}"),
                                                 Name = "5-test",
                                                 Value = 6m
                                             },
                                     };

            var internalApiProvider = new TestProvider(initialObjects);
            var publishingProvider = new DirectProvider(internalApiProvider, this.output.WriteLine) { UseJsonRepack = true };
            var schemaProvider = this.WindsorContainer.Resolve<SchemaProvider>();
            schemaProvider.CurrentSchema = SchemaGenerator.Generate(new List<ApiProvider> { publishingProvider });

            var client = new RestClient($"http://localhost:{this.OwinPort}/api/1.x/graphQL/") { Timeout = 5000 };

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
                    forwardedArray,
                    syncArrayOfScalarField,
                    nestedSync {
                        asyncScalarField,
                        syncScalarField  
                    },
                    syncScalarField,
                    syncEnumField,
                    syncFlagsField,
                    connection(sort: [value_asc, name_asc], filter: {value_gt: 10}, offset: 1, limit: 2) {
                            count,
                            edges {
                                cursor,
                                node {
                                    id,
                                    __id,
                                    name,
                                    value
                                }                    
                            }
                        }
                }
            }                
            ";

            var stopwatch = new Stopwatch();
            var request = new RestRequest { Method = Method.POST };
            request.AddHeader("Accept", "application/json, text/json");
            var body = $"{{\"query\": {JsonConvert.SerializeObject(query)}, \"variables\": null}}";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            stopwatch.Start();
            var result = client.ExecuteTaskAsync(request).Result;
            stopwatch.Stop();
            Assert.Equal(ResponseStatus.Completed, result.ResponseStatus);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            this.Sys.Log.Info("!!!!!!!!!! First request served in {StopWatch}ms", (double)stopwatch.ElapsedTicks * 1000 / Stopwatch.Frequency);
            stopwatch.Reset();
            stopwatch.Start();
            client.ExecuteTaskAsync(request).Wait();
            stopwatch.Stop();
            this.Sys.Log.Info("!!!!!!!!!! Second request served in {StopWatch}ms", (double)stopwatch.ElapsedTicks * 1000 / Stopwatch.Frequency);
            stopwatch.Reset();
            stopwatch.Start();

            var numberOfRequests = 3000;
            var options = new ParallelOptions
                              {
                                  MaxDegreeOfParallelism = 10
                              };
            Parallel.ForEach(
                Enumerable.Range(1, numberOfRequests),
                options,
                i => client.Execute(request));

            // var tasks = Enumerable.Range(1, numberOfRequests).Select<int, Task>(n => client.ExecuteTaskAsync(request));
            // Task.WaitAll(tasks.ToArray());
            stopwatch.Stop();
            this.Sys.Log.Info(
                "!!!!!!!!!! Served {numberOfRequests} requests in {StopWatch}ms ({StopWatchRequest}ms/req)",
                numberOfRequests,
                (double)stopwatch.ElapsedTicks * 1000 / Stopwatch.Frequency,
                (double)stopwatch.ElapsedTicks * 1000 / numberOfRequests / Stopwatch.Frequency);

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
            var internalApiProvider = new TestProvider();
            var publishingProvider = new DirectProvider(internalApiProvider, this.output.WriteLine) { UseJsonRepack = true };
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
            this.Sys.Log.Info("Response {Response}", result.Content);
            Assert.Equal(ResponseStatus.Completed, result.ResponseStatus);
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
                              ""faultedSyncField"": null,
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
                              ""faultedASyncMethod"": null
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
                            WebHostPort = {port},
 			                BindAddress = ""http://*:{port}"",
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
                return ConfigurationFactory.ParseString("{ ClusterKit.Web.Debug.Trace = true }");
            }

            /// <inheritdoc />
            protected override void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store)
            {
                container.Register(Component.For<ITokenManager>().ImplementedBy<MoqTokenManager>().LifestyleSingleton());
            }
        }
    }
}