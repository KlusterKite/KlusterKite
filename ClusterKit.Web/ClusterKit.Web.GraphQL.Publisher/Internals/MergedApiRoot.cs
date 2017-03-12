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

        /// <summary>
        /// Gets or sets the node searcher
        /// </summary>
        public NodeSearcher NodeSearher { get; internal set; }

        /// <inheritdoc />
        public override MergedObjectType Clone()
        {
            var clone = new MergedApiRoot(this.OriginalTypeName);
            this.FillWithMyFields(clone);
            foreach (var mutation in this.Mutations)
            {
                clone.Mutations[mutation.Key] = mutation.Value.Clone();
            }

            return clone;
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

        /// <inheritdoc />
        public override IGraphType GenerateGraphType(NodeInterface nodeInterface)
        {
            var graphType = (VirtualGraphType)base.GenerateGraphType(nodeInterface);
            var nodeFieldType = new FieldType();
            nodeFieldType.Name = "__node";
            nodeFieldType.ResolvedType = nodeInterface;
            nodeFieldType.Description = "The node global searcher according to Relay specification";
            nodeFieldType.Arguments =
                new QueryArguments(
                    new QueryArgument(typeof(IdGraphType)) { Name = "id", Description = "The node global id" });
            nodeFieldType.Resolver = this.NodeSearher;
            graphType.AddField(nodeFieldType);

            return graphType;
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
                if (request.Count > 0)
                {
                    taskList.Add(provider.GetData(request, requestContext));
                }
            }

            if (taskList.Count == 0)
            {
                return new JObject();
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
                this.provider = this.mergedField.Providers.First();
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
                var idSubRequestRequest = new List<ApiRequest>
                                              {
                                                  new ApiRequest { FieldName = nodeType.KeyName, Alias = "__id" }
                                              };
                var idRequestRequest = new ApiRequest
                {
                    Alias = "__idRequest",
                    FieldName = "result",
                    Fields = idSubRequestRequest
                };
                requestedFields.Add(idRequestRequest);

                var topFields = GetRequestedFields(context.FieldAst.SelectionSet, context, this.mergedField.Type.ComplexTypeName).ToList();

                var nodeRequests = topFields.Where(f => f.Name == "node" || f.Name == "edge").ToList();

                var nodeAliases = new List<string>();
                foreach (var nodeRequest in nodeRequests)
                {
                    var nodeAlias = nodeRequest.Alias ?? nodeRequest.Name;
                    nodeAliases.Add(nodeAlias);
                    switch (nodeRequest.Name)
                    {
                        case "node":
                            var nodeFields = nodeType.GatherSingleApiRequest(nodeRequest, context).ToList();
                            nodeFields.Add(new ApiRequest { Alias = "__id", FieldName = nodeType.KeyName });
                            requestedFields.Add(
                                new ApiRequest
                                    {
                                        Alias = nodeAlias,
                                        FieldName = "result",
                                        Fields =
                                            nodeFields
                                    });
                            break;
                        case "edge":
                            var edgeFields = new List<ApiRequest>();
                            foreach (var edgeNodeRequests in
                                GetRequestedFields(nodeRequest.SelectionSet, context, edgeType.ComplexTypeName)
                                    .Where(f => f.Name == "node"))
                            {
                                edgeFields.AddRange(nodeType.GatherSingleApiRequest(edgeNodeRequests, context)
                                    .Select(
                                        f =>
                                            {
                                                f.Alias =
                                                    $"{edgeNodeRequests.Alias ?? edgeNodeRequests.Name}_{f.Alias ?? f.FieldName}";
                                                return f;
                                            }));
                            }

                            edgeFields.Add(new ApiRequest { Alias = "__id", FieldName = nodeType.KeyName });
                            requestedFields.Add(
                                new ApiRequest { Alias = nodeAlias, FieldName = "result", Fields = edgeFields });

                            break;
                    }
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
                    Arguments = context.FieldAst.Arguments.ToJson(context),
                    FieldName = this.mergedField.FieldName,
                    Fields = requestedFields
                };

                var data = await this.provider.GetData(new List<ApiRequest> { request }, requestContext);
                var path = data?.Property("__requestPath")?.Value as JArray;
                var result = data?.Property("__idRequest")?.Value as JObject;
                
                if (path != null && result != null)
                {
                    var id = result.Property("__id")?.Value;
                    var globalId = new JObject
                                       {
                                           { "p", path },
                                           { "api", this.provider.Description.ApiName },
                                           { "id", id }
                                       };

                    var globalIdString = globalId.PackGlobalId();
                    foreach (var nodeAlias in nodeAliases)
                    {
                        (data.Property(nodeAlias)?.Value as JObject)?.Add("__globalId", globalIdString);
                    }

                    var deletedId = data.Property("__deletedId")?.Value;
                    if (deletedId != null)
                    {
                        var deletedGlobalId = new JObject
                                       {
                                           { "p", path },
                                           { "api", this.provider.Description.ApiName },
                                           { "id", deletedId }
                                       };
                        foreach (var field in topFields.Where(f => f.Name == "deletedId"))
                        {
                            data.Add(field.Alias ?? field.Name, deletedGlobalId.PackGlobalId());
                        }
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
                
                foreach (var topField in topFields.Where(f => f.Name == "result"))
                {
                    requestedFields.AddRange(responseType.OriginalReturnType.GatherSingleApiRequest(topField, context));
                }

                var request = new MutationApiRequest
                {
                    Arguments = context.FieldAst.Arguments.ToJson(context),
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
                                      Arguments = context.FieldAst.Arguments.ToJson(context),
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