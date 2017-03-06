// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeSearcher.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The node searcher
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher.Internals
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ClusterKit.API.Client;
    using ClusterKit.Security.Client;

    using global::GraphQL.Language.AST;
    using global::GraphQL.Resolvers;
    using global::GraphQL.Types;

    using JetBrains.Annotations;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The node searcher
    /// </summary>
    internal class NodeSearcher : IFieldResolver
    {
        /// <summary>
        /// The list of api providers
        /// </summary>
        private readonly List<ApiProvider> providers;

        /// <summary>
        /// The api root.
        /// </summary>
        private readonly MergedApiRoot apiRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeSearcher"/> class.
        /// </summary>
        /// <param name="providers">
        /// The providers.
        /// </param>
        /// <param name="apiRoot">
        /// The api Root.
        /// </param>
        public NodeSearcher(List<ApiProvider> providers, MergedApiRoot apiRoot)
        {
            this.providers = providers;
            this.apiRoot = apiRoot;
        }

        /// <inheritdoc />
        public object Resolve(ResolveFieldContext context)
        {
            return this.SearchNode(context);
        }

        /// <summary>
        /// Performs node search request
        /// </summary>
        /// <param name="context">The request context</param>
        /// <returns>The searched node</returns>
        private async Task<JObject> SearchNode(ResolveFieldContext context)
        {
            var serializedId = (context.FieldAst.Arguments.ValueFor("id") as StringValue)?.Value;
            if (string.IsNullOrWhiteSpace(serializedId))
            {
                return null;
            }

            var globalId = JsonConvert.DeserializeObject<GlobalId>(serializedId);
            if (globalId?.ApiName == null || globalId.Id == null || globalId.Path == null || globalId.Path.Count == 0)
            {
                return null;
            }

            var api = this.providers.FirstOrDefault(p => p.Description.ApiName == globalId.ApiName);
            if (api == null)
            {
                return null;
            }

            var queue = new Queue<RequestPathElement>(globalId.Path);
            MergedType mergedType = this.apiRoot;
            while (queue.Count > 0)
            {
                var objectType = mergedType as MergedObjectType;
                if (objectType == null)
                {
                    return null;
                }

                var field = queue.Dequeue();
                MergedField mergedField;
                if (!objectType.Fields.TryGetValue(field.FieldName, out mergedField))
                {
                    return null;
                }

                mergedType = mergedField.Type;
            }

            var connectionType = mergedType as MergedConnectionType;
            if (connectionType == null)
            {
                return null;
            }

            var nodeRequest = connectionType.ElementType.GatherSingleApiRequest(context.FieldAst, context);
            var searchNode = await 
                api.SearchNode(
                    globalId.Id.ToString(Formatting.None),
                    globalId.Path,
                    new ApiRequest { Fields = nodeRequest.ToList() },
                    context.UserContext as RequestContext);

            if (searchNode != null)
            {
                searchNode = connectionType.ElementType.ResolveData(searchNode);
                searchNode.Add("__resolvedType", connectionType.ElementType.ComplexTypeName);
                searchNode.Add("__globalId", serializedId);
            }

            return searchNode;
        }

        /// <summary>
        /// The class to deserialize global id
        /// </summary>
        private class GlobalId
        {
            /// <summary>
            /// Gets or sets the api name
            /// </summary>
            [JsonProperty("api")]
            [UsedImplicitly]
            public string ApiName { get; set; }

            /// <summary>
            /// Gets or sets the id value
            /// </summary>
            [JsonProperty("id")]
            [UsedImplicitly]
            public JValue Id { get; set; }

            /// <summary>
            /// Gets or sets the path to the connection
            /// </summary>
            [JsonProperty("p")]
            [UsedImplicitly]
            public List<RequestPathElement> Path { get; set; }
        }
    }
}