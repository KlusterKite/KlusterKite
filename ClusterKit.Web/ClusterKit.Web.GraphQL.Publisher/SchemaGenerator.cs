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
                typeName => types.FirstOrDefault(t => t.ComplexTypeName == typeName)?.GenerateGraphType());

            graphTypes[ApiField.TypeNameString] = new StringGraphType();
            graphTypes[ApiField.TypeNameInt] = new IntGraphType();

            graphTypes.Values
                .Where(a => a is VirtualGraphType)
                .Cast<VirtualGraphType>()
                .SelectMany(a => a.Fields)
                .ForEach(
                    f =>
                        {
                            var fieldDescription = f.GetMetadata<MergedType>(MergedType.MetaDataKey);
                            if (fieldDescription != null)
                            {
                                f.ResolvedType = fieldDescription.WrapForField(graphTypes[fieldDescription.ComplexTypeName]);
                                f.Resolver = fieldDescription;
                            }
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
        private static MergedType MergeApis(List<ApiProvider> providers)
        {
            var rootField = new MergedEndType("root")
            {
                Category = MergedEndType.EnCategory.ApiRoot
            };

            var apiRoot = new MergedEndType("api")
            {
                Category = MergedEndType.EnCategory.ApiRoot
            };

            apiRoot.AddProviders(providers.Select(p => new FieldProvider { Provider = p, FieldType = p.Description }));

            foreach (var provider in providers)
            {
                MergeFields(apiRoot, provider.Description.Fields, provider, new List<string>());
            }

            rootField.Fields["api"] = new MergedField(apiRoot);
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
        private static void MergeFields(
            MergedEndType field,
            List<ApiField> apiFields,
            ApiProvider provider,
            List<string> path)
        {
            // todo: refactor this
            foreach (var apiField in apiFields)
            {
                var apiFieldType = apiField.Flags.HasFlag(ApiField.EnFlags.IsScalar)
                                       ? null
                                       : provider.Description.Types.First(t => t.TypeName == apiField.TypeName);

                MergedField subField;
                if (!field.Fields.TryGetValue(apiField.Name, out subField))
                {
                    var linkEndType = new MergedEndType(apiField.TypeName)
                                          {
                                              Category =
                                                  apiField.Flags.HasFlag(ApiField.EnFlags.IsScalar)
                                                      ? MergedEndType.EnCategory.Scalar
                                                      : MergedEndType.EnCategory
                                                          .SingleApiType
                                          };

                    if (!apiField.Flags.HasFlag(ApiField.EnFlags.IsArray))
                    {
                        subField = new MergedField(linkEndType, apiField.Flags);
                    }
                    else
                    {
                        linkEndType.AddProvider(new FieldProvider { FieldType = apiFieldType, Provider = provider });
                        if (apiFieldType != null)
                        {
                            MergeFields(
                                linkEndType,
                                apiFieldType.Fields,
                                provider,
                                path.Union(new[] { apiFieldType.TypeName }).ToList());
                        }

                        subField = new MergedField(new MergedConnectionType(
                            apiField.TypeName,
                            new FieldProvider { FieldType = apiFieldType, Provider = provider },
                            linkEndType));
                    }

                    field.Fields[apiField.Name] = subField;
                }
                else
                {
                    // got the same type from different API providers
                    if (subField.Type.OriginalTypeName == apiField.TypeName)
                    {
                        // todo: write merge error
                        continue;
                    }
                }

                var endType = subField.Type as MergedEndType;
                if (endType != null)
                {
                    if (endType.Providers.Any() && (endType.Category == MergedEndType.EnCategory.Scalar || apiField.Flags.HasFlag(ApiField.EnFlags.IsScalar)))
                    {
                        // todo: write merge error
                        continue;
                    }

                    endType.AddProvider(new FieldProvider { FieldType = apiFieldType, Provider = provider });
                    if (endType.Providers.Count() > 1)
                    {
                        endType.Category = MergedEndType.EnCategory.MultipleApiType;
                    }

                    if (apiFieldType != null)
                    {
                        if (path.Contains(apiFieldType.TypeName))
                        {
                            // todo: write circular reference error
                            continue;
                        }

                        MergeFields(
                            endType,
                            apiFieldType.Fields,
                            provider,
                            path.Union(new[] { apiFieldType.TypeName }).ToList());
                    }
                }
            }
        }
    }
}