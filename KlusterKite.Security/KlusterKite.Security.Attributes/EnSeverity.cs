// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnSeverity.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The operation severity from the security point of view
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Security.Attributes
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