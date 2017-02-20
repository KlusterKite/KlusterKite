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
        /// <param name="context">The request context</param>
        /// <returns>Resolved value</returns>
        public override object Resolve(ResolveFieldContext context)
        {
            return this.DoApiRequests(context);
        }

        /// <summary>
        /// Creates an api requests to gather all data
        /// </summary>
        /// <param name="context">The request contexts</param>
        /// <returns>The request data</returns>
        private async Task<JObject> DoApiRequests(ResolveFieldContext context)
        {
            var taskList = new List<Task<string>>();
            foreach (var provider in this.Providers.Select(fp => fp.Provider))
            {
                var request = this.GatherMultipleApiRequest(provider, context.FieldAst).ToList();
                taskList.Add(provider.GetData(request));
            }

            var responses = await Task.WhenAll(taskList);
            var options = new JsonMergeSettings
                              {
                                  MergeArrayHandling = MergeArrayHandling.Merge,
                                  MergeNullValueHandling = MergeNullValueHandling.Ignore
                              };

            return responses.Select(JsonConvert.DeserializeObject).Aggregate(
                new JObject(),
                (seed, next) =>
                    {
                        seed.Merge(next, options);
                        return seed;
                    });
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
                return this.DoApiRequests(context);
            }

            /// <summary>
            /// Creates an api requests to gather all data
            /// </summary>
            /// <param name="context">The request contexts</param>
            /// <returns>The request data</returns>
            private async Task<JObject> DoApiRequests(ResolveFieldContext context)
            {
                var request = new TempApiRequest { Arguments = context.FieldAst.Arguments, Name = this.mergedField.FieldName };
                var response = await this.provider.GetData(new List<TempApiRequest> { request });
                return (JObject)JsonConvert.DeserializeObject(response);
            }
        }
    }
}