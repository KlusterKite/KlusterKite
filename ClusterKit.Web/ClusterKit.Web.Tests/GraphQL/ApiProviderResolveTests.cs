// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiProviderResolveTests.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing <see cref="ApiProvider" /> for resolving in various scenarios
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Tests.GraphQL
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using ClusterKit.Security.Client;
    using ClusterKit.Web.GraphQL.API;
    using ClusterKit.Web.GraphQL.Client;
    using ClusterKit.Web.GraphQL.Client.Attributes;

    using Google.ProtocolBuffers;

    using JetBrains.Annotations;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Testing <see cref="ApiProvider"/> for resolving in various scenarios
    /// </summary>
    public class ApiProviderResolveTests
    {
        /// <summary>
        /// The output.
        /// </summary>
        private readonly ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiProviderResolveTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public ApiProviderResolveTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// Testing sync scalar field
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task AsyncArrayOfScalarFieldTest()
        {
            var provider = this.GetProvider();

            var context = new RequestContext();
            var query = new List<ApiRequest> { new ApiRequest { FieldName = "asyncArrayOfScalarField" } };

            var result = await this.Query(provider, query, context);
            Assert.NotNull(result);
            Assert.NotNull(result.Property("asyncArrayOfScalarField"));
            var array = (decimal[])result.Property("asyncArrayOfScalarField").Value.ToObject(typeof(decimal[]));
            Assert.NotNull(array);
            Assert.Equal(2, array.Length);
            Assert.Equal(4M, array[0]);
            Assert.Equal(5M, array[1]);
        }

        /// <summary>
        /// Testing sync scalar field
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task AsyncFrowardedScalarFieldTest()
        {
            var provider = this.GetProvider();

            var context = new RequestContext();
            var query = new List<ApiRequest> { new ApiRequest { FieldName = "asyncForwardedScalar" } };
            var result = await this.Query(provider, query, context);
            Assert.NotNull(result);
            Assert.NotNull(result.Property("asyncForwardedScalar"));
            Assert.Equal("AsyncForwardedScalar", result.Property("asyncForwardedScalar").ToObject<string>());
        }

        /// <summary>
        /// Testing sync nested scalar field
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task AsyncNestedScalarFieldTest()
        {
            var provider = this.GetProvider();

            var context = new RequestContext();
            var query = new List<ApiRequest>
                            {
                                new ApiRequest
                                    {
                                        FieldName = "nestedAsync",
                                        Fields =
                                            new List<ApiRequest>
                                                {
                                                    new ApiRequest
                                                        {
                                                            FieldName
                                                                =
                                                                "asyncScalarField"
                                                        }
                                                }
                                    }
                            };
            var result = await this.Query(provider, query, context);
            Assert.NotNull(result);
            Assert.NotNull(result.Property("nestedAsync"));
            Assert.IsType<JObject>(result.Property("nestedAsync").Value);
            var nested = (JObject)result.Property("nestedAsync").Value;
            Assert.Equal("AsyncScalarField", nested.Property("asyncScalarField")?.ToObject<string>());
        }

        /// <summary>
        /// Testing sync scalar field
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task AsyncObjectMethodTest()
        {
            var provider = this.GetProvider();

            var context = new RequestContext();
            var methodParameters =
                "{\"intArrayArg\": [7, 8, 9], \"intArg\": 1, \"stringArg\": \"test\", \"objArg\": {syncScalarField: \"nested test\"}}";
            var query = new List<ApiRequest>
                            {
                                new ApiRequest
                                    {
                                        FieldName = "asyncObjectMethod",
                                        Arguments =
                                            (JObject)
                                            JsonConvert.DeserializeObject(methodParameters),
                                        Fields =
                                            new List<ApiRequest>
                                                {
                                                    new ApiRequest
                                                        {
                                                            FieldName
                                                                =
                                                                "syncScalarField"
                                                        }
                                                }
                                    }
                            };

            var result = await this.Query(provider, query, context);
            Assert.NotNull(result);
            Assert.NotNull(result.Property("asyncObjectMethod")?.Value as JObject);
            var resultObject = (JObject)result.Property("asyncObjectMethod").Value;
            Assert.Equal("returned type", resultObject.Property("syncScalarField").ToObject<string>());
        }

        /// <summary>
        /// Testing sync scalar field
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task AsyncScalarFieldTest()
        {
            var provider = this.GetProvider();
            Assert.Equal(0, provider.GenerationErrors.Count);
            Assert.Equal(0, provider.GenerationWarnings.Count);

            var context = new RequestContext();
            var query = new List<ApiRequest> { new ApiRequest { FieldName = "asyncScalarField" } };
            var result = await this.Query(provider, query, context);
            Assert.NotNull(result);
            Assert.NotNull(result.Property("asyncScalarField"));
            Assert.Equal("AsyncScalarField", result.Property("asyncScalarField").ToObject<string>());
        }

        /// <summary>
        /// Testing sync scalar field
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task SyncArrayOfScalarFieldTest()
        {
            var provider = this.GetProvider();

            var context = new RequestContext();
            var query = new List<ApiRequest> { new ApiRequest { FieldName = "syncArrayOfScalarField" } };

            var result = await this.Query(provider, query, context);
            Assert.NotNull(result);
            Assert.NotNull(result.Property("syncArrayOfScalarField"));
            var array = (int[])result.Property("syncArrayOfScalarField").Value.ToObject(typeof(int[]));
            Assert.NotNull(array);
            Assert.Equal(3, array.Length);
            Assert.Equal(1, array[0]);
            Assert.Equal(2, array[1]);
            Assert.Equal(3, array[2]);
        }

        /// <summary>
        /// Testing sync scalar field
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task SyncFrowardedArrayOfScalarFieldTest()
        {
            var provider = this.GetProvider();

            var context = new RequestContext();
            var query = new List<ApiRequest> { new ApiRequest { FieldName = "forwardedArray" } };

            var result = await this.Query(provider, query, context);
            Assert.NotNull(result);
            Assert.NotNull(result.Property("forwardedArray"));
            var array = (int[])result.Property("forwardedArray").Value.ToObject(typeof(int[]));
            Assert.NotNull(array);
            Assert.Equal(3, array.Length);
            Assert.Equal(5, array[0]);
            Assert.Equal(6, array[1]);
            Assert.Equal(7, array[2]);
        }

        /// <summary>
        /// Testing sync nested scalar field
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task SyncNestedScalarFieldTest()
        {
            var provider = this.GetProvider();

            var context = new RequestContext();
            var query = new List<ApiRequest>
                            {
                                new ApiRequest
                                    {
                                        FieldName = "nestedSync",
                                        Fields =
                                            new List<ApiRequest>
                                                {
                                                    new ApiRequest
                                                        {
                                                            FieldName
                                                                =
                                                                "syncScalarField"
                                                        }
                                                }
                                    }
                            };
            var result = await this.Query(provider, query, context);
            Assert.NotNull(result);
            Assert.NotNull(result.Property("nestedSync"));
            Assert.IsType<JObject>(result.Property("nestedSync").Value);
            var nested = (JObject)result.Property("nestedSync").Value;
            Assert.Equal("SyncScalarField", nested.Property("syncScalarField")?.ToObject<string>());
        }

        /// <summary>
        /// Testing sync scalar field
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task SyncScalarFieldTest()
        {
            var provider = this.GetProvider();

            var context = new RequestContext();
            var query = new List<ApiRequest> { new ApiRequest { FieldName = "syncScalarField" } };

            var result = await this.Query(provider, query, context);
            Assert.NotNull(result);
            Assert.NotNull(result.Property("syncScalarField"));
            Assert.Equal("SyncScalarField", result.Property("syncScalarField").ToObject<string>());
        }

        /// <summary>
        /// Testing sync scalar field
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task SyncFaultedScalarFieldTest()
        {
            var provider = this.GetProvider();

            var context = new RequestContext();
            var query = new List<ApiRequest> { new ApiRequest { FieldName = "faultedSyncField" } };

            var result = await this.Query(provider, query, context);
            Assert.NotNull(result);
            Assert.NotNull(result.Property("faultedSyncField"));
            Assert.False(result.Property("faultedSyncField").Value.HasValues);
        }

        /// <summary>
        /// Testing sync scalar field
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task FaultedASyncMethodTest()
        {
            var provider = this.GetProvider();

            var context = new RequestContext();
            var query = new List<ApiRequest>
                            {
                                new ApiRequest
                                    {
                                        FieldName = "faultedASyncMethod",
                                        Fields =
                                            new List<ApiRequest>
                                                {
                                                    new ApiRequest
                                                        {
                                                            FieldName = "syncScalarField"
                                                        }
                                                }
                                    }
                            };

            var result = await this.Query(provider, query, context);
            Assert.NotNull(result);
            Assert.NotNull(result.Property("faultedASyncMethod"));
            Assert.False(result.Property("faultedASyncMethod").Value.HasValues);
        }

        /// <summary>
        /// Testing sync scalar field
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task SyncScalarMethodTest()
        {
            var provider = this.GetProvider();

            var context = new RequestContext();
            var methodParameters =
                "{\"intArg\": 1, \"stringArg\": \"test\", \"objArg\": {syncScalarField: \"nested test\"}}";
            var query = new List<ApiRequest>
                            {
                                new ApiRequest
                                    {
                                        FieldName = "syncScalarMethod",
                                        Arguments =
                                            (JObject)
                                            JsonConvert.DeserializeObject(methodParameters)
                                    }
                            };

            var result = await this.Query(provider, query, context);
            Assert.NotNull(result);
            Assert.NotNull(result.Property("syncScalarMethod"));
            Assert.Equal("ok", result.Property("syncScalarMethod").ToObject<string>());
        }

        /// <summary>
        /// Gets the api provider
        /// </summary>
        /// <returns>The api provider</returns>
        private TestProvider GetProvider()
        {
            var provider = new TestProvider();
            foreach (var error in provider.GenerationErrors)
            {
                this.output.WriteLine($"Error: {error}");
            }

            Assert.Equal(0, provider.GenerationErrors.Count);
            Assert.Equal(0, provider.GenerationWarnings.Count);
            return provider;
        }

        /// <summary>
        /// Executes the query
        /// </summary>
        /// <param name="provider">The provider</param>
        /// <param name="query">The query</param>
        /// <param name="context">The request context</param>
        /// <returns >Result of the execution</returns>
        private async Task<JObject> Query(TestProvider provider, List<ApiRequest> query, RequestContext context)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = await provider.ResolveQuery(query, context, e => this.output.WriteLine($"Resolve error: {e.Message}\n{e.StackTrace}"));
            stopwatch.Stop();
            this.output.WriteLine($"Resolved in {(double)stopwatch.ElapsedTicks * 1000 / Stopwatch.Frequency}ms");
            this.output.WriteLine(result.ToString(Formatting.Indented));
            return result;
        }

        /// <summary>
        /// Nested API
        /// </summary>
        [UsedImplicitly]
        [ApiDescription(Description = "Nested API")]
        public class NestedProvider
        {
            /// <summary>
            /// Async scalar field
            /// </summary>
            [DeclareField]
            [UsedImplicitly]
            public Task<string> AsyncScalarField => Task.FromResult("AsyncScalarField");

            /// <summary>
            /// Gets or sets the sync scalar field
            /// </summary>
            [DeclareField]
            [UsedImplicitly]
            public string SyncScalarField { get; set; } = "SyncScalarField";
        }

        /// <summary>
        /// Provider to test
        /// </summary>
        [UsedImplicitly]
        [ApiDescription(Description = "Tested API")]
        public class TestProvider : ApiProvider
        {
            /// <summary>
            /// Async scalar field
            /// </summary>
            [DeclareField]
            [UsedImplicitly]
            public Task<List<decimal>> AsyncArrayOfScalarField => Task.FromResult(new List<decimal> { 4M, 5M });

            /// <summary>
            /// Gets the forwarded scalar property
            /// </summary>
            [DeclareField(ReturnType = typeof(string))]
            [UsedImplicitly]
            public Task<JValue> AsyncForwardedScalar => Task.FromResult(new JValue("AsyncForwardedScalar"));

            /// <summary>
            /// Async nested provider
            /// </summary>
            [DeclareField(Name = "nestedAsync")]
            [UsedImplicitly]
            public Task<NestedProvider> AsyncNestedProvider => Task.FromResult(new NestedProvider());

            /// <summary>
            /// Async scalar field
            /// </summary>
            [DeclareField]
            [UsedImplicitly]
            public Task<string> AsyncScalarField => Task.FromResult("AsyncScalarField");

            /// <summary>
            /// Gets the forwarded array of scalars
            /// </summary>
            [DeclareField(ReturnType = typeof(int[]))]
            [UsedImplicitly]
            public JArray ForwardedArray => new JArray(new[] { 5, 6, 7 });

            /// <summary>
            /// Async nested provider
            /// </summary>
            [DeclareField]
            [UsedImplicitly]
            public int[] SyncArrayOfScalarField => new[] { 1, 2, 3 };

            /// <summary>
            /// Sync nested provider
            /// </summary>
            [DeclareField(Name = "nestedSync")]
            [UsedImplicitly]
            public NestedProvider SyncNestedProvider => new NestedProvider();

            /// <summary>
            /// Sync scalar field
            /// </summary>
            [DeclareField]
            [UsedImplicitly]
            public string SyncScalarField => "SyncScalarField";

            /// <summary>
            /// Gets a value indicating whether something is faulting
            /// </summary>
            [DeclareField]
            [UsedImplicitly]
            public bool FaultedSyncField
            {
                get
                {
                    throw new Exception("test");
                }
            }

            /// <summary>
            /// Faulted async method
            /// </summary>
            /// <returns>Faulted task</returns>
            [DeclareField]
            [UsedImplicitly]
            public Task<NestedProvider> FaultedASyncMethod()
            {
                return Task.FromException<NestedProvider>(new Exception("test exception"));
            }

            /// <summary>
            /// Some public method
            /// </summary>
            /// <param name="intArg">
            /// Integer parameter
            /// </param>
            /// <param name="stringArg">
            /// String parameter
            /// </param>
            /// <param name="objArg">
            /// Object parameter
            /// </param>
            /// <param name="intArrayArg">
            /// The integer Array parameter.
            /// </param>
            /// <param name="requestContext">
            /// The request context
            /// </param>
            /// <param name="apiRequest">
            /// The sub-request
            /// </param>
            /// <returns>
            /// The test string
            /// </returns>
            [DeclareField]
            [UsedImplicitly]
            public Task<NestedProvider> AsyncObjectMethod(
                int intArg,
                string stringArg,
                NestedProvider objArg,
                int[] intArrayArg,
                RequestContext requestContext,
                ApiRequest apiRequest)
            {
                Assert.Equal(1, intArg);
                Assert.Equal("test", stringArg);
                Assert.NotNull(objArg);
                Assert.Equal("nested test", objArg.SyncScalarField);

                Assert.NotNull(intArrayArg);
                Assert.Equal(3, intArrayArg.Length);
                Assert.Equal(7, intArrayArg[0]);
                Assert.Equal(8, intArrayArg[1]);
                Assert.Equal(9, intArrayArg[2]);

                Assert.NotNull(requestContext);
                Assert.NotNull(apiRequest);
                Assert.Equal("asyncObjectMethod", apiRequest.FieldName);

                return Task.FromResult(new NestedProvider { SyncScalarField = "returned type" });
            }

            /// <summary>
            /// Some public method
            /// </summary>
            /// <param name="intArg">Integer parameter</param>
            /// <param name="stringArg">String parameter</param>
            /// <param name="objArg">Object parameter</param>
            /// <param name="requestContext">The request context</param>
            /// <param name="apiRequest">The sub-request</param>
            /// <returns>The test string</returns>
            [DeclareField]
            [UsedImplicitly]
            public string SyncScalarMethod(
                int intArg,
                string stringArg,
                NestedProvider objArg,
                RequestContext requestContext,
                ApiRequest apiRequest)
            {
                Assert.Equal(1, intArg);
                Assert.Equal("test", stringArg);
                Assert.NotNull(objArg);
                Assert.Equal("nested test", objArg.SyncScalarField);

                Assert.NotNull(requestContext);
                Assert.NotNull(apiRequest);
                Assert.Equal("syncScalarMethod", apiRequest.FieldName);

                return "ok";
            }
        }
    }
}