// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MergedFieldedType.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The api type with fields description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher.Internals
{
    using System.Collections.Generic;
    using System.Linq;

    using ClusterKit.API.Client;
    using ClusterKit.Web.GraphQL.Publisher.GraphTypes;

    using global::GraphQL.Language.AST;
    using global::GraphQL.Resolvers;
    using global::GraphQL.Types;

    /// <summary>
    /// The api type with fields description
    /// </summary>
    internal abstract class MergedFieldedType : MergedType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MergedFieldedType"/> class.
        /// </summary>
        /// <param name="originalTypeName">
        /// The original type name.
        /// </param>
        protected MergedFieldedType(string originalTypeName)
            : base(originalTypeName)
        {
            this.Fields = new Dictionary<string, MergedField>();
        }

        /// <summary>
        /// Gets or sets the list of subfields
        /// </summary>
        public Dictionary<string, MergedField> Fields { get; protected set; }

        /// <inheritdoc />
        public override IEnumerable<ApiRequest> GatherSingleApiRequest(Field contextFieldAst, ResolveFieldContext context)
        {
            foreach (var field in GetRequestedFields(contextFieldAst.SelectionSet, context, this.ComplexTypeName))
            {
                MergedField localField;

                if (!this.Fields.TryGetValue(field.Name, out localField))
                {
                    continue;
                }

                var request = new ApiRequest
                                  {
                                      Arguments = field.Arguments.ToJson(context),
                                      FieldName = field.Name,
                                      Alias = field.Alias,
                                      Fields = localField.Type.GatherSingleApiRequest(field, context).ToList()
                                  };
                if (request.Fields.Count == 0)
                {
                    request.Fields = null;
                }

                yield return request;
            }
        }

        /// <inheritdoc />
        public override IGraphType GenerateGraphType(NodeInterface nodeInterface)
        {
            var fields = this.Fields.Select(this.ConvertApiField);
            return new VirtualGraphType(this.ComplexTypeName, fields.ToList()) { Description = this.Description };
        }

        /// <summary>
        /// Creates <see cref="FieldType"/> from <see cref="ApiObjectType"/>
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
        /// Creates <see cref="FieldType"/> from <see cref="ApiObjectType"/>
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
    }
}