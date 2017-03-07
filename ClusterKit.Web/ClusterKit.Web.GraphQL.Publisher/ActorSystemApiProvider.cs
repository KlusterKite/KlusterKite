// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActorSystemApiProvider.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Provides access to api in whole cluster
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Akka.Actor;
    
    using ClusterKit.API.Client;
    using ClusterKit.API.Client.Messages;
    using ClusterKit.Security.Client;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Provides access to api in whole cluster 
    /// </summary>
    public class ActorSystemApiProvider : ApiProvider
    {
        /// <summary>
        /// The random number generator
        /// </summary>
        private readonly Random random = new Random();

        /// <summary>
        /// Gets or sets the request timeout
        /// </summary>
        private readonly TimeSpan? timeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActorSystemApiProvider"/> class.
        /// </summary>
        /// <param name="description">
        /// The API description.
        /// </param>
        /// <param name="timeout">
        /// The request timeout.
        /// </param>
        public ActorSystemApiProvider(ApiDescription description, TimeSpan? timeout)
        {
            this.timeout = timeout;
            this.Description = description;
        }

        /// <summary>
        /// Gets the endpoints.
        /// </summary>
        public ConcurrentDictionary<Address, IActorRef> Endpoints { get; } = new ConcurrentDictionary<Address, IActorRef>();

        /// <inheritdoc />
        public override async Task<JObject> GetData(List<ApiRequest> requests, RequestContext context)
        {
            var endpoint = this.GetEndpoint();
            if (endpoint == null)
            {
                return null;
            }

            var mutations = requests.OfType<MutationApiRequest>().ToList();
            if (mutations.Count > 0)
            {
                var result = new JObject();
                foreach (var mutation in mutations)
                {
                    mutation.Context = context;
                    
                    // todo: repeat ask in case of exception
                    var midResult = await endpoint.Ask<SurrogatableJObject>(mutation, this.timeout);
                    result.Merge((JObject)midResult);
                }

                return result;
            }

            // todo: repeat ask in case of exception
            var query = new QueriApiRequest { Context = context, Fields = requests };
            return await endpoint.Ask<SurrogatableJObject>(query, this.timeout);
        }

        /// <inheritdoc />
        public override async Task<JObject> SearchNode(
            string id, 
            List<RequestPathElement> path, 
            ApiRequest request,
            RequestContext context)
        {
            var endpoint = this.GetEndpoint();
            if (endpoint == null)
            {
                return null;
            }

            var query = new NodeSearchApiRequest
                            {
                                Id = id,
                                Context = context,
                                Path = path.Select(p => p.ToApiRequest()).ToList(),
                                Request = request
                            };
            return await endpoint.Ask<SurrogatableJObject>(query, this.timeout);
        }

        /// <summary>
        /// Selects the endpoint to solve the request
        /// </summary>
        /// <returns>The endpoint to solve</returns>
        private IActorRef GetEndpoint()
        {
            var endpoints = this.Endpoints.Values.ToList();
            if (endpoints.Count == 0)
            {
                return null;
            }

            var endpoint = endpoints[this.random.Next(0, endpoints.Count)];
            return endpoint;
        }
    }
}