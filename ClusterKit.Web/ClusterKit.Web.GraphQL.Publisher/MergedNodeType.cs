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

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The type to represent node in collection
    /// </summary>
    internal class MergedNodeType : MergedFieldedType
    {
        /// <summary>
        /// The name of original object key field
        /// </summary>
        private readonly string keyName;

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
        public MergedNodeType(FieldProvider provider, MergedObjectType objectType)
            : base(objectType.OriginalTypeName)
        {
            this.Provider = provider;
            this.Fields = objectType.Fields;
            this.keyName = objectType.Fields.FirstOrDefault(f => f.Value.Flags.HasFlag(EnFieldFlags.IsKey)).Key;
            MergedField conflictingField;
            if (objectType.Fields.TryGetValue("id", out conflictingField))
            {
                this.Fields.Remove("id");
                var substitute = new MergedField(
                    "__id",
                    conflictingField.Type,
                    conflictingField.Flags,
                    conflictingField.Arguments.ToDictionary(p => p.Key, p => p.Value),
                    conflictingField.Description);
                this.Fields["__id"] = substitute;
                this.isIdSubstitute = true;
            }
        }

        /// <summary>
        /// Gets the field provider
        /// </summary>
        public FieldProvider Provider { get; }

        /// <inheritdoc />
        public override string ComplexTypeName => $"{EscapeName(this.OriginalTypeName)}_Node";

        /// <inheritdoc />
        public override IEnumerable<FieldProvider> Providers
        {
            get
            {
                yield return this.Provider;
            }
        }

        /// <inheritdoc />
        public override IGraphType GenerateGraphType(NodeInterface nodeInterface)
        {
            var fields = this.Fields.Select(this.ConvertApiField).ToList();
            
            fields.Add(new FieldType
                           {
                               Name = "id",
                               Type = typeof(IdGraphType),
                               Description = "The node global id",
                               Resolver = new GlobalIdResolver(this.keyName, this.Provider.Provider.Description.ApiName)
                           });
            var graphType = new VirtualGraphType(this.ComplexTypeName, fields) { Description = this.Description };
            graphType.AddResolvedInterface(nodeInterface);
            nodeInterface.AddImplementedType(this.ComplexTypeName, graphType);
            return graphType;
        }

        /// <inheritdoc />
        public override IEnumerable<MergedType> GetAllTypes()
        {
            yield return this;
            foreach (var type in this.Fields.Values)
            {
                foreach (var argumentsValue in type.Arguments.Values.SelectMany(t => t.Type.GetAllTypes()))
                {
                    yield return argumentsValue;
                }

                foreach (var subType in type.Type.GetAllTypes())
                {
                    yield return subType;
                }
            }
        }

        /// <inheritdoc />
        public override object Resolve(ResolveFieldContext context)
        {
            var resolve = base.Resolve(context) as JObject;
            if (this.isIdSubstitute)
            {
                var idProperty = resolve?.Property("id");
                if (idProperty != null)
                {
                    resolve.Remove("id");
                    resolve.Add("__id", idProperty.Value);
                }
            }

            return resolve;
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

            if (requests.All(r => r.FieldName != this.keyName)
                && this.GetRequestedFields(contextFieldAst.SelectionSet, context).Any(f => f.Name == "id"))
            {
                requests.Add(new ApiRequest { FieldName = this.keyName });
            }

            return requests;
        }

        /// <summary>
        /// The resolver for the global id
        /// </summary>
        private class GlobalIdResolver : IFieldResolver
        {
            /// <summary>
            /// The name of original object key field
            /// </summary>
            private readonly string keyName;

            /// <summary>
            /// The api name.
            /// </summary>
            private readonly string apiName;

            /// <summary>
            /// Initializes a new instance of the <see cref="GlobalIdResolver"/> class.
            /// </summary>
            /// <param name="keyName">
            /// The name of original object key field
            /// </param>
            /// <param name="apiName">
            /// The api Name.
            /// </param>
            public GlobalIdResolver(string keyName, string apiName)
            {
                this.keyName = keyName;
                this.apiName = apiName;
            }

            /// <inheritdoc />
            public object Resolve(ResolveFieldContext context)
            {
                var presetGlobalId = ((JObject)context.Source)?.GetValue("__globalId");
                if (presetGlobalId != null)
                {
                    return presetGlobalId;
                }

                var requestPath = (((JObject)context.Source)?.Parent?.Parent?.Parent as JObject)?.GetValue("__requestPath") as JArray;
                var id = ((JObject)context.Source)?.GetValue(this.keyName);
                if (requestPath == null || id == null)
                {
                    return null;
                }

                var globalId = new JObject { { "p", requestPath }, { "api", this.apiName }, { "id", id } };
                return globalId.ToString(Formatting.None);
            }
        }
    }
}
