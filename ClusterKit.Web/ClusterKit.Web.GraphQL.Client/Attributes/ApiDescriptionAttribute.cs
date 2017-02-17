// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiDescriptionAttribute.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Describes type (class) to published api
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Client.Attributes
{
    using System;

    /// <summary>
    /// Describes type (class) to published api
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter)]
    public class ApiDescriptionAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the published property / method name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description to publish
        /// </summary>
        public string Description { get; set; }
    }
}