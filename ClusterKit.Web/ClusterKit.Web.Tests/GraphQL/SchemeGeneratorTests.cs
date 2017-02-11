// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SchemeGeneratorTests.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing the <see cref="SchemaGenerator" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Tests.GraphQL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using ClusterKit.Web.GraphQL.Client;
    using ClusterKit.Web.GraphQL.Publisher;

    using global::GraphQL;
    using global::GraphQL.Http;
    using global::GraphQL.Utilities;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Testing the <see cref="SchemaGenerator"/>
    /// </summary>
    public class SchemeGeneratorTests
    {
        /// <summary>
        /// The output.
        /// </summary>
        private readonly ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemeGeneratorTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public SchemeGeneratorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// Testing trivial scheme generator
        /// </summary>
        [Fact]
        public void TrivialTest()
        {
            var scheme = SchemaGenerator.Generate(new List<ApiProvider>());

            using (var printer = new SchemaPrinter(scheme))
            {
                var description = printer.Print();
                this.output.WriteLine(description);
                Assert.False(string.IsNullOrEmpty(description));
            }
        }

        /// <summary>
        /// Testing generator for some empty single api
        /// </summary>
        [Fact]
        public void Trivial2Test()
        {
            var api = new ApiDescription
                          {
                              ApiName = "Test-Api-1",
                              TypeName = "Test-Api-1",
                              Version = new Version("0.0.0.1"),
                          };

            var provider = new MoqProvider { Description = api };
            var scheme = SchemaGenerator.Generate(new List<ApiProvider> { provider });

            using (var printer = new SchemaPrinter(scheme))
            {
                var description = printer.Print();
                this.output.WriteLine(description);
                Assert.False(string.IsNullOrWhiteSpace(description));
            }

            Assert.NotNull(scheme.Query);
            Assert.Equal(1, scheme.Query.Fields.Count());
            Assert.True(scheme.Query.HasField("api"));
        }

        /// <summary>
        /// Testing generator for some simple single api
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task NonEmptyApiTest()
        {
            var viewerType = new ApiType(
                "viewer",
                new[] { ApiField.Scalar("id", EnScalarType.Integer), ApiField.Scalar("name", EnScalarType.String) });

            var api = new ApiDescription(
                "Test-Api-1",
                "0.0.0.1",
                new[] { viewerType },
                new[] { viewerType.CreateField("viewer") });

            var provider = new MoqProvider
            {
                                   Description = api,
                                   Data = "{\"viewer\": {\"id\": 1, \"name\": \"test name\"}}"
            };

            var scheme = SchemaGenerator.Generate(new List<ApiProvider> { provider });

            using (var printer = new SchemaPrinter(scheme))
            {
                var description = printer.Print();
                this.output.WriteLine("-------- Schema -----------");
                this.output.WriteLine(description);
                Assert.False(string.IsNullOrWhiteSpace(description));
            }

            Assert.NotNull(scheme.Query);
            Assert.Equal(1, scheme.Query.Fields.Count());
            Assert.True(scheme.Query.HasField("api"));

            var result = await new DocumentExecuter().ExecuteAsync(
                             r =>
                                 {
                                     r.Schema = scheme;
                                     r.Query = @"
                                query {
                                    api {
                                        viewer {
                                            id,
                                            name
                                        }
                                    }
                                }            
                                ";
                                 }).ConfigureAwait(true);

            this.output.WriteLine("-------- Response -----------");
            var response = new DocumentWriter(true).Write(result);
            this.output.WriteLine(response);
            var expectedResponse = @"{
                                      ""data"": {
                                        ""api"": {
                                          ""viewer"": {
                                            ""id"": 1,
                                            ""name"": ""test name""
                                          }
                                        }
                                      }
                                    }";
            Assert.Equal(CleanResponse(expectedResponse), CleanResponse(response));
        }

        /// <summary>
        /// Testing generator for some api with arrays
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task ArraysApiTest()
        {
            var viewerType = new ApiType(
                "viewer",
                new[] { ApiField.Scalar("id", EnScalarType.Integer), ApiField.Scalar("name", EnScalarType.String) });

            var objectType = new ApiType("object", new[] { ApiField.Scalar("id", EnScalarType.String, EnFieldFlags.IsKey) });

            var api = new ApiDescription(
                "Test-Api-1",
                "0.0.0.1",
                new[] { viewerType, objectType },
                new[] { viewerType.CreateField("viewer"), objectType.CreateField("object", EnFieldFlags.IsArray) });

            var provider = new MoqProvider
            {
                Description = api,
                Data = "{\"viewer\": {\"id\": 1, \"name\": \"test name\" }, \"object\": { \"count\": 2, \"items\": [{\"id\": 10}, {\"id\": 20}]}}"
            };

            var scheme = SchemaGenerator.Generate(new List<ApiProvider> { provider });

            using (var printer = new SchemaPrinter(scheme))
            {
                var description = printer.Print();
                this.output.WriteLine("-------- Schema -----------");
                this.output.WriteLine(description);
                Assert.False(string.IsNullOrWhiteSpace(description));
            }

            Assert.NotNull(scheme.Query);
            Assert.Equal(1, scheme.Query.Fields.Count());
            Assert.True(scheme.Query.HasField("api"));

            var result = await new DocumentExecuter().ExecuteAsync(
                             r =>
                             {
                                 r.Schema = scheme;
                                 r.Query = @"
                                query {
                                    api {
                                        viewer {
                                            id,
                                            name                                            
                                        },
                                        object {
                                            count,
                                            edges {
                                                cursor,                                                
                                                node {
                                                    id
                                                }
                                            }
                                        }
                                    }
                                }            
                                ";
                             }).ConfigureAwait(true);

            this.output.WriteLine("-------- Response -----------");
            var response = new DocumentWriter(true).Write(result);
            this.output.WriteLine(response);

            var expectedResponse = @"{
                                      ""data"": {
                                        ""api"": {
                                          ""viewer"": {
                                            ""id"": 1,
                                            ""name"": ""test name""
                                          },
                                          ""object"": {
                                            ""count"": 2,
                                            ""edges"": [
                                              {
                                                ""cursor"": 10,
                                                ""node"": {
                                                  ""id"": 10
                                                }
                                              },
                                              {
                                                ""cursor"": 20,
                                                ""node"": {
                                                  ""id"": 20
                                                }
                                              }
                                            ]
                                          }
                                        }
                                      }
                                    }";

            Assert.Equal(CleanResponse(expectedResponse), CleanResponse(response));
        }

        /// <summary>
        /// Testing generator for some api with arrays - filtering support
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task ArraysArgumentsApiTest()
        {
            var viewerType = new ApiType(
                "viewer",
                new[] { ApiField.Scalar("id", EnScalarType.Integer), ApiField.Scalar("name", EnScalarType.String) });

            var objectType = new ApiType("object", new[] { ApiField.Scalar("id", EnScalarType.Integer, EnFieldFlags.IsKey), ApiField.Scalar("name", EnScalarType.String) });

            var api = new ApiDescription(
                "Test-Api-1",
                "0.0.0.1",
                new[] { viewerType, objectType },
                new[] { viewerType.CreateField("viewer"), objectType.CreateField("object", EnFieldFlags.IsArray) });

            var provider = new MoqProvider
            {
                Description = api,
                Data = "{\"viewer\": {\"id\": 1, \"name\": \"test name\"}, \"object\": { \"count\": 2, \"items\": [{\"id\": 10,  \"name\": \"test object1\"}, {\"id\": 20,  \"name\": \"test object2\"}]}}"
            };

            var scheme = SchemaGenerator.Generate(new List<ApiProvider> { provider });

            using (var printer = new SchemaPrinter(scheme))
            {
                var description = printer.Print();
                this.output.WriteLine("-------- Schema -----------");
                this.output.WriteLine(description);
                Assert.False(string.IsNullOrWhiteSpace(description));
            }

            Assert.NotNull(scheme.Query);
            Assert.Equal(1, scheme.Query.Fields.Count());
            Assert.True(scheme.Query.HasField("api"));

            var result = await new DocumentExecuter().ExecuteAsync(
                             r =>
                             {
                                 r.Schema = scheme;
                                 r.Query = @"
                                query {
                                    api {
                                        viewer {
                                            id,
                                            name
                                        },
                                        object(filter: { id: 10, AND: [{name: ""test"", OR: [{id_lt: 20}]}] }, sort: [name_DESC, id_ASC], limit: 10, offset: 20) {
                                            count,
                                            edges {
                                                cursor,                                                
                                                node {
                                                    id,
                                                    name
                                                }
                                            }
                                        }
                                    }
                                }            
                                ";
                             }).ConfigureAwait(true);

            this.output.WriteLine("-------- Response -----------");
            var response = new DocumentWriter(true).Write(result);
            this.output.WriteLine(response);

            var expectedResponse = @"{
                                      ""data"": {
                                        ""api"": {
                                          ""viewer"": {
                                            ""id"": 1,
                                            ""name"": ""test name""
                                          },
                                          ""object"": {
                                            ""count"": 2,
                                            ""edges"": [
                                              {
                                                ""cursor"": 10,
                                                ""node"": {
                                                  ""id"": 10,
                                                  ""name"": ""test object1""
                                                }
                                              },
                                              {
                                                ""cursor"": 20,
                                                ""node"": {
                                                  ""id"": 20,
                                                  ""name"": ""test object2""
                                                }
                                              }
                                            ]
                                          }
                                        }
                                      }
                                    }";

            Assert.Equal(CleanResponse(expectedResponse), CleanResponse(response));
        }

        /// <summary>
        /// Testing generator for some simple single api
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task MultipleApiTest()
        {
            var viewerType1 = new ApiType(
                "viewer1",
                new[] { ApiField.Scalar("id", EnScalarType.Integer), ApiField.Scalar("name", EnScalarType.String) });

            var viewerType2 = new ApiType("viewer2", new[] { ApiField.Scalar("description", EnScalarType.String) });

            var objectType1 = new ApiType("object1", new[] { ApiField.Scalar("id", EnScalarType.String) });
            var objectType2 = new ApiType("object2", new[] { ApiField.Scalar("id", EnScalarType.String) });

            var api1 = new ApiDescription(
                "Test-Api-1",
                "0.0.0.1",
                new[] { viewerType1, objectType1 },
                new[] { viewerType1.CreateField("viewer"), objectType1.CreateField("object1") });

            var api2 = new ApiDescription(
                "Test-Api-2",
                "0.0.0.1",
                new[] { viewerType2, objectType2 },
                new[] { viewerType2.CreateField("viewer"), objectType2.CreateField("object2") });

            var provider1 = new MoqProvider
                                {
                                    Description = api1,
                                    Data =
                                        "{\"viewer\": {\"id\": 1, \"name\": \"test name\"}, \"object1\": {\"id\": 10}}"
                                };

            var provider2 = new MoqProvider
                                {
                                    Description = api2,
                                    Data =
                                        "{\"viewer\": {\"description\": \"test description\"}, \"object2\": {\"id\": 123}}"
                                };

            var scheme = SchemaGenerator.Generate(new List<ApiProvider> { provider1, provider2 });

            using (var printer = new SchemaPrinter(scheme))
            {
                var description = printer.Print();
                this.output.WriteLine("-------- Schema -----------");
                this.output.WriteLine(description);
                Assert.False(string.IsNullOrWhiteSpace(description));
            }

            Assert.NotNull(scheme.Query);
            Assert.Equal(1, scheme.Query.Fields.Count());
            Assert.True(scheme.Query.HasField("api"));

            var result = await new DocumentExecuter().ExecuteAsync(
                             r =>
                                 {
                                     r.Schema = scheme;
                                     r.Query = @"
                                query {
                                    api {
                                        viewer {
                                            id,
                                            name,
                                            description
                                        },
                                        object1 {
                                            id
                                        },
                                        object2 {
                                            id
                                        }
                                    }
                                }            
                                ";
                                 }).ConfigureAwait(true);

            this.output.WriteLine("-------- Response -----------");
            var response = new DocumentWriter(true).Write(result);
            this.output.WriteLine(response);
            var expectedResponse = @"{
                                      ""data"": {
                                        ""api"": {
                                          ""viewer"": {
                                            ""id"": 1,
                                            ""name"": ""test name"",
                                            ""description"": ""test description""
                                          },
                                          ""object1"": {
                                            ""id"": 10
                                          },
                                          ""object2"": {
                                            ""id"": 123
                                          }
                                        }
                                      }
                                    }";
            Assert.Equal(CleanResponse(expectedResponse), CleanResponse(response));
        }

        /// <summary>
        /// Removes formatting values from response json
        /// </summary>
        /// <param name="response">The json string</param>
        /// <returns>Cleaned json string</returns>
        private static string CleanResponse(string response)
        {
            return Regex.Replace(
                response,
                "[ \t\r\n]",
                string.Empty,
                RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        }

        /// <summary>
        /// Impersonate api provider with preset data
        /// </summary>
        private class MoqProvider : ApiProvider
        {
            /// <summary>
            /// Gets or sets the api data
            /// </summary>
            public string Data { get; set; }

            /// <inheritdoc />
            public override Task<string> GetData(List<ApiRequest> requests)
            {
                return Task.FromResult(this.Data);
            }
        }
    }
}
