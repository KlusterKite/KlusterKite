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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ClusterKit.Security.Client;
    using ClusterKit.Web.GraphQL.Client;
    using ClusterKit.Web.GraphQL.Publisher;

    using global::GraphQL;
    using global::GraphQL.Http;
    using global::GraphQL.Utilities;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Xunit;
    using Xunit.Abstractions;

    using ApiProvider = ClusterKit.Web.GraphQL.API.ApiProvider;

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
        /// Testing correct schema generation from generated <see cref="ApiDescription"/>
        /// </summary>
        /// <returns>Async task</returns>
        [Fact]
        public async Task SchemaGenerationTest()
        {
            var internalApiProvider = new ApiProviderResolveTests.TestProvider();
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
            Assert.Equal(1, schema.Query.Fields.Count());
            Assert.True(schema.Query.HasField("api"));

            var result = await new DocumentExecuter().ExecuteAsync(
                             r =>
                             {
                                 r.Schema = schema;
                                 r.Query = Resources.IntrospectionQuery;
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
            var internalApiProvider = new ApiProviderResolveTests.TestProvider();
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
                    }
                }
            }
            ";

            var result = await new DocumentExecuter().ExecuteAsync(
                                        r =>
                                        {
                                            r.Schema = schema;
                                            r.Query = query;
                                        }).ConfigureAwait(true);
            var response = new DocumentWriter(true).Write(result);
            this.output.WriteLine(response);
        }

        /// <summary>
        /// Testing simple fields requests from <see cref="ApiDescription"/>
        /// </summary>
        /// <returns>Async task</returns>
        [Fact]
        public async Task MethodsRequestTest()
        {
            var internalApiProvider = new ApiProviderResolveTests.TestProvider();
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
        }


        /// <summary>
        /// Removes formatting values from response json
        /// </summary>
        /// <param name="response">The json string</param>
        /// <returns>Cleaned json string</returns>
        private static string CleanResponse(string response)
        {
            return JsonConvert.DeserializeObject<JObject>(response).ToString(Formatting.None);
        }

        /// <summary>
        /// Test provider to unite GraphQL publishing and <see cref="ApiProvider"/>
        /// </summary>
        private class TestProvider : Web.GraphQL.Publisher.ApiProvider
        {
            /// <summary>
            /// The provider.
            /// </summary>
            private readonly ApiProvider provider;

            /// <summary>
            /// The test output
            /// </summary>
            private readonly ITestOutputHelper output;

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
            public override Task<JObject> GetData(List<ApiRequest> requests, RequestContext context)
            {
                return this.provider.ResolveQuery(
                    requests,
                    context,
                    exception => this.output.WriteLine($"Resolve error: {exception.Message}\n{exception.StackTrace}"));
            }
        }
    }
}
