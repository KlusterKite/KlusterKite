// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MergedNodeType.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The type to represent node in collection
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher
{
    using System.Collections.Generic;
    using System.Linq;

    using ClusterKit.API.Client;
    using ClusterKit.Web.GraphQL.Publisher.GraphTypes;
    using ClusterKit.Web.GraphQL.Publisher.Internals;

    using global::GraphQL.Language.AST;
    using global::GraphQL.Resolvers;
    using global::GraphQL.Types;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The type to represent node in collection
    /// </summary>
    internal class MergedNodeType : MergedFieldedType
    {
        /// <summary>
        /// A value indicating whether the original id field was substituted
        /// </summary>
        private readonly bool isIdSubstitute;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergedNodeType"/> class.
        /// </summary>
        /// <param name="provider">
        /// The provider.
        /// </param>
        /// <param name="objectType">
        /// The object Type.
        /// </param>
        public MergedNodeType(
            ApiProvider provider, 
            MergedObjectType objectType)
            : base(objectType.OriginalTypeName)
        {
            this.Provider = provider;
            this.Fields = objectType.Fields.ToDictionary(f => f.Key, f => f.Value.Clone());
            this.KeyName = objectType.Fields.FirstOrDefault(f => f.Value.Flags.HasFlag(EnFieldFlags.IsKey)).Key;
            MergedField conflictingField;
            if (objectType.Fields.TryGetValue("id", out conflictingField))
            {
                this.Fields.Remove("id");
                var substitute = new MergedField(
                    "__id",
                    conflictingField.Type,
                    this.Provider,
                    null,
                    conflictingField.Flags,
                    conflictingField.Arguments.ToDictionary(p => p.Key, p => p.Value),
                    conflictingField.Description);
                this.Fields["__id"] = substitute;
                this.isIdSubstitute = true;
            }
        }

        /// <summary>
        /// Gets the name of original object key field
        /// </summary>
        public string KeyName { get; }

        /// <summary>
        /// Gets the field provider
        /// </summary>
        public ApiProvider Provider { get; }

        /// <inheritdoc />
        public override string ComplexTypeName => $"{EscapeName(this.OriginalTypeName)}_Node";

        /// <inheritdoc />
        public override IGraphType GenerateGraphType(NodeInterface nodeInterface)
        {
            var fields = this.Fields.Select(this.ConvertApiField).ToList();
            
            fields.Add(new FieldType
                           {
                               Name = "id",
                               Type = typeof(IdGraphType),
                               Description = "The node global id",
                               Resolver = new GlobalIdResolver()
                           });
            var graphType = new VirtualGraphType(this.ComplexTypeName, fields) { Description = this.Description };
            graphType.AddResolvedInterface(nodeInterface);
            nodeInterface.AddImplementedType(this.ComplexTypeName, graphType);
            return graphType;
        }

        /// <inheritdoc />
        public override object Resolve(ResolveFieldContext context)
        {
            var resolve = context.Source as JObject;
            return this.ResolveData(resolve, context.FieldAst.Alias ?? context.FieldAst.Name);
        }

        /// <summary>
        /// Modifies resolved data
        /// </summary>
        /// <param name="source">
        /// The node data
        /// </param>
        /// <param name="fieldName">
        /// The containing field name.
        /// </param>
        /// <returns>
        /// The adapted node data
        /// </returns>
        public JObject ResolveData(JObject source, string fieldName = null)
        {
            if (fieldName != null)
            {
                var filteredSource = new JObject();
                var prefix = $"{fieldName}_";
                foreach (var property in source.Properties().Where(p => p.Name.StartsWith(prefix)))
                {
                    filteredSource.Add(property.Name.Substring(prefix.Length), property.Value);
                }

                filteredSource.Add("__id", source.Property("__id")?.Value);

                var globalId = source.Property("__globalId")?.Value.ToObject<string>();
                if (globalId == null)
                {
                    var requestPath = (source.Parent?.Parent?.Parent as JObject)?.GetValue("__requestPath") as JArray;
                    var id = source.Property("__id")?.Value;
                    if (requestPath != null && id != null)
                    {
                        globalId =
                            new JObject
                                {
                                    { "p", requestPath },
                                    { "api", this.Provider.Description.ApiName },
                                    { "id", id }
                                }.PackGlobalId();
                    }
                }

                if (globalId != null)
                {
                    filteredSource.Add("__globalId", globalId);
                }

                source = filteredSource;
            }

            return source;
        }

        /// <inheritdoc />
        public override IEnumerable<ApiRequest> GatherSingleApiRequest(Field contextFieldAst, ResolveFieldContext context)
        {
            var requests = base.GatherSingleApiRequest(contextFieldAst, context).ToList();
            if (this.isIdSubstitute)
            {
                var list = requests.ToList();
                foreach (var idField in list.Where(r => r.FieldName == "__id"))
                {
                    idField.FieldName = "id";
                }

                requests = list;
            }

            return requests;
        }
        
        /// <summary>
        /// The resolver for the global id
        /// </summary>
        private class GlobalIdResolver : IFieldResolver
        {
            /// <inheritdoc />
            public object Resolve(ResolveFieldContext context)
            {
                return ((JObject)context.Source)?.GetValue("__globalId");
            }
        }
    }
}
