// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeclareConnectionAttribute.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Declares current propertie as a node connection
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Client.Attributes
{
    using System;

    /// <summary>
    /// Declares current method as a node connection
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class DeclareConnectionAttribute : PublishToApiAttribute
    {
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
