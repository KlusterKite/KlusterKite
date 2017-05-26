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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    using ClusterKit.API.Attributes;
    using ClusterKit.API.Client;

    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Creates a EF query based on <see cref="ApiRequest"/>
    /// </summary>
    public static class SelectBuilder
    {
        /// <summary>
        /// Creates includes to the query according to the api request fields
        /// </summary>
        /// <typeparam name="T">The type of queried object</typeparam>
        /// <typeparam name="TContext">The type of context</typeparam>
        /// <param name="query">The original query</param>
        /// <param name="context">The data context</param>
        /// <param name="request">The request</param>
        /// <returns>Modified query</returns>
        public static IQueryable<T> SetIncludes<T, TContext>(
            this IQueryable<T> query,
            TContext context,
            ApiRequest request)
            where T : class where TContext : DbContext
        {
            var paths = PathCreator<T, TContext>.CreatePaths(null, context, request).Distinct().ToList();
            var reducedPaths = paths.Where(p => !paths.Any(op => op.StartsWith(p) && op != p)).ToList();

            return reducedPaths.Aggregate(query, (current, reducedPath) => current.Include(reducedPath));
        }

        /// <summary>
        /// Creates paths for type 
        /// </summary>
        /// <typeparam name="T">The type of an object</typeparam>
        /// <typeparam name="TContext">The type of context</typeparam>
        [SuppressMessage("ReSharper", "StaticMemberInGenericType", Justification = "Making use of static generic classes")]
        private static class PathCreator<T, TContext> where TContext : DbContext
        {
            /// <summary>
            /// The internal lock
            /// </summary>
            private static readonly object LockObject = new object();

            /// <summary>
            /// The list of cached helpers
            /// </summary>
            private static readonly Dictionary<string, PropertyDescription<TContext>> NavigationProperties
                = new Dictionary<string, PropertyDescription<TContext>>();

            /// <summary>
            /// A value indicating whether the type was initialized
            /// </summary>
            private static bool isInitialized;

            /// <summary>
            /// Creates the list of paths
            /// </summary>
            /// <param name="prefix">The path prefix</param>
            /// <param name="context">The data context</param>
            /// <param name="request">The api request</param>
            /// <returns>The list of paths</returns>
            public static IEnumerable<string> CreatePaths(string prefix, TContext context, ApiRequest request)
            {
                Initialize(context);

                if (request?.Fields == null)
                {
                    yield break;
                }

                foreach (var field in request.Fields)
                {
                    PropertyDescription<TContext> description;
                    if (!NavigationProperties.TryGetValue(field.FieldName, out description))
                    {
                        continue;
                    }

                    var currentPath = string.IsNullOrWhiteSpace(prefix)
                                          ? description.PropertyName
                                          : $"{prefix}.{description.PropertyName}";

                    yield return currentPath;
                    foreach (var nestedPath in description.CreatePaths(currentPath, context, field))
                    {
                        yield return nestedPath;
                    }
                }
            }

            /// <summary>
            /// Initializes static members of the <see cref="PathCreator{T, TContext}"/> class.
            /// </summary>
            /// <param name="context">
            /// The data context.
            /// </param>
            private static void Initialize(TContext context)
            {
                if (isInitialized)
                {
                    return;
                }

                lock (LockObject)
                {
                    if (isInitialized)
                    {
                        return;
                    }

                    isInitialized = true;

                    var entityType = context.Model.FindEntityType(typeof(T));
                    if (entityType == null)
                    {
                        return;
                    }

                    foreach (var navigation in entityType.GetNavigations())
                    {
                        var declareFieldAttribute = navigation.PropertyInfo.GetCustomAttribute<DeclareFieldAttribute>();
                        var propertyName = declareFieldAttribute?.Name
                                           ?? ApiDescriptionAttribute.ToCamelCase(navigation.PropertyInfo.Name);

                        var propertyType = navigation.PropertyInfo.PropertyType.GetInterfaces()
                                               .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                                               ?.GetGenericArguments()[0] ?? navigation.PropertyInfo.PropertyType;

                        NavigationProperties[propertyName] = new PropertyDescription<TContext>(navigation.Name, propertyType);
                    }
                }
            }
        }

        /// <summary>
        /// The property description
        /// </summary>
        /// <typeparam name="TContext">The type of data context</typeparam>
        private class PropertyDescription<TContext> where TContext : DbContext
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
            private Func<string, TContext, ApiRequest, IEnumerable<string>> createPathsFunc;

            /// <summary>
            /// Initializes a new instance of the <see cref="PropertyDescription{TContext}"/> class.
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
            /// <param name="prefix">
            /// The path prefix
            /// </param>
            /// <param name="context">
            /// The context.
            /// </param>
            /// <param name="request">
            /// The api request
            /// </param>
            /// <returns>
            /// The list of discovered includes
            /// </returns>
            public IEnumerable<string> CreatePaths(string prefix, TContext context, ApiRequest request)
            {
                if (this.createPathsFunc == null)
                {
                    lock (this.lockObject)
                    {
                        if (this.createPathsFunc == null)
                        {
                            var creatorType = typeof(PathCreator<,>).MakeGenericType(this.propertyType, typeof(TContext));
                            var func = creatorType.GetMethod("CreatePaths", BindingFlags.Static | BindingFlags.Public);
                            this.createPathsFunc =
                                (Func<string, TContext, ApiRequest, IEnumerable<string>>)
                                func.CreateDelegate(typeof(Func<string, TContext, ApiRequest, IEnumerable<string>>));
                        }
                    }
                }

                return this.createPathsFunc(prefix, context, request);
            }
        }
    }
}
