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
            var api = MergeApis(providers);
            var root = new MergedRoot("Query");
            root.Fields["api"] = new MergedField("api", api, description: "The united api access");

            var types = root.GetAllTypes().ToList();

            var typeNames = types.Select(t => t.ComplexTypeName).Distinct().ToList();

            var graphTypes = typeNames.ToDictionary(
                typeName => typeName,
                typeName => types.FirstOrDefault(t => t.ComplexTypeName == typeName)?.GenerateGraphType());

            var mutationType = api.GenerateMutationType();
            graphTypes[mutationType.Name] = mutationType;

            graphTypes.Values.Where(a => a is IComplexGraphType)
                .Cast<IComplexGraphType>()
                .SelectMany(a => a.Fields)
                .ForEach(
                    f =>
                        {
                            var fieldDescription = f.GetMetadata<MergedField>(MergedType.MetaDataTypeKey);
                            if (fieldDescription == null)
                            {
                                return;
                            }

                            var typeArguments = fieldDescription.Type.GenerateArguments(graphTypes)
                                                ?? new QueryArguments();
                            var fieldArguments =
                                fieldDescription.Arguments.Select(
                                    p =>
                                        new QueryArgument(typeof(VirtualInputGraphType))
                                            {
                                                Name = p.Key,
                                                ResolvedType =
                                                    graphTypes[p.Value.Type.ComplexTypeName],
                                                Description = p.Value.Description
                                            });

                            var resultingArguments = typeArguments.Union(fieldArguments).ToList();

                            if (resultingArguments.Any())
                            {
                                f.Arguments = new QueryArguments(resultingArguments);
                            }

                            f.ResolvedType = fieldDescription.Flags.HasFlag(EnFieldFlags.IsArray)
                                                 ? new ListGraphType(
                                                     graphTypes[fieldDescription.Type.ComplexTypeName])
                                                 : graphTypes[fieldDescription.Type.ComplexTypeName];

                            if (f.Resolver == null)
                            {
                                f.Resolver = fieldDescription.Type;
                            }

                            if (!string.IsNullOrWhiteSpace(fieldDescription.Description))
                            {
                                f.Description = fieldDescription.Description;
                            }
                        });

            var schema = new Schema
                             {
                                 Query = (VirtualGraphType)graphTypes[root.ComplexTypeName],
                                 Mutation = mutationType
            };

            schema.Initialize();
            return schema;
        }

        /// <summary>
        /// Merges schemes from multiple APIs
        /// </summary>
        /// <param name="providers">The API providers descriptions</param>
        /// <returns>Merged API</returns>
        private static MergedApiRoot MergeApis(List<ApiProvider> providers)
        {
            var apiRoot = new MergedApiRoot("api");

            apiRoot.AddProviders(providers.Select(p => new FieldProvider { Provider = p, FieldType = p.Description }));
            
            foreach (var provider in providers)
            {
                MergeFields(apiRoot, provider.Description.Fields, provider, new List<string>());

                foreach (var apiMutation in provider.Description.Mutations)
                {
                    var returnType = CreateMergedType(provider, apiMutation, null, new List<string>(), false);
                    var arguments = apiMutation.Arguments.ToDictionary(
                        a => a.Name,
                        a =>
                            new MergedField(
                                a.Name,
                                CreateMergedType(provider, a, null, new List<string>(), true),
                                apiMutation.Flags,
                                description: a.Description));

                    apiRoot.Mutations[$"{provider.Description.ApiName}_{apiMutation.Name}"] = new MergedField(
                        apiMutation.Name,
                        returnType,
                        apiMutation.Flags,
                        arguments,
                        apiMutation.Description);
                }
            }

            return apiRoot;
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
        /// <param name="createAsInput">A value indicating that an input type is assembled</param>
        private static void MergeFields(
            MergedObjectType parentType,
            IEnumerable<ApiField> apiFields,
            ApiProvider provider,
            ICollection<string> path,
            bool createAsInput = false)
        {
            foreach (var apiField in apiFields)
            {
                MergedField complexField;
                if (parentType.Fields.TryGetValue(apiField.Name, out complexField))
                {
                    if (apiField.ScalarType != EnScalarType.None 
                        || createAsInput
                        || apiField.Flags.HasFlag(EnFieldFlags.IsConnection)
                        || apiField.Flags.HasFlag(EnFieldFlags.IsArray) 
                        || !(complexField.Type is MergedObjectType)
                        || complexField.Arguments.Any() || apiField.Arguments.Any())
                    {
                        // todo: write merge error
                        continue;
                    }
                }

                var fieldType = CreateMergedType(provider, apiField, complexField, path, createAsInput);

                if (fieldType == null)
                {
                    continue;
                }

                var fieldArguments = new Dictionary<string, MergedField>();

                if (!createAsInput)
                {
                    foreach (var argument in apiField.Arguments)
                    {
                        var fieldArgumentType = CreateMergedType(provider, argument, null, path, true);
                        fieldArguments[argument.Name] = new MergedField(argument.Name, fieldArgumentType, argument.Flags, description: argument.Description);
                    }
                }

                var description =
                    string.Join("\n", new[] { complexField?.Description, apiField.Description }.Where(s => !string.IsNullOrWhiteSpace(s)));
                var field = new MergedField(apiField.Name, fieldType, apiField.Flags, fieldArguments, string.IsNullOrWhiteSpace(description) ? null : description);

                parentType.Fields[apiField.Name] = field;
            }
        }

        /// <summary>
        /// Creates field from api description
        /// </summary>
        /// <param name="provider">The api provider</param>
        /// <param name="apiField">The api field description</param>
        /// <param name="complexField">The same field merged from previous api descriptions</param>
        /// <param name="path">The list of processed types</param>
        /// <param name="createAsInput">A value indicating that an input type is assembled</param>
        /// <returns>The field description</returns>
        private static MergedType CreateMergedType(
            ApiProvider provider,
            ApiField apiField,
            MergedField complexField,
            ICollection<string> path,
            bool createAsInput)
        {
            MergedType fieldType;
            if (apiField.ScalarType != EnScalarType.None)
            {
                fieldType = new MergedScalarType(apiField.ScalarType, new FieldProvider { Provider = provider });
            }
            else
            {
                var apiFieldType = provider.Description.Types.First(t => t.TypeName == apiField.TypeName);
                var objectType = complexField?.Type as MergedObjectType
                                 ?? (createAsInput
                                         ? new MergedInputType($"{provider.Description.ApiName}_{apiField.TypeName}")
                                         : new MergedObjectType($"{provider.Description.ApiName}_{apiField.TypeName}"));
                objectType.AddProvider(new FieldProvider { FieldType = apiFieldType, Provider = provider });
                if (complexField != null)
                {
                    objectType.Category = MergedObjectType.EnCategory.MultipleApiType;
                }

                if (path.Contains(apiFieldType.TypeName))
                {
                    // todo: write circular reference error
                    return null;
                }

                var fieldsToMerge = createAsInput
                                        ? apiFieldType.Fields.Where(
                                            f => !f.Flags.HasFlag(EnFieldFlags.IsConnection) && !f.Arguments.Any())
                                        : apiFieldType.Fields;

                MergeFields(
                    objectType,
                    fieldsToMerge,
                    provider,
                    path.Union(new[] { apiFieldType.TypeName }).ToList());

                fieldType = objectType;

                if (apiField.Flags.HasFlag(EnFieldFlags.IsConnection))
                {
                    fieldType = new MergedConnectionType(
                        objectType.OriginalTypeName,
                        new FieldProvider { Provider = provider, FieldType = apiFieldType },
                        objectType);
                }
            }

            return fieldType;
        }
    }
}