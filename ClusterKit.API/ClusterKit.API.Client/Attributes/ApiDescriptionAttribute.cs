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
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter | AttributeTargets.Enum)]
    public class ApiDescriptionAttribute : Attribute
    {
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
            return attr?.GetName(type) ?? NamingUtilities.ToCSharpRepresentation(type);
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