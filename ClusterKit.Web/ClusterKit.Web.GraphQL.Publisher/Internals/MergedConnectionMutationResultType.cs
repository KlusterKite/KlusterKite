// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MergedConnectionMutationResultType.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The type representing the mutation payload for the connection mutation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher.Internals
{
    using System.Collections.Generic;
    
    using ClusterKit.API.Client;
    using ClusterKit.Web.GraphQL.Publisher.GraphTypes;

    using global::GraphQL.Resolvers;
    using global::GraphQL.Types;

    using JetBrains.Annotations;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The type representing the mutation payload for the connection mutation
    /// </summary>
    internal class MergedConnectionMutationResultType : MergedObjectType
    {
        /// <summary>
        /// The edge type
        /// </summary>
        private readonly MergedApiRoot root;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergedConnectionMutationResultType"/> class.
        /// </summary>
        /// <param name="nodeType">
        /// The node type.
        /// </param>
        /// <param name="root">
        /// The root.
        /// </param>
        /// <param name="errorType">
        /// The error type.
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        public MergedConnectionMutationResultType(
            MergedNodeType nodeType, 
            MergedApiRoot root,
            MergedType errorType,
            ApiProvider provider)
            : base(nodeType.OriginalTypeName)
        {
            this.EdgeType = new MergedEdgeType(nodeType.OriginalTypeName, provider, nodeType);
            this.root = root;
            this.ErrorType = errorType;
            this.Provider = provider;
        }

        /// <summary>
        /// Gets the field provider
        /// </summary>
        [UsedImplicitly]
        public ApiProvider Provider { get; }

        /// <summary>
        /// Gets the root type
        /// </summary>
        public MergedEdgeType EdgeType { get; }
        
        /// <summary>
        /// Gets the error type
        /// </summary>
        public MergedType ErrorType { get; }

        /// <inheritdoc />
        public override string ComplexTypeName => $"{this.OriginalTypeName}_NodeMutationPayload";

        /// <inheritdoc />
        public override IGraphType GenerateGraphType(NodeInterface nodeInterface)
        {
            var graphType = new VirtualGraphType(this.ComplexTypeName);
            graphType.AddField(new FieldType { Name = "deletedId", ResolvedType = new IdGraphType(), Resolver = new DeletedIdResolver() });
            graphType.AddField(this.CreateField("node", this.EdgeType.ObjectType, new ResultNodeResolver(this.EdgeType.ObjectType)));
            graphType.AddField(this.CreateField("edge", this.EdgeType, new ResultEdgeResolver()));
            graphType.AddField(this.CreateField("errors", this.ErrorType, new ResultErrorsResolver()));
            graphType.AddField(new FieldType { Name = "clientMutationId", ResolvedType = new StringGraphType(), Resolver = new ClientMutationIdIdResolver() });
            graphType.AddField(this.CreateField("api", this.root, this.root));
            return graphType;
        }

        /// <inheritdoc />
        public override object Resolve(ResolveFieldContext context)
        {
            var resolve = base.Resolve(context);
            return resolve;
        }

        /// <summary>
        /// Creates virtual field
        /// </summary>
        /// <param name="name">
        /// The field name
        /// </param>
        /// <param name="type">
        /// The field type
        /// </param>
        /// <param name="resolver">
        /// The field resolver
        /// </param>
        /// <param name="flags">
        /// The field flags.
        /// </param>
        /// <returns>
        /// The field
        /// </returns>
        private FieldType CreateField(string name, MergedType type, IFieldResolver resolver = null, EnFieldFlags flags = EnFieldFlags.None)
        {
            var mergedField = new MergedField(name, type, this.Provider, null, flags);
            return this.ConvertApiField(new KeyValuePair<string, MergedField>(name, mergedField), resolver);
        }

        /// <summary>
        /// Resolves data for node value
        /// </summary>
        internal class ClientMutationIdIdResolver : IFieldResolver
        {
            /// <inheritdoc />
            public object Resolve(ResolveFieldContext context)
            {
                return (context.Source as JObject)?.Property("clientMutationId")?.Value;
            }
        }

        /// <summary>
        /// Resolves data for node value
        /// </summary>
        private class ResultNodeResolver : IFieldResolver
        {
            /// <summary>
            /// The node type
            /// </summary>
            private readonly MergedNodeType nodeType;

            /// <summary>
            /// Initializes a new instance of the <see cref="ResultNodeResolver"/> class.
            /// </summary>
            /// <param name="nodeType">
            /// The node type.
            /// </param>
            public ResultNodeResolver(MergedNodeType nodeType)
            {
                this.nodeType = nodeType;
            }

            /// <inheritdoc />
            public object Resolve(ResolveFieldContext context)
            {
                var source = ((JObject)context.Source)?.Property(context.FieldAst.Alias ?? context.FieldAst.Name)?.Value as JObject;
                return source == null ? null : this.nodeType.ResolveData((JObject)source.DeepClone());
            }
        }

        /// <summary>
        /// Resolves data for edge value
        /// </summary>
        private class ResultEdgeResolver : IFieldResolver
        {
            /// <inheritdoc />
            public object Resolve(ResolveFieldContext context)
            {
                return ((JObject)context.Source)?.Property(context.FieldAst.Alias ?? context.FieldAst.Name)?.Value as JObject; 
            }
        }

        /// <summary>
        /// Resolves data for edge value
        /// </summary>
        private class ResultErrorsResolver : IFieldResolver
        {
            /// <inheritdoc />
            public object Resolve(ResolveFieldContext context)
            {
                return ((JObject)context.Source)?.Property(context.FieldAst.Alias ?? context.FieldName)?.Value?.DeepClone();
            }
        } 

        /// <summary>
        /// Resolves data for node value
        /// </summary>
        private class DeletedIdResolver : IFieldResolver
        {
            /// <inheritdoc />
            public object Resolve(ResolveFieldContext context)
            {
                return ((JObject)context.Source)?.Property(context.FieldAst.Alias ?? context.FieldAst.Name)?.Value;
            }
        }
    }
}
