// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MergedUntypedMutationResult.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The type representing the mutation payload for the untyped mutation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher.Internals
{
    using System.Collections.Generic;

    using ClusterKit.API.Client;
    using ClusterKit.Web.GraphQL.Publisher.GraphTypes;

    using global::GraphQL.Resolvers;
    using global::GraphQL.Types;

    /// <summary>
    /// The type representing the mutation payload for the untyped mutation
    /// </summary>
    internal class MergedUntypedMutationResult : MergedObjectType
    {
        /// <summary>
        /// The edge type
        /// </summary>
        private readonly MergedApiRoot root;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergedUntypedMutationResult"/> class.
        /// </summary>
        /// <param name="originalReturnType">
        /// The original mutation return type.
        /// </param>
        /// <param name="root">
        /// The root.
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        public MergedUntypedMutationResult(
            MergedObjectType originalReturnType,
            MergedApiRoot root,
            FieldProvider provider)
            : base(originalReturnType.OriginalTypeName)
        {
            this.OriginalReturnType = originalReturnType;
            this.root = root;
            this.Provider = provider;
        }

        /// <summary>
        /// Gets the provider
        /// </summary>
        public FieldProvider Provider { get; }

        /// <inheritdoc />
        public override IEnumerable<FieldProvider> Providers
        {
            get
            {
                yield return this.Provider;
            }
        }

        /// <inheritdoc />
        public override string Description => this.OriginalReturnType.Description;

        /// <inheritdoc />
        public override string ComplexTypeName => $"{EscapeName(this.OriginalTypeName)}_MutationPayload";

        /// <summary>
        /// Gets the original return type
        /// </summary>
        public MergedObjectType OriginalReturnType { get; }

        /// <inheritdoc />
        public override IGraphType GenerateGraphType(NodeInterface nodeInterface)
        {
            var graphType = new VirtualGraphType(this.ComplexTypeName);
            graphType.AddField(this.CreateField("result", this.OriginalReturnType, new ResultResolver()));
            graphType.AddField(new FieldType { Name = "clientMutationId", ResolvedType = new StringGraphType(), Resolver = new MergedConnectionMutationResultType.ClientMutationIdIdResolver() });
            graphType.AddField(this.CreateField("api", this.root, this.root));
            return graphType;
        }

        /// <inheritdoc />
        public override IEnumerable<MergedType> GetAllTypes()
        {
            yield return this;
            foreach (var mergedType in this.OriginalReturnType.GetAllTypes())
            {
                yield return mergedType;
            }
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
            var mergedField = new MergedField(name, type, flags);
            return this.ConvertApiField(new KeyValuePair<string, MergedField>(name, mergedField), resolver);
        }

        /// <summary>
        /// The result resolver
        /// </summary>
        private class ResultResolver : IFieldResolver
        {
            /// <inheritdoc />
            public object Resolve(ResolveFieldContext context)
            {
                return context.Source;
            }
        }
    }
}
