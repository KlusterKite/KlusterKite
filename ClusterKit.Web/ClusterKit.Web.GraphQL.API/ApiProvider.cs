// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiProvider.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the ApiProvider type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.API
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using ClusterKit.Web.GraphQL.Client;
    using ClusterKit.Web.GraphQL.Client.Attributes;

    /// <summary>
    /// Declares some api to be provided
    /// </summary>
    public abstract class ApiProvider
    {
        /// <summary>
        /// The list of discovered type methods used in API
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, MethodInfo>> discoveredMethods =
            new Dictionary<string, Dictionary<string, MethodInfo>>();

        /// <summary>
        /// The list of discovered type properties used in API
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, PropertyInfo>> discoveredProperties =
            new Dictionary<string, Dictionary<string, PropertyInfo>>();

        /// <summary>
        /// The list of discovered types used in API
        /// </summary>
        private readonly Dictionary<string, Type> discoveredTypes = new Dictionary<string, Type>();

        /// <summary>
        /// The list of discovered mutations in API
        /// </summary>
        private readonly Dictionary<string, MethodInfo> discoveredMutations = new Dictionary<string, MethodInfo>();

        /// <summary>
        /// The api description
        /// </summary>
        private ApiDescription apiDescription;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiProvider"/> class.
        /// </summary>
        protected ApiProvider()
        {
            this.GenerateApiDescription();
        }

        /// <summary>
        /// Gets the api description
        /// </summary>
        public ApiDescription ApiDescription => this.apiDescription;

        /// <summary>
        /// Checks the type to be scalar
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>The corresponding scalar type</returns>
        private EnScalarType CheckScalarType(Type type)
        {
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
        private Type CheckType(Type type, Type typeToCheck)
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

            foreach (var baseType in this.GetBaseTypes(type))
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
        /// Generates the api description
        /// </summary>
        private void GenerateApiDescription()
        {
            this.discoveredTypes.Clear();
            this.discoveredMethods.Clear();
            this.discoveredProperties.Clear();
            this.discoveredMutations.Clear();

            this.apiDescription = new ApiDescription { Version = this.GetType().Assembly.GetName().Version };

            var queryType = this.GenerateTypeDescription(this.apiDescription, this.GetType());
            this.apiDescription.Types.Remove(queryType);
            this.apiDescription.Fields = queryType.Fields;
            this.apiDescription.ApiName = queryType.TypeName;
            this.apiDescription.Description = queryType.Description;
            this.apiDescription.Mutations =
                new List<ApiField>(this.GenerateMutations(queryType, new List<string>(), new List<string> { queryType.TypeName }));
        }

        /// <summary>
        /// Generates mutations
        /// </summary>
        /// <param name="apiType">The current api type</param>
        /// <param name="path">The path to current field</param>
        /// <param name="typesUsed">The types already used to avoid circular references</param>
        /// <returns>The list of mutations</returns>
        private IEnumerable<ApiField> GenerateMutations(ApiType apiType, List<string> path, List<string> typesUsed)
        {
            Type type;
            if (!this.discoveredTypes.TryGetValue(apiType.TypeName, out type))
            {
                yield break;
            }

            foreach (var mutation in this.GenerateDirectMutations(type, path))
            {
                yield return mutation;
            }

            foreach (var mutation in this.GenerateFieldsMutations(apiType, path, typesUsed))
            {
                yield return mutation;
            }

            var connections =
                apiType.Fields.Where(
                    f => !f.Arguments.Any() && f.ScalarType == EnScalarType.None && f.Flags.HasFlag(EnFieldFlags.IsConnection));

            foreach (var connection in connections)
            {
                var propertyInfo = this.discoveredProperties[apiType.TypeName][connection.Name];
                var idType =
                    this.CheckType(propertyInfo.PropertyType, typeof(INodeConnection<,>))?.GenericTypeArguments[1];
                var connectionType = this.apiDescription.Types.FirstOrDefault(t => t.TypeName == connection.TypeName);

                if (idType == null && connectionType == null)
                {
                    // todo: report error
                    continue;
                }

                var idScalarType = this.CheckScalarType(idType);

                if (idScalarType == EnScalarType.None)
                {
                    // todo: report error
                    continue;
                }

                var attribute =
                    propertyInfo.GetCustomAttribute(typeof(DeclareConnectionAttribute)) as DeclareConnectionAttribute;

                if (attribute == null)
                {
                    continue;
                }

                if (attribute.CanCreate)
                {
                    var name = string.Join(".", new List<string>(path) { connection.Name, "create" });
                    yield return ApiField.Object(
                        name, 
                        connection.TypeName, 
                        arguments: new List<ApiField> { ApiField.Object("new", connection.TypeName, description: "The object's data") },
                        description: attribute.CreateDescription);
                }

                if (attribute.CanUpdate)
                {
                    var name = string.Join(".", new List<string>(path) { connection.Name, "update" });
                    yield return ApiField.Object(
                        name,
                        connection.TypeName,
                        arguments: new List<ApiField> { ApiField.Scalar("id", idScalarType, description: "The object's id"), ApiField.Object("new", connection.TypeName, description: "The object's data") },
                        description: attribute.CreateDescription);
                }

                if (attribute.CanCreate)
                {
                    var name = string.Join(".", new List<string>(path) { connection.Name, "delete" });
                    yield return ApiField.Object(
                        name,
                        connection.TypeName,
                        arguments: new List<ApiField> { ApiField.Scalar("id", idScalarType, description: "The object's id") },
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
        /// <returns>The list of mutations</returns>
        private IEnumerable<ApiField> GenerateFieldsMutations(ApiType apiType, List<string> path, List<string> typesUsed)
        {
            var possibleMutationSubcontainers =
                apiType.Fields.Where(
                    f =>
                        !f.Arguments.Any() && f.ScalarType == EnScalarType.None && !f.Flags.HasFlag(EnFieldFlags.IsArray)
                        && !f.Flags.HasFlag(EnFieldFlags.IsConnection));

            foreach (var subContainer in possibleMutationSubcontainers)
            {
                if (typesUsed.Contains(subContainer.TypeName))
                {
                    // circular type use
                    continue;
                }

                var subType = this.apiDescription.Types.FirstOrDefault(t => t.TypeName == subContainer.TypeName);
                if (subType == null)
                {
                    // todo: report error
                    continue;
                }

                var newPath = new List<string>(path) { subContainer.Name };
                var newTypesUsed = new List<string>(typesUsed) { subContainer.Name };

                foreach (var generateMutation in this.GenerateMutations(subType, newPath, newTypesUsed))
                {
                    yield return generateMutation;
                }
            }
        }

        /// <summary>
        /// Generate mutations directly declared in the type
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="path">The fields path</param>
        /// <returns>The list of mutations</returns>
        private IEnumerable<ApiField> GenerateDirectMutations(Type type, List<string> path)
        {
            var methods =
                type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Select(
                        p =>
                            new
                                {
                                    Method = p,
                                    Attribute = (DeclareMutationAttribute)p.GetCustomAttribute(typeof(DeclareMutationAttribute))
                                })
                    .Where(p => p.Attribute != null && !p.Method.IsGenericMethod && !p.Method.IsGenericMethodDefinition)
                    .ToList();

            foreach (var method in methods)
            {
                var field = this.GeneratePropertyDescription(
                    this.apiDescription,
                    method.Method.ReturnType,
                    method.Attribute,
                    method.Method.Name);

                if (field == null)
                {
                    continue;
                }

                foreach (var parameterInfo in method.Method.GetParameters())
                {
                    var argument = this.GeneratePropertyDescription(
                        this.apiDescription,
                        parameterInfo.ParameterType,
                        (ApiDescriptionAttribute)parameterInfo.GetCustomAttribute(typeof(ApiDescriptionAttribute)) ?? new ApiDescriptionAttribute(),
                        parameterInfo.Name);

                    field.Arguments.Add(argument);
                }

                var newPath = new List<string>(path) { field.Name };
                field.Name = string.Join(".", newPath);

                this.discoveredMutations[field.Name] = method.Method;
                yield return field;
            }
        }

        /// <summary>
        /// Generates field description
        /// </summary>
        /// <param name="api">The global api description</param>
        /// <param name="propertyType">The type of property</param>
        /// <param name="attribute">The description attribute</param>
        /// <param name="originalPropertyName">The original property name</param>
        /// <returns>The generated field</returns>
        private ApiField GeneratePropertyDescription(
            ApiDescription api,
            Type propertyType,
            ApiDescriptionAttribute attribute,
            string originalPropertyName)
        {
            var flags = EnFieldFlags.None;
            var scalarType = this.CheckScalarType(propertyType);

            if (scalarType == EnScalarType.None)
            {
                var asyncType = this.CheckType(propertyType, typeof(Task<>));
                if (asyncType != null)
                {
                    propertyType = asyncType.GenericTypeArguments[0];
                }

                var enumerable = this.CheckType(propertyType, typeof(IEnumerable<>));
                var connection = this.CheckType(propertyType, typeof(INodeConnection<,>));

                if (connection != null)
                {
                    flags |= EnFieldFlags.IsConnection;
                    propertyType = connection.GenericTypeArguments[0];
                    scalarType = this.CheckScalarType(propertyType);
                }
                else if (enumerable != null)
                {
                    flags |= EnFieldFlags.IsArray;
                    propertyType = enumerable.GenericTypeArguments[0];
                    scalarType = this.CheckScalarType(propertyType);
                }

                var nullable = this.CheckType(propertyType, typeof(Nullable<>));
                if (nullable != null)
                {
                    propertyType = nullable.GenericTypeArguments[0];
                    scalarType = this.CheckScalarType(propertyType);
                }
            }

            var name = attribute.Name ?? this.ToCamelCase(originalPropertyName);
            if (scalarType != EnScalarType.None)
            {
                return ApiField.Scalar(name, scalarType, flags, description: attribute.Description);
            }

            var type = this.GenerateTypeDescription(api, propertyType);
            return ApiField.Object(name, type.TypeName, flags, description: attribute.Description);
        }

        /// <summary>
        /// Generates type description
        /// </summary>
        /// <param name="api">The overall api description</param>
        /// <param name="type">The type to describe</param>
        /// <returns>The api type description</returns>
        private ApiType GenerateTypeDescription(ApiDescription api, Type type)
        {
            var apiType = api.Types.FirstOrDefault(t => t.TypeName == type.FullName);
            if (apiType != null)
            {
                return apiType;
            }

            var descriptionAttribute = (ApiDescriptionAttribute)type.GetCustomAttribute(typeof(ApiDescriptionAttribute));
            apiType = new ApiType(descriptionAttribute?.Name ?? type.FullName)
                          {
                              Description =
                                  descriptionAttribute?.Description
                          };

            this.discoveredTypes[apiType.TypeName] = type;
            this.discoveredProperties[apiType.TypeName] = new Dictionary<string, PropertyInfo>();
            this.discoveredMethods[apiType.TypeName] = new Dictionary<string, MethodInfo>();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(
                    p =>
                        new
                            {
                                Property = p,
                                Attribute = (PublishToApiAttribute)p.GetCustomAttribute(typeof(PublishToApiAttribute))
                            })
                .Where(p => p.Attribute != null && p.Property.CanRead)
                .Select(
                    p =>
                        {
                            var propertyType = this.GeneratePropertyDescription(
                                api,
                                p.Property.PropertyType,
                                p.Attribute,
                                p.Property.Name);

                            if (propertyType != null)
                            {
                                this.discoveredProperties[apiType.TypeName][propertyType.Name] = p.Property;
                            }

                            return propertyType;
                        }).Where(f => f != null);

            apiType.Fields.AddRange(properties);

            var methods =
                type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Select(
                        p =>
                            new
                                {
                                    Method = p,
                                    Attribute =
                                    (PublishToApiAttribute)p.GetCustomAttribute(typeof(PublishToApiAttribute))
                                })
                    .Where(p => p.Attribute != null && !p.Method.IsGenericMethod && !p.Method.IsGenericMethodDefinition)
                    .ToList();

            foreach (var method in methods)
            {
                var field = this.GeneratePropertyDescription(
                    api,
                    method.Method.ReturnType,
                    method.Attribute,
                    method.Method.Name);

                if (field == null)
                {
                    continue;
                }

                foreach (var parameterInfo in method.Method.GetParameters())
                {
                    var argument = this.GeneratePropertyDescription(
                        api,
                        parameterInfo.ParameterType,
                        (ApiDescriptionAttribute)parameterInfo.GetCustomAttribute(typeof(ApiDescriptionAttribute)) ?? new ApiDescriptionAttribute(),
                        parameterInfo.Name);

                    field.Arguments.Add(argument);
                }

                this.discoveredMethods[apiType.TypeName][field.Name] = method.Method;
                apiType.Fields.Add(field);
            }

            api.Types.Add(apiType);
            return apiType;
        }

        /// <summary>
        /// Gets the base type hierarchy
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>The list of base types</returns>
        private IEnumerable<Type> GetBaseTypes(Type type)
        {
            if (type.BaseType == null)
            {
                yield break;
            }

            yield return type.BaseType;
            foreach (var baseType in this.GetBaseTypes(type.BaseType))
            {
                yield return baseType;
            }
        }

        /// <summary>
        /// Converts string to camel case
        /// </summary>
        /// <param name="name">The property / method name</param>
        /// <returns>The name in camel case</returns>
        private string ToCamelCase(string name)
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
    }
}