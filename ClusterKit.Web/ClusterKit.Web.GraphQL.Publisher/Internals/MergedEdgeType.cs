// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MergedEdgeType.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The edge representative
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher.Internals
{
    using System.Collections.Generic;
    using System.Linq;

    using ClusterKit.Web.GraphQL.Client;

    using global::GraphQL.Resolvers;
    using global::GraphQL.Types;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The edge representative
    /// </summary>
    internal class MergedEdgeType : MergedType
    {
        /// <summary>
        /// The end type
        /// </summary>
        private readonly MergedEndType endType;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergedEdgeType"/> class.
        /// </summary>
        /// <param name="originalTypeName">
        /// The original type name.
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        /// <param name="endType">
        /// The end Type.
        /// </param>
        public MergedEdgeType(string originalTypeName, FieldProvider provider, MergedEndType endType) : base(originalTypeName)
        {
            this.endType = endType;
            this.Provider = provider;
        }

        /// <inheritdoc />
        public override string ComplexTypeName => $"{this.OriginalTypeName}-Edge";

        /// <summary>
        /// Gets the field provider
        /// </summary>
        public FieldProvider Provider { get; }

        /// <inheritdoc />
        public override IEnumerable<FieldProvider> Providers => new[]
                                                                    {
                                                                        this.Provider
                                                                    };

        /// <inheritdoc />
        public override IGraphType GenerateGraphType()
        {
            var fields = new List<FieldType>
                             {
                                 new FieldType
                                     {
                                         Name = "cursor",
                                         ResolvedType = new StringGraphType(),
                                         Resolver = new CursorResolver()
                                     },
                                 new FieldType
                                     {
                                         Name = "node",
                                         Metadata =
                                             new Dictionary<string, object>
                                                 {
                                                     {
                                                         MetaDataKey,
                                                         this.endType
                                                     }
                                                 }
                                     }
                             };


            return new VirtualGraphType(this.ComplexTypeName, fields);
        }

        /// <inheritdoc />
        public override IEnumerable<MergedType> GetAllTypes()
        {
            return this.endType.GetAllTypes().Union(new[] { this });
        }

        /// <inheritdoc />
        public override IGraphType WrapForField(IGraphType type)
        {
            return new ListGraphType(type);
        }

        /// <inheritdoc />
        public override object Resolve(ResolveFieldContext context)
        {
            var parentData = context.Source as JObject;
            return parentData?.GetValue("items");
        }

        /// <summary>
        /// Resolves value for the edge cursor
        /// </summary>
        private class CursorResolver : IFieldResolver
        {
            /// <inheritdoc />
            public object Resolve(ResolveFieldContext context)
            {
                var parentData = context.Source as JObject;
                return parentData?.GetValue("id");
            }
        }
    }
}