// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SchemaGenerator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Generator of the GraphQL scheme from api providers
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher
{
    using System.Collections.Generic;
    using System.Linq;

    using Akka.Util.Internal;

    using ClusterKit.Web.GraphQL.Client;
    using ClusterKit.Web.GraphQL.Publisher.Internals;

    using global::GraphQL.Types;

    /// <summary>
    /// Generator of the GraphQL scheme from api providers
    /// </summary>
    public static class SchemaGenerator
    {
        /// <summary>
        /// Generates GraphQL schema
        /// </summary>
        /// <param name="providers">The list of providers</param>
        /// <returns>The new GraphQL schema</returns>
        public static Schema Generate(List<ApiProvider> providers)
        {
            var root = MergeApis(providers);
            var types = root.GetAllTypes().ToList();
            var typeNames = types.Select(t => t.ComplexTypeName).Distinct().ToList();

            var graphTypes = typeNames.ToDictionary(
                typeName => typeName,
                typeName => (IGraphType)new VirtualGraphType(types.FirstOrDefault(t => t.ComplexTypeName == typeName)));

            graphTypes[ApiField.TypeNameString] = new StringGraphType();
            graphTypes[ApiField.TypeNameInt] = new IntGraphType();

            graphTypes.Values
                .Where(a => a is VirtualGraphType)
                .Cast<VirtualGraphType>()
                .SelectMany(a => a.Fields)
                .ForEach(
                    f =>
                        {
                            var fieldDescription = f.GetMetadata<FieldDescription>(VirtualGraphType.MetaDataKey);
                            f.ResolvedType = graphTypes[fieldDescription.ComplexTypeName];
                            f.Resolver = fieldDescription;
                        });

            var schema = new Schema
                             {
                                 Query = (VirtualGraphType)graphTypes[root.ComplexTypeName],
                             };

            schema.Initialize();
            return schema;
        }

        /// <summary>
        /// Merges schemes from multiple APIs
        /// </summary>
        /// <param name="providers">The API providers descriptions</param>
        /// <returns>Merged API</returns>
        private static FieldDescription MergeApis(List<ApiProvider> providers)
        {
            var rootField = new FieldDescription
            {
                Category = FieldDescription.EnCategory.ApiRoot,
                OriginalTypeName = "root"
            };

            var apiRoot = new FieldDescription
            {
                Category = FieldDescription.EnCategory.ApiRoot,
                OriginalTypeName = "api",
                Providers = providers.Select(p => new FieldProvider { Provider = p, FieldType = p.Description }).ToList()
            };

            foreach (var provider in providers)
            {
                MergeFields(apiRoot, provider.Description.Fields, provider, new List<string>());
            }

            rootField.Fields["api"] = apiRoot;
            return rootField;
        }

        /// <summary>
        /// Insert new fields from new provider into current field list
        /// </summary>
        /// <param name="field">
        /// Field to update
        /// </param>
        /// <param name="apiFields">
        /// The list of subfields from api
        /// </param>
        /// <param name="provider">
        /// The api provider
        /// </param>
        /// <param name="path">
        /// The types names path to avoid circular references.
        /// </param>
        private static void MergeFields(FieldDescription field, List<ApiField> apiFields, ApiProvider provider, List<string> path)
        {
            foreach (var apiField in apiFields)
            {
                FieldDescription subField;
                if (!field.Fields.TryGetValue(apiField.Name, out subField))
                {
                    subField = new FieldDescription
                    {
                        Category =
                                           apiField.IsScalar
                                               ? FieldDescription.EnCategory.Scalar
                                               : FieldDescription.EnCategory.SingleApiType,
                        OriginalTypeName = apiField.TypeName
                    };

                    field.Fields[apiField.Name] = subField;
                }

                var apiFieldType = apiField.IsScalar
                                       ? null
                                       : provider.Description.Types.First(t => t.TypeName == apiField.TypeName);

                if (subField.OriginalTypeName != apiField.TypeName)
                {
                    if (subField.Category == FieldDescription.EnCategory.Scalar)
                    {
                        // todo: write merge error
                        continue;
                    }

                    subField.Category = FieldDescription.EnCategory.MultipleApiType;
                }

                subField.Providers.Add(new FieldProvider { FieldType = apiFieldType, Provider = provider });

                if (apiFieldType != null)
                {
                    if (path.Contains(apiFieldType.TypeName))
                    {
                        // todo: write circular reference error
                        continue;
                    }

                    MergeFields(subField, apiFieldType.Fields, provider, path.Union(new[] { apiFieldType.TypeName }).ToList());
                }
            }
        }
    }
}