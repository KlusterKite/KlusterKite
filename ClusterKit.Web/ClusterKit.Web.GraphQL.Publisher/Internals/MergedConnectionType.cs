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

    using ClusterKit.Web.GraphQL.Client;

    using global::GraphQL.Resolvers;
    using global::GraphQL.Types;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The connection representation
    /// </summary>
    internal class MergedConnectionType : MergedType
    {
        /// <summary>
        /// The type of the edge
        /// </summary>
        private readonly MergedEdgeType edgeType;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergedConnectionType"/> class.
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
        public MergedConnectionType(string originalTypeName, FieldProvider provider, MergedEndType endType) : base(originalTypeName)
        {
            this.Provider = provider;
            this.edgeType = new MergedEdgeType(this.OriginalTypeName, provider, endType);
        }

        /// <summary>
        /// Gets the field provider
        /// </summary>
        public FieldProvider Provider { get; }

        /// <inheritdoc />
        public override string ComplexTypeName => $"{this.OriginalTypeName}-Connection";

        /// <inheritdoc />
        public override IEnumerable<FieldProvider> Providers => new[]
                                                                    {
                                                                        this.Provider
                                                                    };

        /// <inheritdoc />
        public override object Resolve(ResolveFieldContext context)
        {
            var parentData = context.Source as JObject;
            return parentData?.GetValue(context.FieldName);
        }

        /// <inheritdoc />
        public override IEnumerable<MergedType> GetAllTypes()
        {
            return this.edgeType.GetAllTypes().Union(new[] { this });
        }

        /// <inheritdoc />
        public override IGraphType GenerateGraphType()
        {
            var fields = new List<FieldType>
                             {
                                 new FieldType
                                     {
                                         Name = "count",
                                         ResolvedType = new IntGraphType(),
                                         Resolver = new CountResolver()
                                     },
                                 new FieldType
                                     {
                                         Name = "edges",
                                         Metadata = new Dictionary<string, object> { { MetaDataKey, this.edgeType } }
                                     }
                             };


            return new VirtualGraphType(this.ComplexTypeName, fields);
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
                return parentData?.GetValue("count");
            }
        }
    }
}
