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
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using ClusterKit.Security.Client;
    using ClusterKit.Web.GraphQL.API;
    using ClusterKit.Web.GraphQL.Client;
    using ClusterKit.Web.GraphQL.Client.Attributes;

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
        /// Testing connection query resolve
        /// </summary>
        /// <param name="filterJson">
        /// The filter Json.
        /// </param>
        /// <param name="sortJson">
        /// The sort Json.
        /// </param>
        /// <param name="limit">
        /// The limit.
        /// </param>
        /// <param name="offset">
        /// The offset.
        /// </param>
        /// <param name="expectedCount">
        /// The expected Count.
        /// </param>
        /// <param name="expectedNames">
        /// The expected list of received object names.
        /// </param>
        /// <returns>
        /// The async task
        /// </returns>
        [Theory]
        [InlineData(null, null, 10, 0, 5, new[] { "1-test", "2-test", "3-test", "4-test", "5-test" })]
        [InlineData(null, "[\"value_asc\", \"name_desc\"]", 10, 0, 5, new[] { "5-test", "3-test", "2-test", "4-test", "1-test" })]
        [InlineData(null, "[\"value_desc\"]", 10, 0, 5, new[] { "1-test", "4-test", "2-test", "3-test", "5-test" })]

        [InlineData("{\"value_lt\": 50}", null, 10, 0, 1, new[] { "5-test" })]
        [InlineData("{\"value_lte\": 50}", null, 10, 0, 3, new[] { "2-test", "3-test", "5-test" })]
        [InlineData("{\"value_not\": 50}", null, 10, 0, 3, new[] { "1-test", "4-test", "5-test" })]
        [InlineData("{\"value\": 50}", null, 10, 0, 2, new[] { "2-test", "3-test" })]
        [InlineData("{\"OR\": [{\"value\": 50}, {\"value\": 70}]}", null, 10, 0, 3, new[] { "2-test", "3-test", "4-test" })]
        [InlineData("{\"AND\": [{\"value\": 50}, {\"name\": \"2-test\"}]}", null, 10, 0, 1, new[] { "2-test" })]

        [InlineData("{\"name_in\": \"1-test, 3-test\"}", null, 10, 0, 2, new[] { "1-test", "3-test" })]
        [InlineData("{\"name_not_in\": \"1-test, 3-test\"}", null, 10, 0, 3, new[] { "2-test", "4-test", "5-test" })]
        [InlineData("{\"name_contains\": \"tes\"}", null, 10, 0, 5, new[] { "1-test", "2-test", "3-test", "4-test", "5-test" })]
        [InlineData("{\"name_contains\": \"1-tes\"}", null, 10, 0, 1, new[] { "1-test" })]
        [InlineData("{\"name_not_contains\": \"tes\"}", null, 10, 0, 0, new string[0])]
        [InlineData("{\"name_not_contains\": \"1-tes\"}", null, 10, 0, 4, new[] { "2-test", "3-test", "4-test", "5-test" })]

        [InlineData("{\"name_starts_with\": \"1-tes\"}", null, 10, 0, 1, new[] { "1-test" })]
        [InlineData("{\"name_starts_with\": \"tes\"}", null, 10, 0, 0, new string[0])]
        [InlineData("{\"name_not_starts_with\": \"1-tes\"}", null, 10, 0, 4, new[] { "2-test", "3-test", "4-test", "5-test" })]

        [InlineData("{\"name_ends_with\": \"test\"}", null, 10, 0, 5, new[] { "1-test", "2-test", "3-test", "4-test", "5-test" })]
        [InlineData("{\"name_ends_with\": \"tes\"}", null, 10, 0, 0, new string[0])]
        [InlineData("{\"name_not_ends_with\": \"test\"}", null, 10, 0, 0, new string[0])]
        public async Task ConnectionQueryTests(string filterJson, string sortJson, int limit, int offset, int expectedCount, string[] expectedNames)
        {
            var initialObjects = new List<TestObject>
                                     {
                                         new TestObject { Name = "1-test", Value = 100m },
                                         new TestObject { Name = "2-test", Value = 50m },
                                         new TestObject { Name = "3-test", Value = 50m },
                                         new TestObject { Name = "4-test", Value = 70m },
                                         new TestObject { Name = "5-test", Value = 6m },
                                     };

            var provider = this.GetProvider(initialObjects);
            var context = new RequestContext();

            var objFields = new List<ApiRequest>
                                {
                                    new ApiRequest { FieldName = "uid" },
                                    new ApiRequest { FieldName = "name" },
                                    new ApiRequest { FieldName = "value" }
                                };

            var arguments = new JObject();
            if (!string.IsNullOrWhiteSpace(filterJson))
            {
                arguments.Add("filter", JsonConvert.DeserializeObject(filterJson) as JToken);
            }

            if (!string.IsNullOrWhiteSpace(sortJson))
            {
                arguments.Add("sort", JsonConvert.DeserializeObject(sortJson) as JToken);
            }

            arguments.Add("limit", limit);
            arguments.Add("offset", offset);
            
            var query = new List<ApiRequest> { new ApiRequest { FieldName = "connection", Fields = objFields, Arguments = arguments } };
            var result = await this.Query(provider, query, context);

            Assert.NotNull(result);
            Assert.NotNull(result.Property("connection"));
            var connectionData = result.Property("connection").Value as JObject;
            Assert.NotNull(connectionData);
            Assert.NotNull(connectionData.Property("count"));
            Assert.Equal(expectedCount, connectionData.Property("count").Value.ToObject<int>());

            var nodes = connectionData.Property("items")?.Value as JArray;
            Assert.NotNull(nodes);
            Assert.Equal(expectedNames.Length, nodes.Count);
            Assert.Equal(
                string.Join(", ", expectedNames),
                string.Join(", ", nodes.Select(n => (n as JObject)?.Property("name").Value)));
        }

        /// <summary>
        /// Testing connection mutation resolve
        /// </summary>
        /// <param name="mutationName">
        /// The mutation name to call
        /// </param>
        /// <param name="mutationRequest">
        /// The mutation arguments
        /// </param>
        /// <param name="expectResult">
        /// A value indicating whether to expect result or error
        /// </param>
        /// <param name="expectedResult">
        /// The expected value (or errors if no result expected).
        /// </param>
        /// <returns>
        /// The async task
        /// </returns>
        [Theory]
        [InlineData("connection.create", "{\"newNode\": { \"name\":\"6-test\", \"value\": 1 }}", true, "{\"name\":\"6-test\",\"value\": 1.0 }")]
        [InlineData("connection.create", "{\"newNode\": { \"value\": 1 }}", false, "[{\"field\": null, \"message\": \"Create failed\"}, {\"field\":\"name\",\"message\":\"name should be set\"}]")]
        [InlineData("connection.create", null, false, "[{\"field\": null, \"message\": \"Create failed\"}, {\"field\": null,\"message\":\"object data was not provided\"}]")]
        [InlineData("connection.update", "{\"id\": \"B500CA20-F649-4DCD-BDA8-1FA5031ECDD3\", \"newNode\": { \"value\": 1.0 }}", true, "{\"name\":\"2-test\",\"value\": 1.0 }")]
        [InlineData("connection.update", "{\"id\": \"B500CA20-F649-4DCD-BDA8-1FA5031ECDD3\", \"newNode\": { \"uid\": \"{C12EE96B-2420-4F54-AAE5-788995B10679}\" }}", true, "{\"name\":\"2-test\",\"value\": 50.0 }")]
        [InlineData("connection.update", "{\"id\": \"B500CA20-F649-4DCD-BDA8-1FA5031ECDD4\", \"newNode\": { \"value\": 1.0 }}", false, "[{\"field\": null, \"message\": \"Update failed\"}, {\"field\":\"id\",\"message\":\"Node not found\"}]")]
        [InlineData("connection.update", "{\"id\": \"B500CA20-F649-4DCD-BDA8-1FA5031ECDD3\", \"newNode\": { \"uid\": \"{F0607502-5B77-4A3C-9142-E6197A7EE61E}\" }}", false, "[{\"field\": null, \"message\": \"Update failed\"}, {\"field\":\"uid\",\"message\":\"Duplicate key\"}]")]
        [InlineData("connection.delete", "{\"id\": \"B500CA20-F649-4DCD-BDA8-1FA5031ECDD3\"}", true, "{\"name\":\"2-test\",\"value\": 50.0 }")]
        [InlineData("connection.delete", "{\"id\": \"B500CA20-F649-4DCD-BDA8-1FA5031ECDD4\"}", false, "[{\"field\": null, \"message\": \"Delete failed\"}, {\"field\":\"id\",\"message\":\"Node not found\"}]")]
        public async Task ConnectionMutationTests(string mutationName, string mutationRequest, bool expectResult, string expectedResult)
        {
            var initialObjects = new List<TestObject>
                                     {
                                         new TestObject { Uid = Guid.Parse("{3BEEE369-11DF-4A30-BF11-1D8465C87110}"), Name = "1-test", Value = 100m },
                                         new TestObject { Uid = Guid.Parse("{B500CA20-F649-4DCD-BDA8-1FA5031ECDD3}"), Name = "2-test", Value = 50m },
                                         new TestObject { Uid = Guid.Parse("{67885BA0-B284-438F-8393-EE9A9EB299D1}"), Name = "3-test", Value = 50m },
                                         new TestObject { Uid = Guid.Parse("{3AF2C973-D985-4F95-A0C7-AA928D276881}"), Name = "4-test", Value = 70m },
                                         new TestObject { Uid = Guid.Parse("{F0607502-5B77-4A3C-9142-E6197A7EE61E}"), Name = "5-test", Value = 6m },
                                     };

            var provider = this.GetProvider(initialObjects);
            var context = new RequestContext();
            var request = new ApiRequest
                              {
                                  FieldName = mutationName,
                                  Arguments = string.IsNullOrWhiteSpace(mutationRequest) ? null : JsonConvert.DeserializeObject(mutationRequest) as JObject,
                                  Fields = new List<ApiRequest>
                                               {
                                                   new ApiRequest { FieldName = "result", Fields = new List<ApiRequest> { new ApiRequest { FieldName = "name" }, new ApiRequest { FieldName = "value" } } },
                                                   new ApiRequest { FieldName = "errors", Fields = new List<ApiRequest> { new ApiRequest { FieldName = "field" }, new ApiRequest { FieldName = "message" } } },
                                               }
                              };

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = await provider.ResolveMutation(
                             request,
                             context,
                             e => this.output.WriteLine($"Resolve error: {e.Message}\n{e.StackTrace}"));
            stopwatch.Stop();
            this.output.WriteLine($"Resolved in {(double)stopwatch.ElapsedTicks * 1000 / Stopwatch.Frequency}ms");
            Assert.NotNull(result);
            this.output.WriteLine(result.ToString(Formatting.Indented));

            var resultObject = result.Property("result");
            var resultErrors = result.Property("errors");
            Assert.NotNull(resultObject);
            Assert.NotNull(resultErrors);
            Assert.True(resultObject.HasValues);
            Assert.True(resultErrors.HasValues);

            if (expectResult)
            {
                Assert.False(resultErrors.Value.HasValues);
                Assert.True(resultObject.Value.HasValues);
                Assert.Equal(
                    ((JObject)JsonConvert.DeserializeObject(expectedResult)).ToString(Formatting.None), 
                    resultObject.Value.ToString(Formatting.None));
            }
            else
            {
                Assert.True(resultErrors.Value.HasValues);
                Assert.False(resultObject.Value.HasValues);
                Assert.Equal(
                    ((JArray)JsonConvert.DeserializeObject(expectedResult)).ToString(Formatting.None),
                    resultErrors.Value.ToString(Formatting.None));
            }
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
                                                            FieldName = "asyncScalarField"
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
                                                            FieldName
                                                                =
                                                                "syncScalarField"
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
        /// <param name="objects">
        /// The initial objects list.
        /// </param>
        /// <returns>
        /// The api provider
        /// </returns>
        private TestProvider GetProvider(List<TestObject> objects = null)
        {
            var provider = new TestProvider(objects);
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
            var result = await provider.ResolveQuery(
                             query,
                             context,
                             e => this.output.WriteLine($"Resolve error: {e.Message}\n{e.StackTrace}"));
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
        /// Test object
        /// </summary>
        public class TestObject
        {
            /// <summary>
            /// Gets or sets the name of the object
            /// </summary>
            [DeclareField]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the object uid
            /// </summary>
            [DeclareField]
            public Guid Uid { get; set; } = Guid.NewGuid();

            /// <summary>
            /// Gets or sets some value
            /// </summary>
            [DeclareField]
            public decimal Value { get; set; }
        }

        /// <summary>
        /// The <see cref="TestObject"/> connection provider
        /// </summary>
        public class TestObjectConnection : INodeConnection<TestObject, Guid>
        {
            /// <summary>
            /// Gets the stored objects (virtual database)
            /// </summary>
            private readonly Dictionary<Guid, TestObject> objects;

            /// <summary>
            /// Initializes a new instance of the <see cref="TestObjectConnection"/> class.
            /// </summary>
            /// <param name="objects">
            /// The initial objects state.
            /// </param>
            public TestObjectConnection(Dictionary<Guid, TestObject> objects)
            {
                this.objects = objects;
            }

            /// <inheritdoc />
            public Task<MutationResult<TestObject>> Create(TestObject newNode)
            {
                if (newNode == null)
                {
                    var errors = new List<ErrorDescription>
                                     {
                                         new ErrorDescription(null, "Create failed"),
                                         new ErrorDescription(null, "object data was not provided")
                                     };

                    return Task.FromResult(new MutationResult<TestObject> { Errors = errors });
                }

                if (string.IsNullOrWhiteSpace(newNode.Name))
                {
                    var errors = new List<ErrorDescription>
                                     {
                                         new ErrorDescription(null, "Create failed"),
                                         new ErrorDescription("name", "name should be set")
                                     };

                    return Task.FromResult(new MutationResult<TestObject> { Errors = errors });
                }

                newNode.Uid = Guid.NewGuid();
                this.objects.Add(newNode.Uid, newNode);
                return Task.FromResult(new MutationResult<TestObject> { Result = newNode });
            }

            /// <inheritdoc />
            public Task<MutationResult<TestObject>> Delete(Guid id)
            {
                TestObject obj;
                if (!this.objects.TryGetValue(id, out obj))
                {
                    var errors = new List<ErrorDescription>
                                     {
                                         new ErrorDescription(null, "Delete failed"),
                                         new ErrorDescription("id", "Node not found")
                                     };

                    return Task.FromResult(new MutationResult<TestObject> { Errors = errors });
                }

                this.objects.Remove(id);
                return Task.FromResult(new MutationResult<TestObject> { Result = obj });
            }

            /// <inheritdoc />
            public Task<TestObject> GetById(Guid id)
            {
                TestObject obj;
                if (this.objects.TryGetValue(id, out obj))
                {
                    return Task.FromResult(obj);
                }

                throw new Exception("not found");
            }

            /// <inheritdoc />
            public Task<QueryResult<TestObject>> Query(
                Expression<Func<TestObject, bool>> filter,
                Expression<Func<IQueryable<TestObject>, IOrderedQueryable<TestObject>>> sort,
                int limit,
                int offset)
            {
                var query = this.objects.Values.AsQueryable();

                if (filter != null)
                {
                    query = query.Where(filter);
                }

                var count = query.Count();
                if (sort != null)
                {
                    query = sort.Compile().Invoke(query);
                }

                return Task.FromResult(new QueryResult<TestObject> { Count = count, Items = query });
            }

            /// <inheritdoc />
            public Task<MutationResult<TestObject>> Update(Guid id, TestObject newNode, ApiRequest request)
            {
                TestObject obj;
                if (!this.objects.TryGetValue(id, out obj))
                {
                    var errors = new List<ErrorDescription>
                                     {
                                         new ErrorDescription(null, "Update failed"),
                                         new ErrorDescription("id", "Node not found")
                                     };

                    return Task.FromResult(new MutationResult<TestObject> { Errors = errors });
                }

                var description = request?.Arguments.Property("newNode").Value as JObject;
                if (description == null)
                {
                    var errors = new List<ErrorDescription>
                                     {
                                         new ErrorDescription(null, "Update failed"),
                                         new ErrorDescription(null, "Update description not found")
                                     };

                    return Task.FromResult(new MutationResult<TestObject> { Errors = errors });
                }

                if (description.Property("uid") != null)
                {
                    if (newNode.Uid != obj.Uid && this.objects.ContainsKey(newNode.Uid))
                    {
                        var errors = new List<ErrorDescription>
                                     {
                                         new ErrorDescription(null, "Update failed"),
                                         new ErrorDescription("uid", "Duplicate key")
                                     };

                        return Task.FromResult(new MutationResult<TestObject> { Errors = errors });
                    }

                    obj.Uid = newNode.Uid;
                }

                if (description.Property("name") != null)
                {
                    obj.Name = newNode.Name;
                }

                if (description.Property("value") != null)
                {
                    obj.Value = newNode.Value;
                }

                this.objects.Remove(id);
                this.objects[obj.Uid] = obj;

                return Task.FromResult(new MutationResult<TestObject> { Result = obj });
            }
        }

        /// <summary>
        /// Provider to test
        /// </summary>
        [UsedImplicitly]
        [ApiDescription(Description = "Tested API")]
        public class TestProvider : ApiProvider
        {
            /// <summary>
            /// The test objects connection
            /// </summary>
            private TestObjectConnection connection;

            /// <summary>
            /// Initializes a new instance of the <see cref="TestProvider"/> class.
            /// </summary>
            /// <param name="objects">
            /// The list of pre-stored objects.
            /// </param>
            public TestProvider(List<TestObject> objects = null)
            {
                if (objects == null)
                {
                    objects = new List<TestObject>();
                }

                this.connection = new TestObjectConnection(objects.ToDictionary(o => o.Uid));
            }

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
            /// Test objects connection
            /// </summary>
            [DeclareConnection(CanCreate = true, CanDelete = true, CanUpdate = true)]
            [UsedImplicitly]
            public TestObjectConnection Connection => this.connection;

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