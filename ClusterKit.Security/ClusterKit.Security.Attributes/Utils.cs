// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Utils.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The budle of utils for current library
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Security.Attributes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

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
        /// Gets the list of defined privileges
        /// </summary>
        /// <param name="container">The privilege type container</param>
        /// <returns>The list of privilege descriptions</returns>
        public static IEnumerable<PrivilegeDescription> GetDefinedPrivileges(Type container)
        {
            if (container.GetCustomAttribute(typeof(PrivilegesContainerAttribute), true) == null)
            {
                return new PrivilegeDescription[0];
            }

            return SearchDefinedPrivileges(container)
                    .OrderBy(d => d.Privilege)
                    .ThenBy(d => d.Action)
                    .ToArray();
        }

        /// <summary> 
        /// Scans current configuration for defined privileges
        /// </summary>
        public static void CreatePrivilegesCache()
        {
            DefinedPrivileges =
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(
                        a =>
                            {
                                try
                                {
                                    return
                                        a.GetTypes()
                                            .Where(
                                                t =>
                                                    t.IsAbstract && t.IsSealed
                                                    && t.GetCustomAttribute(typeof(PrivilegesContainerAttribute), true)
                                                    != null)
                                            .SelectMany(SearchDefinedPrivileges);
                                }
                                catch (Exception e)
                                {
                                    throw new Exception($"Could not get types from module {a.FullName}", e);
                                }
                            })
                    .OrderBy(d => d.AssemblyName)
                    .ThenBy(d => d.Privilege)
                    .ThenBy(d => d.Action)
                    .ToList()
                    .AsReadOnly();
        }

        /// <summary>
        /// Gets the list of defined privileges
        /// </summary>
        /// <param name="container">The privilege type container</param>
        /// <returns>The list of privilege descriptions</returns>
        private static IEnumerable<PrivilegeDescription> SearchDefinedPrivileges(Type container)
        {
            return
                container.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                    .SelectMany(
                        f =>
                            {
                                var descriptionAttribute =
                                    (PrivilegeDescriptionAttribute)
                                    f.GetCustomAttribute(typeof(PrivilegeDescriptionAttribute));
                                if (descriptionAttribute == null)
                                {
                                    return new[]
                                               {
                                                   new PrivilegeDescription(
                                                       container.Assembly.GetName().Name,
                                                       null,
                                                       (string)f.GetValue(null))
                                               };
                                }

                                if (descriptionAttribute.Actions != null && descriptionAttribute.Actions.Length > 0)
                                {
                                    return
                                        descriptionAttribute.Actions.Select(
                                            action =>
                                                new PrivilegeDescription(
                                                    container.Assembly.GetName().Name,
                                                    descriptionAttribute.Description,
                                                    $"{f.GetValue(null)}.{action}",
                                                    action,
                                                    descriptionAttribute.Target));
                                }

                                return new[]
                                           {
                                               new PrivilegeDescription(
                                                   container.Assembly.GetName().Name,
                                                   descriptionAttribute.Description,
                                                   (string)f.GetValue(null),
                                                   target: descriptionAttribute.Target)
                                           };
                            });
        } 
    }
}
