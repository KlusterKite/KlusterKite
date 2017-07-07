// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrivilegeDescriptionAttribute.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Describes privilege
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Security.Attributes
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
        /// <param name="actions">
        /// The possible actions extensions
        /// </param>
        public PrivilegeDescriptionAttribute(string description, params string[] actions)
        {
            this.Description = description;
            this.Actions = actions;
        }

        /// <summary>
        /// Gets the description of the privilege
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets or sets the target to grant privilege to
        /// </summary>
        public EnPrivilegeTarget Target { get; set; }

        /// <summary>
        /// Gets or sets the possible actions extensions
        /// </summary>
        public string[] Actions { get; set; }
    }
}
