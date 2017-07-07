// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VirtualGraphType.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The virtual graph type used to convert <see cref="ApiType" /> to <see cref="GraphType" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Web.GraphQL.Publisher.GraphTypes
{
    using System.Collections.Generic;

    using global::GraphQL.Resolvers;
    using global::GraphQL.Types;

    using KlusterKite.API.Client;

    /// <summary>
    /// The virtual graph type used to convert <see cref="ApiObjectType"/> to <see cref="GraphType"/>
    /// </summary>
    internal class VirtualGraphType : ObjectGraphType
    {
        /// <summary>
        /// Storage of field resolvers
        /// </summary>
        private readonly Dictionary<string, IFieldResolver> storedResolvers = new Dictionary<string, IFieldResolver>();

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualGraphType"/> class.
        /// </summary>
        /// <param name="virtualTypeName">
        /// The virtual type name.
        /// </param>
        /// <param name="fields">
        /// The list of fields.
        /// </param>
        public VirtualGraphType(string virtualTypeName, List<FieldType> fields = null)
        {
            this.Name = virtualTypeName;
            if (fields != null)
            {
                foreach (var field in fields)
                {
                    this.AddField(field);
                }
            }
        }

        /// <summary>
        /// Stores the field resolvers
        /// </summary>
        public void StoreFieldResolvers()
        {
            this.storedResolvers.Clear();
            foreach (var fieldType in this.Fields)
            {
                this.storedResolvers[fieldType.Name] = fieldType.Resolver;
            }
        }

        /// <summary>
        /// Stores the field resolvers
        /// </summary>
        public void RestoreFieldResolvers()
        {
            foreach (var fieldType in this.Fields)
            {
                IFieldResolver resolver;
                if (this.storedResolvers.TryGetValue(fieldType.Name, out resolver))
                {
                    fieldType.Resolver = resolver;
                }
            }
        }

        /// <summary>
        /// The same as <see cref="VirtualGraphType"/> but with array container flag
        /// </summary>
        public class Array : VirtualGraphType, IArrayContainerGraph
        {
            /// <inheritdoc />
            public Array(string virtualTypeName, List<FieldType> fields = null)
                : base(virtualTypeName, fields)
            {
            }
        }
    }
}