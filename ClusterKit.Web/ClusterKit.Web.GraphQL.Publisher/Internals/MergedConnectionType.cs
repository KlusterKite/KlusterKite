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
    using ClusterKit.Web.GraphQL.Publisher.GraphTypes;

    using global::GraphQL.Language.AST;
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
        public override string Description => $"The list of connected {this.elementType.ComplexTypeName}\n {this.elementType.Description}";

        /// <inheritdoc />
        public override string ComplexTypeName => $"{this.OriginalTypeName}_Connection";

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
                                         Resolver = new CountResolver(),
                                         Description = "The total count of objects satisfying filter conditions"
                                     },
                                 new FieldType
                                     {
                                         Name = "edges",
                                         Description = "The list of edges according to filtering and paging conditions",
                                         ResolvedType = new VirtualGraphType("tmp"),
                                         Metadata = new Dictionary<string, object>
                                                        {
                                                            { MetaDataTypeKey, new MergedField("edges", this.edgeType, EnFieldFlags.IsArray, description: "The list of edges according to filtering and paging conditions") }
                                                        }
                                     }
                             };

            return new VirtualGraphType(this.ComplexTypeName, fields) { Description = this.Description };
        }

        /// <inheritdoc />
        public override IEnumerable<QueryArgument> GenerateArguments(Dictionary<string, IGraphType> registeredTypes)
        {
            // todo: move to declared types use
            if (!(this.elementType is MergedObjectType))
            {
                yield break;
            }

            yield return
                new QueryArgument(typeof(GraphType)) { Name = "filter", ResolvedType = this.GenerateFilterType(), Description = "The filtering conditions" };

            yield return new QueryArgument(typeof(GraphType)) { Name = "sort", ResolvedType = this.GenerateSortType(), Description = "The sorting function (the sequence of functions to sort, the next function will be used if all previous will give equal values)" };

            yield return new QueryArgument(typeof(GraphType)) { Name = "limit", ResolvedType = new IntGraphType(), Description = "The maximum number of objects to return" };

            yield return new QueryArgument(typeof(GraphType)) { Name = "offset", ResolvedType = new IntGraphType(), Description = "The number of objects to skip from the start of list" };
        }

        /// <inheritdoc />
        public override IEnumerable<ApiRequest> GatherSingleApiRequest(Field contextFieldAst)
        {
            foreach (var field in contextFieldAst.SelectionSet.Selections.OfType<Field>())
            {
                switch (field.Name)
                {
                    case "count":
                        yield return new ApiRequest { FieldName = "count" };
                        break;
                    case "edges":
                        {
                            var nodeSelection =
                                field.SelectionSet.Selections.OfType<Field>()
                                    .FirstOrDefault(f => f.Name == "node");

                            var fields = nodeSelection != null
                                             ? this.elementType.GatherSingleApiRequest(nodeSelection).ToList()
                                             : null;

                            yield return new ApiRequest { FieldName = "items", Fields = fields };
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// Generates filter graph type object
        /// </summary>
        /// <returns>The filter graph type</returns>
        private GraphType GenerateFilterType()
        {
            var objectType = (MergedObjectType)this.elementType;
            var graphType = new VirtualInputGraphType($"{this.OriginalTypeName}_Filter") { Description = $"The filter conditions for a {this.elementType.ComplexTypeName} connected objects" };
            graphType.AddField(new FieldType { Name = "AND", ResolvedType = new ListGraphType(graphType), Description = "The filtering conditions that will pass if all internal conditions are passed" });
            graphType.AddField(new FieldType { Name = "OR", ResolvedType = new ListGraphType(graphType), Description = "The filtering conditions that will pass if any of internal conditions is passed" });
            foreach (var itemField in objectType.Fields.Where(p => p.Value.Type is MergedScalarType && !p.Value.Flags.HasFlag(EnFieldFlags.IsArray) && !p.Value.Arguments.Any()))
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
                        graphType.AddFields(this.GenerateStrictEqualFilterFields(itemField.Key, itemField.Value.Description, type.GenerateGraphType()));
                        break;
                    case EnScalarType.Enum:
                        // todo: work with enum
                        break;
                    case EnScalarType.Float:
                    case EnScalarType.Decimal:
                    case EnScalarType.Integer:
                        graphType.AddFields(this.GenerateNumberFilterFields(itemField.Key, itemField.Value.Description, type.GenerateGraphType()));
                        break;
                    case EnScalarType.String:
                        graphType.AddFields(this.GenerateStringFilterFields(itemField.Key, itemField.Value.Description));
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
            var enumType = new EnumerationGraphType { Name = $"{this.OriginalTypeName}_OrderByEnum", Description = $"The list of {this.elementType.ComplexTypeName} fields that can be used as sorting functions" };
            foreach (var itemField in objectType.Fields.Where(p => p.Value.Type is MergedScalarType && !p.Value.Flags.HasFlag(EnFieldFlags.IsArray) && !p.Value.Arguments.Any()))
            {
                enumType.AddValue($"{itemField.Key}_asc", $"{itemField.Key} ascending\n{itemField.Value.Description}", $"{itemField.Key}_asc");
                enumType.AddValue($"{itemField.Key}_desc", $"{itemField.Key} descending\n{itemField.Value.Description}", $"{itemField.Key}_desc");
            }

            return new ListGraphType(enumType) { Description = "The sorting instructions" };
        }

        /// <summary>
        /// Generates the filters for string properties of an object
        /// </summary>
        /// <param name="fieldName">The field name</param>
        /// <param name="fieldDescription">The field description</param>
        /// <returns>The list of properties</returns>
        private IEnumerable<FieldType> GenerateStringFilterFields(string fieldName, string fieldDescription)
        {
            yield return new FieldType { Name = fieldName, ResolvedType = new StringGraphType(), Description = $"The {fieldName} exactly equals the value\n{fieldDescription}" };
            yield return new FieldType { Name = $"{fieldName}_not", ResolvedType = new StringGraphType(), Description = $"The {fieldName} not equals the value\n{fieldDescription}" };
            yield return new FieldType { Name = $"{fieldName}_in", ResolvedType = new StringGraphType(), Description = $"The value contains the {fieldName} as substring\n{fieldDescription}" };
            yield return new FieldType { Name = $"{fieldName}_not_in", ResolvedType = new StringGraphType(), Description = $"The value not contains the {fieldName} as substring\n{fieldDescription}" };
            yield return new FieldType { Name = $"{fieldName}_contains", ResolvedType = new StringGraphType(), Description = $"The {fieldName} contains the value as substring\n{fieldDescription}" };
            yield return new FieldType { Name = $"{fieldName}_not_contains", ResolvedType = new StringGraphType(), Description = $"The {fieldName} not contains the value as substring\n{fieldDescription}" };
            yield return new FieldType { Name = $"{fieldName}_starts_with", ResolvedType = new StringGraphType(), Description = $"The {fieldName} starts with the value\n{fieldDescription}" };
            yield return new FieldType { Name = $"{fieldName}_not_starts_with", ResolvedType = new StringGraphType(), Description = $"The {fieldName} not starts with the value\n{fieldDescription}" };
            yield return new FieldType { Name = $"{fieldName}_ends_with", ResolvedType = new StringGraphType(), Description = $"The {fieldName} ends with the value\n{fieldDescription}" };
            yield return new FieldType { Name = $"{fieldName}_not_ends_with", ResolvedType = new StringGraphType(), Description = $"The {fieldName} not ends with the value\n{fieldDescription}" };
        }

        /// <summary>
        /// Generates the filters for integer properties of an object
        /// </summary>
        /// <param name="fieldName">
        /// The field name
        /// </param>
        /// <param name="fieldDescription">The field description</param>
        /// <param name="graphType">
        /// The field graph Type.
        /// </param>
        /// <returns>
        /// The list of properties
        /// </returns>
        private IEnumerable<FieldType> GenerateNumberFilterFields(string fieldName, string fieldDescription, IGraphType graphType)
        {
            yield return new FieldType { Name = fieldName, ResolvedType = graphType, Description = $"The {fieldName} exactly equals the value\n{fieldDescription}" };
            yield return new FieldType { Name = $"{fieldName}_not", ResolvedType = graphType, Description = $"The {fieldName} not equals the value\n{fieldDescription}" };
            yield return new FieldType { Name = $"{fieldName}_lt", ResolvedType = graphType, Description = $"The {fieldName} is less then the value\n{fieldDescription}" };
            yield return new FieldType { Name = $"{fieldName}_lte", ResolvedType = graphType, Description = $"The {fieldName} is less or equal then the value\n{fieldDescription}" };
            yield return new FieldType { Name = $"{fieldName}_gt", ResolvedType = graphType, Description = $"The {fieldName} is greater then the value\n{fieldDescription}" };
            yield return new FieldType { Name = $"{fieldName}_gte", ResolvedType = graphType, Description = $"The {fieldName} is greater or equal then the value\n{fieldDescription}" };
        }

        /// <summary>
        /// Generates the filters for properties of an object that can be only equal or not equal to value
        /// </summary>
        /// <param name="fieldName">
        /// The field name
        /// </param>
        /// <param name="fieldDescription">The field description</param>
        /// <param name="graphType">
        /// The field graph Type.
        /// </param>
        /// <returns>
        /// The list of properties
        /// </returns>
        private IEnumerable<FieldType> GenerateStrictEqualFilterFields(string fieldName, string fieldDescription, IGraphType graphType)
        {
            yield return new FieldType { Name = fieldName, ResolvedType = graphType, Description = $"The {fieldName} exactly equals the value\n{fieldDescription}" };
            yield return new FieldType { Name = $"{fieldName}_not", ResolvedType = graphType, Description = $"The {fieldName} not equals the value\n{fieldDescription}" };
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
