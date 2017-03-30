// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiDescriptionAttribute.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Describes type (class) to published api
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client.Attributes
{
    using System;
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
        /// Gets or sets the published property / method name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description to publish
        /// </summary>
        public string Description { get; set; }

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
        /// Creates name for the type 
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>The type name</returns>
        public static string GetTypeName(Type type)
        {
            var attr = type.GetCustomAttribute<ApiDescriptionAttribute>();
            var name = attr?.GetName(type) ?? NamingUtilities.ToCSharpRepresentation(type);
            return type.IsGenericType && !string.IsNullOrWhiteSpace(attr?.GetName(type))
                ? $"{name}<{string.Join(",", type.GetGenericArguments().Select(GetTypeName))}>"
                : name;
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
    }
}