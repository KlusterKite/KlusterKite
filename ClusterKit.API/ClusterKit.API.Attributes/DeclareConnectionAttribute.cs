// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeclareConnectionAttribute.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Declares current propertie as a node connection
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Attributes
{
    using System;

    /// <summary>
    /// Declares current method as a node connection
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class DeclareConnectionAttribute : DeclareFieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeclareConnectionAttribute"/> class.
        /// </summary>
        public DeclareConnectionAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeclareConnectionAttribute"/> class.
        /// </summary>
        /// <param name="description">
        /// The description.
        /// </param>
        public DeclareConnectionAttribute(string description)
            : base(description)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether new node creation is possible
        /// </summary>
        public bool CanCreate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether node update is possible
        /// </summary>
        public bool CanUpdate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether new node deletion is possible
        /// </summary>
        public bool CanDelete { get; set; }

        /// <summary>
        /// Gets or sets the description for the create mutation
        /// </summary>
        public string CreateDescription { get; set; }

        /// <summary>
        /// Gets or sets the description for the update mutation
        /// </summary>
        public string UpdateDescription { get; set; }

        /// <summary>
        /// Gets or sets the description for the delete mutation
        /// </summary>
        public string DeleteDescription { get; set; }
    }
}
