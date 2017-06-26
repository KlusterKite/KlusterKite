// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Extensions.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The extension methods
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Provider.Resolvers
{
    using System;

    /// <summary>
    /// The extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Creates an instance from type
        /// </summary>
        /// <typeparam name="T">The expected type</typeparam>
        /// <param name="type">The instance type</param>
        /// <returns>The new type instance</returns>
        public static T CreateInstance<T>(this Type type)
        {
            return (T)Activator.CreateInstance(type);
        }
    }
}
