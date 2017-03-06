// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectionResolverGenerator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Generates the <see cref="ConnectionResolver" /> code
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Provider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    using ClusterKit.API.Client;
    using ClusterKit.API.Provider.Resolvers;

    /// <summary>
    /// Generates the <see cref="ConnectionResolver{T,TId}"/> code
    /// </summary>
    internal class ConnectionResolverGenerator : ResolverGenerator
    {
        /// <summary>
        /// The type metadata
        /// </summary>
        private TypeMetadata typeMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionResolverGenerator"/> class.
        /// </summary>
        /// <param name="typeMetadata">
        /// The type metadata.
        /// </param>
        /// <param name="data">
        /// The data.
        /// </param>
        public ConnectionResolverGenerator(TypeMetadata typeMetadata, AssembleTempData data)
            : base(typeMetadata.Type, data)
        {
            this.typeMetadata = typeMetadata;
        }

        /// <inheritdoc />
        protected override string ClassName => "ConnectionResolver"
                                               + $"_{Regex.Replace(ToCSharpRepresentation(this.ObjectType), "[^a-zA-Z0-9]", string.Empty)}"
                                               + $"_{this.Uid:N}";

        /// <inheritdoc />
        public override string Generate()
        {
            var nodeApiType = this.Data.ApiTypeByOriginalTypeNames[this.typeMetadata.Type.FullName];
            var mutationResultType = typeof(MutationResult<>).MakeGenericType(this.typeMetadata.Type);

            ApiType mutationResultApiType;
            string mutationResultResolverDeclaration;
            if (this.Data.ApiTypeByOriginalTypeNames.TryGetValue(mutationResultType.FullName, out mutationResultApiType))
            {
                mutationResultResolverDeclaration = $"new {this.Data.ObjectResolverNames[nodeApiType.TypeName]}()";
            }
            else
            {
                mutationResultResolverDeclaration = "null";
            }

            return $@"
                    namespace ClusterKit.API.Provider.Dynamic 
                    {{
                        using System;
                        using System.Threading.Tasks;
                        using System.Collections.Generic;

                        using System.Linq;
                        using System.Linq.Expressions;

                        using Newtonsoft.Json;
                        using Newtonsoft.Json.Linq;
                    
                        using ClusterKit.Security.Client;
                        using ClusterKit.API.Client;
                        using ClusterKit.API.Provider.Resolvers;

                        // Connection resolver for {ToCSharpRepresentation(this.ObjectType)}
                        public class {this.ClassName} : ConnectionResolver<{ToCSharpRepresentation(this.ObjectType)},{ToCSharpRepresentation(this.typeMetadata.TypeOfId)}>
                        {{  
                            private IResolver nodeResolver = new {this.Data.ObjectResolverNames[nodeApiType.TypeName]}();
                            public override IResolver NodeResolver {{ get {{ return this.nodeResolver; }} }}

                            private IResolver mutationResultResolver = {mutationResultResolverDeclaration};
                            public override IResolver MutationResultResolver {{ get {{ return this.mutationResultResolver; }} }}
          
                            protected override Expression<Func<{ToCSharpRepresentation(this.ObjectType)}, bool>> CreateFilter(JObject filter) 
                            {{
                                return GenerateFilterExpression(filter);
                            }}

                            protected override IEnumerable<SortingCondition> CreateSort(JArray sortArgument)
                            {{
                                return GenerateSortingExpression(sortArgument);
                            }}

                            {this.GenerateConnectionFilter()}
                            {this.GenerateConnectionSorter()}
                        }}
                    }}
            ";
        }

        /// <summary>
        /// Generates method to create filter expression
        /// </summary>
        /// <returns>The properties and methods C# code</returns>
        protected virtual string GenerateConnectionFilter()
        {
            ApiType sortedApiType;
            if (!this.Data.ApiTypeByOriginalTypeNames.TryGetValue(this.typeMetadata.Type.FullName, out sortedApiType))
            {
                throw new InvalidOperationException($"The returned type of connection {this.typeMetadata.Type.FullName} is not registered");
            }

            var sortedApiObjectType = sortedApiType as ApiObjectType;
            if (sortedApiObjectType == null)
            {
                throw new InvalidOperationException($"The returned type of connection {this.typeMetadata.Type.FullName} is not an object type");
            }

            var className = ToCSharpRepresentation(this.typeMetadata.Type);
            var filterable = sortedApiObjectType.Fields
                .Where(f => f.Flags.HasFlag(EnFieldFlags.IsFilterable))
                .Select(f => new { f.Name, Property = this.Data.Members[sortedApiType.TypeName][f.Name] as PropertyInfo, f.ScalarType })
                .Where(d => d.Property != null /*&& d.ScalarType != EnScalarType.None*/)
                .Select(d => new
                {
                    ExpressionParameter = $"filterProperty_{d.Property.Name}",
                    ApiName = d.Name,
                    d.ScalarType,
                    d.Property
                })
                .ToList();

            return $@"
                private static Expression<Func<string, string, bool>> filterStringCheckIn = (left, right) => left.Contains(right);
                private static Expression<Func<string, string, bool>> filterStringStartsWith = (left, right) => left.StartsWith(right);
                private static Expression<Func<string, string, bool>> filterStringEndsWith = (left, right) => left.EndsWith(right);
                private static readonly ParameterExpression filterableEntity = Expression.Parameter(typeof({className}));
                {string.Join(
                        "\n",
                        filterable.Select(f => $"private static readonly Expression {f.ExpressionParameter} "
                                               + $"= Expression.Property(filterableEntity, typeof({className}), \"{f.Property.Name}\");"))}

                private static readonly Dictionary<string, Func<JProperty, Expression>> FilterChecks 
                    = new Dictionary<string, Func<JProperty, Expression>>
                {{
                        {{ ""OR"", prop =>
                                {{
                                    var subFilters = prop.Value as JArray;
                                    if (subFilters != null)
                                    {{
                                        Expression or = Expression.Constant(false);
                                        or = subFilters.Children()
                                            .OfType<JObject>()
                                            .Aggregate(
                                                or,
                                                (current, subFilter) =>
                                                    Expression.Or(current, GenerateFilterExpressionPart(subFilter)));

                                        return or;
                                    }}

                                    return Expression.Constant(true);
                                }}
                            }},

                            {{ ""AND"", prop =>
                                {{
                                    var subFilters = prop.Value as JArray;
                                    if (subFilters != null)
                                    {{
                                        Expression and = Expression.Constant(true);
                                        and = subFilters.Children()
                                            .OfType<JObject>()
                                            .Aggregate(
                                                and,
                                                (current, subFilter) =>
                                                    Expression.And(current, GenerateFilterExpressionPart(subFilter)));

                                        return and;
                                    }}

                                    return Expression.Constant(true);
                                }}
                            }},
                    {string.Join(",\n", filterable.SelectMany(d => this.GenerateConnectionFilterChecks(d.ApiName, d.ExpressionParameter, d.Property, d.ScalarType)))}
                }};

                private static Expression<Func<{className}, bool>> GenerateFilterExpression(JObject arguments)
                {{
                    var left = GenerateFilterExpressionPart(arguments);
                    return Expression.Lambda<Func<{className}, bool>>(left, filterableEntity);
                }}            

                private static Expression GenerateFilterExpressionPart(JObject filterProperty)
                {{
                    Expression left = Expression.Constant(true);

                    foreach (var prop in filterProperty.Properties())
                    {{
                        Func<JProperty, Expression> check;
                        if (FilterChecks.TryGetValue(prop.Name, out check))
                        {{
                            left = Expression.And(left, check(prop));
                        }}
                    }}

                    return left;
                }}
            ";
        }

        /// <summary>
        /// Generates connection individual field checks
        /// </summary>
        /// <param name="apiName">The field api name</param>
        /// <param name="expressionParameter">The expression parameter name, generated for this field</param>
        /// <param name="property"><see cref="PropertyInfo"/> for this field</param>
        /// <param name="scalarType">The field detected type</param>
        /// <returns>The list of filter checks</returns>
        protected virtual IEnumerable<string> GenerateConnectionFilterChecks(
            string apiName,
            string expressionParameter,
            PropertyInfo property,
            EnScalarType scalarType)
        {
            yield return $"{{ \"{apiName}\", prop => Expression.Equal({expressionParameter}, Expression.Constant(prop.Value.ToObject<{ToCSharpRepresentation(property.PropertyType)}>())) }}";
            yield return $"{{ \"{apiName}_not\", prop => Expression.NotEqual({expressionParameter}, Expression.Constant(prop.Value.ToObject<{ToCSharpRepresentation(property.PropertyType)}>())) }}";

            switch (scalarType)
            {
                case EnScalarType.Float:
                case EnScalarType.Decimal:
                case EnScalarType.Integer:
                    yield return $"{{ \"{apiName}_lt\", prop => Expression.LessThan({expressionParameter}, Expression.Constant(prop.Value.ToObject<{ToCSharpRepresentation(property.PropertyType)}>())) }}";
                    yield return $"{{ \"{apiName}_lte\", prop => Expression.LessThanOrEqual({expressionParameter}, Expression.Constant(prop.Value.ToObject<{ToCSharpRepresentation(property.PropertyType)}>())) }}";
                    yield return $"{{ \"{apiName}_gt\", prop => Expression.GreaterThan({expressionParameter}, Expression.Constant(prop.Value.ToObject<{ToCSharpRepresentation(property.PropertyType)}>())) }}";
                    yield return $"{{ \"{apiName}_gte\", prop => Expression.GreaterThanOrEqual({expressionParameter}, Expression.Constant(prop.Value.ToObject<{ToCSharpRepresentation(property.PropertyType)}>())) }}";
                    break;
                case EnScalarType.String:
                    yield return
                        $"{{ \"{apiName}_in\", prop => Expression.Invoke(filterStringCheckIn, Expression.Constant(prop.Value.ToObject<string>()), {expressionParameter}) }}";
                    yield return
                        $"{{ \"{apiName}_not_in\", prop => Expression.Not(Expression.Invoke(filterStringCheckIn, Expression.Constant(prop.Value.ToObject<string>()), {expressionParameter})) }}";
                    yield return
                        $"{{ \"{apiName}_contains\", prop => Expression.Invoke(filterStringCheckIn, {expressionParameter}, Expression.Constant(prop.Value.ToObject<string>())) }}";
                    yield return
                        $"{{ \"{apiName}_not_contains\", prop => Expression.Not(Expression.Invoke(filterStringCheckIn, {expressionParameter}, Expression.Constant(prop.Value.ToObject<string>()))) }}";
                    yield return
                        $"{{ \"{apiName}_starts_with\", prop => Expression.Invoke(filterStringStartsWith, {expressionParameter}, Expression.Constant(prop.Value.ToObject<string>())) }}";
                    yield return
                        $"{{ \"{apiName}_not_starts_with\", prop => Expression.Not(Expression.Invoke(filterStringStartsWith, {expressionParameter}, Expression.Constant(prop.Value.ToObject<string>()))) }}";
                    yield return
                        $"{{ \"{apiName}_ends_with\", prop => Expression.Invoke(filterStringEndsWith, {expressionParameter}, Expression.Constant(prop.Value.ToObject<string>())) }}";
                    yield return
                        $"{{ \"{apiName}_not_ends_with\", prop => Expression.Not(Expression.Invoke(filterStringEndsWith, {expressionParameter}, Expression.Constant(prop.Value.ToObject<string>()))) }}";
                    break;
            }
        }

        /// <summary>
        /// Generates method to create sort expression
        /// </summary>
        /// <returns>The properties and methods C# code</returns>
        protected virtual string GenerateConnectionSorter()
        {
            ApiType sortedApiType;
            if (!this.Data.ApiTypeByOriginalTypeNames.TryGetValue(this.typeMetadata.Type.FullName, out sortedApiType))
            {
                throw new InvalidOperationException($"The returned type of connection {this.typeMetadata.Type.FullName} is not registered");
            }

            var sortedApiObjectType = sortedApiType as ApiObjectType;
            if (sortedApiObjectType == null)
            {
                throw new InvalidOperationException($"The returned type of connection {this.typeMetadata.Type.FullName} is not an object type");
            }

            var sortableProperties = sortedApiObjectType.Fields
                .Where(f => f.Flags.HasFlag(EnFieldFlags.IsSortable))
                .Select(f => new { f.Name, Property = this.Data.Members[sortedApiType.TypeName][f.Name] as PropertyInfo })
                .Where(d => d.Property != null)
                .ToList();

            var sortingConditions = sortableProperties.Select(s => $@"
                {{""{s.Name}_asc"", new SortingCondition(""{s.Property.Name}"", SortingCondition.EnDirection.Asc)}},
                {{""{s.Name}_desc"", new SortingCondition(""{s.Property.Name}"", SortingCondition.EnDirection.Desc)}},
            ");

            return $@"
            private static Dictionary<string, SortingCondition> SortingConditions = new Dictionary<string, SortingCondition>()
            {{
                {string.Join(string.Empty, sortingConditions)}
            }};


            private static IEnumerable<SortingCondition> GenerateSortingExpression(JArray arguments)
            {{               
                var sortArgs = arguments.ToObject<string[]>();
                foreach (var sort in sortArgs)
                {{
                    SortingCondition condition;
                    if (SortingConditions.TryGetValue(sort, out condition))
                    {{
                        yield return condition;
                    }}                                      
                }}                
            }} 
            ";
        }
    }
}
