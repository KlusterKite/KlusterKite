// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FieldProvider.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The field description as a pair of field type description and api provider
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher.Internals
{
    using ClusterKit.Web.GraphQL.Client;

    /// <summary>
    /// The field description as a pair of field type description and api provider
    /// </summary>
    internal class FieldProvider
    {
        /// <summary>
        /// Gets or sets the field type
        /// </summary>
        public ApiType FieldType { get; set; }

        /// <summary>
        /// Gets or sets the provider
        /// </summary>
        public ApiProvider Provider { get; set; }
    }
}