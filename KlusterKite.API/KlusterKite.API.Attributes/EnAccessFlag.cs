// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnAccessFlag.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The type of allowed api property access
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Attributes
{
    /// <summary>
    /// The type of allowed api property access
    /// </summary>
    public enum EnAccessFlag
    {
        /// <summary>
        /// The data of the property can be read via query requests
        /// </summary>
        Queryable = 1,

        /// <summary>
        /// In case of object use as method / mutation parameter the value of this property can be set up
        /// </summary>
        Writable = 2,

        /// <summary>
        /// The property can be used both as <see cref="Queryable"/> and <see cref="Writable"/>. This is the default.
        /// </summary>
        All = 3
    }
}