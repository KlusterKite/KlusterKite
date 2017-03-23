// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MergedConnectionType.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the MergedConnectionType type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher.Internals
{
    using System.Collections.Generic;
    using System.Linq;

    using ClusterKit.API.Client;
    using ClusterKit.Web.GraphQL.Publisher.GraphTypes;

    using global::GraphQL.Language.AST;
    using global::GraphQL.Resolvers;
    using global::GraphQL.Types;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The connection representation
    /// </summary>
    internal class MergedConnectionType : MergedType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MergedConnectionType"/> class.
        /// </summary>
        /// <param name="originalTypeName">
        /// The original type name.
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        /// <param name="elementType">
        /// The end Type.
        /// </param>
        public MergedConnectionType(string originalTypeName, ApiProvider provider, MergedObjectType elementType)
            : base(originalTypeName)
        {
            this.ElementType = elementType;
            this.Provider = provider;
            this.EdgeType = new MergedEdgeType(this.OriginalTypeName, provider, this.ElementType);
        }

        /// <inheritdoc />
        public override string ComplexTypeName => $"{EscapeName(this.OriginalTypeName)}_Connection";

        /// <inheritdoc />
        public override string Description
            => $"The list of connected {this.ElementType.ComplexTypeName}\n {this.ElementType.Description}";

        /// <summary>
        /// Gets the object end type
        /// </summary>
        public MergedObjectType ElementType { get; }

        /// <summary>
        /// Gets the type of the edge
        /// </summary>
        public MergedEdgeType EdgeType { get; }

        /// <summary>
        /// Gets the field provider
        /// </summary>
        public ApiProvider Provider { get; }

        /// <inheritdoc />
        public override IEnumerable<ApiRequest> GatherSingleApiRequest(
            Field contextFieldAst,
            ResolveFieldContext context)
        {
            foreach (var field in GetRequestedFields(contextFieldAst.SelectionSet, context, this.ComplexTypeName))
            {
                switch (field.Name)
                {
                    case "count":
                        yield return new ApiRequest { FieldName = "count", Alias = field.Alias };
                        break;
                    case "edges":
                        {
                            var fields = new List<ApiRequest>
                                             {
                                                 new ApiRequest { FieldName = this.ElementType.KeyField.FieldName, Alias = "__id" }
                                             };
                            foreach (var nodeRequest in
                                GetRequestedFields(field.SelectionSet, context, this.ElementType.ComplexTypeName)
                                    .Where(f => f.Name == "node"))
                            {
                                fields.AddRange(
                                    this.ElementType.GatherSingleApiRequest(nodeRequest, context).Select(
                                        f =>
                                            {
                                                f.Alias =
                                                    $"{nodeRequest.Alias ?? nodeRequest.Name}_{f.Alias ?? f.FieldName}";
                                                return f;
                                            }).ToList());
                            }

                            yield return
                                new ApiRequest
                                {
                                    FieldName = "items",
                                    Alias = field.Alias ?? field.Name,
                                    Fields = fields
                                };
                        }

                        break;
                }
            }
        }

        /// <inheritdoc />
        public override IGraphType GenerateGraphType(NodeInterface nodeInterface)
        {
            var fields = new List<FieldType>
                             {
                                 new FieldType
                                     {
                                         Name = "count",
                                         ResolvedType = new IntGraphType(),
                                         Resolver = new CountResolver(),
                                         Description =
                                             "The total count of objects satisfying filter conditions"
                                     },
                                 new FieldType
                                     {
                                         Name = "edges",
                                         Description =
                                             "The list of edges according to filtering and paging conditions",
                                         ResolvedType = new VirtualGraphType("tmp"),
                                         Metadata =
                                             new Dictionary<string, object>
                                                 {
                                                     {
                                                         MetaDataTypeKey,
                                                         new MergedField(
                                                             "edges",
                                                             this.EdgeType,
                                                             this.Provider,
                                                             null,
                                                             EnFieldFlags.IsArray,
                                                             description: "The list of edges according to filtering and paging conditions")
                                                     }
                                                 }
                                     }
                             };

            return new VirtualGraphType(this.ComplexTypeName, fields) { Description = this.Description };
        }

        /// <inheritdoc />
        public override object Resolve(ResolveFieldContext context)
        {
            var resolve = base.Resolve(context) as JObject;
            resolve?.Add("__requestPath", new JArray(this.ResolveRequestPath(resolve)));
            return resolve;
        }

        /// <summary>
        /// Resolves provided request path to the current connection
        /// </summary>
        /// <param name="source">The source element</param>
        /// <returns>The list of requests in path</returns>
        private IEnumerable<JObject> ResolveRequestPath(JToken source)
        {
            if (source.Parent != null)
            {
                foreach (var element in this.ResolveRequestPath(source.Parent))
                {
                    yield return element;
                }
            }

            var request = (source as JObject)?.Property("__request")?.Value as JObject;
            if (request != null)
            {
                yield return request;
            }
        }

        /// <summary>
        /// The count resolver from api
        /// </summary>
        private class CountResolver : IFieldResolver
        {
            /// <inheritdoc />
            public object Resolve(ResolveFieldContext context)
            {
                var parentData = context.Source as JObject;
                return parentData?.GetValue(context.FieldAst.Alias ?? context.FieldAst.Name);
            }
        }
    }
}