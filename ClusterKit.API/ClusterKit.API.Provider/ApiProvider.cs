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
    using System.Threading.Tasks;

    using Castle.Core.Internal;

    using ClusterKit.API.Client;
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