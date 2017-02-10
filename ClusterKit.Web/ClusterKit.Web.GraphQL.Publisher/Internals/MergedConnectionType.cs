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
        /// The object end type
        /// </summary>
        private readonly MergedEndType endType;

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
            this.endType = endType;
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


        /// <inheritdoc />
        public override QueryArguments GenerateArguments()
        {
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
            var graphType = new VirtualInputGraphType($"{this.OriginalTypeName}-Filter");
            graphType.AddField(new FieldType { Name = "AND", ResolvedType = new ListGraphType(graphType) });
            graphType.AddField(new FieldType { Name = "OR", ResolvedType = new ListGraphType(graphType) });
            foreach (var itemField in this.endType.Fields.Where(p => p.Value.Flags.HasFlag(ApiField.EnFlags.IsScalar)))
            {
                var type = itemField.Value.Type as MergedEndType;
                if (type == null)
                {
                    continue;
                }

                if (type.ComplexTypeName == ApiField.TypeNameString)
                {
                    graphType.AddFields(this.GenerateStringFilterFields(itemField.Key));
                }

                if (type.ComplexTypeName == ApiField.TypeNameInt)
                {
                    graphType.AddFields(this.GenerateIntFilterFields(itemField.Key));
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
            var enumType = new EnumerationGraphType { Name = $"{this.OriginalTypeName}-OrderByEnum" };
            foreach (var itemField in this.endType.Fields.Where(p => p.Value.Flags.HasFlag(ApiField.EnFlags.IsScalar)))
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
        }


        /// <summary>
        /// Generates the filters for integer properties of an object
        /// </summary>
        /// <param name="fieldName">The field name</param>
        /// <returns>The list of properties</returns>
        private IEnumerable<FieldType> GenerateIntFilterFields(string fieldName)
        {
            yield return new FieldType { Name = fieldName, ResolvedType = new IntGraphType() };
            yield return new FieldType { Name = $"{fieldName}_not", ResolvedType = new IntGraphType() };
            yield return new FieldType { Name = $"{fieldName}_lt", ResolvedType = new IntGraphType() };
            yield return new FieldType { Name = $"{fieldName}_lte", ResolvedType = new IntGraphType() };
            yield return new FieldType { Name = $"{fieldName}_gt", ResolvedType = new IntGraphType() };
            yield return new FieldType { Name = $"{fieldName}_gte", ResolvedType = new IntGraphType() };
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
