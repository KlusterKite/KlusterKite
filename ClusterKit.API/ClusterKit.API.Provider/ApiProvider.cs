// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiProvider.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Declares some api to be provided
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Provider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using Castle.Core.Internal;

    using ClusterKit.API.Client;
    using ClusterKit.API.Client.Attributes;
    using ClusterKit.API.Provider.Resolvers;
    using ClusterKit.Security.Client;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Public api provider.
    /// </summary>
    public abstract class ApiProvider
    {
        /// <summary>
        /// The list of warnings gathered on generation stage
        /// </summary>
        private readonly List<string> generationErrors = new List<string>();

        /// <summary>
        /// The current api resolver
        /// </summary>
        private ObjectResolver resolver;

        /// <summary>
        /// Prepares serializer to deserialize arguments
        /// </summary>
        private JsonSerializer argumentsSerializer;

        /// <summary>
        /// The list of generated mutations
        /// </summary>
        private Dictionary<string, ObjectResolver.MutationDescription> mutations;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiProvider"/> class.
        /// </summary>
        protected ApiProvider()
        {
            this.Assemble();
        }

        /// <summary>
        /// Gets the api description
        /// </summary>
        public ApiDescription ApiDescription { get; } = new ApiDescription();

        /// <summary>
        /// Gets the list of warnings gathered on generation stage
        /// </summary>
        public IReadOnlyList<string> GenerationErrors => this.generationErrors.AsReadOnly();

        /// <summary>
        /// Resolves query
        /// </summary>
        /// <param name="requests">
        /// The query request
        /// </param>
        /// <param name="context">
        /// The request context.
        /// </param>
        /// <param name="onErrorCallback">
        /// The method that will be called in case of errors
        /// </param>
        /// <returns>
        /// Resolved query
        /// </returns>
        public virtual Task<JToken> ResolveQuery(
            List<ApiRequest> requests,
            RequestContext context,
            Action<Exception> onErrorCallback)
        {
            return this.resolver.ResolveQuery(
                this,
                new ApiRequest { Fields = requests }, 
                ApiField.Object("root", this.ApiDescription.ApiName), 
                context,
                this.argumentsSerializer,
                onErrorCallback);
        }

        /// <summary>
        /// Resolves query
        /// </summary>
        /// <param name="request">
        /// The query request
        /// </param>
        /// <param name="context">
        /// The request context.
        /// </param>
        /// <param name="onErrorCallback">
        /// The method that will be called in case of errors
        /// </param>
        /// <returns>
        /// Resolved query
        /// </returns>
        public virtual async Task<JObject> ResolveMutation(
            ApiRequest request,
            RequestContext context,
            Action<Exception> onErrorCallback)
        {
            try
            {
                ObjectResolver.MutationDescription mutation;

                if (!this.mutations.TryGetValue(request.FieldName, out mutation))
                {
                    return null;
                }

                var resolveResult = await mutation.ResolveContainer(this, context, this.argumentsSerializer, onErrorCallback);
                if (resolveResult == null)
                {
                    return null;
                }

                request.FieldName = mutation.Field.Name;
                var rootRequest = new ApiRequest { Fields = new List<ApiRequest> { request } };

                var objectResolver = mutation.Resolver as ObjectResolver;
                if (objectResolver != null)
                {
                    var result = await objectResolver.ResolveQuery(
                               resolveResult,
                               rootRequest, 
                               mutation.Field,
                               context,
                               this.argumentsSerializer,
                               onErrorCallback) as JObject;

                    return new JObject { { "result", result?.Property(request.FieldName)?.Value } };
                }

                var connectionResolver = mutation.Resolver as IConnectionResolver;
                if (connectionResolver != null)
                {
                    var resolvedMutation = await connectionResolver.ResolveMutation(
                                              resolveResult,
                                              request,
                                              mutation.Field,
                                              context,
                                              this.argumentsSerializer,
                                              onErrorCallback);

                    return resolvedMutation;
                }

                return null;
            }
            catch (Exception e)
            {
                onErrorCallback?.Invoke(e);
                return null;
            }
        }

        /// <summary>
        /// Searches for the connection node
        /// </summary>
        /// <param name="id">
        /// The node id
        /// </param>
        /// <param name="path">
        /// The node connection path in API
        /// </param>
        /// <param name="nodeRequest">The request to the node value</param>
        /// <param name="context">
        /// The request context.
        /// </param>
        /// <param name="onErrorCallback">
        /// The method that will be called in case of errors
        /// </param>
        /// <returns>
        /// The serialized node value
        /// </returns>
        public async Task<JObject> SearchNode(
            string id,
            List<ApiRequest> path,
            ApiRequest nodeRequest,
            RequestContext context,
            Action<Exception> onErrorCallback)
        {
            try
            {
                var resolveResult = await this.resolver.GetNestedFieldValue(
                                        this,
                                        new Queue<string>(path.Select(p => p.FieldName)),
                                        context,
                                        this.argumentsSerializer,
                                        onErrorCallback);

                var connectionResolver = resolveResult?.Item2 as IConnectionResolver;
                if (connectionResolver == null)
                {
                    return null;
                }

                var node = await connectionResolver.GetNodeById(resolveResult.Item1, id);
                return
                    (JObject)
                    await connectionResolver.NodeResolver.ResolveQuery(
                        node,
                        nodeRequest, 
                        resolveResult.Item3,
                        context,
                        this.argumentsSerializer,
                        onErrorCallback);
            }
            catch (Exception e)
            {
                onErrorCallback?.Invoke(e);
                return null;
            }
        }

        /// <summary>
        /// Parses the metadata of returning type for the field
        /// </summary>
        /// <param name="type">The original field return type</param>
        /// <param name="attribute">The description attribute</param>
        /// <returns>The type metadata</returns>
        internal static TypeMetadata GenerateTypeMetadata(Type type, PublishToApiAttribute attribute)
        {
            var metadata = new TypeMetadata();
            var asyncType = TypeMetadata.CheckType(type, typeof(Task<>));
            if (asyncType != null)
            {
                metadata.IsAsync = true;
                type = asyncType.GenericTypeArguments[0];
            }

            var converter = (attribute as DeclareFieldAttribute)?.Converter;
            if (attribute.ReturnType != null)
            {
                type = attribute.ReturnType;
                metadata.IsForwarding = true;
            }
            else if (converter != null)
            {
                var valueConverter =
                    converter
                        .GetInterfaces()
                        .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValueConverter<>));
                if (valueConverter == null)
                {
                    throw new InvalidOperationException($"Converter {converter.FullName} should implement the IValueConverter<>");
                }

                type = valueConverter.GenericTypeArguments[0];
                metadata.ConverterType = converter;
            }

            var scalarType = TypeMetadata.CheckScalarType(type);
            metadata.MetaType = TypeMetadata.EnMetaType.Scalar;
            if (scalarType == EnScalarType.None)
            {
                var enumerable = TypeMetadata.CheckType(type, typeof(IEnumerable<>));
                var connection = TypeMetadata.CheckType(type, typeof(INodeConnection<,>));

                if (connection != null)
                {
                    metadata.MetaType = TypeMetadata.EnMetaType.Connection;
                    metadata.TypeOfId = connection.GenericTypeArguments[1];
                    type = connection.GenericTypeArguments[0];
                    scalarType = TypeMetadata.CheckScalarType(type);
                }
                else if (enumerable != null)
                {
                    metadata.MetaType = TypeMetadata.EnMetaType.Array;
                    type = enumerable.GenericTypeArguments[0];
                    scalarType = TypeMetadata.CheckScalarType(type);
                }
                else
                {
                    metadata.MetaType = TypeMetadata.EnMetaType.Object;
                }
            }

            if (scalarType != EnScalarType.None && type.IsSubclassOf(typeof(Enum)))
            {
                type = Enum.GetUnderlyingType(type);
            }

            // todo: check forwarding type
            metadata.ScalarType = scalarType;
            metadata.Type = type;

            var typeName = type.GetCustomAttribute<ApiDescriptionAttribute>()?.Name ?? type.FullName;
            metadata.TypeName = metadata.TypeOfId != null ? $"{typeName}_{metadata.TypeOfId.FullName}" : typeName;
            return metadata;
        }

        /// <summary>
        /// Generates the api description and prepares all resolvers
        /// </summary>
        private void Assemble()
        {
            this.ApiDescription.Version = this.GetType().Assembly.GetName().Version;
            this.ApiDescription.Types.Clear();
            this.ApiDescription.Mutations.Clear();

            var rootResolver = typeof(ObjectResolver<>)
                .MakeGenericType(this.GetType()).CreateInstance<ObjectResolver>();
            this.resolver = rootResolver;
            var root = rootResolver.GetApiType();
            this.ApiDescription.TypeName = this.ApiDescription.ApiName = root.TypeName;
            this.ApiDescription.Description = root.Description;
            this.ApiDescription.Fields = new List<ApiField>(root.Fields.Select(f => f.Clone()));

            List<ApiType> allTypes;
            List<ObjectResolver.MutationDescription> mutationList;
            List<string> errors;
            rootResolver.CreateApiRoot(out allTypes, out this.argumentsSerializer, out mutationList, out errors);
            this.generationErrors.Clear();
            this.generationErrors.AddRange(errors);
            this.mutations = mutationList.ToDictionary(m => m.MutationName);
            this.ApiDescription.Types = allTypes;
            this.ApiDescription.Mutations = this.mutations.Values.Select(d => d.CreateMutationField()).ToList();
        }
    }
}