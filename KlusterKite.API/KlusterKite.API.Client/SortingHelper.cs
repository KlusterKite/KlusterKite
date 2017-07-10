// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SortingHelper.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Helper to apply sorting to <see cref="IQueryable{T}" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Client
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Helper to apply sorting to <see cref="IQueryable{T}"/>
    /// </summary>
    public static class SortingHelper
    {
        /// <summary>
        /// Applies sorting to the query
        /// </summary>
        /// <typeparam name="TObject">The type of objects to sort</typeparam>
        /// <param name="query">The initial query</param>
        /// <param name="sortingConditions">The list of sorting conditions</param>
        /// <returns>The sorted query</returns>
        public static IQueryable<TObject> ApplySorting<TObject>(
            this IQueryable<TObject> query,
            IEnumerable<SortingCondition> sortingConditions)
        {
            return Helper<TObject>.ApplySorting(query, sortingConditions);
        }

        /// <summary>
        /// Helper to apply sorting to <see cref="IQueryable{T}"/>
        /// </summary>
        /// <typeparam name="TObject">The type of objects to sort</typeparam>
        [SuppressMessage("ReSharper", "StaticMemberInGenericType", Justification = "Suppression is OK here. We are getting use from static members of generic class.")]
        private static class Helper<TObject>
        {
            /// <summary>
            /// The list of sortable properties
            /// </summary>
            private static readonly Dictionary<string, PropertyInfo> Properties;

            /// <summary>
            /// The prepared generics of <see cref="Queryable.OrderBy{TSource,TKey}(IQueryable{TSource},Expression{Func{TSource,TKey}})"/>
            /// </summary>
            private static readonly Dictionary<Type, MethodInfo> OrderByMethods;

            /// <summary>
            /// The prepared generics of <see cref="Queryable.OrderByDescending{TSource,TKey}(IQueryable{TSource},Expression{Func{TSource,TKey}})"/>
            /// </summary>
            private static readonly Dictionary<Type, MethodInfo> OrderByDescendingMethods;

            /// <summary>
            /// The prepared generics of <see cref="Queryable.ThenBy{TSource,TKey}(IOrderedQueryable{TSource},Expression{Func{TSource,TKey}})"/>
            /// </summary>
            private static readonly Dictionary<Type, MethodInfo> ThenByMethods;

            /// <summary>
            /// The prepared generics of <see cref="Queryable.ThenByDescending{TSource,TKey}(IOrderedQueryable{TSource},Expression{Func{TSource,TKey}})"/>
            /// </summary>
            private static readonly Dictionary<Type, MethodInfo> ThenByDescendingMethods;

            /// <summary>
            /// Initializes static members of the <see cref="Helper{TObject}"/> class.
            /// </summary>
            static Helper()
            {
                Properties = new Dictionary<string, PropertyInfo>();
                OrderByMethods = new Dictionary<Type, MethodInfo>();
                OrderByDescendingMethods = new Dictionary<Type, MethodInfo>();
                ThenByMethods = new Dictionary<Type, MethodInfo>();
                ThenByDescendingMethods = new Dictionary<Type, MethodInfo>();

                var queryableType = typeof(Queryable);
                var entityType = typeof(TObject);

                var propertyInfos =
                    typeof(TObject).GetProperties(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
                foreach (var propertyInfo in propertyInfos)
                {
                    Properties[propertyInfo.Name] = propertyInfo;
                    if (OrderByMethods.ContainsKey(propertyInfo.PropertyType))
                    {
                        continue;
                    }

                    OrderByMethods[propertyInfo.PropertyType] =
                        queryableType.GetMethods()
                            .Single(
                                method =>
                                    method.Name == "OrderBy" && method.IsGenericMethodDefinition
                                    && method.GetGenericArguments().Length == 2 && method.GetParameters().Length == 2)
                            .MakeGenericMethod(entityType, propertyInfo.PropertyType);

                    OrderByDescendingMethods[propertyInfo.PropertyType] =
                        queryableType.GetMethods()
                            .Single(
                                method =>
                                    method.Name == "OrderByDescending" && method.IsGenericMethodDefinition
                                    && method.GetGenericArguments().Length == 2 && method.GetParameters().Length == 2)
                            .MakeGenericMethod(entityType, propertyInfo.PropertyType);

                    ThenByMethods[propertyInfo.PropertyType] =
                        queryableType.GetMethods()
                            .Single(
                                method =>
                                    method.Name == "ThenBy" && method.IsGenericMethodDefinition
                                    && method.GetGenericArguments().Length == 2 && method.GetParameters().Length == 2)
                            .MakeGenericMethod(entityType, propertyInfo.PropertyType);

                    ThenByDescendingMethods[propertyInfo.PropertyType] =
                        queryableType.GetMethods()
                            .Single(
                                method =>
                                    method.Name == "ThenByDescending" && method.IsGenericMethodDefinition
                                    && method.GetGenericArguments().Length == 2 && method.GetParameters().Length == 2)
                            .MakeGenericMethod(entityType, propertyInfo.PropertyType);
                }
            }

            /// <summary>
            /// Applies sorting to the query
            /// </summary>
            /// <param name="query">The initial query</param>
            /// <param name="sortingConditions">The list of sorting conditions</param>
            /// <returns>The sorted query</returns>
            public static IQueryable<TObject> ApplySorting(
                IQueryable<TObject> query,
                IEnumerable<SortingCondition> sortingConditions)
            {
                if (sortingConditions == null || query == null)
                {
                    return query;
                }

                var sorted =
                    sortingConditions.Where(c => Properties.ContainsKey(c.PropertyName))
                        .Select(c => new { c.Direction, Property = Properties[c.PropertyName] })
                        .ToList();

                if (sorted.Count == 0)
                {
                    return query;
                }

                var firstCondition = sorted[0];
                var type = typeof(TObject);
                var parameter = Expression.Parameter(type, "e");
                var property = Expression.Property(parameter, firstCondition.Property);
                var expression = Expression.Lambda(property, parameter);

                var sortedQuery = firstCondition.Direction == SortingCondition.EnDirection.Asc 
                    ? (IOrderedQueryable<TObject>)OrderByMethods[firstCondition.Property.PropertyType].Invoke(null, new object[] { query, expression })
                    : (IOrderedQueryable<TObject>)OrderByDescendingMethods[firstCondition.Property.PropertyType].Invoke(null, new object[] { query, expression });

                foreach (var sort in sorted.Skip(1))
                {
                    parameter = Expression.Parameter(type, "e");
                    property = Expression.Property(parameter, sort.Property);
                    expression = Expression.Lambda(property, parameter);

                    sortedQuery = sort.Direction == SortingCondition.EnDirection.Asc
                        ? (IOrderedQueryable<TObject>)ThenByMethods[sort.Property.PropertyType].Invoke(null, new object[] { sortedQuery, expression })
                        : (IOrderedQueryable<TObject>)ThenByDescendingMethods[sort.Property.PropertyType].Invoke(null, new object[] { sortedQuery, expression });
                }

                return sortedQuery;
            }
        }
    } 
}