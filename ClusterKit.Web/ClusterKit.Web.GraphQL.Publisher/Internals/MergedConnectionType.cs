﻿// --------------------------------------------------------------------------------------------------------------------
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
    using ClusterKit.Web.GraphQL.Publisher.GraphTypes;

    using global::GraphQL.Resolvers;
    using global::GraphQL.Types;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The connection representation
    /// </summary>
    internal class MergedConnectionType : MergedType
    {
        /// <summary>
        /// The object end type
        /// </summary>
        private readonly MergedType elementType;

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
        /// <param name="elementType">
        /// The end Type.
        /// </param>
        public MergedConnectionType(string originalTypeName, FieldProvider provider, MergedType elementType) : base(originalTypeName)
        {
            this.elementType = elementType;
            this.Provider = provider;
            this.edgeType = new MergedEdgeType(this.OriginalTypeName, provider, elementType);
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
        public override IEnumerable<MergedType> GetAllTypes()
        {
            yield return this;
            foreach (var type in this.edgeType.GetAllTypes())
            {
                yield return type;
            }
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
                                         Metadata = new Dictionary<string, object>
                                                        {
                                                            { MetaDataTypeKey, this.edgeType },
                                                            { MetaDataFlagsKey, EnFieldFlags.IsArray }
                                                        }
                                     }
                             };

            return new VirtualGraphType(this.ComplexTypeName, fields);
        }

        /// <inheritdoc />
        public override QueryArguments GenerateArguments()
        {
            if (!(this.elementType is MergedObjectType))
            {
                return null;
            }

            var arguments = new[]
                                {
                                    new QueryArgument(typeof(GraphType))
                                        {
                                            Name = "filter",
                                            ResolvedType = this.GenerateFilterType()
                                        },
                                    new QueryArgument(typeof(GraphType))
                                        {
                                            Name = "sort",
                                            ResolvedType = this.GenerateSortType()
                                        },
                                    new QueryArgument(typeof(GraphType))
                                        {
                                            Name = "limit",
                                            ResolvedType = new IntGraphType()
                                        },
                                    new QueryArgument(typeof(GraphType))
                                        {
                                            Name = "offset",
                                            ResolvedType = new IntGraphType()
                                        },
                                };

            return new QueryArguments(arguments);
        }

        /// <summary>
        /// Generates filter graph type object
        /// </summary>
        /// <returns>The filter graph type</returns>
        private GraphType GenerateFilterType()
        {
            var objectType = (MergedObjectType)this.elementType;
            var graphType = new VirtualInputGraphType($"{this.OriginalTypeName}-Filter");
            graphType.AddField(new FieldType { Name = "AND", ResolvedType = new ListGraphType(graphType) });
            graphType.AddField(new FieldType { Name = "OR", ResolvedType = new ListGraphType(graphType) });
            foreach (var itemField in objectType.Fields.Where(p => p.Value.Type is MergedScalarType))
            {
                var type = (MergedScalarType)itemField.Value.Type;
                if (type == null)
                {
                    continue;
                }

                switch (type.ScalarType)
                {
                    case EnScalarType.Guid:
                    case EnScalarType.Boolean:
                        graphType.AddFields(this.GenerateStrictEqualFilterFields(itemField.Key, type.GenerateGraphType()));
                        break;
                    case EnScalarType.Enum:
                        // todo: work with enum
                        break;
                    case EnScalarType.Float:
                    case EnScalarType.Decimal:
                    case EnScalarType.Integer:
                        graphType.AddFields(this.GenerateNumberFilterFields(itemField.Key, type.GenerateGraphType()));
                        break;
                    case EnScalarType.String:
                        graphType.AddFields(this.GenerateStringFilterFields(itemField.Key));
                        break;
                }
            }

            return graphType;
        }

        /// <summary>
        /// Generates sort graph type object
        /// </summary>
        /// <returns>The filter graph type</returns>
        private GraphType GenerateSortType()
        {
            var objectType = (MergedObjectType)this.elementType;
            var enumType = new EnumerationGraphType { Name = $"{this.OriginalTypeName}-OrderByEnum" };
            foreach (var itemField in objectType.Fields.Where(p => p.Value.Type is MergedScalarType))
            {
                enumType.AddValue($"{itemField.Key}_ASC", string.Empty, $"{itemField.Key}_ASC");
                enumType.AddValue($"{itemField.Key}_DESC", string.Empty, $"{itemField.Key}_DESC");
            }

            return new ListGraphType(enumType);
        }

        /// <summary>
        /// Generates the filters for string properties of an object
        /// </summary>
        /// <param name="fieldName">The field name</param>
        /// <returns>The list of properties</returns>
        private IEnumerable<FieldType> GenerateStringFilterFields(string fieldName)
        {
            yield return new FieldType { Name = fieldName, ResolvedType = new StringGraphType() };
            yield return new FieldType { Name = $"{fieldName}_not", ResolvedType = new StringGraphType() };
            yield return new FieldType { Name = $"{fieldName}_in", ResolvedType = new StringGraphType() };
            yield return new FieldType { Name = $"{fieldName}_not_in", ResolvedType = new StringGraphType() };
            yield return new FieldType { Name = $"{fieldName}_contains", ResolvedType = new StringGraphType() };
            yield return new FieldType { Name = $"{fieldName}_not_contains", ResolvedType = new StringGraphType() };
            yield return new FieldType { Name = $"{fieldName}_starts_with", ResolvedType = new StringGraphType() };
            yield return new FieldType { Name = $"{fieldName}_not_starts_with", ResolvedType = new StringGraphType() };
            yield return new FieldType { Name = $"{fieldName}_ends", ResolvedType = new StringGraphType() };
            yield return new FieldType { Name = $"{fieldName}_ends_with", ResolvedType = new StringGraphType() };
            yield return new FieldType { Name = $"{fieldName}_not_ends_with", ResolvedType = new StringGraphType() };
        }

        /// <summary>
        /// Generates the filters for integer properties of an object
        /// </summary>
        /// <param name="fieldName">
        /// The field name
        /// </param>
        /// <param name="graphType">
        /// The field graph Type.
        /// </param>
        /// <returns>
        /// The list of properties
        /// </returns>
        private IEnumerable<FieldType> GenerateNumberFilterFields(string fieldName, IGraphType graphType)
        {
            yield return new FieldType { Name = fieldName, ResolvedType = graphType };
            yield return new FieldType { Name = $"{fieldName}_not", ResolvedType = graphType };
            yield return new FieldType { Name = $"{fieldName}_lt", ResolvedType = graphType };
            yield return new FieldType { Name = $"{fieldName}_lte", ResolvedType = graphType };
            yield return new FieldType { Name = $"{fieldName}_gt", ResolvedType = graphType };
            yield return new FieldType { Name = $"{fieldName}_gte", ResolvedType = graphType };
        }

        /// <summary>
        /// Generates the filters for properties of an object that can be only equal or not equal to value
        /// </summary>
        /// <param name="fieldName">
        /// The field name
        /// </param>
        /// <param name="graphType">
        /// The field graph Type.
        /// </param>
        /// <returns>
        /// The list of properties
        /// </returns>
        private IEnumerable<FieldType> GenerateStrictEqualFilterFields(string fieldName, IGraphType graphType)
        {
            yield return new FieldType { Name = fieldName, ResolvedType = graphType };
            yield return new FieldType { Name = $"{fieldName}_not", ResolvedType = graphType };
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
