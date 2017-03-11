// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DirectProvider.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing publishing and resolving integration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ClusterKit.API.Client;
    using ClusterKit.Security.Client;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Direct publish of <see cref="ClusterKit.API.Provider.ApiProvider"/>
    /// </summary>
    public class DirectProvider : ApiProvider
    {
        /// <summary>
        /// The provider.
        /// </summary>
        private readonly API.Provider.ApiProvider provider;

        /// <summary>
        /// The action to output resolve errors
        /// </summary>
        private readonly Action<string> errorOutput;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectProvider"/> class.
        /// </summary>
        /// <param name="provider">
        /// The provider.
        /// </param>
        /// <param name="errorOutput">
        /// The action to output resolve errors
        /// </param>
        public DirectProvider(API.Provider.ApiProvider provider, Action<string> errorOutput)
        {
            this.provider = provider;
            this.errorOutput = errorOutput;
            this.Description = provider.ApiDescription;
        }

        /// <inheritdoc />
        public override async Task<JObject> GetData(List<ApiRequest> requests, RequestContext context)
        {
            var mutations = requests.OfType<MutationApiRequest>().ToList();
            if (mutations.Count > 0)
            {
                var result = new JObject();
                foreach (var mutation in mutations)
                {
                    var midResult = await this.provider.ResolveMutation(
                                        mutation,
                                        context,
                                        exception =>
                                            this.errorOutput?.Invoke(
                                                $"Resolve error: {exception.Message}\n{exception.StackTrace}"));
                    result.Merge(midResult);
                }

                return result;
            }

            return await this.provider.ResolveQuery(
                       requests,
                       context,
                       exception =>
                           this.errorOutput?.Invoke($"Resolve error: {exception.Message}\n{exception.StackTrace}"));
        }

        /// <inheritdoc />
        public override async Task<JObject> SearchNode(
            string id,
            List<RequestPathElement> path,
            ApiRequest request,
            RequestContext context)
        {
            return await this.provider.SearchNode(
                       id,
                       path.Select(p => p.ToApiRequest()).ToList(),
                       request,
                       context,
                       exception =>
                           this.errorOutput?.Invoke($"Resolve error: {exception.Message}\n{exception.StackTrace}"));
        }
    }
}