// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Role.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the Role type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.Tests.Mock
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using ClusterKit.API.Client.Attributes;

    /// <summary>
    /// The role for test data context
    /// </summary>
    public class Role
    {
        /// <summary>
        /// Gets or sets the role uid
        /// </summary>
        [Key]
        [DeclareField(IsKey = true)]
        public Guid Uid { get; set; }

        /// <summary>
        /// Gets or sets the role name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the role users
        /// </summary>
        public List<User> Users { get; set; }
    }
}
