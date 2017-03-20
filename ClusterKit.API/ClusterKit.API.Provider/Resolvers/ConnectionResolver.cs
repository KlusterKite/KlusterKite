// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectionResolver.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Resolves requests to the connection
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Provider.Resolvers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    using ClusterKit.API.Client;
    using ClusterKit.API.Client.Attributes;
    using ClusterKit.API.Client.Attributes.Authorization;
    using ClusterKit.Security.Client;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Resolves requests to the connection
    /// </summary>
    public abstract class ConnectionResolver : IResolver, IConnectionResolver
    {
        /// <summary>
        /// Gets the resolver for the element node
        /// </summary>
        public abstract IResolver NodeResolver { get; }

        /// <inheritdoc />
        public abstract ApiType GetElementType();

        /// <inheritdoc />
        public abstract Task<object> GetNodeById(object nodeConnection, string id);

        /// <inheritdoc />
        public abstract Task<JObject> ResolveMutation(
            object nodeConnection,
            ApiRequest request,
            ApiField field,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback);

        /// <inheritdoc />
        public abstract Task<JToken> ResolveQuery(object source, ApiRequest request, ApiField apiField, RequestContext context, JsonSerializer argumentsSerializer, Action<Exception> onErrorCallback);
    }

    /// <summary>
    /// Resolves requests to the connection
    /// </summary>
    /// <typeparam name="T">The type of node</typeparam>
    /// <typeparam name="TId">The type of node id</typeparam>
    [SuppressMessage("ReSharper", "StaticMemberInGenericType",
        Justification = "Making use of static properties in generic classes")]

    // ReSharper disable once StyleCop.SA1402
    public class ConnectionResolver<T, TId> : ConnectionResolver
        where T : class, new()
    {
        /// <summary>
        /// The filter checks pre-created expressions
        /// </summary>
        private static readonly Dictionary<string, Func<JProperty, Expression>> FilterChecks =
            new Dictionary<string, Func<JProperty, Expression>>();

        /// <summary>
        /// The filter checks pre-created expressions
        /// </summary>
        private static readonly ParameterExpression FilterSourceParameter = Expression.Parameter(typeof(T));

        /// <summary>
        /// The initialization lock to make it thread-safe
        /// </summary>
        private static readonly object LockObject = new object();

        /// <summary>
        /// The node metadata
        /// </summary>
        private static readonly TypeMetadata NodeMetaData = TypeMetadata.GenerateTypeMetadata(
            typeof(T),
            new DeclareFieldAttribute());

        /// <summary>
        /// The available sorting conditions
        /// </summary>
        private static readonly Dictionary<string, SortingCondition> SortingConditions =
            new Dictionary<string, SortingCondition>();

        /// <summary>
        /// A value indicating whether the type initialization process was completed
        /// </summary>
        private static bool isInitialized;

        /// <summary>
        /// Gets or sets the mutation result resolver
        /// </summary>
        private static ObjectResolver mutationResultResolver;

        /// <inheritdoc />
        public ConnectionResolver()
        {
            InitializeType();
            if (NodeMetaData.ScalarType == EnScalarType.None)
            {
                this.NodeResolver = new ObjectResolver<T>();
            }
            else
            {
                this.NodeResolver = new ScalarResolver<T>();
            }
        }

        /// <inheritdoc />
        public override IResolver NodeResolver { get; }

        /// <inheritdoc />
        public override ApiType GetElementType()
        {
            return NodeMetaData.ScalarType == EnScalarType.None ? ObjectResolver<T>.GeneratedType : null;
        }

        /// <inheritdoc />
        public override async Task<object> GetNodeById(object nodeConnection, string id)
        {
            var connection = nodeConnection as INodeConnection<T, TId>;
            if (connection == null)
            {
                return null;
            }

            TId realId;
            try
            {
                realId = JsonConvert.DeserializeObject<TId>(id);
            }
            catch
            {
                return null;
            }

            return await connection.GetById(realId);
        }

        /// <inheritdoc />
        public override Task<JObject> ResolveMutation(
            object nodeConnection,
            ApiRequest request,
            ApiField field,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback)
        {
            var connection = nodeConnection as INodeConnection<T, TId>;
            if (connection == null)
            {
                return Task.FromResult<JObject>(null);
            }

            switch (request.FieldName)
            {
                case "create":
                    return this.MutationCreate(
                        connection,
                        request,
                        field,
                        context,
                        argumentsSerializer,
                        onErrorCallback);
                case "update":
                    return this.MutationUpdate(
                        connection,
                        request,
                        field,
                        context,
                        argumentsSerializer,
                        onErrorCallback);
                case "delete":
                    return this.MutationDelete(
                        connection,
                        request,
                        field,
                        context,
                        argumentsSerializer,
                        onErrorCallback);
                default:
                    return Task.FromResult<JObject>(null);
            }
        }

        /// <inheritdoc />
        public override async Task<JToken> ResolveQuery(
            object source,
            ApiRequest request,
            ApiField apiField,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback)
        {
            var connection = source as INodeConnection<T, TId>;
            if (connection == null)
            {
                onErrorCallback?.Invoke(new Exception("Source is not a node connection"));
                return JValue.CreateNull();
            }

            var arguments = (JObject)request.Arguments;
            var filterArgument = arguments?.Property("filter")?.Value as JObject;
            var sortArgument = arguments?.Property("sort")?.Value as JArray;
            var limit = (int?)(arguments?.Property("limit")?.Value as JValue);
            var offset = (int?)(arguments?.Property("offset")?.Value as JValue);

            var filter = filterArgument != null ? CreateFilter(filterArgument) : null;
            var sort = sortArgument != null ? this.CreateSort(sortArgument) : null;
            var items = await connection.Query(filter, sort, limit, offset, request);

            SetLog(request, apiField, context, EnConnectionAction.Query);

            if (items == null)
            {
                return JValue.CreateNull();
            }

            var result = new JObject();
            foreach (var requestField in request.Fields)
            {
                switch (requestField.FieldName)
                {
                    case "count":
                        result.Add(requestField.Alias ?? requestField.FieldName, new JValue(items.Count));
                        break;
                    case "items":
                        {
                            var itemsValue = await new CollectionResolver(this.NodeResolver).ResolveQuery(
                                                 items.Items,
                                                 requestField,
                                                 apiField,
                                                 context,
                                                 argumentsSerializer,
                                                 onErrorCallback);
                            result.Add(requestField.Alias ?? requestField.FieldName, itemsValue);
                        }

                        break;
                }
            }

            if (request.FieldName != null)
            {
                var requestDescription = new JObject { { "f", request.FieldName } };
                if (request.Arguments != null)
                {
                    var reservedNames = new[] { "filter", "sort", "limit", "offset" };
                    var requestArguments =
                        ((JObject)request.Arguments).Properties().Where(p => !reservedNames.Contains(p.Name)).ToList();
                    if (requestArguments.Count > 0)
                    {
                        var requestArgumentsObject = new JObject();
                        requestArguments.ForEach(ra => requestArgumentsObject.Add(ra.Name, ra.Value));
                        requestDescription.Add("a", requestArgumentsObject);
                    }
                }

                result.Add("__request", requestDescription);
            }

            return result;
        }

        /// <summary>
        /// Sets the operation log
        /// </summary>
        /// <param name="request">
        /// The initial request
        /// </param>
        /// <param name="apiField">
        /// The connection field
        /// </param>
        /// <param name="context">
        /// The request context
        /// </param>
        /// <param name="action">
        /// The action performed
        /// </param>
        private static void SetLog(ApiRequest request, ApiField apiField, RequestContext context, EnConnectionAction action)
        {
            if (apiField.LogAccessRules == null || !apiField.LogAccessRules.Any())
            {
                return;
            }

            var rule =
                apiField.LogAccessRules.OrderByDescending(r => r.Severity)
                    .FirstOrDefault(r => r.ConnectionActions.HasFlag(action));

            if (rule == null)
            {
                return;
            }

            var operationGranted = SecurityLog.EnType.OperationGranted;
            switch (action)
            {
                case EnConnectionAction.Create:
                    operationGranted = SecurityLog.EnType.DataCreateGranted;
                    break;
                case EnConnectionAction.Update:
                    operationGranted = SecurityLog.EnType.DataUpdateGranted;
                    break;
                case EnConnectionAction.Delete:
                    operationGranted = SecurityLog.EnType.DataDeleteGranted;
                    break;
            }

            SecurityLog.CreateRecord(
                operationGranted,
                rule.Severity,
                context,
                rule.LogMessage,
                ((JObject)request.Arguments).ToString(Formatting.None));
        }

        /// <summary>
        /// Performs final type initialization
        /// </summary>
        private static void InitializeType()
        {
            if (isInitialized)
            {
                return;
            }

            lock (LockObject)
            {
                if (isInitialized)
                {
                    return;
                }

                mutationResultResolver = new ObjectResolver<MutationResult<T>>();

                isInitialized = true;
                var nodeResolver = new ObjectResolver<T>();
                var nodeType = nodeResolver.GetApiType() as ApiObjectType;
                if (nodeType == null)
                {
                    return;
                }

                var realFieldNames = nodeResolver.GetFieldsNames().ToDictionary(n => n.Value, n => n.Key);
                var sortableFields = nodeType.Fields.Where(f => f.Flags.HasFlag(EnFieldFlags.IsSortable));
                foreach (var sortableField in sortableFields)
                {
                    SortingConditions[$"{sortableField.Name}_asc"] =
                        new SortingCondition(realFieldNames[sortableField.Name].Name, SortingCondition.EnDirection.Asc);
                    SortingConditions[$"{sortableField.Name}_desc"] =
                        new SortingCondition(realFieldNames[sortableField.Name].Name, SortingCondition.EnDirection.Desc);
                }

                FilterChecks["OR"] = prop =>
                    {
                        var subFilters = prop.Value as JArray;
                        if (subFilters == null)
                        {
                            return Expression.Constant(true);
                        }

                        Expression or = Expression.Constant(false);
                        or = subFilters.Children()
                            .OfType<JObject>()
                            .Aggregate(or, (current, subFilter) => Expression.Or(current, CreateFilterPart(subFilter)));

                        return or;
                    };

                FilterChecks["AND"] = prop =>
                    {
                        var subFilters = prop.Value as JArray;
                        if (subFilters == null)
                        {
                            return Expression.Constant(true);
                        }

                        Expression and = Expression.Constant(true);
                        and = subFilters.Children()
                            .OfType<JObject>()
                            .Aggregate(
                                and,
                                (current, subFilter) => Expression.And(current, CreateFilterPart(subFilter)));

                        return and;
                    };

                var filterableFields = nodeType.Fields.Where(f => f.Flags.HasFlag(EnFieldFlags.IsFilterable));

                var stringContains = typeof(string).GetMethod("Contains");
                var stringStartsWith = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
                var stringEndsWith = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
                var stringToLower = typeof(string).GetMethod("ToLower", new Type[0]);

                foreach (var filterableField in filterableFields)
                {
                    var property = realFieldNames[filterableField.Name] as PropertyInfo;
                    if (property == null)
                    {
                        continue;
                    }

                    var propertyExpression = Expression.Property(FilterSourceParameter, property);
                    
                    Func<JProperty, Expression> createConstant =
                        prop => Expression.Constant(prop.Value.ToObject(property.PropertyType), property.PropertyType);

                    FilterChecks[$"{filterableField.Name}"] =
                        prop => Expression.Equal(propertyExpression, createConstant(prop));

                    FilterChecks[$"{filterableField.Name}_not"] =
                        prop => Expression.NotEqual(propertyExpression, createConstant(prop));

                    switch (filterableField.ScalarType)
                    {
                        case EnScalarType.Float:
                        case EnScalarType.Decimal:
                        case EnScalarType.Integer:
                        case EnScalarType.DateTime:
                            FilterChecks[$"{filterableField.Name}_lt"] =
                                prop => Expression.LessThan(propertyExpression, createConstant(prop));
                            FilterChecks[$"{filterableField.Name}_lte"] =
                                prop => Expression.LessThanOrEqual(propertyExpression, createConstant(prop));
                            FilterChecks[$"{filterableField.Name}_gt"] =
                                prop => Expression.GreaterThan(propertyExpression, createConstant(prop));
                            FilterChecks[$"{filterableField.Name}_gte"] =
                                prop => Expression.GreaterThanOrEqual(propertyExpression, createConstant(prop));
                            break;
                        case EnScalarType.String:
                            var propertyToLower = Expression.Call(propertyExpression, stringToLower);
                            FilterChecks[$"{filterableField.Name}_in"] =
                                prop => Expression.Call(createConstant(prop), stringContains, propertyExpression);
                            FilterChecks[$"{filterableField.Name}_not_in"] =
                                prop =>
                                    Expression.Not(
                                        Expression.Call(createConstant(prop), stringContains, propertyExpression));
                            FilterChecks[$"{filterableField.Name}_contains"] =
                                prop => Expression.Call(propertyExpression, stringContains, createConstant(prop));
                            FilterChecks[$"{filterableField.Name}_not_contains"] =
                                prop =>
                                    Expression.Not(
                                        Expression.Call(propertyExpression, stringContains, createConstant(prop)));
                            FilterChecks[$"{filterableField.Name}_starts_with"] =
                                prop => Expression.Call(propertyExpression, stringStartsWith, createConstant(prop));
                            FilterChecks[$"{filterableField.Name}_not_starts_with"] =
                                prop =>
                                    Expression.Not(
                                        Expression.Call(propertyExpression, stringStartsWith, createConstant(prop)));
                            FilterChecks[$"{filterableField.Name}_ends_with"] =
                                prop => Expression.Call(propertyExpression, stringEndsWith, createConstant(prop));
                            FilterChecks[$"{filterableField.Name}_not_ends_with"] =
                                prop =>
                                    Expression.Not(
                                        Expression.Call(propertyExpression, stringEndsWith, createConstant(prop)));

                            FilterChecks[$"{filterableField.Name}_l"] =
                                prop => Expression.Equal(propertyToLower, createConstant(prop));

                            FilterChecks[$"{filterableField.Name}_l_not"] =
                                prop => Expression.NotEqual(propertyToLower, createConstant(prop));

                            FilterChecks[$"{filterableField.Name}_l_in"] =
                                prop => Expression.Call(createConstant(prop), stringContains, propertyToLower);
                            FilterChecks[$"{filterableField.Name}_l_not_in"] =
                                prop =>
                                    Expression.Not(
                                        Expression.Call(createConstant(prop), stringContains, propertyToLower));
                            FilterChecks[$"{filterableField.Name}_l_contains"] =
                                prop => Expression.Call(propertyToLower, stringContains, createConstant(prop));
                            FilterChecks[$"{filterableField.Name}_l_not_contains"] =
                                prop =>
                                    Expression.Not(
                                        Expression.Call(propertyToLower, stringContains, createConstant(prop)));
                            FilterChecks[$"{filterableField.Name}_l_starts_with"] =
                                prop => Expression.Call(propertyToLower, stringStartsWith, createConstant(prop));
                            FilterChecks[$"{filterableField.Name}_l_not_starts_with"] =
                                prop =>
                                    Expression.Not(
                                        Expression.Call(propertyToLower, stringStartsWith, createConstant(prop)));
                            FilterChecks[$"{filterableField.Name}_l_ends_with"] =
                                prop => Expression.Call(propertyToLower, stringEndsWith, createConstant(prop));
                            FilterChecks[$"{filterableField.Name}_l_not_ends_with"] =
                                prop =>
                                    Expression.Not(
                                        Expression.Call(propertyToLower, stringEndsWith, createConstant(prop)));
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Creates filter expression by filter request
        /// </summary>
        /// <param name="filter">The filter request</param>
        /// <returns>The filter expression</returns>
        private static Expression<Func<T, bool>> CreateFilter(JObject filter)
        {
            var left = CreateFilterPart(filter);
            return Expression.Lambda<Func<T, bool>>(left, FilterSourceParameter);
        }

        /// <summary>
        /// Creates part of of filter expression according to json description
        /// </summary>
        /// <param name="filterProperty">The filter json description</param>
        /// <returns>The expression</returns>
        private static Expression CreateFilterPart(JObject filterProperty)
        {
            Expression left = Expression.Constant(true);

            foreach (var prop in filterProperty.Properties())
            {
                Func<JProperty, Expression> check;
                if (FilterChecks.TryGetValue(prop.Name, out check))
                {
                    left = Expression.And(left, check(prop));
                }
            }

            return left;
        }

        /// <summary>
        /// Creates the list of sorting conditions by sorting request
        /// </summary>
        /// <param name="arguments">The sorting request</param>
        /// <returns>The list of sorting conditions</returns>
        private IEnumerable<SortingCondition> CreateSort(JArray arguments)
        {
            var sortArgs = arguments.ToObject<string[]>();
            foreach (var sort in sortArgs)
            {
                SortingCondition condition;
                if (SortingConditions.TryGetValue(sort, out condition))
                {
                    yield return condition;
                }
            }
        }

        /// <summary>
        /// Runs the creation mutation
        /// </summary>
        /// <param name="connection">The connection</param>
        /// <param name="request">The request</param>
        /// <param name="field">
        /// The connection field
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="argumentsSerializer">
        /// The arguments serializer.
        /// </param>
        /// <param name="onErrorCallback">
        /// The on error callback.
        /// </param>
        /// <returns>The resolved data</returns>
        private async Task<JObject> MutationCreate(
            INodeConnection<T, TId> connection,
            ApiRequest request,
            ApiField field,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback)
        {
            var serializedData = ((JObject)request.Arguments)?.Property("newNode")?.Value as JObject;
            var newNode = serializedData?.ToObject<T>();
            var result = await connection.Create(newNode);
            var mutationCreate =
                (JObject)
                await mutationResultResolver.ResolveQuery(
                    result,
                    request,
                    field,
                    context,
                    argumentsSerializer,
                    onErrorCallback);
            SetLog(request, field, context, EnConnectionAction.Create);
            return mutationCreate;
        }

        /// <summary>
        /// Runs the creation mutation
        /// </summary>
        /// <param name="connection">The connection</param>
        /// <param name="request">The request</param>
        /// <param name="field">
        /// The connection field
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="argumentsSerializer">
        /// The arguments serializer.
        /// </param>
        /// <param name="onErrorCallback">
        /// The on error callback.
        /// </param>
        /// <returns>The resolved data</returns>
        private async Task<JObject> MutationUpdate(
            INodeConnection<T, TId> connection,
            ApiRequest request,
            ApiField field,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback)
        {
            var serializedData = ((JObject)request.Arguments)?.Property("newNode")?.Value as JObject;
            var serializedId = ((JObject)request.Arguments)?.Property("id")?.Value;
            var id = serializedId != null ? serializedId.ToObject<TId>() : default(TId);
            var newNode = serializedData?.ToObject<T>();
            var result = await connection.Update(id, newNode, request);
            var mutationUpdate =
                (JObject)
                await mutationResultResolver.ResolveQuery(
                    result,
                    request, 
                    field,
                    context,
                    argumentsSerializer,
                    onErrorCallback);
            if (result.Result != null)
            {
                if (id != null && !id.Equals(connection.GetId(result.Result)))
                {
                    mutationUpdate.Add("__deletedId", JsonConvert.SerializeObject(id));
                }
            }

            SetLog(request, field, context, EnConnectionAction.Update);
            return mutationUpdate;
        }

        /// <summary>
        /// Runs the creation mutation
        /// </summary>
        /// <param name="connection">The connection</param>
        /// <param name="request">The request</param>
        /// <param name="field">
        /// The connection field
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="argumentsSerializer">
        /// The arguments serializer.
        /// </param>
        /// <param name="onErrorCallback">
        /// The on error callback.
        /// </param>
        /// <returns>The resolved data</returns>
        private async Task<JObject> MutationDelete(
            INodeConnection<T, TId> connection,
            ApiRequest request,
            ApiField field,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback)
        {
            var serializedId = ((JObject)request.Arguments)?.Property("id")?.Value;
            var id = serializedId != null ? serializedId.ToObject<TId>() : default(TId);
            var result = await connection.Delete(id);
            var mutationDelete =
                (JObject)
                await mutationResultResolver.ResolveQuery(
                    result,
                    request, 
                    field,
                    context,
                    argumentsSerializer,
                    onErrorCallback);
            if (result.Result != null)
            {
                mutationDelete.Add("__deletedId", JsonConvert.SerializeObject(id));
            }

            SetLog(request, field, context, EnConnectionAction.Delete);
            return mutationDelete;
        }
    }
}