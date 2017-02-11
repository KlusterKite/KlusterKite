// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MergedObjectType.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The merged api type description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ClusterKit.Web.GraphQL.Client;
    using ClusterKit.Web.GraphQL.Publisher.GraphTypes;

    using global::GraphQL.Language.AST;
    using global::GraphQL.Types;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The merged api type description
    /// </summary>
    internal class MergedObjectType : MergedType
    {
        /// <summary>
        /// the list of providers
        /// </summary>
        private readonly List<FieldProvider> providers = new List<FieldProvider>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MergedObjectType"/> class.
        /// </summary>
        /// <param name="originalTypeName">
        /// The original type name.
        /// </param>
        public MergedObjectType(string originalTypeName) : base(originalTypeName)
        {
            this.Category = EnCategory.SingleApiType;
            this.Fields = new Dictionary<string, MergedField>();
        }

        /// <summary>
        /// The field type category
        /// </summary>
        public enum EnCategory
        {
            /// <summary>
            /// This is object provided by some single api
            /// </summary>
            SingleApiType,

            /// <summary>
            /// This is object that is combined from multiple API providers (some fields provided by one API, some from other)
            /// </summary>
            MultipleApiType,

            /// <summary>
            /// This node is the api root
            /// </summary>
            ApiRoot
        }

        /// <summary>
        /// Gets or sets the field category
        /// </summary>
        public EnCategory Category { get; set; }

        /// <summary>
        /// Gets combined name from all provider
        /// </summary>
        public override string ComplexTypeName
            => this.Providers.Any()
                        ? string.Join(
                            "|",
                            this.Providers.Select(p => p.FieldType.TypeName).Distinct().OrderBy(s => s).ToArray())
                        : this.OriginalTypeName;

        /// <summary>
        /// Gets the list of subfields
        /// </summary>
        public Dictionary<string, MergedField> Fields { get; }

        /// <summary>
        /// Gets or sets the list of providers
        /// </summary>
        public override IEnumerable<FieldProvider> Providers => this.providers;

        /// <summary>
        /// Adds a provider to the provider list
        /// </summary>
        /// <param name="provider">The provider</param>
        public void AddProvider(FieldProvider provider)
        {
            this.providers.Add(provider);
        }

        /// <summary>
        /// Adds the list of providers to the provider list
        /// </summary>
        /// <param name="newProviders">The list of providers</param>
        public void AddProviders(IEnumerable<FieldProvider> newProviders)
        {
            this.providers.AddRange(newProviders);
        }

        /// <inheritdoc />
        public override IGraphType GenerateGraphType()
        {
            var fields = this.Fields.Select(this.ConvertApiField);
            return new VirtualGraphType(this.ComplexTypeName, fields.ToList());
        }

        /// <summary>
        /// Get all included types recursively
        /// </summary>
        /// <returns>The list of all defined types</returns>
        public override IEnumerable<MergedType> GetAllTypes()
        {
            yield return this;
            foreach (var type in this.Fields.Values)
            {
                foreach (var subType in type.Type.GetAllTypes())
                {
                    yield return subType;
                }
            }
        }

        /// <summary>
        /// Resolves request value
        /// </summary>
        /// <param name="context">The request context</param>
        /// <returns>Resolved value</returns>
        public override object Resolve(ResolveFieldContext context)
        {
            switch (this.Category)
            {
                case EnCategory.ApiRoot:
                    return this.DoApiRequests(context);
                case EnCategory.SingleApiType:
                case EnCategory.MultipleApiType:
                    return base.Resolve(context);
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
        /// Gather request parameters for the specified api provider
        /// </summary>
        /// <param name="provider">The api provider</param>
        /// <param name="contextFieldAst">The request context</param>
        /// <returns>The list of api requests</returns>
        private IEnumerable<ApiRequest> GatherMultipleApiRequest(ApiProvider provider, Field contextFieldAst)
        {
            var usedFields =
                contextFieldAst.SelectionSet.Selections.Where(s => s is Field)
                    .Cast<Field>()
                    .Join(
                        this.Fields.Where(f => f.Value.Type.Providers.Any(fp => fp.Provider == provider)),
                        s => s.Name,
                        fp => fp.Key,
                        (s, fp) => new { Ast = s, Field = fp.Value })
                    .ToList();

            foreach (var usedField in usedFields)
            {
                var request = new ApiRequest { Arguments = usedField.Ast.Arguments, Name = usedField.Ast.Name };
                var endType = usedField.Field.Type as MergedObjectType;

                request.Fields = endType?.Category == EnCategory.MultipleApiType 
                    ? endType.GatherMultipleApiRequest(provider, usedField.Ast).ToList() 
                    : this.GatherSingleApiRequest(usedField.Ast).ToList();

                yield return request;
            }
        }

        /// <summary>
        /// Gather request parameters
        /// </summary>
        /// <param name="contextFieldAst">The request context</param>
        /// <returns>The list of api requests</returns>
        private IEnumerable<ApiRequest> GatherSingleApiRequest(Field contextFieldAst)
        {
            foreach (var field in contextFieldAst.SelectionSet.Selections.Where(s => s is Field).Cast<Field>())
            {
                var request = new ApiRequest
                                  {
                                      Arguments = field.Arguments,
                                      Name = field.Name,
                                      Fields = this.GatherSingleApiRequest(field).ToList()
                                  };
                if (request.Fields.Count == 0)
                {
                    request.Fields = null;
                }

                yield return request;
            }
        }

        /// <summary>
        /// Creates <see cref="FieldType"/> from <see cref="ApiType"/>
        /// </summary>
        /// <param name="description">The api field description</param>
        /// <returns>The <see cref="FieldType"/></returns>
        private FieldType ConvertApiField(KeyValuePair<string, MergedField> description)
        {
            var field = new FieldType
            {
                Name = description.Key,
                Metadata = new Dictionary<string, object> { { MetaDataKey, description.Value.Type } }
            };
            return field;
        }
    }
}