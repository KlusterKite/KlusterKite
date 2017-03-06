// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiProviderPublishResolveIntegration.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing publishing and resolving integration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Tests.GraphQL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ClusterKit.API.Client;
    using ClusterKit.API.Tests.Mock;
    using ClusterKit.Security.Client;
    using ClusterKit.Web.GraphQL.Publisher;

    using global::GraphQL;
    using global::GraphQL.Http;
    using global::GraphQL.Utilities;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Xunit;
    using Xunit.Abstractions;

    using ApiProvider = ClusterKit.API.Provider.ApiProvider;

    /// <summary>
    /// Testing publishing and resolving integration
    /// </summary>
    public class ApiProviderPublishResolveIntegration
    {
        /// <summary>
        /// The output.
        /// </summary>
        private readonly ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiProviderPublishResolveIntegration"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public ApiProviderPublishResolveIntegration(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// Removes formatting values from response json
        /// </summary>
        /// <param name="response">The json string</param>
        /// <returns>Cleaned json string</returns>
        public static string CleanResponse(string response)
        {
            return JsonConvert.DeserializeObject<JObject>(response).ToString(Formatting.None);
        }

        /// <summary>
        /// Testing connection query request from <see cref="ApiDescription"/>
        /// </summary>
        /// <returns>Async task</returns>
        [Fact]
        public async Task ConnectionQueryTest()
        {
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

            var internalApiProvider = new API.Tests.Mock.TestProvider(initialObjects);
            var publishingProvider = new TestProvider(internalApiProvider, this.output);
            var schema = SchemaGenerator.Generate(new List<Web.GraphQL.Publisher.ApiProvider> { publishingProvider });

            var query = @"
            {                
                api {
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

            var result = await new DocumentExecuter().ExecuteAsync(
                             r =>
                                 {
                                     r.Schema = schema;
                                     r.Query = query;
                                     r.UserContext = new RequestContext();
                                 }).ConfigureAwait(true);
            var response = new DocumentWriter(true).Write(result);
            this.output.WriteLine(response);
            var expectedResult = @"
                            {
                              ""data"": {
                                ""api"": {
                                  ""connection"": {
                                    ""count"": 4,
                                    ""edges"": [
                                      {
                                        ""cursor"": ""67885ba0-b284-438f-8393-ee9a9eb299d1"",
                                        ""node"": {
                                          ""id"": ""{\""p\"":[{\""f\"":\""connection\""}],\""api\"":\""TestApi\"",\""id\"":\""67885ba0-b284-438f-8393-ee9a9eb299d1\""}"",
                                          ""__id"": ""67885ba0-b284-438f-8393-ee9a9eb299d1"",
                                          ""name"": ""3-test"",
                                          ""value"": 50.0
                                        }
                                      },
                                      {
                                        ""cursor"": ""3af2c973-d985-4f95-a0c7-aa928d276881"",
                                        ""node"": {
                                          ""id"": ""{\""p\"":[{\""f\"":\""connection\""}],\""api\"":\""TestApi\"",\""id\"":\""3af2c973-d985-4f95-a0c7-aa928d276881\""}"",
                                          ""__id"": ""3af2c973-d985-4f95-a0c7-aa928d276881"",
                                          ""name"": ""4-test"",
                                          ""value"": 70.0
                                        }
                                      }
                                    ]
                                  }
                                }
                              }";
             Assert.Equal(CleanResponse(expectedResult), CleanResponse(response));
        }

        /// <summary>
        /// Testing simple fields requests from <see cref="ApiDescription"/>
        /// </summary>
        /// <returns>Async task</returns>
        [Fact]
        public async Task NodeQueryTest()
        {
            var initialObjects = new List<TestObject>
                                     {
                                         new TestObject
                                             {
                                                 Id =
                                                     Guid.Parse(
                                                         "67885ba0-b284-438f-8393-ee9a9eb299d1"),
                                                 Name = "1-test",
                                                 Value = 100m
                                             }
                                     };

            var internalApiProvider = new API.Tests.Mock.TestProvider(initialObjects);
            var publishingProvider = new TestProvider(internalApiProvider, this.output);
            var schema = SchemaGenerator.Generate(new List<Web.GraphQL.Publisher.ApiProvider> { publishingProvider });

            var query = @"
            { 
                node(id: ""{\""p\"":[{\""f\"":\""connection\""}],\""api\"":\""TestApi\"",\""id\"":\""67885ba0-b284-438f-8393-ee9a9eb299d1\""}"") {
                    ...FO
                }

                fragmentFO on TestApi_TestObject_Node {
                    __id,
                    id,
                    name,
                    value
                }            
            }            
            ";
           
            /*
            var query = @"
            { 
                node(id: ""{\""p\"":[{\""f\"":\""connection\""}],\""api\"":\""TestApi\"",\""id\"":\""67885ba0-b284-438f-8393-ee9a9eb299d1\""}"") {
                   __typename,
                   id
                }

                           
            }            
            ";*/

            var result = await new DocumentExecuter().ExecuteAsync(
                             r =>
                             {
                                 r.Schema = schema;
                                 r.Query = query;
                                 r.UserContext = new RequestContext();
                             }).ConfigureAwait(true);
            var response = new DocumentWriter(true).Write(result);
            this.output.WriteLine(response);
            return;
            var expectedResult = @"
                            {
                              ""data"": {
                                ""api"": {
                                  ""connection"": {
                                    ""count"": 4,
                                    ""edges"": [
                                      {
                                        ""cursor"": ""67885ba0-b284-438f-8393-ee9a9eb299d1"",
                                        ""node"": {
                                          ""id"": ""{\""p\"":[{\""f\"":\""connection\""}],\""api\"":\""TestApi\"",\""id\"":\""67885ba0-b284-438f-8393-ee9a9eb299d1\""}"",
                                          ""__id"": ""67885ba0-b284-438f-8393-ee9a9eb299d1"",
                                          ""name"": ""3-test"",
                                          ""value"": 50.0
                                        }
                                      },
                                      {
                                        ""cursor"": ""3af2c973-d985-4f95-a0c7-aa928d276881"",
                                        ""node"": {
                                          ""id"": ""{\""p\"":[{\""f\"":\""connection\""}],\""api\"":\""TestApi\"",\""id\"":\""3af2c973-d985-4f95-a0c7-aa928d276881\""}"",
                                          ""__id"": ""3af2c973-d985-4f95-a0c7-aa928d276881"",
                                          ""name"": ""4-test"",
                                          ""value"": 70.0
                                        }
                                      }
                                    ]
                                  }
                                }
                              }";
            Assert.Equal(CleanResponse(expectedResult), CleanResponse(response));
        }

        /// <summary>
        /// Testing simple fields requests from <see cref="ApiDescription"/>
        /// </summary>
        /// <returns>Async task</returns>
        [Fact]
        public async Task MethodsRequestTest()
        {
            var internalApiProvider = new API.Tests.Mock.TestProvider();
            var publishingProvider = new TestProvider(internalApiProvider, this.output);
            var schema = SchemaGenerator.Generate(new List<Web.GraphQL.Publisher.ApiProvider> { publishingProvider });

            var query = @"
            {                
                api {
                    syncScalarMethod(intArg: 1, stringArg: ""test"", objArg: { syncScalarField: ""nested test"" }),
                    asyncObjectMethod(intArg: 1, stringArg: ""test"", objArg: { syncScalarField: ""nested test"" }, intArrayArg: [7, 8, 9]) {
                        syncScalarField,
                        asyncScalarField
                    },
                }
            }
            ";

            var result = await new DocumentExecuter().ExecuteAsync(
                             r =>
                                 {
                                     r.Schema = schema;
                                     r.Query = query;
                                     r.UserContext = new RequestContext();
                                 }).ConfigureAwait(true);
            var response = new DocumentWriter(true).Write(result);
            this.output.WriteLine(response);

            var expectedResult = @"
                        {
                          ""data"": {
                            ""api"": {
                              ""syncScalarMethod"": ""ok"",
                              ""asyncObjectMethod"": {
                                ""syncScalarField"": ""returned type"",
                                ""asyncScalarField"": ""AsyncScalarField""
                              }
                            }
                          }
                        }";
            Assert.Equal(CleanResponse(expectedResult), CleanResponse(response));
        }

        /// <summary>
        /// Testing call of simple mutation
        /// </summary>
        /// <returns>Async task</returns>
        [Fact]
        public async Task MutationConnectionRequestTest()
        {
            var internalApiProvider = new API.Tests.Mock.TestProvider();
            var publishingProvider = new TestProvider(internalApiProvider, this.output);
            var schema = SchemaGenerator.Generate(new List<Web.GraphQL.Publisher.ApiProvider> { publishingProvider });

            var query = @"                          
            mutation M {
                    call: TestApi_connection_create(newNode: {name: ""hello world"", value: 10}) {
                        result {
                            name,
                            value
                        },
                        errors {
                            field,
                            message
                        }
                    }
            }            
            ";

            var result = await new DocumentExecuter().ExecuteAsync(
                             r =>
                                 {
                                     r.Schema = schema;
                                     r.Query = query;
                                     r.UserContext = new RequestContext();
                                 }).ConfigureAwait(true);
            var response = new DocumentWriter(true).Write(result);
            this.output.WriteLine(response);

            var expectedResult = @"
                        {
                          ""data"": {
                            ""call"": {     
                                ""result"": {                              
                                    ""name"": ""hello world"",
                                    ""value"": 10.0
                                },
                                ""errors"": []
                            }
                          }
                        }";
            Assert.Equal(CleanResponse(expectedResult), CleanResponse(response));
        }

        /// <summary>
        /// Testing call of simple mutation
        /// </summary>
        /// <returns>Async task</returns>
        [Fact]
        public async Task MutationSimpleRequestTest()
        {
            var internalApiProvider = new API.Tests.Mock.TestProvider();
            var publishingProvider = new TestProvider(internalApiProvider, this.output);
            var schema = SchemaGenerator.Generate(new List<Web.GraphQL.Publisher.ApiProvider> { publishingProvider });

            var query = @"                          
            mutation M {
                    call: TestApi_nestedAsync_setName(name: ""hello world"") {
                        name
                    }
            }            
            ";

            var result = await new DocumentExecuter().ExecuteAsync(
                             r =>
                                 {
                                     r.Schema = schema;
                                     r.Query = query;
                                     r.UserContext = new RequestContext();
                                 }).ConfigureAwait(true);
            var response = new DocumentWriter(true).Write(result);
            this.output.WriteLine(response);

            var expectedResult = @"
                        {
                          ""data"": {
                            ""call"": {
                                ""name"": ""hello world""
                            }
                          }
                        }";
            Assert.Equal(CleanResponse(expectedResult), CleanResponse(response));
        }

        /// <summary>
        /// Testing correct schema generation from generated <see cref="ApiDescription"/>
        /// </summary>
        /// <returns>Async task</returns>
        [Fact]
        public async Task SchemaGenerationTest()
        {
            var internalApiProvider = new API.Tests.Mock.TestProvider();
            var publishingProvider = new TestProvider(internalApiProvider, this.output);

            var schema = SchemaGenerator.Generate(new List<Web.GraphQL.Publisher.ApiProvider> { publishingProvider });

            using (var printer = new SchemaPrinter(schema))
            {
                var description = printer.Print();
                this.output.WriteLine("-------- Schema -----------");
                this.output.WriteLine(description);
                Assert.False(string.IsNullOrWhiteSpace(description));
            }

            Assert.NotNull(schema.Query);
            Assert.Equal(2, schema.Query.Fields.Count());
            Assert.True(schema.Query.HasField("api"));

            var result = await new DocumentExecuter().ExecuteAsync(
                             r =>
                                 {
                                     r.Schema = schema;
                                     r.Query = Resources.IntrospectionQuery;
                                     r.UserContext = new RequestContext();
                                 }).ConfigureAwait(true);
            var response = new DocumentWriter(true).Write(result);
            this.output.WriteLine(response);
            Assert.Equal(CleanResponse(Resources.ApiProviderResolveTestProviderSchemaSnapshot), CleanResponse(response));
        }

        /// <summary>
        /// Testing simple fields requests from <see cref="ApiDescription"/>
        /// </summary>
        /// <returns>Async task</returns>
        [Fact]
        public async Task SimpleFieldsRequestTest()
        {
            var internalApiProvider = new API.Tests.Mock.TestProvider();
            var publishingProvider = new TestProvider(internalApiProvider, this.output);
            var schema = SchemaGenerator.Generate(new List<Web.GraphQL.Publisher.ApiProvider> { publishingProvider });

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
                    },
                    syncEnumField,
                    syncFlagsField
                }
            }
            ";

            var result = await new DocumentExecuter().ExecuteAsync(
                             r =>
                                 {
                                     r.Schema = schema;
                                     r.Query = query;
                                     r.UserContext = new RequestContext();
                                 }).ConfigureAwait(true);
            var response = new DocumentWriter(true).Write(result);
            this.output.WriteLine(response);

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
                              },
                              ""syncEnumField"": ""EnumItem1"",
                              ""syncFlagsField"": 1,
                            }
                          }
                        }
                        ";
            Assert.Equal(CleanResponse(expectedResult), CleanResponse(response));
        }

        /// <summary>
        /// Testing requests with use of fragments from <see cref="ApiDescription"/>
        /// </summary>
        /// <returns>Async task</returns>
        [Fact]
        public async Task RequestWithFragmentsTest()
        {
            var internalApiProvider = new API.Tests.Mock.TestProvider();
            var publishingProvider = new TestProvider(internalApiProvider, this.output);
            var schema = SchemaGenerator.Generate(new List<Web.GraphQL.Publisher.ApiProvider> { publishingProvider });

            var query = @"
            query FragmentsRequest {                
                api {
                   ...F
                }
            }

            fragment F on TestApi_TestApi {
                    syncScalarField
            }
            ";

            var result = await new DocumentExecuter().ExecuteAsync(
                             r =>
                             {
                                 r.Schema = schema;
                                 r.Query = query;
                                 r.UserContext = new RequestContext();
                             }).ConfigureAwait(true);
            var response = new DocumentWriter(true).Write(result);
            this.output.WriteLine(response);

            var expectedResult = @"
                        {
                          ""data"": {
                            ""api"": {
                              ""syncScalarField"": ""SyncScalarField""
                            }
                          }
                        }
                        ";
            Assert.Equal(CleanResponse(expectedResult), CleanResponse(response));
        }

        /// <summary>
        /// Testing requests with use of fragments from <see cref="ApiDescription"/>
        /// </summary>
        /// <returns>Async task</returns>
        [Fact]
        public async Task RequestWithAliasesTest()
        {
            var internalApiProvider = new API.Tests.Mock.TestProvider();
            var publishingProvider = new TestProvider(internalApiProvider, this.output);
            var schema = SchemaGenerator.Generate(new List<Web.GraphQL.Publisher.ApiProvider> { publishingProvider });

            var query = @"
            query FragmentsRequest {                
                api {
                   test1: syncScalarField,
                   test2: syncScalarField,
                   nestedSync {
                        test3: syncScalarField,
                        test4: syncScalarField
                   }
                }
            }
            
            ";

            var result = await new DocumentExecuter().ExecuteAsync(
                             r =>
                             {
                                 r.Schema = schema;
                                 r.Query = query;
                                 r.UserContext = new RequestContext();
                             }).ConfigureAwait(true);
            var response = new DocumentWriter(true).Write(result);
            this.output.WriteLine(response);

            var expectedResult = @"
                        {
                          ""data"": {
                            ""api"": {
                              ""test1"": ""SyncScalarField"",
                              ""test2"": ""SyncScalarField"",
                              ""nestedSync"": {
                                  ""test3"": ""SyncScalarField"",
                                  ""test4"": ""SyncScalarField""
                              }
                            }
                          }
                        }
                        ";

            Assert.Equal(
                CleanResponse(expectedResult), 
                CleanResponse(response));
        }

        /// <summary>
        /// Test provider to unite GraphQL publishing and <see cref="API.Provider.ApiProvider"/>
        /// </summary>
        public class TestProvider : Web.GraphQL.Publisher.ApiProvider
        {
            /// <summary>
            /// The test output
            /// </summary>
            private readonly ITestOutputHelper output;

            /// <summary>
            /// The provider.
            /// </summary>
            private readonly ApiProvider provider;

            /// <summary>
            /// Initializes a new instance of the <see cref="TestProvider"/> class.
            /// </summary>
            /// <param name="provider">
            /// The provider.
            /// </param>
            /// <param name="output">
            /// The test output.
            /// </param>
            public TestProvider(ApiProvider provider, ITestOutputHelper output)
            {
                this.provider = provider;
                this.Description = provider.ApiDescription;
                this.output = output;
            }

            /// <inheritdoc />
            public override async Task<JObject> GetData(List<ApiRequest> requests, RequestContext context)
            {
                var mutations = requests.OfType<MutationApiRequest>().ToList();
                if (mutations.Count > 0)
                {
                    var result = new JObject();
                    foreach (var mutation in mutations)
                    {
                        var midResult = await this.provider.ResolveMutation(
                                            mutation,
                                            context,
                                            exception =>
                                                this.output.WriteLine(
                                                    $"Resolve error: {exception.Message}\n{exception.StackTrace}"));
                        result.Merge(midResult);
                    }

                    return result;
                }

                return await this.provider.ResolveQuery(
                           requests,
                           context,
                           exception =>
                               this.output.WriteLine($"Resolve error: {exception.Message}\n{exception.StackTrace}"));
            }

            /// <inheritdoc />
            public override async Task<JObject> SearchNode(
                string id,
                List<RequestPathElement> path,
                ApiRequest request,
                RequestContext context)
            {
                return await this.provider.SearchNode(
                           id,
                           path.Select(p => p.ToApiRequest()).ToList(),
                           request,
                           context,
                           exception =>
                               this.output.WriteLine($"Resolve error: {exception.Message}\n{exception.StackTrace}"));
            }
        }
    }
}