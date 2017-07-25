// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiDescriptionAttribute.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Describes type (class) to published api
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Attributes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Describes type (class) to published api
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter | AttributeTargets.Enum | AttributeTargets.Field)]
    public class ApiDescriptionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiDescriptionAttribute"/> class.
        /// </summary>
        public ApiDescriptionAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiDescriptionAttribute"/> class.
        /// </summary>
        /// <param name="description">
        /// The description.
        /// </param>
        public ApiDescriptionAttribute(string description)
        {
            this.Description = description;
        }

        /// <summary>
        /// Gets or sets the description to publish
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the published property / method name
        /// </summary>
        public string Name { get; set; }

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
        /// Creates name for the method parameter 
        /// </summary>
        /// <param name="parameter">The type</param>
        /// <returns>The type name</returns>
        public static string GetParameterName(ParameterInfo parameter)
        {
            var attr = parameter.GetCustomAttribute<ApiDescriptionAttribute>();
            return attr?.Name ?? ToCamelCase(parameter.Name);
        }

        /// <summary>
        /// Creates name for the type 
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>The type name</returns>
        public static string GetTypeName(Type type)
        {
            var listType = CheckType(type, typeof(List<>));
            if (listType != null)
            {
                return $"List<{GetTypeName(listType.GenericTypeArguments[0])}>";
            }

            var attr = type.GetTypeInfo().GetCustomAttribute<ApiDescriptionAttribute>();
            var name = attr?.GetName(type) ?? NamingUtilities.ToCSharpRepresentation(type);
            return type.GetTypeInfo().IsGenericType && !string.IsNullOrWhiteSpace(attr?.GetName(type))
                       ? $"{name}<{string.Join(",", type.GetGenericArguments().Select(GetTypeName))}>"
                       : name;
        }

        /// <summary>
        /// Converts string to camel case
        /// </summary>
        /// <param name="name">The property / method name</param>
        /// <returns>The name in camel case</returns>
        public static string ToCamelCase(string name)
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
        /// Creates name for the type field
        /// </summary>
        /// <param name="member">The type member</param>
        /// <returns>The member name</returns>
        public string GetName(MemberInfo member)
        {
            return this.Name ?? ToCamelCase(member.Name);
        }

        /// <summary>
        /// Creates name for the type 
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>The type name</returns>
        public string GetName(Type type)
        {
            return this.Name ?? NamingUtilities.ToCSharpRepresentation(type);
        }

        /// <summary>
        /// Gets the base type hierarchy
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>The list of base types</returns>
        private static IEnumerable<Type> GetBaseTypes(Type type)
        {
            if (type.GetTypeInfo().BaseType == null)
            {
                yield break;
            }

            yield return type.GetTypeInfo().BaseType;
            foreach (var baseType in GetBaseTypes(type.GetTypeInfo().BaseType))
            {
                yield return baseType;
            }
        }
    }
}