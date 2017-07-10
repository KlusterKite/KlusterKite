// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FieldProvider.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The field description as a pair of field type description and api provider
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Web.GraphQL.Publisher.Internals
{
    using KlusterKite.API.Client;

    /// <summary>
    /// The field description as a pair of field type description and api provider
    /// </summary>
    internal class FieldProvider
    {
        /// <summary>
        /// Gets or sets the field type
        /// </summary>
        public ApiObjectType FieldType { get; set; }

        /// <summary>
        /// Gets or sets the provider
        /// </summary>
        public ApiProvider Provider { get; set; }
    }
}