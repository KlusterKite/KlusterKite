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
    using ClusterKit.Web.GraphQL.Publisher.GraphTypes;
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
                                f.Arguments = fieldDescription.GenerateArguments();
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
            var rootField = new MergedObjectType("root")
            {
                Category = MergedObjectType.EnCategory.ApiRoot
            };

            var apiRoot = new MergedObjectType("api")
            {
                Category = MergedObjectType.EnCategory.ApiRoot
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
        /// Insert new fields from new provider into current type
        /// </summary>
        /// <param name="parentType">
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
            MergedObjectType parentType,
            List<ApiField> apiFields,
            ApiProvider provider,
            List<string> path)
        {
            foreach (var apiField in apiFields)
            {
                MergedField complexField;
                if (parentType.Fields.TryGetValue(apiField.Name, out complexField))
                {
                    if (apiField.Flags.HasFlag(EnFieldFlags.IsScalar) || apiField.Flags.HasFlag(EnFieldFlags.IsArray)
                        || !(complexField.Type is MergedObjectType))
                    {
                        // todo: write merge error
                        continue;
                    }
                }

                MergedType newFieldType;
                if (apiField.Flags.HasFlag(EnFieldFlags.IsScalar))
                {
                    newFieldType = new MergedScalarType(apiField.ScalarType, new FieldProvider { Provider = provider });
                }
                else
                {
                    var apiFieldType = provider.Description.Types.First(t => t.TypeName == apiField.TypeName);
                    var objectType = complexField?.Type as MergedObjectType ?? new MergedObjectType(apiField.TypeName);
                    objectType.AddProvider(new FieldProvider { FieldType = apiFieldType, Provider = provider });
                    if (complexField != null)
                    {
                        objectType.Category = MergedObjectType.EnCategory.MultipleApiType;
                    }

                    if (path.Contains(apiFieldType.TypeName))
                    {
                        // todo: write circular reference error
                        continue;
                    }

                    MergeFields(
                        objectType,
                        apiFieldType.Fields,
                        provider,
                        path.Union(new[] { apiFieldType.TypeName }).ToList());

                    newFieldType = objectType;

                    if (apiField.Flags.HasFlag(EnFieldFlags.IsArray))
                    {
                        newFieldType = new MergedConnectionType(
                            objectType.OriginalTypeName,
                            new FieldProvider { Provider = provider, FieldType = apiFieldType },
                            objectType);
                    }
                }

                parentType.Fields[apiField.Name] = new MergedField(newFieldType, apiField.Flags);
            }
        }
    }
}