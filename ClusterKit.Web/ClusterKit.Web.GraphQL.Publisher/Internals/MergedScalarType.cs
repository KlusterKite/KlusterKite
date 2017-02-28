// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MergedScalarType.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The merged type representing scalar value
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher.Internals
{
    using System;
    using System.Collections.Generic;

    using ClusterKit.API.Client;
    using ClusterKit.Web.GraphQL.Publisher.GraphTypes;

    using global::GraphQL.Types;

    /// <summary>
    /// The merged type representing scalar value
    /// </summary>
    internal class MergedScalarType : MergedType
    {
        /// <inheritdoc />
        public MergedScalarType(EnScalarType type, FieldProvider provider)
            : base(type.ToString())
        {
            this.Provider = provider;
            this.ScalarType = type;
        }

        /// <inheritdoc />
        public override string ComplexTypeName => $"Scalar-{this.ScalarType}";

        /// <summary>
        /// Gets the field provider
        /// </summary>
        public FieldProvider Provider { get; }

        /// <inheritdoc />
        public override IEnumerable<FieldProvider> Providers => new[]
                                                                    {
                                                                        this.Provider
                                                                    };

        /// <summary>
        /// Gets the scalar type
        /// </summary>
        public EnScalarType ScalarType { get; }

        /// <inheritdoc />
        public override IGraphType GenerateGraphType()
        {
            switch (this.ScalarType)
            {
                case EnScalarType.Boolean:
                    return new BooleanGraphType();
                case EnScalarType.Enum:
                    throw new NotImplementedException();
                case EnScalarType.Float:
                    return new FloatGraphType();
                case EnScalarType.Decimal:
                    return new DecimalGraphType();
                case EnScalarType.Integer:
                    return new IntGraphType();
                case EnScalarType.String:
                    return new StringGraphType();
                case EnScalarType.Guid:
                    return new GuidGraphType();
                case EnScalarType.DateTime:
                    return new DateGraphType();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc />
        public override IEnumerable<MergedType> GetAllTypes()
        {
            yield return this;
        }
    }
}