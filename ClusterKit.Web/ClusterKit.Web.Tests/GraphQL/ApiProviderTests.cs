// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiProviderTests.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing the api provider
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Tests.GraphQL
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    using Castle.Core.Internal;

    using ClusterKit.Security.Client;
    using ClusterKit.Web.GraphQL.API;
    using ClusterKit.Web.GraphQL.API.Resolvers;
    using ClusterKit.Web.GraphQL.Client;
    using ClusterKit.Web.GraphQL.Client.Attributes;

    using JetBrains.Annotations;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Testing the api provider
    /// </summary>
    public class ApiProviderTests
    {
        /// <summary>
        /// The output.
        /// </summary>
        private readonly ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiProviderTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public ApiProviderTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// Testing the correct description generation
        /// </summary>
        [Fact]
        public void DescriptionGenerationTest()
        {
            var provider = new TestApi();

            provider.GenerationErrors.ForEach(e => this.output.WriteLine($"Generation error: {e}"));
            provider.GenerationWarnings.ForEach(w => this.output.WriteLine($"Generation warning: {w}"));

            Assert.Equal(0, provider.GenerationErrors.Count);
            Assert.Equal(0, provider.GenerationWarnings.Count);

            var description = provider.ApiDescription;
            Assert.NotNull(description);

            var jsonDescription = JsonConvert.SerializeObject(
                description,
                Formatting.Indented,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Error });
            this.output.WriteLine(jsonDescription);

            Assert.Equal("TestApi", description.ApiName);
            Assert.Equal(this.GetType().Assembly.GetName().Version, description.Version);
            Assert.Equal(1, description.Types.Count);

            var nodeType = description.Types.FirstOrDefault(t => t.TypeName.ToLower().Contains("nodeobject"));
            Assert.NotNull(nodeType);

            Assert.True(description.Fields.Any(f => f.Name == "arrayedObjects"));
            Assert.Equal(nodeType.TypeName, description.Fields.First(f => f.Name == "arrayedObjects").TypeName);
            Assert.Equal(EnFieldFlags.IsArray, description.Fields.First(f => f.Name == "arrayedObjects").Flags);

            Assert.True(description.Fields.Any(f => f.Name == "connectedObjects"));
            Assert.Equal(nodeType.TypeName, description.Fields.First(f => f.Name == "connectedObjects").TypeName);
            Assert.Equal(EnFieldFlags.IsConnection, description.Fields.First(f => f.Name == "connectedObjects").Flags);
            Assert.True(description.Mutations.Any(m => m.Name == "connectedObjects.create"));
            Assert.Equal(nodeType.TypeName, description.Mutations.First(m => m.Name == "connectedObjects.create").TypeName);
            Assert.Equal(nodeType.TypeName, description.Mutations.First(m => m.Name == "connectedObjects.create").Arguments.First().TypeName);
            Assert.True(description.Mutations.Any(m => m.Name == "connectedObjects.update"));
            Assert.Equal(nodeType.TypeName, description.Mutations.First(m => m.Name == "connectedObjects.update").TypeName);
            Assert.Equal(EnScalarType.Guid, description.Mutations.First(m => m.Name == "connectedObjects.update").Arguments.First().ScalarType);
            Assert.Equal(nodeType.TypeName, description.Mutations.First(m => m.Name == "connectedObjects.update").Arguments.Skip(1).First().TypeName);
            Assert.True(description.Mutations.Any(m => m.Name == "connectedObjects.delete"));
            Assert.Equal(nodeType.TypeName, description.Mutations.First(m => m.Name == "connectedObjects.delete").TypeName);
            Assert.Equal(EnScalarType.Guid, description.Mutations.First(m => m.Name == "connectedObjects.delete").Arguments.First().ScalarType);

            Assert.True(description.Fields.Any(f => f.Name == "publishedStringProperty"));
            Assert.Equal(EnScalarType.String, description.Fields.First(f => f.Name == "publishedStringProperty").ScalarType);
            Assert.Equal(EnFieldFlags.IsFilterable | EnFieldFlags.IsSortable, description.Fields.First(f => f.Name == "publishedStringProperty").Flags);

            Assert.True(description.Fields.Any(f => f.Name == "stringArray"));
            Assert.Equal(EnScalarType.String, description.Fields.First(f => f.Name == "stringArray").ScalarType);
            Assert.Equal(EnFieldFlags.IsArray, description.Fields.First(f => f.Name == "stringArray").Flags);

            Assert.False(description.Fields.Any(f => f.Name == "unPublishedStringProperty"));

            Assert.True(description.Fields.Any(f => f.Name == "publishedStringMethod"));
            Assert.Equal(EnScalarType.String, description.Fields.First(f => f.Name == "publishedStringMethod").ScalarType);
            Assert.Equal(EnFieldFlags.None, description.Fields.First(f => f.Name == "publishedStringMethod").Flags);
            Assert.Equal(2, description.Fields.First(f => f.Name == "publishedStringMethod").Arguments.Count);

            Assert.True(description.Fields.Any(f => f.Name == "validateNode"));
            Assert.Equal(EnScalarType.Boolean, description.Fields.First(f => f.Name == "validateNode").ScalarType);
            Assert.Equal(EnFieldFlags.None, description.Fields.First(f => f.Name == "validateNode").Flags);
            Assert.Equal(2, description.Fields.First(f => f.Name == "validateNode").Arguments.Count);

            Assert.True(description.Mutations.Any(m => m.Name == "mutateNode"));
            Assert.Equal(nodeType.TypeName, description.Mutations.First(m => m.Name == "mutateNode").TypeName);
            Assert.Equal(nodeType.TypeName, description.Mutations.First(m => m.Name == "mutateNode").Arguments.First().TypeName);
        }

        /// <summary>
        /// Pretests the compiling capabilities
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task CompileTest()
        {
            var comDomProvider = CodeDomProvider.CreateProvider("C#");
            var compilerParameters = new CompilerParameters { GenerateInMemory = true, IncludeDebugInformation = false };
            compilerParameters.ReferencedAssemblies.AddRange(
                AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic)
                    .Select(
                        a => a.CodeBase.Replace("file:\\", string.Empty).Replace("file:///", string.Empty))
                    .ToArray());
            
            var code = @"
                namespace ClusterKit.Web.GraphQL.Dynamic {
                    using System;
                    using System.Threading.Tasks;
                    using System.Collections.Generic;

                    using Newtonsoft.Json;
                    using Newtonsoft.Json.Linq;
                    
                    using ClusterKit.Security.Client;
                    using ClusterKit.Web.GraphQL.Client;
                    using ClusterKit.Web.GraphQL.API.Resolvers;

                    public class Resolver_E3EA3942A7CF40359011147A82607E51 : PropertyResolver {
                        public override Task<JToken> Resolve(object source, ApiRequest query, RequestContext context, JsonSerializer argumentsSerializer, Action<Exception> onErrorCallback) {
                            return Task.FromResult<JToken>(new JValue(((ClusterKit.Web.Tests.GraphQL.ApiProviderTests.NodeObject)source).Id));
                        }
                    }
                }
            ";

            var compiledResult = comDomProvider.CompileAssemblyFromSource(compilerParameters, code);

            foreach (var error in compiledResult.Errors)
            {
                this.output.WriteLine(error.ToString());
            }

            Assert.False(compiledResult.Errors.HasErrors);

            var resolver =
                (PropertyResolver)
                compiledResult.CompiledAssembly.CreateInstance("ClusterKit.Web.GraphQL.Dynamic.Resolver_E3EA3942A7CF40359011147A82607E51");

            Assert.NotNull(resolver);
            
            var node = new NodeObject { Id = Guid.NewGuid() };
            Assert.Equal(node.Id, await resolver.Resolve(node, null, null, null, e => this.output.WriteLine(e.Message)));
        }

        /// <summary>
        /// The test node object
        /// </summary>
        public class NodeObject
        {
            /// <summary>
            /// Gets or sets the node object id
            /// </summary>
            [UsedImplicitly]
            [DeclareField(Description = "the object uid")]
            public Guid Id { get; set; }

            /// <summary>
            /// Gets or sets the integer value
            /// </summary>
            [UsedImplicitly]
            [DeclareField(Description = "the integer property")]
            public int IntValue { get; set; }

            /// <summary>
            /// Gets or sets the integer value
            /// </summary>
            [UsedImplicitly]
            [DeclareField(Description = "the nullable integer property")]
            public int? IntNullableValue { get; set; }

            /// <summary>
            /// Gets the string array
            /// </summary>
            [UsedImplicitly]
            [DeclareField(Description = "the string array")]
            public string[] StringArray => new[] { "object test 1", "object test 2" };

            /// <summary>
            /// Published scalar method
            /// </summary>
            /// <param name="argument1">String argument number 1</param>
            /// <param name="argument2">String argument number 2</param>
            /// <returns>The test string</returns>
            [UsedImplicitly]
            [DeclareField(Description = "the scalar method")]
            public string PublishedStringMethod(string argument1, int argument2) => "Published string method";
        }

        /// <summary>
        /// The test non-node object
        /// </summary>
        [UsedImplicitly]
        [ApiDescription(Description = "Side non-node object")]
        public class NotNodeObject
        {
            /// <summary>
            /// Gets or sets the node object id
            /// </summary>
            [DeclareField]
            [UsedImplicitly]
            public Guid Id { get; set; }
        }

        /// <summary>
        /// Some test api
        /// </summary>
        [ApiDescription(Name = "TestApi", Description = "Test api containing all possible field / method combinations")]
        public class TestApi : ApiProvider
        {
            /// <summary>
            /// The node object connection
            /// </summary>
            private readonly NodeObjectConnection nodeObjectConnection = new NodeObjectConnection();

            /// <summary>
            /// Gets just the array of objects
            /// </summary>
            [UsedImplicitly]
            [DeclareField(Description = "An array of objects")]
            public Task<IEnumerable<NodeObject>> ArrayedObjects => Task.FromResult((IEnumerable<NodeObject>)new[] { new NodeObject(), new NodeObject() });

            /// <summary>
            /// Gets the list of connected objects
            /// </summary>
            [UsedImplicitly]
            [DeclareConnection(Description = "The connected objects", CanCreate = true, CanUpdate = true, CanDelete = true)]
            public INodeConnection<NodeObject, Guid> ConnectedObjects => this.nodeObjectConnection;

            /// <summary>
            /// The published string property.
            /// </summary>
            [UsedImplicitly]
            [DeclareField(Description = "Published string")]
            public string PublishedStringProperty => "Published string";

            /// <summary>
            /// Gets the string array
            /// </summary>
            [UsedImplicitly]
            [DeclareField(Description = "Published string array")]
            public string[] StringArray => new[] { "test 1", "test 2" };

            /// <summary>
            /// The unpublished string property.
            /// </summary>
            [UsedImplicitly]
            public string UnPublishedStringProperty => "Unpublished string";

            /// <summary>
            /// Published scalar method
            /// </summary>
            /// <param name="argument1">String argument number 1</param>
            /// <param name="argument2">String argument number 2</param>
            /// <returns>The test string</returns>
            [UsedImplicitly]
            [DeclareField(Description = "Published string method")]
            public string PublishedStringMethod(
                [ApiDescription(Description = "first argument")] string argument1,
                [ApiDescription(Description = "second argument")] int argument2) => "Published string method";

            /// <summary>
            /// Published method with object and array arguments
            /// </summary>
            /// <param name="node">An object argument</param>
            /// <param name="properties">An array argument</param>
            /// <returns>The scalar value </returns>
            [UsedImplicitly]
            [DeclareField(Description = "Published method with complex arguments")]
            public bool ValidateNode(NodeObject node, List<string> properties)
            {
                return true;
            }

            /// <summary>
            /// Publishes a mutation
            /// </summary>
            /// <param name="node">The node value</param>
            /// <returns>The node result</returns>
            [DeclareMutation(Description = "Mutation declaration")]
            [UsedImplicitly]
            public Task<NodeObject> MutateNode([ApiDescription(Description = "mutating node")]NodeObject node)
            {
                return Task.FromResult(node);
            }

            /// <summary>
            /// The node object connection
            /// </summary>
            private class NodeObjectConnection : INodeConnection<NodeObject, Guid>
            {
                /// <summary>
                /// The initial nodes list
                /// </summary>
                private readonly List<NodeObject> nodes = new List<NodeObject> { new NodeObject(), new NodeObject() };

                /// <inheritdoc />
                public Task<NodeObject> GetById(Guid id)
                {
                    return Task.FromResult(this.nodes.FirstOrDefault(n => n.Id == id));
                }

                /// <inheritdoc />
                public Task<QueryResult<NodeObject>> Query(Expression<Func<NodeObject, bool>> filter, Expression<Func<IQueryable<NodeObject>, IOrderedQueryable<NodeObject>>> sort, int limit, int offset)
                {
                    var query = this.nodes.AsQueryable().Where(filter);
                    var count = query.Count();
                    var items = sort.Compile().Invoke(query).Skip(offset).Take(limit);
                    return Task.FromResult(new QueryResult<NodeObject> { Count = count, Items = items });
                }

                /// <inheritdoc />
                public Task<NodeObject> Create(NodeObject newNode)
                {
                    newNode.Id = Guid.NewGuid();
                    this.nodes.Add(newNode);
                    return Task.FromResult(newNode);
                }

                /// <inheritdoc />
                public async Task<NodeObject> Update(Guid id, NodeObject newNode, List<string> updatedFields)
                {
                    var oldNode = await this.GetById(id);
                    var updatedProperties = updatedFields
                        .Select(p => typeof(NodeObject).GetProperty(p, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase))
                        .Where(p => p != null && p.CanRead && p.CanWrite);

                    foreach (var property in updatedProperties)
                    {
                        property.SetValue(oldNode, property.GetValue(newNode));
                    }

                    return newNode;
                }

                /// <inheritdoc />
                public async Task<NodeObject> Delete(Guid id)
                {
                    var oldNode = await this.GetById(id);
                    if (oldNode != null)
                    {
                        this.nodes.Remove(oldNode);
                    }

                    return oldNode;
                }
            }
        }

        /// <summary>
        /// Just class to demonstrate api declaration
        /// </summary>
        [ApiDescription(Name = "DemoApi", Description = "Just class do demonstrate api declaration")]
        public class DemoApi : ApiProvider
        {
            /// <summary>
            /// The published scalar property.
            /// </summary>
            /// <remarks>
            /// The result will be transformed to <see cref="JValue"/>
            /// </remarks>
            [UsedImplicitly]
            [DeclareField(Description = "The published scalar property")]
            public string PublishedStringProperty => "Published string";

            /// <summary>
            /// Some nested api
            /// </summary>
            /// <remarks>
            /// The result will be resolved for each nested field / method
            /// </remarks>
            [UsedImplicitly]
            [DeclareField(Description = "The published nested api")]
            public TestApi Viewer => new TestApi();

            /// <summary>
            /// The regular api method
            /// </summary>
            /// <remarks>
            /// The result will be resolved for each nested field / method
            /// </remarks>
            /// <param name="nodeUid">
            /// The node Uid.
            /// </param>
            /// <returns>Some object</returns>
            [UsedImplicitly]
            [DeclareField(Description = "The regular api method")]
            public NodeObject JustMethod([ApiDescription(Description = "The node uid")]Guid nodeUid) => new NodeObject { Id = nodeUid };

            /// <summary>
            /// The api method with context usage. Context argument is not published to api.
            /// </summary>
            /// <remarks>
            /// The result will be resolved for each nested field / method
            /// </remarks>
            /// <param name="context">
            /// The context.
            /// </param>
            /// <param name="nodeUid">
            /// The node Uid.
            /// </param>
            /// <returns>
            /// Some object
            /// </returns>
            [UsedImplicitly]
            [DeclareField(Description = "The api method with context usage")]
            public NodeObject JustMethodWithContext(RequestContext context, [ApiDescription(Description = "The node uid")]Guid nodeUid) => new NodeObject { Id = nodeUid };

            /// <summary>
            /// The api method with sub-request parsing
            /// </summary>
            /// <param name="subRequest">
            /// The sub-request
            /// </param>
            /// <param name="nodeUid">
            /// The node Uid.
            /// </param>
            /// <returns>
            /// Some object
            /// </returns>
            [UsedImplicitly]
            [DeclareField(Description = "The api method with subrequest parsing")]
            public NodeObject JustMethodWithSubRequest(ApiRequest subRequest, [ApiDescription(Description = "The node uid")]Guid nodeUid) => new NodeObject { Id = nodeUid };

            /// <summary>
            /// This method is considered as forwarding to some other api provider. T
            /// </summary>
            /// <remarks>
            /// The result will be forward back to requester with no further process
            /// </remarks>
            /// <param name="subRequest">
            /// The sub-request
            /// </param>
            /// <param name="nodeUid">
            /// The node Uid.
            /// </param>
            /// <returns>
            /// Some object
            /// </returns>
            [UsedImplicitly]
            [DeclareField(Description = "The api method with context usage")]
            public JObject ApiResolveForward(ApiRequest subRequest, [ApiDescription(Description = "The node uid")]Guid nodeUid) => new JObject();
        }
    }
}