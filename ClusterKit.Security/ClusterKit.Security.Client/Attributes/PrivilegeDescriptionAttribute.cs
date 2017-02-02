// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrivilegeDescriptionAttribute.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Describes privilege
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Security.Client.Attributes
{
    using System;

    /// <summary>
    /// Describes privilege
    /// </summary>
    public class PrivilegeDescriptionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrivilegeDescriptionAttribute"/> class.
        /// </summary>
        /// <param name="description">
        /// the description of the privilege
        /// </param>
        public PrivilegeDescriptionAttribute(string description)
        {
            this.Description = description;
        }

        /// <summary>
        /// Gets the description of the privilege
        /// </summary>
        public string Description { get; }
    }
}
