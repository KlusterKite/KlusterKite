// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResolverGenerator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Helper, that generates C# code for the resolvers
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Provider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using ClusterKit.API.Client;
    using ClusterKit.API.Client.Attributes;
    using ClusterKit.API.Provider.Resolvers;
    using ClusterKit.Security.Client;

    /// <summary>
    /// Helper, that generates C# code for the resolvers
    /// </summary>
    /// <remarks>
    /// Resolver gets the type field value for the request. So for each published property in each published type we should have personal resolver
    /// </remarks>
    internal class ResolverGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResolverGenerator"/> class.
        /// </summary>
        /// <param name="member">
        /// The member.
        /// </param>
        /// <param name="metadata">
        /// The return type metadata.
        /// </param>
        /// <param name="sourceType">
        /// The source type.
        /// </param>
        /// <param name="data">
        /// The data.
        /// </param>
        public ResolverGenerator(MemberInfo member, TypeMetadata metadata, Type sourceType, AssembleTempData data)
        {
            this.Member = member;
            this.Metadata = metadata;
            this.SourceType = sourceType;
            this.Data = data;
        }

        /// <summary>
        /// Gets the generated class name
        /// </summary>
        public string ClassName => $"ClusterKit.API.Provider.Dynamic.Resolver{this.Uid:N}";

        /// <summary>
        /// Gets a value indicating whether <see cref="PropertyResolver.GetValue"/> method should be declared as async
        /// </summary>
        protected virtual bool GetValueIsAsync => this.Metadata.IsAsync;

        /// <summary>
        /// Gets the <see cref="MemberInfo"/> representing field
        /// </summary>
        protected MemberInfo Member { get; }

        /// <summary>
        /// Gets the return type metadata
        /// </summary>
        protected TypeMetadata Metadata { get; }

        /// <summary>
        /// Gets the source data type
        /// </summary>
        protected Type SourceType { get; }

        /// <summary>
        /// Gets the assemble data
        /// </summary>
        protected AssembleTempData Data { get; }

        /// <summary>
        /// Gets the resolver uid
        /// </summary>
        protected Guid Uid { get; } = Guid.NewGuid();

        /// <summary>
        /// Creates c# code for defined parameters
        /// </summary>
        /// <returns>The field resolver definition in C#</returns>
        public virtual string Generate()
        {
            bool isAsync;
            string async;
            string returnNull;

            if (this.Metadata.IsAsync || this.Metadata.ScalarType == EnScalarType.None)
            {
                isAsync = true;
                async = "async";
                returnNull = "null";
            }
            else
            {
                isAsync = false;
                async = string.Empty;
                returnNull = "Task.FromResult<JToken>(null)";
            }

            var checkQuery = this.Metadata.ScalarType == EnScalarType.None 
                ? $@"
                    if (query == null || query.Fields == null) 
                    {{
                        return {returnNull};
                    }}"
                : string.Empty;

            var code = $@"
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

                        // {this.GetType().Name} for {ToCSharpRepresentation(this.SourceType, true)} property {this.Member.Name}
                        public class Resolver{this.Uid:N} : PropertyResolver 
                        {{
                            public override {(this.GetValueIsAsync ? "async" : string.Empty)} Task<object> GetValue(object source, ApiRequest query, RequestContext context, JsonSerializer argumentsSerializer) 
                            {{
                                var arguments = (JObject)query.Arguments;
                                {this.GenerateResultSourceAcquirement()}
                                return {(this.GetValueIsAsync ? "resultSource" : "Task.FromResult<object>(resultSource)")};
                            }}

                            public override {async} Task<JToken> Resolve(object source, ApiRequest query, RequestContext context, JsonSerializer argumentsSerializer, Action<Exception> onErrorCallback) 
                            {{
                                var arguments = (JObject)query.Arguments;
                                try 
                                {{
                                    var resultSource = ({ToCSharpRepresentation(this.GetPropertyReturnType(), true)}){(isAsync ? " await this.GetValue(source,query,context,argumentsSerializer)" : "this.GetValue(source,query,context,argumentsSerializer).Result")};
                                    {checkQuery}
                                    if (resultSource == null)
                                    {{
                                         return {(isAsync ? "null" : "Task.FromResult<JToken>(null)")};
                                    }}

                                    {(this.Metadata.IsForwarding ? this.GenerateForwardedReturn() : this.GenerateRecursiveResolve())}
                                }}
                                catch(Exception e)
                                {{
                                    if (onErrorCallback != null) 
                                    {{
                                        onErrorCallback(e);
                                    }}

                                    return {returnNull};
                                }}
                            }}

                            {this.GenerateHelperMethods()}
                        }}
                    }}
                ";

            return code;
        }

        /// <summary>
        /// Converts string to camel case
        /// </summary>
        /// <param name="name">The property / method name</param>
        /// <returns>The name in camel case</returns>
        internal static string ToCamelCase(string name)
        {
            name = name?.Trim();

            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            var array = name.ToCharArray();
            array[0] = array[0].ToString().ToLowerInvariant().ToCharArray().First();
            return new string(array);
        }

        /// <summary>
        /// Creates valid type name representation
        /// </summary>
        /// <remarks>
        /// Original code: <see href="http://stackoverflow.com/questions/2579734/get-the-type-name"/>
        /// </remarks>
        /// <param name="type">The type</param>
        /// <param name="trimArgCount">A value indicating whether to trim arguments count</param>
        /// <returns>A valid C# name</returns>
        internal static string ToCSharpRepresentation(Type type, bool trimArgCount)
        {
            if (type.IsGenericType)
            {
                var genericArgs = type.GetGenericArguments().ToList();
                return ToCSharpRepresentation(type, trimArgCount, genericArgs);
            }

            return type.FullName.Replace("+", ".");
        }

        /// <summary>
        /// Generates helper properties and methods for generator
        /// </summary>
        /// <returns>The properties and methods C# code</returns>
        protected virtual string GenerateHelperMethods()
        {
            if (this.Metadata.MetaType == TypeMetadata.EnMetaType.Connection)
            {
                return this.GenerateConnectionHelpers();
            }

            return string.Empty;
        }

        /// <summary>
        /// Generates helpers to work with connection
        /// </summary>
        /// <returns>The properties and methods C# code</returns>
        protected virtual string GenerateConnectionHelpers()
        {
            return $@"
            {this.GenerateConnectionSorter()}
            {this.GenerateConnectionFilter()}
            ";
        }

        /// <summary>
        /// Generates method to create sort expression
        /// </summary>
        /// <returns>The properties and methods C# code</returns>
        protected virtual string GenerateConnectionSorter()
        {
            ApiObjectType sortedApiType;
            if (!this.Data.ApiTypeByOriginalTypeNames.TryGetValue(this.Metadata.Type.FullName, out sortedApiType))
            {
                throw new InvalidOperationException($"The returned type of connection {this.Metadata.Type.FullName} is not registered");
            }
            
            var sortableProperties = sortedApiType.Fields
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


            private static IEnumerable<SortingCondition> GenerateSortingExpression(JObject arguments)
            {{                
                var sortProperty = arguments.Property(""sort"");
                if (sortProperty == null || !sortProperty.Value.HasValues)
                {{
                    yield break;
                }}

                var sortArgs = sortProperty.Value.ToObject<string[]>();
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

        /// <summary>
        /// Generates method to create filter expression
        /// </summary>
        /// <returns>The properties and methods C# code</returns>
        protected virtual string GenerateConnectionFilter()
        {
            ApiObjectType sortedApiType;
            if (!this.Data.ApiTypeByOriginalTypeNames.TryGetValue(this.Metadata.Type.FullName, out sortedApiType))
            {
                throw new InvalidOperationException($"The returned type of connection {this.Metadata.Type.FullName} is not registered");
            }

            var className = ToCSharpRepresentation(this.Metadata.Type, true);
            var filterable = sortedApiType.Fields
                .Where(f => f.Flags.HasFlag(EnFieldFlags.IsFilterable))
                .Select(f => new { f.Name, Property = this.Data.Members[sortedApiType.TypeName][f.Name] as PropertyInfo, f.ScalarType })
                .Where(d => d.Property != null && d.ScalarType != EnScalarType.None)
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
                    Expression<Func<{className}, bool>> filter = null;
                    var jproperty = arguments.Property(""filter"");
                    if (jproperty == null)
                    {{
                        return null;
                    }}

                    var filterProperty = jproperty.Value as JObject;
                    if (filterProperty == null)
                    {{
                        return null;
                    }}

                    var left = GenerateFilterExpressionPart(filterProperty);
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
            yield return $"{{ \"{apiName}\", prop => Expression.Equal({expressionParameter}, Expression.Constant(prop.Value.ToObject<{ToCSharpRepresentation(property.PropertyType, true)}>())) }}";
            yield return $"{{ \"{apiName}_not\", prop => Expression.NotEqual({expressionParameter}, Expression.Constant(prop.Value.ToObject<{ToCSharpRepresentation(property.PropertyType, true)}>())) }}";

            switch (scalarType)
            {
                case EnScalarType.Float:
                case EnScalarType.Decimal:
                case EnScalarType.Integer:
                    yield return $"{{ \"{apiName}_lt\", prop => Expression.LessThan({expressionParameter}, Expression.Constant(prop.Value.ToObject<{ToCSharpRepresentation(property.PropertyType, true)}>())) }}";
                    yield return $"{{ \"{apiName}_lte\", prop => Expression.LessThanOrEqual({expressionParameter}, Expression.Constant(prop.Value.ToObject<{ToCSharpRepresentation(property.PropertyType, true)}>())) }}";
                    yield return $"{{ \"{apiName}_gt\", prop => Expression.GreaterThan({expressionParameter}, Expression.Constant(prop.Value.ToObject<{ToCSharpRepresentation(property.PropertyType, true)}>())) }}";
                    yield return $"{{ \"{apiName}_gte\", prop => Expression.GreaterThanOrEqual({expressionParameter}, Expression.Constant(prop.Value.ToObject<{ToCSharpRepresentation(property.PropertyType, true)}>())) }}";
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
        /// Generates the execution of property / method of source value
        /// </summary>
        /// <returns>Code to acquire the source value</returns>
        protected virtual string GenerateResultSourceAcquirement()
        {
            var init = $@"
                        var sourceTyped = source as {ToCSharpRepresentation(this.SourceType, true)};
                        if (sourceTyped == null) 
                        {{
                            throw new Exception(""Source object of unexpected type"");
                        }}
                       ";

            var property = this.Member as PropertyInfo;
            if (property != null)
            {
                return $"{init}{this.GenerateResultSourceFromPropertyAcquirement(property)}";
            }

            var method = this.Member as MethodInfo;
            if (method != null)
            {
                return $"{init}{this.GenerateResultSourceFromMethodAcquirement(method)}";
            }

            throw new InvalidOperationException($"Member is of the invalid type {this.Member.GetType().Name}");
        }

        /// <summary>
        /// Gets the property return type
        /// </summary>
        /// <returns>The type</returns>
        protected virtual Type GetPropertyReturnType()
        {
            var property = this.Member as PropertyInfo;
            if (property != null)
            {
                return this.Metadata.IsAsync ? TypeMetadata.CheckType(property.PropertyType, typeof(Task<>)).GenericTypeArguments[0] : property.PropertyType;
            }

            var method = this.Member as MethodInfo;
            if (method != null)
            {
                return this.Metadata.IsAsync ? TypeMetadata.CheckType(method.ReturnType, typeof(Task<>)).GenericTypeArguments[0] : method.ReturnType;
            }

            throw new InvalidOperationException($"Member is of the invalid type {this.Member.GetType().Name}");
        }

        /// <summary>
        /// Generates the acquiring of method value
        /// </summary>
        /// <param name="method">The method</param>
        /// <returns>Code to acquire the source value</returns>
        protected virtual string GenerateResultSourceFromMethodAcquirement(MethodInfo method)
        {
            var await = this.Metadata.IsAsync ? "await" : string.Empty;
            List<string> codeCommands = new List<string>();

            var parameterIndex = 0;
            foreach (var parameter in method.GetParameters())
            {
                var parameterDescription =
                    parameter.GetCustomAttribute(typeof(ApiDescriptionAttribute)) as ApiDescriptionAttribute;
                var parameterName = parameterDescription?.Name ?? ToCamelCase(parameter.Name);

                string command;
                if (parameter.ParameterType == typeof(RequestContext))
                {
                    command = $"var arg{parameterIndex} = context;";
                }
                else if (parameter.ParameterType == typeof(ApiRequest))
                {
                    command = $"var arg{parameterIndex} = query;";
                }
                else
                {
                    command = $@"
                    var prop{parameterIndex} = arguments != null ? arguments.Property(""{ parameterName}"") : null;
                    var arg{parameterIndex} = prop{parameterIndex} != null && prop{parameterIndex}.Value != null
                            ? prop{parameterIndex}.Value.ToObject<{ToCSharpRepresentation(parameter.ParameterType, true)}>(argumentsSerializer)
                            : default({ToCSharpRepresentation(parameter.ParameterType, true)});
                    ";
                }

                codeCommands.Add(command);
                parameterIndex++;
            }

            return $@"
                {string.Join("\r\n", codeCommands)}
                var resultSource = {@await} sourceTyped.{method.Name}({string.Join(
                ", ",
                Enumerable.Range(0, method.GetParameters().Length).Select(n => $"arg{n}"))});
            ";
        }

        /// <summary>
        /// Generates the acquiring of property value
        /// </summary>
        /// <param name="property">The property</param>
        /// <returns>Code to acquire the source value</returns>
        protected virtual string GenerateResultSourceFromPropertyAcquirement(PropertyInfo property)
        {
            var await = this.Metadata.IsAsync ? "await" : string.Empty;
            return $"var resultSource = {@await} sourceTyped.{property.Name};";
        }

        /// <summary>
        /// Generates resolve of the forwarded request
        /// </summary>
        /// <returns>Code to return result</returns>
        protected virtual string GenerateForwardedReturn()
        {
            return this.Metadata.IsAsync ? "return resultSource;" : "return Task.FromResult<JToken>(resultSource);";
        }

        /// <summary>
        /// Generates resolve of the request value recursively
        /// </summary>
        /// <returns>Code to return result</returns>
        protected virtual string GenerateRecursiveResolve()
        {
            var isSync = !this.Metadata.IsAsync && this.Metadata.ScalarType != EnScalarType.None;

            if (this.Metadata.MetaType == TypeMetadata.EnMetaType.Scalar)
            {
                return isSync ? "return Task.FromResult<JToken>(new JValue(resultSource));" : "return new JValue(resultSource);";
            }

            if (this.Metadata.MetaType == TypeMetadata.EnMetaType.Object)
            {
                return $@"
                    {this.GenerateObjectResolve("resultSource", this.Metadata.Type)}
                    return result;
                ";
            }

            if (this.Metadata.MetaType == TypeMetadata.EnMetaType.Array)
            {
                return $@"
                    var resultArray = new JArray();
                    foreach (var item in resultSource)
                    {{
                        {(this.Metadata.ScalarType == EnScalarType.None ? this.GenerateObjectResolve("item", this.Metadata.Type) : "var result = new JValue(item);")}
                        resultArray.Add(result);
                    }}
                    return {(isSync? "Task.FromResult<JToken>(resultArray)" : "resultArray")};
                ";
            }

            if (this.Metadata.MetaType == TypeMetadata.EnMetaType.Connection)
            {
                return this.GenerateConnectionResolve();
            }

            return "return null;";
        }

        /// <summary>
        /// Generates resolve of the request connection value
        /// </summary>
        /// <returns>Code to return result</returns>
        protected virtual string GenerateConnectionResolve()
        {
            return $@"
                int? limit = null;
                int? offset = null;

                var limitArgument = arguments.Property(""limit"");
                var offsetArgument = arguments.Property(""offset"");

                if (limitArgument != null && limitArgument.HasValues && limitArgument.Value != null && limitArgument.Value.Type != JTokenType.Null)
                {{
                    limit = limitArgument.Value.ToObject<int>();
                }}

                if (offsetArgument != null && offsetArgument.HasValues && offsetArgument.Value != null && offsetArgument.Value.Type != JTokenType.Null)
                {{
                    offset = offsetArgument.Value.ToObject<int>();
                }}

                var connectionResult = await resultSource.Query(GenerateFilterExpression(arguments), GenerateSortingExpression(arguments), limit, offset);
                var connectionResultJson = new JObject();
                connectionResultJson.Add(""count"",  connectionResult.Count);
                var resultArray = new JArray();
        
                var itemsQuery = query.Fields.FirstOrDefault(f => f.FieldName == ""items"");
                if (itemsQuery != null) 
                {{
                    foreach (var item in connectionResult.Items)
                    {{
                        {(this.Metadata.ScalarType == EnScalarType.None ? this.GenerateObjectResolve("item", this.Metadata.Type, "itemsQuery") : "var result = new JValue(item);")}
                        resultArray.Add(result);
                    }} 
                }}  
                connectionResultJson.Add(""items"",  resultArray);                            
                return connectionResultJson;
            ";
        }

        /// <summary>
        /// Generates resolve of the object value
        /// </summary>
        /// <param name="itemName">The item variable name</param>
        /// <param name="type">The item type</param>
        /// <param name="queryVariableName">The variable name with query stored</param>
        /// <returns>Code to return result</returns>
        protected virtual string GenerateObjectResolve(string itemName, Type type, string queryVariableName = "query")
        {
            var prefix = @"
                var result = new JObject();
                ApiRequest fieldQuery;
                PropertyResolver resolver;                
            ";

            List<string> codeCommands = new List<string> { prefix };

            ApiObjectType apiType;
            if (!this.Data.ApiTypeByOriginalTypeNames.TryGetValue(type.FullName, out apiType))
            {
                throw new InvalidOperationException($"Type {type.FullName} was not described as API type");
            }

            foreach (var apiField in apiType.Fields)
            {
                var command = $@"
                    fieldQuery = {queryVariableName}.Fields.FirstOrDefault(f => f.FieldName == ""{apiField.Name}"");
                    if (fieldQuery != null)
                    {{
                        resolver = new {this.Data.ResolverNames[apiType.TypeName][apiField.Name]}();
                        result.Add(""{apiField.Name}"", await resolver.Resolve({itemName}, fieldQuery, context, argumentsSerializer, onErrorCallback));
                    }}
                ";

                codeCommands.Add(command);
            }

            return string.Join("\r\n", codeCommands);
        }

        /// <summary>
        /// Creates valid type name representation
        /// </summary>
        /// <remarks>
        /// Original code: <see href="http://stackoverflow.com/questions/2579734/get-the-type-name"/>
        /// </remarks>
        /// <param name="type">The type</param>
        /// <param name="trimArgCount">A value indicating whether to trim arguments count</param>
        /// <param name="availableArguments">The list of type parameters for generic classes</param>
        /// <returns>A valid C# name</returns>
        private static string ToCSharpRepresentation(Type type, bool trimArgCount, List<Type> availableArguments)
        {
            if (!type.IsGenericType)
            {
                return type.FullName.Replace("+", ".");
            }

            var value = type.FullName.Replace("+", ".");
            if (trimArgCount && value.IndexOf("`", StringComparison.InvariantCulture) > -1)
            {
                value = value.Substring(0, value.IndexOf("`", StringComparison.InvariantCulture));
            }

            if (type.DeclaringType != null)
            {
                // This is a nested type, build the nesting type first
                value = ToCSharpRepresentation(type.DeclaringType, trimArgCount, availableArguments) + "+" + value;
            }

            // Build the type arguments (if any)
            var argString = string.Empty;
            var thisTypeArgs = type.GetGenericArguments();
            for (var i = 0; i < thisTypeArgs.Length && availableArguments.Count > 0; i++)
            {
                if (i != 0)
                {
                    argString += ", ";
                }

                argString += ToCSharpRepresentation(availableArguments[0], trimArgCount);
                availableArguments.RemoveAt(0);
            }

            // If there are type arguments, add them with < >
            if (argString.Length > 0)
            {
                value += "<" + argString + ">";
            }

            return value;
        }
    }
}