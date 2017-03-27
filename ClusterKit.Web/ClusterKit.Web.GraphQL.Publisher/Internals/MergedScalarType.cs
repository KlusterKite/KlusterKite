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

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The merged type representing scalar value
    /// </summary>
    internal class MergedScalarType : MergedType
    {
        /// <inheritdoc />
        public MergedScalarType(EnScalarType type)
            : base(type.ToString())
        {
            this.ScalarType = type;
        }

        /// <inheritdoc />
        public override string ComplexTypeName => $"Scalar-{this.ScalarType}";

        /// <summary>
        /// Gets the scalar type
        /// </summary>
        public EnScalarType ScalarType { get; }

        /// <inheritdoc />
        public override IGraphType ExtractInterface(ApiProvider provider)
        {
            return this.GenerateGraphType(null, null);
        }

        /// <inheritdoc />
        public override string GetInterfaceName(ApiProvider provider)
        {
            return this.GenerateGraphType(null, null).Name;
        }

        /// <inheritdoc />
        public override IGraphType GenerateGraphType(NodeInterface nodeInterface, List<TypeInterface> interfaces)
        {
            switch (this.ScalarType)
            {
                case EnScalarType.Boolean:
                    return new BooleanGraphType();
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
        public override object Resolve(ResolveFieldContext context)
        {
            var resolve = base.Resolve(context);
            if (resolve is JArray)
            {
                return resolve;
            }

            return (resolve as JValue)?.Value;
        }
    }
}