// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnScalarType.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Supported primitive types (scalars)
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Client
{
    /// <summary>
    /// Supported primitive types (scalars)
    /// </summary>
    public enum EnScalarType
    {
        /// <summary>
        /// The field is not of scalar type
        /// </summary>
        None,

        /// <summary>
        /// Boolean value
        /// </summary>
        Boolean,

        /// <summary>
        /// Enumeration type
        /// </summary>
        Enum,

        /// <summary>
        /// Floating point values
        /// </summary>
        Float,

        /// <summary>
        /// Decimal values
        /// </summary>
        Decimal,

        /// <summary>
        /// Integer values
        /// </summary>
        Integer,

        /// <summary>
        /// String values
        /// </summary>
        String,

        /// <summary>
        /// <see cref="System.Guid"/> values
        /// </summary>
        Guid,
    }
}
