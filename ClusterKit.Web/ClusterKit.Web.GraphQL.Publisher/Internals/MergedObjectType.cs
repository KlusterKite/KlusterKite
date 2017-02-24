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
    using System.Collections.Generic;
    using System.Linq;

    using ClusterKit.Web.GraphQL.Client;
    using ClusterKit.Web.GraphQL.Publisher.GraphTypes;

    using global::GraphQL.Language.AST;
    using global::GraphQL.Resolvers;
    using global::GraphQL.Types;

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
        public MergedObjectType(string originalTypeName)
            : base(originalTypeName)
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
            MultipleApiType
        }

        /// <summary>
        /// Gets or sets the field category
        /// </summary>
        public EnCategory Category { get; set; }

        /// <summary>
        /// Gets combined name from all provider
        /// </summary>
        public override string ComplexTypeName
        {
            get
            {
                if (this.Providers.Any())
                {
                    var providersNames = this.Providers.Select(p => $"{EscapeName(p.Provider.Description.ApiName)}_{EscapeName(p.FieldType.TypeName)}")
                        .Distinct()
                        .OrderBy(s => s)
                        .ToArray();

                    return string.Join("_", providersNames);
                }

                return EscapeName(this.OriginalTypeName);
            }
        }

        /// <inheritdoc />
        public override string Description
            =>
                this.Providers.Any()
                    ? string.Join(
                        "\n",
                        this.Providers.Select(p => p.FieldType.Description).Distinct().OrderBy(s => s).ToArray())
                    : null;

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

        /// <summary>
        /// Gather request parameters
        /// </summary>
        /// <param name="contextFieldAst">The request context</param>
        /// <returns>The list of api requests</returns>
        public override IEnumerable<ApiRequest> GatherSingleApiRequest(Field contextFieldAst)
        {
            foreach (var field in contextFieldAst.SelectionSet.Selections.Where(s => s is Field).Cast<Field>())
            {
                MergedField localField;

                if (!this.Fields.TryGetValue(field.Name, out localField))
                {
                    continue;
                }

                var request = new ApiRequest
                                  {
                                      Arguments = field.Arguments.ToJson(),
                                      FieldName = field.Name,
                                      Fields = localField.Type.GatherSingleApiRequest(field).ToList()
                                  };
                if (request.Fields.Count == 0)
                {
                    request.Fields = null;
                }

                yield return request;
            }
        }

        /// <inheritdoc />
        public override IGraphType GenerateGraphType()
        {
            var fields = this.Fields.Select(this.ConvertApiField);
            return new VirtualGraphType(this.ComplexTypeName, fields.ToList()) { Description = this.Description };
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
                foreach (var argumentsValue in type.Arguments.Values.SelectMany(t => t.Type.GetAllTypes()))
                {
                    yield return argumentsValue;
                }

                foreach (var subType in type.Type.GetAllTypes())
                {
                    yield return subType;
                }
            }
        }

        /// <summary>
        /// Creates <see cref="FieldType"/> from <see cref="ApiType"/>
        /// </summary>
        /// <param name="description">
        /// The api field description
        /// </param>
        /// <returns>
        /// The <see cref="FieldType"/>
        /// </returns>
        protected FieldType ConvertApiField(KeyValuePair<string, MergedField> description)
        {
            return this.ConvertApiField(description, null);
        }

        /// <summary>
        /// Creates <see cref="FieldType"/> from <see cref="ApiType"/>
        /// </summary>
        /// <param name="description">
        /// The api field description
        /// </param>
        /// <param name="resolver">
        /// The field resolver.
        /// </param>
        /// <returns>
        /// The <see cref="FieldType"/>
        /// </returns>
        protected FieldType ConvertApiField(KeyValuePair<string, MergedField> description, IFieldResolver resolver)
        {
            var field = new FieldType
                            {
                                Name = description.Key,
                                Type = typeof(VirtualGraphType), // to workaround internal library checks
                                Metadata =
                                    new Dictionary<string, object> { { MetaDataTypeKey, description.Value } },
                                Resolver = resolver,
                                Description = description.Value.Description
                            };
            return field;
        }

        /// <summary>
        /// Gather request parameters for the specified api provider
        /// </summary>
        /// <param name="provider">The api provider</param>
        /// <param name="contextFieldAst">The request context</param>
        /// <returns>The list of api requests</returns>
        protected IEnumerable<ApiRequest> GatherMultipleApiRequest(ApiProvider provider, Field contextFieldAst)
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
                var request = new ApiRequest
                                  {
                                      Arguments = usedField.Ast.Arguments.ToJson(),
                                      FieldName = usedField.Ast.Name
                                  };
                var endType = usedField.Field.Type as MergedObjectType;

                request.Fields = endType?.Category == EnCategory.MultipleApiType
                                     ? endType.GatherMultipleApiRequest(provider, usedField.Ast).ToList()
                                     : usedField.Field.Type.GatherSingleApiRequest(usedField.Ast).ToList();

                yield return request;
            }
        }
    }
}