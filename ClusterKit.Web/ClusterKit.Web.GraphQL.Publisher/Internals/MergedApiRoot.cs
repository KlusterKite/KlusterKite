// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MergedApiRoot.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The merged api root description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher.Internals
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ClusterKit.API.Client;
    using ClusterKit.Security.Client;
    using ClusterKit.Web.GraphQL.Publisher.GraphTypes;

    using global::GraphQL.Resolvers;
    using global::GraphQL.Types;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The merged api root description
    /// </summary>
    internal class MergedApiRoot : MergedObjectType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MergedApiRoot"/> class.
        /// </summary>
        /// <param name="originalTypeName">
        /// The original type name.
        /// </param>
        public MergedApiRoot(string originalTypeName)
            : base(originalTypeName)
        {
        }

        /// <summary>
        /// Gets the list of declared mutations
        /// </summary>
        public Dictionary<string, MergedField> Mutations { get; } = new Dictionary<string, MergedField>();

        /// <inheritdoc />
        public override IEnumerable<MergedType> GetAllTypes()
        {
            foreach (var type in this.Mutations.Values)
            {
                foreach (var argumentsValue in type.Arguments.Values.SelectMany(t => t.Type.GetAllTypes()))
                {
                    yield return argumentsValue;
                }

                foreach (var subType in type.Type.GetAllTypes())
                {
                    yield return subType;
                }
            }

            foreach (var type in base.GetAllTypes())
            {
                yield return type;
            }
        }

        /// <summary>
        /// Generate graph type for all registered mutations
        /// </summary>
        /// <returns>The mutations graph type</returns>
        public IObjectGraphType GenerateMutationType()
        {
            var fields = this.Mutations.Select(f => this.ConvertApiField(f, new MutationResolver(f.Value)));
            return new VirtualGraphType("Mutations", fields.ToList()) { Description = "The list of all detected mutations" };
        }

        /// <summary>
        /// Resolves request value
        /// </summary>
        /// <param name="context">
        /// The request context
        /// </param>
        /// <returns>
        /// Resolved value
        /// </returns>
        public override object Resolve(ResolveFieldContext context)
        {
            return this.DoApiRequests(context, context.UserContext as RequestContext);
        }

        /// <summary>
        /// Creates an api requests to gather all data
        /// </summary>
        /// <param name="context">
        /// The request contexts
        /// </param>
        /// <param name="requestContext">
        /// The request Context.
        /// </param>
        /// <returns>
        /// The request data
        /// </returns>
        private async Task<JObject> DoApiRequests(ResolveFieldContext context, RequestContext requestContext)
        {
            var taskList = new List<Task<JObject>>();
            foreach (var provider in this.Providers.Select(fp => fp.Provider))
            {
                var request = this.GatherMultipleApiRequest(provider, context.FieldAst, context).ToList();
                taskList.Add(provider.GetData(request, requestContext));
            }

            var responses = await Task.WhenAll(taskList);
            var options = new JsonMergeSettings
                              {
                                  MergeArrayHandling = MergeArrayHandling.Merge,
                                  MergeNullValueHandling = MergeNullValueHandling.Ignore
                              };

            var response = responses.Aggregate(
                new JObject(),
                (seed, next) =>
                    {
                        seed.Merge(next, options);
                        return seed;
                    });

            return response;
        }

        /// <summary>
        /// Resolves mutation requests
        /// </summary>
        private class MutationResolver : IFieldResolver
        {
            /// <summary>
            /// The mutation description
            /// </summary>
            private readonly MergedField mergedField;

            /// <summary>
            /// Mutation API provider
            /// </summary>
            private ApiProvider provider;

            /// <summary>
            /// Initializes a new instance of the <see cref="MutationResolver"/> class.
            /// </summary>
            /// <param name="mergedField">
            /// The merged field.
            /// </param>
            public MutationResolver(MergedField mergedField)
            {
                this.mergedField = mergedField;
                this.provider = this.mergedField.Type.Providers.First().Provider;
            }

            /// <summary>
            /// Resolves mutation value (sends request to API)
            /// </summary>
            /// <param name="context">
            /// The context.
            /// </param>
            /// <returns>
            /// The <see cref="object"/>.
            /// </returns>
            public object Resolve(ResolveFieldContext context)
            {
                var connectionMutationResultType = this.mergedField.Type as MergedConnectionMutationResultType;
                if (connectionMutationResultType != null)
                {
                    return this.DoConnectionMutationApiRequests(
                        context,
                        context.UserContext as RequestContext,
                        connectionMutationResultType);
                }

                var untypedMutationResultType = this.mergedField.Type as MergedUntypedMutationResult;
                if (untypedMutationResultType != null)
                {
                    return this.DoUntypedMutationApiRequests(
                        context,
                        context.UserContext as RequestContext,
                        untypedMutationResultType);
                }

                return this.DoApiRequests(context, context.UserContext as RequestContext);
            }

            /// <summary>
            /// Creates an api requests to gather all data
            /// </summary>
            /// <param name="context">
            /// The request contexts
            /// </param>
            /// <param name="requestContext">
            /// The request Context.
            /// </param>
            /// <param name="responseType">response type</param>
            /// <returns>
            /// The request data
            /// </returns>
            private async Task<JObject> DoConnectionMutationApiRequests(
                ResolveFieldContext context,
                RequestContext requestContext,
                MergedConnectionMutationResultType responseType)
            {
                var edgeType = responseType.EdgeType;
                var nodeType = responseType.EdgeType.ObjectType;
                List<ApiRequest> requestedFields = new List<ApiRequest>();

                var topFields = GetRequestedFields(context.FieldAst.SelectionSet, context, this.mergedField.Type.ComplexTypeName).ToList();

                var nodeRequests = topFields.Where(f => f.Name == "node" || f.Name == "edge" || f.Name == "deletedId").ToList();
                if (nodeRequests.Count > 0)
                {
                    var resultFields = new List<ApiRequest>();

                    // todo: resolve alias collisions
                    foreach (var nodeRequest in nodeRequests)
                    {
                        switch (nodeRequest.Name)
                        {
                            case "node":
                                resultFields.AddRange(nodeType.GatherSingleApiRequest(nodeRequest, context));
                                break;
                            case "edge":
                                resultFields.AddRange(edgeType.GatherSingleApiRequest(nodeRequest, context));
                                break;
                            case "deletedId":
                                resultFields.Add(new ApiRequest { FieldName = nodeType.KeyName });
                                break;
                            case "errors":
                                if (responseType.ErrorType != null)
                                {
                                    resultFields.AddRange(responseType.ErrorType.GatherSingleApiRequest(nodeRequest, context));
                                }

                                break;
                        }
                    }

                    var resultRequest = new ApiRequest
                                            {
                                                FieldName = "result",
                                                Fields = resultFields
                                            };
                    requestedFields.Add(resultRequest);
                }

                if (responseType.ErrorType != null)
                {
                    var errorsRequest = topFields.Where(f => f.Name == "errors");
                    foreach (var field in errorsRequest)
                    {
                        requestedFields.Add(
                            new ApiRequest
                                {
                                    FieldName = "errors",
                                    Alias = field.Alias,
                                    Fields =
                                        responseType.ErrorType.GatherSingleApiRequest(field, context)
                                            .ToList()
                                });
                    }
                }

                var request = new MutationApiRequest
                {
                    Arguments = context.FieldAst.Arguments.ToJson(),
                    FieldName = this.mergedField.FieldName,
                    Fields = requestedFields
                };

                var data = await this.provider.GetData(new List<ApiRequest> { request }, requestContext);
                var path = data?.Property("__requestPath")?.Value as JArray;
                var result = data?.Property("result")?.Value as JObject;
                
                if (path != null && result != null)
                {
                    var id = result.Property(nodeType.KeyName)?.Value;
                    var globalId = new JObject
                                       {
                                           { "p", path },
                                           { "api", this.provider.Description.ApiName },
                                           { "id", id }
                                       };

                    result.Add("__globalId", globalId.ToString(Formatting.None));

                    var deletedId = data.Property("__deletedId")?.Value;
                    if (deletedId != null)
                    {
                        var deletedGlobalId = new JObject
                                       {
                                           { "p", path },
                                           { "api", this.provider.Description.ApiName },
                                           { "id", deletedId }
                                       };
                        result.Add("deletedId", deletedGlobalId.ToString(Formatting.None));
                    }
                }

                data?.Add("clientMutationId", context.GetArgument<string>("clientMutationId"));
                return data;
            }

            /// <summary>
            /// Creates an api requests to gather all data
            /// </summary>
            /// <param name="context">
            /// The request contexts
            /// </param>
            /// <param name="requestContext">
            /// The request Context.
            /// </param>
            /// <param name="responseType">
            /// The response type
            /// </param>
            /// <returns>
            /// The request data
            /// </returns>
            private async Task<JObject> DoUntypedMutationApiRequests(
                ResolveFieldContext context, 
                RequestContext requestContext,
                MergedUntypedMutationResult responseType)
            {
                var topFields = GetRequestedFields(context.FieldAst.SelectionSet, context, this.mergedField.Type.ComplexTypeName).ToList();
                var requestedFields = new List<ApiRequest>();
                
                // todo: resolve alias collisions
                foreach (var topField in topFields.Where(f => f.Name == "result"))
                {
                    requestedFields.AddRange(responseType.OriginalReturnType.GatherSingleApiRequest(topField, context));
                }

                var request = new MutationApiRequest
                {
                    Arguments = context.FieldAst.Arguments.ToJson(),
                    FieldName = this.mergedField.FieldName,
                    Fields = requestedFields
                };

                var data = await this.provider.GetData(new List<ApiRequest> { request }, requestContext);
                data?.Add("clientMutationId", context.GetArgument<string>("clientMutationId"));
                return data;
            }

            /// <summary>
            /// Creates an api requests to gather all data
            /// </summary>
            /// <param name="context">
            /// The request contexts
            /// </param>
            /// <param name="requestContext">
            /// The request Context.
            /// </param>
            /// <returns>
            /// The request data
            /// </returns>
            private Task<JObject> DoApiRequests(ResolveFieldContext context, RequestContext requestContext)
            {
                var request = new MutationApiRequest
                                  {
                                      Arguments = context.FieldAst.Arguments.ToJson(),
                                      FieldName = this.mergedField.FieldName,
                                      Fields =
                                          this.mergedField.Type.GatherSingleApiRequest(
                                              context.FieldAst, context).ToList()
                                  };

                var apiRequests = this.provider.GetData(new List<ApiRequest> { request }, requestContext);
                return apiRequests;
            }
        }
    }
}