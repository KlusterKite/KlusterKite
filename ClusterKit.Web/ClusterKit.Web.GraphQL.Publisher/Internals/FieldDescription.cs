// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FieldDescription.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The merged api type field description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using global::GraphQL.Language.AST;
    using global::GraphQL.Resolvers;
    using global::GraphQL.Types;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The merged api type field description
    /// </summary>
    internal class FieldDescription : IFieldResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldDescription"/> class.
        /// </summary>
        public FieldDescription()
        {
            this.Category = EnCategory.SingleApiType;
            this.Fields = new Dictionary<string, FieldDescription>();
            this.Providers = new List<FieldProvider>();
        }

        /// <summary>
        /// The field type category
        /// </summary>
        public enum EnCategory
        {
            /// <summary>
            /// The end type is simple primitive
            /// </summary>
            Scalar,

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
            ApiRoot,
        }

        /// <summary>
        /// Gets or sets the type name of the field
        /// </summary>
        public string OriginalTypeName { get; set; }

        /// <summary>
        /// Gets combined name from all provider
        /// </summary>
        public string ComplexTypeName => this.Category == EnCategory.Scalar 
                                             ? this.OriginalTypeName 
                                             : this.Providers.Any() 
                                                 ? string.Join("|", this.Providers.Select(p => p.FieldType.TypeName).Distinct().OrderBy(s => s).ToArray())
                                                 : this.OriginalTypeName;

        /// <summary>
        /// Gets or sets the field category
        /// </summary>
        public EnCategory Category { get; set; }

        /// <summary>
        /// Gets or sets the list of providers
        /// </summary>
        public List<FieldProvider> Providers { get; set; }

        /// <summary>
        /// Gets or sets the list of subfields
        /// </summary>
        public Dictionary<string, FieldDescription> Fields { get; set; }

        /// <summary>
        /// Get all fields recursively
        /// </summary>
        /// <returns>The list of all defined types</returns>
        public IEnumerable<FieldDescription> GetAllTypes()
        {
            return this.Fields.Values.SelectMany(f => f.GetAllTypes()).Union(new[] { this });
        }

        /// <summary>
        /// Resolves request value
        /// </summary>
        /// <param name="context">The request context</param>
        /// <returns>Resolved value</returns>
        public object Resolve(ResolveFieldContext context)
        {
            switch (this.Category)
            {
                case EnCategory.ApiRoot:
                    return this.DoApiRequests(context);
                case EnCategory.Scalar:
                case EnCategory.SingleApiType:
                case EnCategory.MultipleApiType:
                    {
                        var parentData = context.Source as JObject;
                        return parentData?.GetValue(context.FieldName);
                    }

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
                contextFieldAst.SelectionSet.Selections.Where(s => s is Field).Cast<Field>()
                    .Join(
                        this.Fields.Where(f => f.Value.Providers.Any(fp => fp.Provider == provider)),
                        s => s.Name,
                        fp => fp.Key,
                        (s, fp) => new { Ast = s, Field = fp.Value }).ToList();

            foreach (var usedField in usedFields)
            {
                var request = new ApiRequest { Arguments = usedField.Ast.Arguments, Name = usedField.Ast.Name };
                switch (usedField.Field.Category)
                {
                    case EnCategory.SingleApiType:
                        request.Fields = usedField.Field.GatherSingleApiRequest(usedField.Ast).ToList();
                        break;
                    case EnCategory.MultipleApiType:
                        request.Fields = usedField.Field.GatherMultipleApiRequest(provider, usedField.Ast).ToList();
                        break;
                }

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
    }
}