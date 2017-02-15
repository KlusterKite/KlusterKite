// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VirtualGraphType.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The virtual graph type used to convert <see cref="ApiType" /> to <see cref="GraphType" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher.GraphTypes
{
    using System.Collections.Generic;

    using ClusterKit.Web.GraphQL.Client;
    
    using global::GraphQL.Types;

    /// <summary>
    /// The virtual graph type used to convert <see cref="ApiType"/> to <see cref="GraphType"/>
    /// </summary>
    internal class VirtualGraphType : ObjectGraphType
    {
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