// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseTypeInterface.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The base class for GraphQL interface implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher.GraphTypes
{
    using System;
    using System.Collections.Generic;

    using global::GraphQL.Types;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The base class for GraphQL interface implementation
    /// </summary>
    public abstract class BaseTypeInterface : InterfaceGraphType
    {
        /// <summary>
        /// The list of implemented types
        /// </summary>
        private readonly Dictionary<string, IObjectGraphType> implementedTypes = new Dictionary<string, IObjectGraphType>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTypeInterface"/> class.
        /// </summary>
        protected BaseTypeInterface()
        {
            this.ResolveType = this.GetResolvedType;
        }

        /// <summary>
        /// Adds new type implementation
        /// </summary>
        /// <param name="typeName">The implementation name</param>
        /// <param name="graphType">The implementing graph type</param>
        public void AddImplementedType(string typeName, IObjectGraphType graphType)
        {
            if (this.implementedTypes.ContainsKey(typeName))
            {
                throw new InvalidOperationException("The type is laready registered");
            }

            this.AddPossibleType(graphType);
            this.implementedTypes.Add(typeName, graphType);
        }

        /// <summary>
        /// Resolves current interface implementation by data
        /// </summary>
        /// <param name="source">The node found</param>
        /// <returns>The resolved type</returns>
        private IObjectGraphType GetResolvedType(object source)
        {
            var json = source as JObject;
            var typeName = json?.Property("__resolvedType")?.Value?.ToString();
            if (typeName == null)
            {
                return null;
            }

            IObjectGraphType type;
            if (this.implementedTypes.TryGetValue(typeName, out type))
            {
                // todo: search and remove some strange function in GraphQL lib that removes installed field resolvers AND REMOVE THIS KLUDGE!!!!!
                (type as VirtualGraphType)?.RestoreFieldResolvers();
                return type;
            }

            return null;
        }
    }
}