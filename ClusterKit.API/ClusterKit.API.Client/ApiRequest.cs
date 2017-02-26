// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiRequest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the ApiRequest type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client
{
    using System.Collections.Generic;

    using ClusterKit.API.Client.Messages;

    /// <summary>
    /// The request partial description
    /// </summary>
    public class ApiRequest
    {
        /// <summary>
        /// Gets or sets the requested field / method name
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// Gets or sets the argument values for methods
        /// </summary>
        public SurrogatableJObject Arguments { get; set; }

        /// <summary>
        /// Gets or sets the list of requested fields
        /// </summary>
        public List<ApiRequest> Fields { get; set; }
    }
}
