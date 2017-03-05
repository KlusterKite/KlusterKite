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

    using ClusterKit.API.Client;
    using ClusterKit.Security.Client;

    using global::GraphQL.Language.AST;
    using global::GraphQL.Resolvers;
    using global::GraphQL.Types;

    using JetBrains.Annotations;

    using Newtonsoft.Json;

    /// <summary>
    /// The node searcher
    /// </summary>
    internal class NodeSearcher : IFieldResolver
    {
        /// <summary>
        /// The list of api providers
        /// </summary>
        private List<ApiProvider> providers;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeSearcher"/> class.
        /// </summary>
        /// <param name="providers">
        /// The providers.
        /// </param>
        public NodeSearcher(List<ApiProvider> providers)
        {
            this.providers = providers;
        }

        /// <inheritdoc />
        public object Resolve(ResolveFieldContext context)
        {
            var serializedId = (context.FieldAst.Arguments.ValueFor("id") as StringValue)?.Value;
            if (string.IsNullOrWhiteSpace(serializedId))
            {
                return null;
            }

            var globalId = JsonConvert.DeserializeObject<GlobalId>(serializedId);
            if (globalId?.ApiName == null || string.IsNullOrWhiteSpace(globalId.Id) || globalId.Path == null
                || globalId.Path.Count == 0)
            {
                return null;
            }

            var api = this.providers.FirstOrDefault(p => p.Description.ApiName == globalId.ApiName);

            return api?.SearchNode(globalId.Id, globalId.Path, context.UserContext as RequestContext);
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
            public string Id { get; set; }

            /// <summary>
            /// Gets or sets the path to the connection
            /// </summary>
            [JsonProperty("p")]
            [UsedImplicitly]
            public List<RequestPathElement> Path { get; set; }
        }
    }
}