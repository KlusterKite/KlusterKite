// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectBuilder.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Creates a EF query based on <see cref="ApiRequest" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.EF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    using ClusterKit.API.Client;
    using ClusterKit.API.Client.Attributes;

    /// <summary>
    /// Creates a EF query based on <see cref="ApiRequest"/>
    /// </summary>
    public static class SelectBuilder
    {
        /// <summary>
        /// Creates includes to the query according to the api request fields
        /// </summary>
        /// <typeparam name="T">The type of queried object</typeparam>
        /// <param name="query">The original query</param>
        /// <param name="request">The request</param>
        /// <returns>Modified query</returns>
        public static IQueryable<T> SetIncludes<T>(this IQueryable<T> query, ApiRequest request)
        {
            var paths = PathCreator<T>.CreatePaths(null, request).Distinct().ToList();
            var reducedPaths = paths.Where(p => !paths.Any(op => op.StartsWith(p) && op != p)).ToList();

            foreach (var reducedPath in reducedPaths)
            {
                query = query.Include(reducedPath);
            }

            return query;
        }

        /// <summary>
        /// Creates paths for type
        /// </summary>
        /// <typeparam name="T">The type of an object</typeparam>
        private static class PathCreator<T>
        {
            /// <summary>
            /// The list of cached helpers
            /// </summary>
            [SuppressMessage("ReSharper", "StaticMemberInGenericType", Justification = "Making use of static fields of generic classes")]
            private static readonly Dictionary<string, PropertyDescription> CollectionProperties
                = new Dictionary<string, PropertyDescription>();

            /// <summary>
            /// Initializes static members of the <see cref="PathCreator{T}"/> class.
            /// </summary>
            static PathCreator()
            {
                var collectionProperties =
                    typeof(T).GetProperties(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty
                        | BindingFlags.SetProperty)
                        .Where(p =>
                            p.PropertyType.GetInterfaces()
                                .Any(
                                    i =>
                                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                                        && i.GetGenericArguments()[0].IsClass))
                        .Where(p => p.GetCustomAttribute<DeclareFieldAttribute>() != null 
                        && p.GetCustomAttribute<NotMappedAttribute>() == null).ToList();

                foreach (var propertyInfo in collectionProperties)
                {
                    var declareFieldAttribute = propertyInfo.GetCustomAttribute<DeclareFieldAttribute>();
                    var propertyName = declareFieldAttribute.Name
                                       ?? ApiDescriptionAttribute.ToCamelCase(propertyInfo.Name);

                    var propertyType =
                        propertyInfo.PropertyType.GetInterfaces()
                            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                            .GetGenericArguments()[0];

                    var description = new PropertyDescription(propertyInfo.Name, propertyType);
                    CollectionProperties[propertyName] = description;
                }
            }

            /// <summary>
            /// Creates the list of paths
            /// </summary>
            /// <param name="prefix">The path prefix</param>
            /// <param name="request">The api request</param>
            /// <returns>The list of paths</returns>
            public static IEnumerable<string> CreatePaths(string prefix, ApiRequest request)
            {
                if (request?.Fields == null)
                {
                    yield break;
                }

                foreach (var field in request.Fields)
                {
                    PropertyDescription description;
                    if (!CollectionProperties.TryGetValue(field.FieldName, out description))
                    {
                        continue;
                    }

                    var currentPath = string.IsNullOrWhiteSpace(prefix)
                                          ? description.PropertyName
                                          : $"{prefix}.{description.PropertyName}";

                    yield return currentPath;
                    foreach (var nestedPath in description.CreatePaths(currentPath, field))
                    {
                        yield return nestedPath;
                    }
                }
            }
        }

        /// <summary>
        /// The property description
        /// </summary>
        private class PropertyDescription
        {
            /// <summary>
            /// the property type
            /// </summary>
            private readonly Type propertyType;

            /// <summary>
            /// Internal lock object to make thread-safe initialization
            /// </summary>
            private readonly object lockObject = new object();

            /// <summary>
            /// The resolved path creation function
            /// </summary>
            private Func<string, ApiRequest, IEnumerable<string>> createPathsFunc;

            /// <summary>
            /// Initializes a new instance of the <see cref="PropertyDescription"/> class.
            /// </summary>
            /// <param name="propertyName">
            /// The property name.
            /// </param>
            /// <param name="propertyType">
            /// The property type.
            /// </param>
            public PropertyDescription(string propertyName, Type propertyType)
            {
                this.PropertyName = propertyName;
                this.propertyType = propertyType;
            }

            /// <summary>
            /// Gets the property type
            /// </summary>
            public string PropertyName { get; }

            /// <summary>
            /// Calls the create path function for current property
            /// </summary>
            /// <param name="prefix">The path prefix</param>
            /// <param name="request">The api request</param>
            /// <returns>The list of discovered includes</returns>
            public IEnumerable<string> CreatePaths(string prefix, ApiRequest request)
            {
                if (this.createPathsFunc == null)
                {
                    lock (this.lockObject)
                    {
                        if (this.createPathsFunc == null)
                        {
                            var creatorType = typeof(PathCreator<>).MakeGenericType(this.propertyType);
                            var func = creatorType.GetMethod("CreatePaths", BindingFlags.Static | BindingFlags.Public);
                            this.createPathsFunc =
                                (Func<string, ApiRequest, IEnumerable<string>>)
                                func.CreateDelegate(typeof(Func<string, ApiRequest, IEnumerable<string>>));
                        }
                    }
                }

                return this.createPathsFunc(prefix, request);
            }
        }
    }
}
