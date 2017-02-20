// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiProvider.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Declares some api to be provided
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.API
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using ClusterKit.Security.Client;
    using ClusterKit.Web.GraphQL.Client;
    using ClusterKit.Web.GraphQL.Client.Attributes;

    using JetBrains.Annotations;

    /// <summary>
    /// Public api provider.
    /// </summary>
    public abstract class ApiProvider
    {
        /// <summary>
        /// The list of warnings gathered on generation stage
        /// </summary>
        private readonly List<string> generationErrors = new List<string>();

        /// <summary>
        /// The list of errors gathered on generation stage
        /// </summary>
        private readonly List<string> generationWarnings = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiProvider"/> class.
        /// </summary>
        protected ApiProvider()
        {
            this.Assemble();
        }

        /// <summary>
        /// Gets the api description
        /// </summary>
        public ApiDescription ApiDescription { get; } = new ApiDescription();

        /// <summary>
        /// Gets the list of warnings gathered on generation stage
        /// </summary>
        public IReadOnlyList<string> GenerationErrors => this.generationErrors.AsReadOnly();

        /// <summary>
        /// Gets the list of errors gathered on generation stage
        /// </summary>
        public IReadOnlyList<string> GenerationWarnings => this.generationWarnings.AsReadOnly();

        /// <summary>
        /// Converts string to camel case
        /// </summary>
        /// <param name="name">The property / method name</param>
        /// <returns>The name in camel case</returns>
        private static string ToCamelCase(string name)
        {
            name = name?.Trim();

            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            var array = name.ToCharArray();
            array[0] = array[0].ToString().ToLowerInvariant().ToCharArray().First();
            return new string(array);
        }

        /// <summary>
        /// Generates the api description and prepares all resolvers
        /// </summary>
        private void Assemble()
        {
            this.ApiDescription.Version = this.GetType().Assembly.GetName().Version;
            this.ApiDescription.Types.Clear();
            this.ApiDescription.Mutations.Clear();

            var assembleData = new AssembleTempData();
            var root = this.GenerateTypeDescription(this.GetType(), assembleData);

            this.ApiDescription.TypeName = this.ApiDescription.ApiName = root.TypeName;
            this.ApiDescription.Types =
                assembleData.DiscoveredApiTypes.Values.Where(t => t.TypeName != root.TypeName).ToList();
            this.ApiDescription.Description = root.Description;

            var mutations = this.GenerateMutations(root, new List<string>(), new List<string>(), assembleData);
            this.ApiDescription.Mutations = mutations.ToList();
            this.ApiDescription.Fields = root.Fields;
        }

        /// <summary>
        /// Generates api field from type method
        /// </summary>
        /// <param name="method">
        /// The method to process
        /// </param>
        /// <param name="attribute">
        /// The method description attribute
        /// </param>
        /// <param name="data">
        /// The temporary data used during assemble process
        /// </param>
        /// <returns>
        /// The field description
        /// </returns>
        private ApiField GenerateFieldFromMethod(
            MethodInfo method,
            PublishToApiAttribute attribute,
            AssembleTempData data)
        {
            var type = method.ReflectedType;
            TypeMetadata metadata;
            try
            {
                metadata = this.GenerateTypeMetadata(method.ReturnType, attribute);
            }
            catch (Exception exception)
            {
                this.generationErrors.Add(
                    $"Error while parsing return type of method {method.Name} of type {type?.FullName}: {exception.Message}");
                return null;
            }

            var name = attribute.Name ?? ToCamelCase(method.Name);
            ApiField field;
            if (metadata.ScalarType != EnScalarType.None)
            {
                field = ApiField.Scalar(
                    name,
                    metadata.ScalarType,
                    metadata.GetFlags(),
                    description: attribute.Description);
            }
            else
            {
                var returnApiType = this.GenerateTypeDescription(metadata.Type, data);
                field = ApiField.Object(
                    name,
                    returnApiType.TypeName,
                    metadata.GetFlags(),
                    description: attribute.Description);
            }

            foreach (var parameterInfo in method.GetParameters())
            {
                if (parameterInfo.ParameterType == typeof(RequestContext))
                {
                    // todo: take context in account
                    continue;
                }

                if (parameterInfo.ParameterType == typeof(ApiRequest))
                {
                    // todo: take request in account
                    continue;
                }

                var parameterMetadata = this.GenerateTypeMetadata(
                    parameterInfo.ParameterType,
                    new DeclareFieldAttribute());
                var description =
                    parameterInfo.GetCustomAttribute(typeof(ApiDescriptionAttribute)) as ApiDescriptionAttribute;

                var parameterName = description?.Name ?? ToCamelCase(parameterInfo.Name);
                if (parameterMetadata.ScalarType != EnScalarType.None)
                {
                    field.Arguments.Add(
                        ApiField.Scalar(
                            parameterName,
                            parameterMetadata.ScalarType,
                            parameterMetadata.GetFlags(),
                            description: description?.Description));
                }
                else
                {
                    var parameterApiType = this.GenerateTypeDescription(parameterMetadata.Type, data);
                    field.Arguments.Add(
                        ApiField.Object(
                            parameterName,
                            parameterApiType.TypeName,
                            parameterMetadata.GetFlags(),
                            description: description?.Description));
                }
            }

            // todo: create resolver
            return field;
        }

        /// <summary>
        /// Generates api field from type method
        /// </summary>
        /// <param name="property">
        /// The property to process
        /// </param>
        /// <param name="attribute">
        /// The method description attribute
        /// </param>
        /// <param name="data">
        /// The temporary data used during assemble process
        /// </param>
        /// <returns>
        /// The field description
        /// </returns>
        private ApiField GenerateFieldFromProperty(
            PropertyInfo property,
            PublishToApiAttribute attribute,
            AssembleTempData data)
        {
            var declaringType = property.ReflectedType;

            TypeMetadata metadata;
            try
            {
                metadata = this.GenerateTypeMetadata(property.PropertyType, attribute);
            }
            catch (Exception exception)
            {
                this.generationErrors.Add(
                    $"Error while parsing return type of property {property.Name} of type {declaringType?.FullName}: {exception.Message}");
                return null;
            }

            var name = attribute.Name ?? ToCamelCase(property.Name);

            if (property.CanWrite && !metadata.IsAsync && !metadata.IsForwarding)
            {
                // todo: create input type assembler
            }

            if (metadata.ScalarType != EnScalarType.None)
            {
                // todo: create resolver
                return ApiField.Scalar(
                    name,
                    metadata.ScalarType,
                    metadata.GetFlags(),
                    description: attribute.Description);
            }

            var returnApiType = this.GenerateTypeDescription(metadata.Type, data);

            // todo: create resolver
            return ApiField.Object(
                name,
                returnApiType.TypeName,
                metadata.GetFlags(),
                description: attribute.Description);
        }

        /// <summary>
        /// Generates mutations
        /// </summary>
        /// <param name="apiType">
        /// The current api type
        /// </param>
        /// <param name="path">
        /// The path to current field
        /// </param>
        /// <param name="typesUsed">
        /// The types already used to avoid circular references
        /// </param>
        /// <param name="data">
        /// The temporary data used during assemble process
        /// </param>
        /// <returns>
        /// The list of mutations
        /// </returns>
        private IEnumerable<ApiField> GenerateMutations(
            ApiType apiType,
            List<string> path,
            List<string> typesUsed,
            AssembleTempData data)
        {
            Type type;
            if (!data.DiscoveredTypes.TryGetValue(apiType.TypeName, out type))
            {
                yield break;
            }

            foreach (var mutation in this.GenerateMutationsDirect(type, path, data))
            {
                yield return mutation;
            }

            foreach (var mutation in this.GenerateMutationsFromFields(apiType, path, typesUsed, data))
            {
                yield return mutation;
            }

            foreach (var apiField in this.GenerateMutationsFromConnections(type, apiType, path, data))
            {
                yield return apiField;
            }
        }

        /// <summary>
        /// Generate mutations directly declared in the type
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="path">The fields path</param>
        /// <param name="data">
        /// The temporary data used during assemble process
        /// </param>
        /// <returns>The list of mutations</returns>
        private IEnumerable<ApiField> GenerateMutationsDirect(Type type, List<string> path, AssembleTempData data)
        {
            var fields =
                type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Select(
                        p =>
                            new
                                {
                                    Method = p,
                                    Attribute =
                                    (DeclareMutationAttribute)p.GetCustomAttribute(typeof(DeclareMutationAttribute))
                                })
                    .Where(p => p.Attribute != null && !p.Method.IsGenericMethod && !p.Method.IsGenericMethodDefinition)
                    .Select(m => this.GenerateFieldFromMethod(m.Method, m.Attribute, data))
                    .Where(f => f != null);

            foreach (var apiField in fields)
            {
                apiField.Name = string.Join(".", new List<string>(path) { apiField.Name });
                yield return apiField;
            }
        }

        /// <summary>
        /// Generate mutations directly declared in the type
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="apiType">The api description of the type</param>
        /// <param name="path">The fields path</param>
        /// <param name="data">
        /// The temporary data used during assemble process
        /// </param>
        /// <returns>The list of mutations</returns>
        private IEnumerable<ApiField> GenerateMutationsFromConnections(
            Type type,
            ApiType apiType,
            List<string> path,
            AssembleTempData data)
        {
            var members = type.GetMembers().Where(m => m is PropertyInfo || m is MethodInfo).Select(
                m =>
                    {
                        var description =
                            (DeclareConnectionAttribute)m.GetCustomAttribute(typeof(DeclareConnectionAttribute));
                        var name = description?.Name ?? ToCamelCase(m.Name);
                        return
                            new
                                {
                                    Name = name,
                                    Member = m,
                                    Description = description,
                                    Type = (m as PropertyInfo)?.PropertyType ?? (m as MethodInfo)?.ReturnType
                                };
                    }).Where(m => m.Description != null).ToList();

            var connections =
                apiType.Fields.Where(
                    f =>
                        !f.Arguments.Any() && f.ScalarType == EnScalarType.None
                        && f.Flags.HasFlag(EnFieldFlags.IsConnection));

            foreach (var connection in connections)
            {
                var description = members.FirstOrDefault(m => m.Name == connection.Name);
                if (description == null)
                {
                    continue;
                }

                var attribute = description.Description;
                var connectionType = attribute.ReturnType ?? description.Type;
                if (connectionType == null)
                {
                    this.generationErrors.Add(
                        $"Type discover error on connection {connection.Name} of type {type.FullName}");
                    continue;
                }

                var idType = TypeMetadata.CheckType(connectionType, typeof(INodeConnection<,>))?.GenericTypeArguments[1];
                if (idType == null)
                {
                    this.generationErrors.Add($"Wrong type on connection {connection.Name} of type {type.FullName}");
                    continue;
                }

                var idScalarType = TypeMetadata.CheckScalarType(idType);
                if (idScalarType == EnScalarType.None)
                {
                    this.generationErrors.Add(
                        $"Defined type of id should be scalar type on connection {connection.Name} of type {type.FullName}");
                    continue;
                }

                ApiType connectionApiType;
                if (!data.DiscoveredApiTypes.TryGetValue(connection.TypeName, out connectionApiType))
                {
                    this.generationErrors.Add(
                        $"Defined type of connection {connection.Name} of type {type.FullName} was not processed on previous steps");
                    continue;
                }

                if (attribute.CanCreate)
                {
                    // todo: generate resolver
                    var name = string.Join(".", new List<string>(path) { connection.Name, "create" });
                    yield return
                        ApiField.Object(
                            name,
                            connection.TypeName,
                            arguments:
                            new List<ApiField>
                                {
                                    ApiField.Object(
                                        "new",
                                        connection.TypeName,
                                        description: "The object's data")
                                },
                            description: attribute.CreateDescription);
                }

                if (attribute.CanUpdate)
                {
                    // todo: generate resolver
                    var name = string.Join(".", new List<string>(path) { connection.Name, "update" });
                    yield return
                        ApiField.Object(
                            name,
                            connection.TypeName,
                            arguments:
                            new List<ApiField>
                                {
                                    ApiField.Scalar("id", idScalarType, description: "The object's id"),
                                    ApiField.Object(
                                        "new",
                                        connection.TypeName,
                                        description: "The object's data")
                                },
                            description: attribute.CreateDescription);
                }

                if (attribute.CanCreate)
                {
                    // todo: generate resolver
                    var name = string.Join(".", new List<string>(path) { connection.Name, "delete" });
                    yield return
                        ApiField.Object(
                            name,
                            connection.TypeName,
                            arguments:
                            new List<ApiField> { ApiField.Scalar("id", idScalarType, description: "The object's id") },
                            description: attribute.CreateDescription);
                }
            }
        }

        /// <summary>
        /// Generate mutation from type fields
        /// </summary>
        /// <param name="apiType">The api description of the type</param>
        /// <param name="path">The type field path</param>
        /// <param name="typesUsed">Already used types to avoid circular references</param>
        /// <param name="data">
        /// The temporary data used during assemble process
        /// </param>
        /// <returns>The list of mutations</returns>
        private IEnumerable<ApiField> GenerateMutationsFromFields(
            ApiType apiType,
            List<string> path,
            List<string> typesUsed,
            AssembleTempData data)
        {
            var possibleMutationSubcontainers =
                apiType.Fields.Where(
                    f =>
                        !f.Arguments.Any() && f.ScalarType == EnScalarType.None
                        && !f.Flags.HasFlag(EnFieldFlags.IsArray) && !f.Flags.HasFlag(EnFieldFlags.IsConnection));

            foreach (var subContainer in possibleMutationSubcontainers)
            {
                if (typesUsed.Contains(subContainer.TypeName))
                {
                    // circular type use
                    this.generationWarnings.Add(
                        $"Circular property found for type {apiType.TypeName} field {subContainer.Name}");
                    continue;
                }

                ApiType subType;
                if (!data.DiscoveredApiTypes.TryGetValue(subContainer.TypeName, out subType))
                {
                    this.generationErrors.Add($"Could not find declared api type {subContainer.TypeName}");
                    continue;
                }

                var newPath = new List<string>(path) { subContainer.Name };
                var newTypesUsed = new List<string>(typesUsed) { subContainer.Name };

                foreach (var generateMutation in this.GenerateMutations(subType, newPath, newTypesUsed, data))
                {
                    yield return generateMutation;
                }
            }
        }

        /// <summary>
        /// Generates type description
        /// </summary>
        /// <param name="type">
        /// The type to describe
        /// </param>
        /// <param name="data">
        /// The temporary data used during assemble process.
        /// </param>
        /// <returns>
        /// The api type description
        /// </returns>
        private ApiType GenerateTypeDescription([NotNull] Type type, [NotNull] AssembleTempData data)
        {
            ApiType apiType;
            if (data.DiscoveredApiTypes.TryGetValue(type.FullName, out apiType))
            {
                return apiType;
            }

            var descriptionAttribute = (ApiDescriptionAttribute)type.GetCustomAttribute(typeof(ApiDescriptionAttribute));
            apiType = new ApiType(descriptionAttribute?.Name ?? type.FullName)
                          {
                              Description =
                                  descriptionAttribute?.Description
                          };

            data.DiscoveredTypes[apiType.TypeName] = type;
            data.DiscoveredApiTypes[apiType.TypeName] = apiType;

            apiType.Fields.AddRange(this.GenerateTypeProperties(type, apiType, data));
            apiType.Fields.AddRange(this.GenerateTypeMethods(type, apiType, data));

            return apiType;
        }

        /// <summary>
        /// Parses the metadata of returning type for the field
        /// </summary>
        /// <param name="type">The original field return type</param>
        /// <param name="attribute">The description attribute</param>
        /// <returns>The type metadata</returns>
        private TypeMetadata GenerateTypeMetadata(Type type, PublishToApiAttribute attribute)
        {
            var metadata = new TypeMetadata();

            var asyncType = TypeMetadata.CheckType(type, typeof(Task<>));
            if (asyncType != null)
            {
                metadata.IsAsync = true;
                type = asyncType.GenericTypeArguments[0];
            }

            if (attribute.ReturnType != null)
            {
                type = attribute.ReturnType;
                metadata.IsForwarding = true;
            }

            var scalarType = TypeMetadata.CheckScalarType(type);
            metadata.MetaType = TypeMetadata.EnMetaType.Scalar;
            if (scalarType == EnScalarType.None)
            {
                var enumerable = TypeMetadata.CheckType(type, typeof(IEnumerable<>));
                var connection = TypeMetadata.CheckType(type, typeof(INodeConnection<,>));

                if (connection != null)
                {
                    metadata.MetaType = TypeMetadata.EnMetaType.Connection;
                    type = connection.GenericTypeArguments[0];
                    scalarType = TypeMetadata.CheckScalarType(type);
                }
                else if (enumerable != null)
                {
                    metadata.MetaType = TypeMetadata.EnMetaType.Array;
                    type = enumerable.GenericTypeArguments[0];
                    scalarType = TypeMetadata.CheckScalarType(type);
                }
                else
                {
                    metadata.MetaType = TypeMetadata.EnMetaType.Object;
                }
            }

            // todo: check forwarding type
            metadata.ScalarType = scalarType;
            metadata.Type = type;
            return metadata;
        }

        /// <summary>
        /// Generate fields from type methods
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="apiType">The type api description</param>
        /// <param name="data">The temporary data used during assemble process</param>
        /// <returns>The field api description</returns>
        private IEnumerable<ApiField> GenerateTypeMethods(Type type, ApiType apiType, [NotNull] AssembleTempData data)
        {
            return
                type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Select(
                        p =>
                            new
                                {
                                    Method = p,
                                    Attribute =
                                    (DeclareFieldAttribute)p.GetCustomAttribute(typeof(DeclareFieldAttribute))
                                })
                    .Where(p => p.Attribute != null && !p.Method.IsGenericMethod && !p.Method.IsGenericMethodDefinition)
                    .Select(m => this.GenerateFieldFromMethod(m.Method, m.Attribute, data))
                    .Where(f => f != null);
        }

        /// <summary>
        /// Generate fields from type properties
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="apiType">The type api description</param>
        /// <param name="data">The temporary data used during assemble process</param>
        /// <returns>The field api description</returns>
        private IEnumerable<ApiField> GenerateTypeProperties(
            Type type,
            ApiType apiType,
            [NotNull] AssembleTempData data)
        {
            return
                type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(
                        p =>
                            new
                                {
                                    Property = p,
                                    Attribute =
                                    (DeclareFieldAttribute)p.GetCustomAttribute(typeof(DeclareFieldAttribute))
                                })
                    .Where(p => p.Attribute != null && p.Property.CanRead)
                    .Select(o => this.GenerateFieldFromProperty(o.Property, o.Attribute, data))
                    .Where(f => f != null);
        }

        /// <summary>
        /// The temporary data used during assemble process
        /// </summary>
        private class AssembleTempData
        {
            /// <summary>
            /// Gets the list of descriptions of discovered types used in API
            /// </summary>
            [NotNull]
            public Dictionary<string, ApiType> DiscoveredApiTypes { get; } = new Dictionary<string, ApiType>();

            /// <summary>
            /// Gets the list of descriptions of discovered types used in API
            /// </summary>
            [NotNull]
            public Dictionary<string, Type> DiscoveredTypes { get; } = new Dictionary<string, Type>();
        }

        /// <summary>
        /// Property or method return type description
        /// </summary>
        private class TypeMetadata
        {
            /// <summary>
            /// The return types of a field
            /// </summary>
            public enum EnMetaType
            {
                /// <summary>
                /// This is scalar field
                /// </summary>
                Scalar,

                /// <summary>
                /// The field is an object
                /// </summary>
                Object,

                /// <summary>
                /// The field is an array
                /// </summary>
                Array,

                /// <summary>
                /// The field represents connection
                /// </summary>
                Connection
            }

            /// <summary>
            /// Gets or sets a value indicating whether  this property / method has asynchronous access
            /// </summary>
            public bool IsAsync { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this property / method forwards resolve to other API provider
            /// </summary>
            public bool IsForwarding { get; set; }

            /// <summary>
            /// Gets or sets the meta type
            /// </summary>
            public EnMetaType MetaType { get; set; }

            /// <summary>
            /// Gets or sets the parsed scalar type
            /// </summary>
            public EnScalarType ScalarType { get; set; }

            /// <summary>
            /// Gets or sets the true returning type
            /// </summary>
            public Type Type { get; set; }

            /// <summary>
            /// Checks the type to be scalar
            /// </summary>
            /// <param name="type">The type</param>
            /// <returns>The corresponding scalar type</returns>
            public static EnScalarType CheckScalarType(Type type)
            {
                var nullable = CheckType(type, typeof(Nullable<>));
                if (nullable != null)
                {
                    type = nullable.GenericTypeArguments[0];
                }

                if (type == typeof(string))
                {
                    return EnScalarType.String;
                }

                if (type == typeof(bool))
                {
                    return EnScalarType.Boolean;
                }

                if (type == typeof(Guid))
                {
                    return EnScalarType.Guid;
                }

                if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(uint)
                    || type == typeof(ulong) || type == typeof(ushort))
                {
                    return EnScalarType.Integer;
                }

                if (type == typeof(float) || type == typeof(double))
                {
                    return EnScalarType.Float;
                }

                if (type == typeof(decimal))
                {
                    return EnScalarType.Decimal;
                }

                return EnScalarType.None;
            }

            /// <summary>
            /// Checks the interface implementation
            /// </summary>
            /// <param name="type">The type to check</param>
            /// <param name="typeToCheck">The type of an interface</param>
            /// <returns>The interface or null</returns>
            public static Type CheckType(Type type, Type typeToCheck)
            {
                if (type == typeToCheck)
                {
                    return type;
                }

                if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeToCheck)
                {
                    return type;
                }

                foreach (var @interface in type.GetInterfaces())
                {
                    if (@interface == typeToCheck)
                    {
                        return @interface;
                    }

                    if (@interface.IsConstructedGenericType && @interface.GetGenericTypeDefinition() == typeToCheck)
                    {
                        return @interface;
                    }
                }

                foreach (var baseType in GetBaseTypes(type))
                {
                    if (baseType == typeToCheck)
                    {
                        return baseType;
                    }

                    if (baseType.IsConstructedGenericType && baseType.GetGenericTypeDefinition() == typeToCheck)
                    {
                        return baseType;
                    }
                }

                return null;
            }

            /// <summary>
            /// Gets the <see cref="EnFieldFlags"/> from metadata
            /// </summary>
            /// <returns>The field flags</returns>
            public EnFieldFlags GetFlags()
            {
                switch (this.MetaType)
                {
                    case EnMetaType.Object:
                        return EnFieldFlags.None;
                    case EnMetaType.Array:
                        return EnFieldFlags.IsArray;
                    case EnMetaType.Connection:
                        return EnFieldFlags.IsConnection;
                    default:
                        // todo: report error
                        return EnFieldFlags.None;
                }
            }

            /// <summary>
            /// Gets the base type hierarchy
            /// </summary>
            /// <param name="type">The type</param>
            /// <returns>The list of base types</returns>
            private static IEnumerable<Type> GetBaseTypes(Type type)
            {
                if (type.BaseType == null)
                {
                    yield break;
                }

                yield return type.BaseType;
                foreach (var baseType in GetBaseTypes(type.BaseType))
                {
                    yield return baseType;
                }
            }
        }
    }
}