// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiRequest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The api request
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher
{
    using System.Collections.Generic;

    using global::GraphQL.Language.AST;

    /// <summary>
    /// The api request
    /// </summary>
    public class ApiRequest
    {
        /// <summary>
        /// Gets or sets the field arguments
        /// </summary>
        public Arguments Arguments { get; set; }

        /// <summary>
        /// Gets or sets the field name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the list of requested subfields
        /// </summary>
        public List<ApiRequest> Fields { get; set; }
    }
}