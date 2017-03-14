// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataUpdater.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Utility to update class according to API mutation request
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using ClusterKit.API.Client.Attributes;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Utility to update class according to API mutation request
    /// </summary>
    /// <typeparam name="TObject">The type of object to update</typeparam>
    public static class DataUpdater<TObject>
    {
        /// <summary>
        /// The list of discovered property copiers
        /// </summary>
        private static readonly Dictionary<string, Action<TObject, TObject>> PropertyCopiers
            = new Dictionary<string, Action<TObject, TObject>>();

        /// <summary>
        /// Initializes static members of the <see cref="DataUpdater{TObject}"/> class.
        /// </summary>
        static DataUpdater()
        {
            var properties =
                typeof(TObject).GetProperties(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);

            foreach (var property in properties)
            {
                var publishAttribute = property.GetCustomAttribute<DeclareFieldAttribute>();
                if (publishAttribute == null || !publishAttribute.Access.HasFlag(EnAccessFlag.Writable))
                {
                    continue;
                }

                var propertyName = publishAttribute.Name ?? ApiDescriptionAttribute.ToCamelCase(property.Name);
                var destination = Expression.Parameter(typeof(TObject), "d");
                var source = Expression.Parameter(typeof(TObject), "s");
                var copy = Expression.Assign(
                    Expression.Property(destination, property),
                    Expression.Property(source, property));
                
                var lambda = Expression.Lambda<Action<TObject, TObject>>(
                    copy, 
                    destination, 
                    source);
                PropertyCopiers[propertyName] = lambda.Compile();
            }
        }

        /// <summary>
        /// Performs data copy from source to destination fields
        /// </summary>
        /// <param name="destination">The object to update</param>
        /// <param name="source">The object containing new data</param>
        /// <param name="request">The original api request used to update an object</param>
        public static void Update(TObject destination, TObject source, ApiRequest request)
        {
            var fieldsModified =
                (((JObject)request.Arguments).Property("newNode")?.Value as JObject)?.Properties().Select(p => p.Name);

            if (fieldsModified == null)
            {
                return;
            }

            foreach (var fieldName in fieldsModified)
            {
                Action<TObject, TObject> copier;
                if (PropertyCopiers.TryGetValue(fieldName, out copier))
                {
                    copier(destination, source);
                }
            }
        }
    }
}
