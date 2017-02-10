// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiMethod.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The published type method
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Client
{
    using System.Collections.Generic;

    /// <summary>
    /// The published type method
    /// </summary>
    public class ApiMethod
    {
        /// <summary>
        /// Gets or sets the field name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the method parameters list
        /// </summary>
        public List<ApiField> Parameters { get; set; } = new List<ApiField>();

        /// <summary>
        /// Gets or sets a value indicating whether the return value is an array of elements
        /// </summary>
        public bool ReturnsArray { get; set; }

        /// <summary>
        /// Gets or sets the method return type name
        /// </summary>
        public string ReturnType { get; set; }
    }
}