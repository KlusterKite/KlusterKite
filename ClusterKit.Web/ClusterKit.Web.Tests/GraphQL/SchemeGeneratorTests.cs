﻿// --------------------------------------------------------------------------------------------------------------------
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
    using System.Threading.Tasks;

    using ClusterKit.API.Client;
    using ClusterKit.Security.Client;
    using ClusterKit.Web.GraphQL.Publisher;

    using global::GraphQL;
    using global::GraphQL.Http;
    using global::GraphQL.Utilities;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

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
        /// Testing generator for some api with arrays
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task ArraysApiTest()
        {
            var viewerFields = new[]
                                   {
                                       ApiField.Scalar("id", EnScalarType.Guid),
                                       ApiField.Scalar("name", EnScalarType.String),
                                       ApiField.Scalar("numbers", EnScalarType.Integer, EnFieldFlags.IsArray),
                                       ApiField.Object("objects", "object", EnFieldFlags.IsArray)
                                   };
            var viewerType = new ApiObjectType("viewer", viewerFields);

            var objectFields = new[]
                                   {
                                       ApiField.Scalar("id", EnScalarType.Integer, EnFieldFlags.IsKey),
                                       ApiField.Scalar("name", EnScalarType.String)
                                   };

            var objectType = new ApiObjectType("object", objectFields);

            var api = new ApiDescription(
                "TestApi1",
                "0.0.0.1",
                new[] { viewerType, objectType },
                new[] { viewerType.CreateField("viewer"), objectType.CreateField("object", EnFieldFlags.IsConnection) });

            var provider = new MoqProvider
            {
                Description = api,
                Data = @"{
	                    ""viewer"": {
		                    ""id"": ""FD73BAFB-3698-4FA1-81F5-27C8C83BB4F0"", 
		                    ""name"": ""test name"",
		                    ""numbers"": [1, 2, 3],
                            ""objects"": [
			                    {""id"": 30, ""name"": ""test name""}, 
			                    {""id"": 40, ""name"": ""test name2""}
		                    ]
	                    }, 
	                    ""object"": {
		                    ""count"": 2, 
		                    ""items"": [
			                    {""id"": 10}, 
			                    {""id"": 20}
		                    ],
                            ""__request"": {
                                ""f"": ""object""
                            }
	                    }
                    }"
            };

            var schema = SchemaGenerator.Generate(new List<ApiProvider> { provider });

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
                                     r.Query = @"
                                query {
                                    api {
                                        viewer {
                                            id,
                                            name,
                                            numbers,
                                            objects {
                                                id,
                                                name
                                            }
                                        },
                                        object {
                                            count,
                                            edges {
                                                cursor,                                                
                                                node {
                                                    __id
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
                                            ""id"": ""fd73bafb-3698-4fa1-81f5-27c8c83bb4f0"",
                                            ""name"": ""test name"",
		                                    ""numbers"": [1, 2, 3],
                                            ""objects"": [ {""id"": 30, ""name"": ""test name""}, {""id"": 40, ""name"": ""test name2""}]
                                          },
                                          ""object"": {
                                            ""count"": 2,
                                            ""edges"": [
                                              {
                                                ""cursor"": 10,
                                                ""node"": {
                                                  ""__id"": 10
                                                }
                                              },
                                              {
                                                ""cursor"": 20,
                                                ""node"": {
                                                  ""__id"": 20
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
        public async Task ConnectionsArgumentsApiTest()
        {
            var viewerType = new ApiObjectType(
                "viewer",
                new[] { ApiField.Scalar("id", EnScalarType.Integer), ApiField.Scalar("name", EnScalarType.String) });

            var objectFields = new[]
                                   {
                                       ApiField.Scalar("id", EnScalarType.Integer, EnFieldFlags.IsKey),
                                       ApiField.Scalar("name", EnScalarType.String)
                                   };

            var objectType = new ApiObjectType("object", objectFields);

            var api = new ApiDescription(
                "TestApi1",
                "0.0.0.1",
                new[] { viewerType, objectType },
                new[] { viewerType.CreateField("viewer"), objectType.CreateField("object", EnFieldFlags.IsConnection) });

            var provider = new MoqProvider
                               {
                                   Description = api,
                                   Data =
                                       "{\"viewer\": {\"id\": 1, \"name\": \"test name\"}, \"object\": { \"count\": 2, \"items\": [{\"id\": 10,  \"name\": \"test object1\"}, {\"id\": 20,  \"name\": \"test object2\"}]}}"
                               };

            var schema = SchemaGenerator.Generate(new List<ApiProvider> { provider });

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
                                                    __id,
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
                                                  ""__id"": 10,
                                                  ""name"": ""test object1""
                                                }
                                              },
                                              {
                                                ""cursor"": 20,
                                                ""node"": {
                                                  ""__id"": 20,
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
        /// Testing generator for some api with enum
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task EnumApiTest()
        {
            var enumType = new ApiEnumType("EnumType", new[] { "item1", "item2" });

            var api = new ApiDescription(
                "TestApi1",
                "0.0.0.1",
                new ApiType[] { enumType },
                new[] { ApiField.Object("enumField", enumType.TypeName) });

            var provider = new MoqProvider { Description = api, Data = "{\"enumField\": \"item2\"}" };

            var schema = SchemaGenerator.Generate(new List<ApiProvider> { provider });
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
                                     r.Query = "query { api { enumField } } ";
                                 }).ConfigureAwait(true);

            this.output.WriteLine("-------- Response -----------");
            var response = new DocumentWriter(true).Write(result);
            this.output.WriteLine(response);

            var expectedResponse = @"{
                                      ""data"": {
                                        ""api"": {
                                            ""enumField"": ""item2"" 
                                        }
                                     }                                      
                                    }";

            Assert.Equal(CleanResponse(expectedResponse), CleanResponse(response));
        }


        /// <summary>
        /// Testing generator for some api with boolean field
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task BoolTest()
        {
            var api = new ApiDescription(
                "TestApi1",
                "0.0.0.1",
                new ApiType[] { },
                new[] { ApiField.Scalar("boolField", EnScalarType.Boolean) });

            var provider = new MoqProvider { Description = api, Data = "{\"boolField\": true}" };

            var schema = SchemaGenerator.Generate(new List<ApiProvider> { provider });
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
                                 r.Query = "query { api { boolField } } ";
                             }).ConfigureAwait(true);

            this.output.WriteLine("-------- Response -----------");
            var response = new DocumentWriter(true).Write(result);
            this.output.WriteLine(response);

            var expectedResponse = @"{
                                      ""data"": {
                                        ""api"": {
                                            ""boolField"": true 
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
        public async Task MethodsApiTest()
        {
            var getObjectsArguments = new[]
                                          {
                                              ApiField.Scalar("id", EnScalarType.Integer),
                                              ApiField.Object("sub", "object")
                                          };

            var viewerFields = new[]
                                   {
                                       ApiField.Scalar("id", EnScalarType.Integer),
                                       ApiField.Scalar("name", EnScalarType.String),
                                       ApiField.Object(
                                           "getObjects",
                                           "object",
                                           EnFieldFlags.IsArray,
                                           getObjectsArguments),
                                       ApiField.Object(
                                           "getObjectConnections",
                                           "object",
                                           EnFieldFlags.IsConnection,
                                           getObjectsArguments),
                                   };
            var viewerType = new ApiObjectType("viewer", viewerFields);

            var objectFields = new[]
                                   {
                                       ApiField.Scalar("id", EnScalarType.Integer, EnFieldFlags.IsKey),
                                       ApiField.Scalar("name", EnScalarType.String)
                                   };

            var objectType = new ApiObjectType("object", objectFields);

            var api = new ApiDescription(
                "TestApi1",
                "0.0.0.1",
                new[] { viewerType, objectType },
                new[] { viewerType.CreateField("viewer"), objectType.CreateField("object", EnFieldFlags.IsConnection) });

            var provider = new MoqProvider
                               {
                                   Description = api,
                                   Data =
                                       "{\"viewer\": {\"id\": 1, \"name\": \"test name\", \"getObjects\": [{\"id\": 10,  \"name\": \"test object1\"}, {\"id\": 20,  \"name\": \"test object2\"}], \"getObjectConnections\": { \"count\": 2, \"items\": [{\"id\": 10,  \"name\": \"test object1\"}, {\"id\": 20,  \"name\": \"test object2\"}]}}, \"object\": { \"count\": 2, \"items\": [{\"id\": 10,  \"name\": \"test object1\"}, {\"id\": 20,  \"name\": \"test object2\"}]}}"
                               };

            var schema = SchemaGenerator.Generate(new List<ApiProvider> { provider });

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
                                     r.Query = @"
                                query TestQuery {
                                    api {
                                        viewer {
                                            id,
                                            name,
                                            getObjects(id: 10, sub: { id: 20, name: ""test arg"" }) {
                                                id,
                                                name
                                            },
                                            getObjectConnections(id: 10, sub: { id: 20, name: ""test arg"" }) {
                                                count,
                                                edges {
                                                    cursor,                                                
                                                    node {
                                                        __id,
                                                        name
                                                    }
                                                }
                                            }
                                        },
                                        object(filter: { id: 10, AND: [{name: ""test"", OR: [{id_lt: 20}]}] }, sort: [name_DESC, id_ASC], limit: 10, offset: 20) {
                                            count,
                                            edges {
                                                cursor,                                                
                                                node {
                                                    __id,
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
                                            ""name"": ""test name"",
                                            ""getObjects"": [
                                              {
                                                ""id"": 10,
                                                ""name"": ""test object1""
                                              },
                                              {
                                                ""id"": 20,
                                                ""name"": ""test object2""
                                              }
                                            ],
                                            ""getObjectConnections"": {
                                              ""count"": 2,
                                              ""edges"": [
                                                {
                                                  ""cursor"": 10,
                                                  ""node"": {
                                                    ""__id"": 10,
                                                    ""name"": ""test object1""
                                                  }
                                                },
                                                {
                                                  ""cursor"": 20,
                                                  ""node"": {
                                                    ""__id"": 20,
                                                    ""name"": ""test object2""
                                                  }
                                                }
                                              ]
                                            }
                                          },
                                          ""object"": {
                                            ""count"": 2,
                                            ""edges"": [
                                              {
                                                ""cursor"": 10,
                                                ""node"": {
                                                  ""__id"": 10,
                                                  ""name"": ""test object1""
                                                }
                                              },
                                              {
                                                ""cursor"": 20,
                                                ""node"": {
                                                  ""__id"": 20,
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
            var viewerType1 = new ApiObjectType(
                "viewer1",
                new[] { ApiField.Scalar("id", EnScalarType.Integer), ApiField.Scalar("name", EnScalarType.String) });

            var viewerType2 = new ApiObjectType(
                "viewer2",
                new[] { ApiField.Scalar("description", EnScalarType.String) });

            var objectType1 = new ApiObjectType("object", new[] { ApiField.Scalar("id", EnScalarType.String) });
            var objectType2 = new ApiObjectType("object", new[] { ApiField.Scalar("id", EnScalarType.String) });

            var api1 = new ApiDescription(
                "TestApi1",
                "0.0.0.1",
                new[] { viewerType1, objectType1 },
                new[] { viewerType1.CreateField("viewer"), objectType1.CreateField("object1") });

            var api2 = new ApiDescription(
                "TestApi2",
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

            var schema = SchemaGenerator.Generate(new List<ApiProvider> { provider1, provider2 });

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
        /// Testing mutation description
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task MutationTest()
        {
            var viewerFields = new[]
                                   {
                                       ApiField.Scalar("id", EnScalarType.Integer),
                                       ApiField.Scalar("name", EnScalarType.String),
                                   };

            var viewerType = new ApiObjectType("viewer", viewerFields);

            var objectFields = new[]
                                   {
                                       ApiField.Scalar("id", EnScalarType.Integer, EnFieldFlags.IsKey),
                                       ApiField.Scalar("name", EnScalarType.String)
                                   };

            var objectType = new ApiObjectType("object", objectFields);

            var mutations = new[]
                                {
                                    ApiField.Object(
                                        "objects_create",
                                        "object",
                                        arguments: new[] { objectType.CreateField("new") }),
                                    ApiField.Object(
                                        "objects_update",
                                        "object",
                                        arguments: new[] { objectType.CreateField("new"), ApiField.Scalar("id", EnScalarType.Integer) }),
                                    ApiField.Object(
                                        "objects_delete",
                                        "object",
                                        arguments: new[] { ApiField.Scalar("id", EnScalarType.Integer) })
                                };

            var api = new ApiDescription(
                "TestApi1",
                "0.0.0.1",
                new[] { viewerType, objectType },
                new[] { viewerType.CreateField("viewer"), ApiField.Object("objects", "object", EnFieldFlags.IsConnection) },
                mutations);

            var provider = new MoqProvider { Description = api, Data = "{\"id\": 20,  \"name\": \"new object\"}" };

            var schema = SchemaGenerator.Generate(new List<ApiProvider> { provider });

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
                                     r.Query = @"
                                                    mutation M {
                                                        insert: TestApi1_objects_create(new: {id: 1, name: ""new name""}) {
                                                            id,
                                                            name
                                                        }
                                                        update: TestApi1_objects_update(id: 1, new: {id: 2, name: ""updated name""}) {
                                                            id,
                                                            name
                                                        }
                                                        delete: TestApi1_objects_delete(id: 2) {
                                                            id,
                                                            name
                                                        }
                                                    }
                                                    ";
                                 }).ConfigureAwait(true);

            this.output.WriteLine("-------- Response -----------");
            var response = new DocumentWriter(true).Write(result);
            this.output.WriteLine(response);

            var expectedResponse = @"{
                                      ""data"": {
                                        ""insert"": {
                                          ""id"": 20,
                                          ""name"": ""new object""
                                        },
                                        ""update"": {
                                          ""id"": 20,
                                          ""name"": ""new object""
                                        },
                                        ""delete"": {
                                          ""id"": 20,
                                          ""name"": ""new object""
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
        public async Task NonEmptyApiTest()
        {
            var viewerType = new ApiObjectType(
                "viewer",
                new[] { ApiField.Scalar("id", EnScalarType.Integer), ApiField.Scalar("name", EnScalarType.String) });

            var api = new ApiDescription(
                "TestApi1",
                "0.0.0.1",
                new[] { viewerType },
                new[] { viewerType.CreateField("viewer") });

            var provider = new MoqProvider
                               {
                                   Description = api,
                                   Data = "{\"viewer\": {\"id\": 1, \"name\": \"test name\"}}"
                               };

            var schema = SchemaGenerator.Generate(new List<ApiProvider> { provider });

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
        /// Tests introspection query and api descriptions
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task SchemaDescriptionTest()
        {
            var checkAttributeArguments = new[]
                                              {
                                                  ApiField.Scalar(
                                                      "attribute",
                                                      EnScalarType.String,
                                                      description: "attribute to check")
                                              };

            var objectFields = new[]
                                   {
                                       ApiField.Scalar(
                                           "uid",
                                           EnScalarType.Guid,
                                           description: "The object unique identifier"),
                                       ApiField.Scalar("name", EnScalarType.String, description: "The object name"),
                                       ApiField.Scalar(
                                           "attributes",
                                           EnScalarType.String,
                                           EnFieldFlags.IsArray,
                                           description: "The object attributes"),
                                       ApiField.Scalar(
                                           "checkAttribute",
                                           EnScalarType.Boolean,
                                           arguments: checkAttributeArguments,
                                           description: "checks the attribute")
                                   };

            var objectType = new ApiObjectType("object", objectFields) { Description = "Some abstract object" };
            var mutations = new[]
                                {
                                    ApiField.Object(
                                        "objects_create",
                                        "object",
                                        arguments: new[] { objectType.CreateField("new", description: "The new object data") },
                                        description: "creates a new object")
                                };

            var api = new ApiDescription(
                          "TestApi",
                          "0.0.0.1",
                          new[] { objectType },
                          new[] { objectType.CreateField("objects", EnFieldFlags.IsConnection, "The objects dataset") },
                          mutations) {
                                        Description = "The test api" 
                                     };

            var provider = new MoqProvider { Description = api };
            var schema = SchemaGenerator.Generate(new List<ApiProvider> { provider });

            var result = await new DocumentExecuter().ExecuteAsync(
                             r =>
                                 {
                                     r.Schema = schema;
                                     r.Query = Resources.IntrospectionQuery;
                                 }).ConfigureAwait(true);
            var response = new DocumentWriter(true).Write(result);
            this.output.WriteLine(response);
            Assert.Equal(CleanResponse(Resources.SchemaDescriptionTestSnapshot), CleanResponse(response));
        }

        /// <summary>
        /// Testing generator for some empty single api
        /// </summary>
        [Fact]
        public void Trivial2Test()
        {
            var api = new ApiDescription
                          {
                              ApiName = "TestApi1",
                              TypeName = "TestApi1",
                              Version = new Version("0.0.0.1"),
                          };

            var provider = new MoqProvider { Description = api };
            var schema = SchemaGenerator.Generate(new List<ApiProvider> { provider });

            using (var printer = new SchemaPrinter(schema))
            {
                var description = printer.Print();
                this.output.WriteLine(description);
                Assert.False(string.IsNullOrWhiteSpace(description));
            }

            Assert.NotNull(schema.Query);
            Assert.Equal(2, schema.Query.Fields.Count());
            Assert.True(schema.Query.HasField("api"));
        }

        /// <summary>
        /// Testing trivial schema generator
        /// </summary>
        [Fact]
        public void TrivialTest()
        {
            var schema = SchemaGenerator.Generate(new List<ApiProvider>());

            using (var printer = new SchemaPrinter(schema))
            {
                var description = printer.Print();
                this.output.WriteLine(description);
                Assert.False(string.IsNullOrEmpty(description));
            }
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
        /// Impersonate api provider with preset data
        /// </summary>
        private class MoqProvider : ApiProvider
        {
            /// <summary>
            /// Gets or sets the api data
            /// </summary>
            public string Data { get; set; }

            /// <inheritdoc />
            public override Task<JObject> GetData(List<ApiRequest> requests, RequestContext context)
            {
                return Task.FromResult(JsonConvert.DeserializeObject<JObject>(this.Data));
            }

            /// <inheritdoc />
            public override Task<JObject> SearchNode(string id, List<RequestPathElement> path, ApiRequest request, RequestContext context)
            {
                return Task.FromResult(JsonConvert.DeserializeObject<JObject>(this.Data));
            }
        }
    }
}