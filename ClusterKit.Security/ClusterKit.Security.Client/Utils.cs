// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Utils.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The budle of utils for current library
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Security.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using ClusterKit.Security.Client.Attributes;

    /// <summary>
    /// The bundle of utils for current library
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Gets the defined list of privileges
        /// </summary>
        public static IReadOnlyList<PrivilegeDescription> DefinedPrivileges { get; private set; } = new List<PrivilegeDescription>().AsReadOnly();

        /// <summary> 
        /// Scans current configuration for defined privileges
        /// </summary>
        internal static void CreatePrivilegesCache()
        {
            DefinedPrivileges =
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(
                        a =>
                            a.GetTypes()
                                .Where(t => t.IsAbstract && t.IsSealed)
                                .Where(t => t.GetCustomAttributes(typeof(PrivilegesContainerAttribute), true).Length > 0)
                                .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
                                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                                .Select(f => new PrivilegeDescription(
                                    a.GetName().Name, 
                                    ((PrivilegeDescriptionAttribute)f.GetCustomAttribute(typeof(PrivilegeDescriptionAttribute)))?.Description,
                                    (string)f.GetValue(null))))
                    .ToList()
                    .AsReadOnly();
        }
    }
}
