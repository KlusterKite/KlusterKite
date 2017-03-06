// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MergedEnumType.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The merged type representing some enum value
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher.Internals
{
    using System.Collections.Generic;
    
    using ClusterKit.API.Client;
    using ClusterKit.Web.GraphQL.Publisher.GraphTypes;

    using global::GraphQL.Types;

    /// <summary>
    /// The merged type representing some enum value
    /// </summary>
    internal class MergedEnumType : MergedType
    {
        /// <summary>
        /// The api enum type.
        /// </summary>
        private readonly ApiEnumType apiEnumType;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergedEnumType"/> class.
        /// </summary>
        /// <param name="apiEnumType">
        /// The api enum type.
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        public MergedEnumType(ApiEnumType apiEnumType, FieldProvider provider)
            : base(apiEnumType.TypeName)
        {
            this.apiEnumType = apiEnumType;
            this.Provider = provider;
        }

        /// <inheritdoc />
        public override string ComplexTypeName
            => $"{EscapeName(this.Provider.Provider.Description.ApiName)}_{EscapeName(this.OriginalTypeName)}";

        /// <summary>
        /// Gets the field provider
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
        public override IGraphType GenerateGraphType(NodeInterface nodeInterface)
        {
            var graphType = new EnumerationGraphType
                                {
                                    Description = this.apiEnumType.Description,
                                    Name = this.ComplexTypeName
                                };
            foreach (var enumValue in this.apiEnumType.Values)
            {
                graphType.AddValue(enumValue, null, enumValue);
            }

            return graphType;
        }

        /// <inheritdoc />
        public override IEnumerable<MergedType> GetAllTypes()
        {
            yield return this;
        }
    }
}