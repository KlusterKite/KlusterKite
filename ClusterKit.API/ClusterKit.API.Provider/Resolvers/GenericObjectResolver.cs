// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenericObjectResolver.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Generic class to resolve requests to objects
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

    using Castle.Core.Internal;

    using ClusterKit.API.Client;
    using ClusterKit.API.Client.Attributes;
    using ClusterKit.Security.Client;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The base class for <see cref="GenericObjectResolver{T}"/> just to make untyped methods
    /// </summary>
    public abstract class GenericObjectResolver : IResolver
    {
        /// <summary>
        /// Gets the defined <see cref="ApiType"/>
        /// </summary>
        /// <returns>The api type</returns>
        public abstract ApiType GetApiType();

        /// <summary>
        /// Creates an api root data from current type
        /// </summary>
        /// <param name="types">The list of all types used in api</param>
        /// <param name="argumentsSerializer">The configured JSON serializer to deserialize method arguments</param>
        /// <param name="mutationList">The list of all defined mutations</param>
        public abstract void CreateApiRoot(
            out List<ApiType> types,
            out JsonSerializer argumentsSerializer,
            out List<ApiMutation> mutationList);

        /// <inheritdoc />
        public abstract Task<JToken> ResolveQuery(
            object source,
            ApiRequest request,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback);

        /// <inheritdoc />
        ApiType IResolver.GetElementType() => this.GetApiType();

        /// <summary>
        /// Gets the list <see cref="GenericObjectResolver{T}"/> that a used to resolve the fields of current object
        /// </summary>
        /// <returns>The list of resolvers</returns>
        internal abstract IEnumerable<GenericObjectResolver> GetDirectRelatedObjectResolvers();

        /// <summary>
        /// Gets the names of type fields
        /// </summary>
        /// <returns>The names of type fields</returns>
        internal abstract Dictionary<MemberInfo, string> GetFieldsNames();

        /// <summary>
        /// The related type description
        /// </summary>
        protected class RelatedType
        {
            /// <summary>
            /// Gets or sets the type
            /// </summary>
            public Type Type { get; set; }
            
            /// <summary>
            /// Gets or sets the type resolver
            /// </summary>
            public IResolver Resolver { get; set; }
        }
    }

    /// <summary>
    /// Generic class to resolve requests to objects 
    /// </summary>
    /// <typeparam name="T">
    /// The type of object to resolve
    /// </typeparam>
    [SuppressMessage("ReSharper", "StaticMemberInGenericType",
        Justification = "Making use of static properties in generic classes")]
    // ReSharper disable once StyleCop.SA1402
    public class GenericObjectResolver<T> : GenericObjectResolver
    {
        /// <summary>
        /// The list of field descriptions
        /// </summary>
        private static readonly Dictionary<string, FieldDescription> Fields = new Dictionary<string, FieldDescription>();

        /// <summary>
        /// The list of generation errors
        /// </summary>
        private static readonly List<string> GenerationErrors = new List<string>();

        /// <summary>
        /// The initialization lock to make it thread-safe
        /// </summary>
        private static readonly object LockObject = new object();

        /// <summary>
        /// The list of related types
        /// </summary>
        private static readonly List<RelatedType> RelatedTypes = new List<RelatedType>();

        /// <summary>
        /// A value indicating whether the type initialization process was completed
        /// </summary>
        private static bool isInitialized;

        /// <summary>
        /// Initializes static members of the <see cref="GenericObjectResolver{T}"/> class.
        /// </summary>
        static GenericObjectResolver()
        {
            var type = typeof(T);

            var typeName = ApiDescriptionAttribute.GetTypeName(type);

            if (type.IsSubclassOf(typeof(Enum)))
            {
                GeneratedType = new ApiEnumType(typeName, Enum.GetNames(type));
                return;
            }

            var apiObjectType = new ApiObjectType(typeName);
            apiObjectType.Fields.AddRange(GenerateTypeProperties());
            apiObjectType.Fields.AddRange(GenerateTypeMethods());
            GeneratedType = apiObjectType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericObjectResolver{T}"/> class.
        /// </summary>
        public GenericObjectResolver()
        {
            InitializeType();
        }

        /// <summary>
        /// Gets a value of objects property according to API request
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="request">
        /// The api request.
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
        /// <returns>The value</returns>
        private delegate Task<object> PropertyValueGetter(
            T source,
            ApiRequest request,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback);

        /// <summary>
        /// Gets the generated api type for <see cref="T"/>
        /// </summary>
        public static ApiType GeneratedType { get; }

        /// <summary>
        /// Gets the list of errors occurred on generation stage
        /// </summary>
        public static List<string> GetGenerationErrors => new List<string>(GenerationErrors);

        /// <inheritdoc />
        public override void CreateApiRoot(
            out List<ApiType> types,
            out JsonSerializer argumentsSerializer,
            out List<ApiMutation> mutationList)
        {
            var directTypes = this.GetDirectRelatedObjectResolvers().ToList();
            directTypes.Add(this);
            
            types = new List<ApiType>();
            var resolveQueue = new Queue<GenericObjectResolver>(directTypes);
            var resolvedTypes = new Dictionary<string, GenericObjectResolver>();
            var memberNames = new Dictionary<MemberInfo, string>();

            while (resolveQueue.Count > 0)
            {
                var resolver = resolveQueue.Dequeue();
                var apiType = resolver.GetApiType();
                if (resolvedTypes.ContainsKey(apiType.TypeName))
                {
                    continue;
                }

                resolvedTypes[apiType.TypeName] = resolver;
                types.Add(apiType);
                foreach (var pair in resolver.GetFieldsNames())
                {
                    memberNames[pair.Key] = pair.Value;
                }

                foreach (var objectResolver in resolver.GetDirectRelatedObjectResolvers())
                {
                    resolveQueue.Enqueue(objectResolver);
                }
            }

            var jsonContractResolver = new InputContractResolver(memberNames);
            argumentsSerializer = new JsonSerializer { ContractResolver = jsonContractResolver };
            mutationList = new List<ApiMutation>();
        }

        /// <inheritdoc />
        public override ApiType GetApiType() => GeneratedType;

        /// <summary>
        /// Resolves API request to object
        /// </summary>
        /// <param name="sourceUntyped">
        /// The source.
        /// </param>
        /// <param name="request">
        /// The request to this object as a field of parent object.
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
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public override async Task<JToken> ResolveQuery(
            object sourceUntyped,
            ApiRequest request,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback)
        {
            var source = (T)sourceUntyped;
            if (request?.Fields == null || request.Fields.Count == 0)
            {
                return null;
            }

            var result = new JObject();
            var fields = request.Fields.GroupBy(f => f.Alias ?? f.FieldName).Select(
                g =>
                    {
                        var f = g.First();
                        if (f.Fields == null)
                        {
                            return f;
                        }

                        return new ApiRequest
                                   {
                                       Alias = f.Alias,
                                       Arguments = f.Arguments,
                                       FieldName = f.FieldName,
                                       Fields = g.SelectMany(sr => sr.Fields).ToList()
                                   };
                    });

            foreach (var fieldRequest in fields)
            {
                var fieldName = fieldRequest.Alias ?? fieldRequest.FieldName;
                FieldDescription field;
                if (!Fields.TryGetValue(fieldRequest.FieldName, out field))
                {
                    onErrorCallback?.Invoke(new Exception($"Unknown field {fieldRequest.FieldName}"));
                    continue;
                }

                try
                {
                    var propertyValue = await field.GetValue(
                                            source,
                                            fieldRequest,
                                            context,
                                            argumentsSerializer,
                                            onErrorCallback);

                    var resolvedProperty = propertyValue == null
                                               ? JValue.CreateNull()
                                               : await field.Resolver.ResolveQuery(
                                                     propertyValue,
                                                     fieldRequest,
                                                     context,
                                                     argumentsSerializer,
                                                     onErrorCallback);
                    result.Add(fieldName, resolvedProperty);
                }
                catch (Exception exception)
                {
                    onErrorCallback?.Invoke(exception);
                    result.Add(fieldName, JValue.CreateNull());
                }
            }

            if (request.FieldName != null)
            {
                var requestDescription = new JObject { { "f", request.FieldName } };
                if (request.Arguments?.Json != null && request.Arguments.Json.HasValues)
                {
                    requestDescription.Add("a", request.Arguments);
                }

                result.Add("__request", requestDescription);
            }

            return result;
        }

        /// <summary>
        /// Gets the list <see cref="GenericObjectResolver{T}"/> that a used to resolve the fields of current object
        /// </summary>
        /// <returns>The list of resolvers</returns>
        internal override IEnumerable<GenericObjectResolver> GetDirectRelatedObjectResolvers()
        {
            return RelatedTypes.Select(t => t.Resolver).OfType<GenericObjectResolver>();
        }

        /// <summary>
        /// Gets the names of type fields
        /// </summary>
        /// <returns>The names of type fields</returns>
        internal override Dictionary<MemberInfo, string> GetFieldsNames()
        {
            return Fields.ToDictionary(f => f.Value.TypeMember, f => f.Key);
        }

        /// <summary>
        /// Creates a resolver for the specified type
        /// </summary>
        /// <param name="metadata">The type metadata</param>
        /// <returns>The resolver</returns>
        private static IResolver CreateResolver(TypeMetadata metadata)
        {
            var elementResolver = metadata.ScalarType == EnScalarType.None
                                      ? typeof(GenericObjectResolver<>).MakeGenericType(metadata.Type)
                                          .CreateInstance<IResolver>()
                                      : typeof(ScalarResolver<>).MakeGenericType(metadata.Type)
                                          .CreateInstance<IResolver>();

            if (metadata.IsForwarding)
            {
                return new ForwarderResolver(elementResolver.GetElementType());
            }

            if (metadata.GetFlags().HasFlag(EnFieldFlags.IsConnection))
            {
                var connectionResolverType = typeof(GenericConnectionResolver<,>).MakeGenericType(
                    metadata.Type,
                    metadata.TypeOfId);
                return connectionResolverType.CreateInstance<IResolver>();
            }

            return metadata.GetFlags().HasFlag(EnFieldFlags.IsArray)
                       ? new CollectionResolver(elementResolver)
                       : elementResolver;
        }

        /// <summary>
        /// Generates api field from type method
        /// </summary>
        /// <param name="method">
        /// The method to process
        /// </param>
        /// <param name="attribute">
        /// The method description attribute
        /// </param>
        /// <returns>
        /// The field description
        /// </returns>
        private static ApiField GenerateFieldFromMethod(MethodInfo method, PublishToApiAttribute attribute)
        {
            var type = method.ReflectedType;
            TypeMetadata metadata;
            try
            {
                metadata = TypeMetadata.GenerateTypeMetadata(method.ReturnType, attribute);
            }
            catch (Exception exception)
            {
                GenerationErrors.Add(
                    $"Error while parsing return type of method {method.Name} of type {type?.FullName}: {exception.Message}");
                return null;
            }

            var name = attribute.GetName(method);

            ApiField field;
            if (metadata.ScalarType != EnScalarType.None)
            {
                field = ApiField.Scalar(
                    name,
                    metadata.ScalarType,
                    metadata.GetFlags(),
                    description: attribute.Description);
            }
            else
            {
                field = ApiField.Object(
                    name,
                    ApiDescriptionAttribute.GetTypeName(metadata.Type),
                    metadata.GetFlags(),
                    description: attribute.Description);

                if (RelatedTypes.All(rt => rt.Type != metadata.Type))
                {
                    RelatedTypes.Add(new RelatedType { Type = metadata.Type });
                }
            }

            foreach (var parameterInfo in method.GetParameters())
            {
                if (parameterInfo.ParameterType == typeof(RequestContext))
                {
                    continue;
                }

                if (parameterInfo.ParameterType == typeof(ApiRequest))
                {
                    continue;
                }

                var parameterMetadata = TypeMetadata.GenerateTypeMetadata(
                    parameterInfo.ParameterType,
                    new DeclareFieldAttribute());
                var description =
                    parameterInfo.GetCustomAttribute(typeof(ApiDescriptionAttribute)) as ApiDescriptionAttribute;

                var parameterName = ApiDescriptionAttribute.GetParameterName(parameterInfo);
                if (parameterMetadata.ScalarType != EnScalarType.None)
                {
                    field.Arguments.Add(
                        ApiField.Scalar(
                            parameterName,
                            parameterMetadata.ScalarType,
                            parameterMetadata.GetFlags(),
                            description: description?.Description));
                }
                else
                {
                    field.Arguments.Add(
                        ApiField.Object(
                            parameterName,
                            ApiDescriptionAttribute.GetTypeName(parameterInfo.ParameterType),
                            parameterMetadata.GetFlags(),
                            description: description?.Description));

                    if (RelatedTypes.All(rt => rt.Type != parameterMetadata.Type))
                    {
                        RelatedTypes.Add(new RelatedType { Type = parameterMetadata.Type });
                    }
                }
            }

            field.FillAuthorizationProperties(method);
            Fields[field.Name] = new FieldDescription(field, metadata, method);
            return field;
        }

        /// <summary>
        /// Generates api field from type method
        /// </summary>
        /// <param name="property">
        /// The property to process
        /// </param>
        /// <param name="attribute">
        /// The method description attribute
        /// </param>
        /// <returns>
        /// The field description
        /// </returns>
        private static ApiField GenerateFieldFromProperty(PropertyInfo property, PublishToApiAttribute attribute)
        {
            var declaringType = property.ReflectedType;

            TypeMetadata metadata;
            try
            {
                metadata = TypeMetadata.GenerateTypeMetadata(property.PropertyType, attribute);
            }
            catch (Exception exception)
            {
                GenerationErrors.Add(
                    $"Error while parsing return type of property {property.Name} of type {declaringType?.FullName}: {exception.Message}");
                return null;
            }

            var name = attribute.GetName(property);

            var flags = metadata.GetFlags();

            if ((metadata.Type.IsSubclassOf(typeof(Enum)) || metadata.ScalarType != EnScalarType.None)
                && !metadata.IsAsync && !metadata.IsForwarding
                && (metadata.MetaType == TypeMetadata.EnMetaType.Object
                    || metadata.MetaType == TypeMetadata.EnMetaType.Scalar))
            {
                flags |= EnFieldFlags.IsFilterable | EnFieldFlags.IsSortable;
            }

            if (!metadata.IsAsync && !metadata.IsForwarding && metadata.ConverterType == null && property.CanWrite
                && metadata.MetaType != TypeMetadata.EnMetaType.Connection
                && (!(attribute is DeclareFieldAttribute)
                    || ((DeclareFieldAttribute)attribute).Access.HasFlag(EnAccessFlag.Writable)))
            {
                flags |= EnFieldFlags.CanBeUsedInInput;
            }

            if (attribute is DeclareFieldAttribute
                && !((DeclareFieldAttribute)attribute).Access.HasFlag(EnAccessFlag.Queryable))
            {
                flags &= ~EnFieldFlags.Queryable;
            }

            if (metadata.ScalarType != EnScalarType.None)
            {
                if ((attribute as DeclareFieldAttribute)?.IsKey == true)
                {
                    flags |= EnFieldFlags.IsKey;
                }

                var scalar = ApiField.Scalar(name, metadata.ScalarType, flags, description: attribute.Description);
                scalar.FillAuthorizationProperties(property);
                Fields[scalar.Name] = new FieldDescription(scalar, metadata, property);
                return scalar;
            }

            var field = ApiField.Object(
                name,
                ApiDescriptionAttribute.GetTypeName(metadata.Type),
                flags,
                description: attribute.Description);

            if (RelatedTypes.All(rt => rt.Type != metadata.Type))
            {
                RelatedTypes.Add(new RelatedType { Type = metadata.Type });
            }

            field.FillAuthorizationProperties(property);
            Fields[field.Name] = new FieldDescription(field, metadata, property);
            return field;
        }

        /// <summary>
        /// Generate fields from type methods
        /// </summary>
        /// <returns>The field api description</returns>
        private static IEnumerable<ApiField> GenerateTypeMethods()
        {
            var type = typeof(T);
            return
                type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Select(
                        p =>
                            new
                                {
                                    Method = p,
                                    Attribute =
                                    (DeclareFieldAttribute)p.GetCustomAttribute(typeof(DeclareFieldAttribute))
                                })
                    .Where(p => p.Attribute != null && !p.Method.IsGenericMethod && !p.Method.IsGenericMethodDefinition)
                    .Select(m => GenerateFieldFromMethod(m.Method, m.Attribute))
                    .Where(f => f != null);
        }

        /// <summary>
        /// Generate fields from type properties
        /// </summary>
        /// <returns>The field api description</returns>
        private static IEnumerable<ApiField> GenerateTypeProperties()
        {
            var type = typeof(T);
            return
                type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(
                        p =>
                            new
                                {
                                    Property = p,
                                    Attribute =
                                    (DeclareFieldAttribute)p.GetCustomAttribute(typeof(DeclareFieldAttribute))
                                })
                    .Where(p => p.Attribute != null && p.Property.CanRead)
                    .Select(o => GenerateFieldFromProperty(o.Property, o.Attribute))
                    .Where(f => f != null);
        }

        /// <summary>
        /// Generates a value getter for the specified property
        /// </summary>
        /// <param name="fieldDescription">The field description</param>
        /// <returns>The value getter function</returns>
        private static PropertyValueGetter GenerateValueGetter(FieldDescription fieldDescription)
        {
            var sourceParam = Expression.Parameter(typeof(T), "source");
            var requestParam = Expression.Parameter(typeof(ApiRequest), "request");
            var contextParam = Expression.Parameter(typeof(RequestContext), "context");
            var serializerParam = Expression.Parameter(typeof(JsonSerializer), "serializer");
            var callbackParam = Expression.Parameter(typeof(Action<Exception>), "errorCallback");

            var methodInfo = fieldDescription.TypeMember as MethodInfo;
            var propertyInfo = fieldDescription.TypeMember as PropertyInfo;

            if (methodInfo == null && propertyInfo == null)
            {
                GenerationErrors.Add($"Field {fieldDescription.Field.Name} is nighter property nor method");
                return (source, request, context, serializer, callback) => Task.FromResult<object>(null);
            }

            Expression valueGetter;
            Type valueType;
            if (methodInfo != null)
            {
                valueType = methodInfo.ReturnType;
                var methodArguments = new List<Expression>();

                var jsonPropertyMethod = typeof(JObject).GetMethod("Property", new[] { typeof(string) });
                var jsonPropertyValue = typeof(JProperty).GetProperty("Value");
                var jsonTokenToObject = typeof(JToken).GetMethod("ToObject", new[] { typeof(JsonSerializer) });

                var arguments = Expression.Convert(
                    Expression.Property(requestParam, typeof(ApiRequest).GetProperty("Arguments")),
                    typeof(JObject));

                foreach (var parameterInfo in methodInfo.GetParameters())
                {
                    var parameterName = ApiDescriptionAttribute.GetParameterName(parameterInfo);
                    if (parameterInfo.ParameterType == typeof(RequestContext))
                    {
                        methodArguments.Add(contextParam);
                    }
                    else if (parameterInfo.ParameterType == typeof(ApiRequest))
                    {
                        methodArguments.Add(requestParam);
                    }
                    else
                    {
                        var jsonProperty =
                            Expression.Condition(
                                Expression.NotEqual(arguments, Expression.Constant(null)),
                                Expression.Call(arguments, jsonPropertyMethod, Expression.Constant(parameterName)),
                                Expression.Constant(null, typeof(JProperty)));

                        var jsonValue =
                            Expression.Condition(
                                Expression.NotEqual(jsonProperty, Expression.Constant(null)),
                                Expression.Property(jsonProperty, jsonPropertyValue),
                                Expression.Constant(null, typeof(JToken)));

                        var valueDeserialization = Expression.Call(
                            jsonValue,
                            jsonTokenToObject.MakeGenericMethod(parameterInfo.ParameterType),
                            serializerParam);

                        var argument = Expression.Condition(
                            Expression.NotEqual(jsonValue, Expression.Constant(null)),
                            valueDeserialization,
                            Expression.Default(parameterInfo.ParameterType));

                        methodArguments.Add(argument);
                    }
                }

                valueGetter = Expression.Call(sourceParam, methodInfo, methodArguments);
            }
            else
            {
                valueType = propertyInfo.PropertyType;
                valueGetter = Expression.Property(sourceParam, propertyInfo);
            }

            if (!fieldDescription.Metadata.IsAsync)
            {
                var taskCreator = typeof(Task).GetMethod("FromResult").MakeGenericMethod(valueType);
                valueGetter = Expression.Call(taskCreator, valueGetter);
            }

            valueGetter = GenerateValueGetterConvertResultToObject(valueType, valueGetter);

            var function = Expression.Lambda<PropertyValueGetter>(
                valueGetter,
                sourceParam,
                requestParam,
                contextParam,
                serializerParam,
                callbackParam);

            return function.Compile();
        }

        /// <summary>
        /// Creates an expression that will convert result value to object
        /// </summary>
        /// <param name="returnType">The return object type</param>
        /// <param name="getValue">The value getter expression</param>
        /// <returns>The conversed type expression</returns>
        private static Expression GenerateValueGetterConvertResultToObject(Type returnType, Expression getValue)
        {
            var asyncType = TypeMetadata.CheckType(returnType, typeof(Task<>));
            if (asyncType != null)
            {
                returnType = asyncType.GenericTypeArguments[0];
            }

            var taskType = typeof(Task<>).MakeGenericType(returnType);

            var taskResult = taskType.GetProperty("Result");

            var continueMethod =
                taskType.GetMethods()
                    .First(
                        m =>
                            m.Name == "ContinueWith" && m.IsGenericMethod && m.GetParameters().Length == 1
                            && m.GetParameters().First().ParameterType.GetGenericTypeDefinition() == typeof(Func<,>)
                            && m.GetParameters().First().ParameterType.GenericTypeArguments[0] == taskType)
                    .MakeGenericMethod(typeof(object));

            var resultParameter = Expression.Parameter(taskType, "task");

            var conversion =
                Expression.Lambda(
                    Expression.Convert(Expression.Property(resultParameter, taskResult), typeof(object)),
                    resultParameter);
            getValue = Expression.Call(getValue, continueMethod, conversion);

            return getValue;
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

                isInitialized = true;
                foreach (var field in Fields.Values)
                {
                    field.Resolver = CreateResolver(field.Metadata);
                    field.GetValue = GenerateValueGetter(field);
                }

                foreach (var relatedType in RelatedTypes)
                {
                    relatedType.Resolver =
                        typeof(GenericObjectResolver<>).MakeGenericType(relatedType.Type).CreateInstance<IResolver>();
                }
            }
        }

        /// <summary>
        /// The type field description
        /// </summary>
        private class FieldDescription
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FieldDescription"/> class.
            /// </summary>
            /// <param name="field">
            /// The field.
            /// </param>
            /// <param name="metadata">
            /// The metadata.
            /// </param>
            /// <param name="typeMember">
            /// The type member.
            /// </param>
            public FieldDescription(ApiField field, TypeMetadata metadata, MemberInfo typeMember)
            {
                this.Field = field;
                this.Metadata = metadata;
                this.TypeMember = typeMember;
            }

            /// <summary>
            /// Gets the api field
            /// </summary>
            public ApiField Field { get; }

            /// <summary>
            /// Gets or sets a field value resolver method
            /// </summary>
            public PropertyValueGetter GetValue { get; set; }

            /// <summary>
            /// Gets the field metadata
            /// </summary>
            public TypeMetadata Metadata { get; }

            /// <summary>
            /// Gets or sets the resolver 
            /// </summary>
            public IResolver Resolver { get; set; }

            /// <summary>
            /// Gets the type member
            /// </summary>
            public MemberInfo TypeMember { get; }
        }
    }
}