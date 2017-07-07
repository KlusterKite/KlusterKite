// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnSeverity.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The operation severity from the security point of view
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Security.Attributes
{
    /// <summary>
    /// The operation severity from the security point of view
    /// </summary>
    public enum EnSeverity
    {
        /// <summary>
        /// Trivial (non dangerous) operation
        /// </summary>
        Trivial,

        /// <summary>
        /// Crucial operation for the whole application
        /// </summary>
        Crucial
    }
}