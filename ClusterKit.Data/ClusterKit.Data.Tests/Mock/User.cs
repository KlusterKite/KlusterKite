// --------------------------------------------------------------------------------------------------------------------
// <copyright file="User.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the User type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.Tests.Mock
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using ClusterKit.API.Client.Attributes;

    /// <summary>
    /// The user for test data context
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the user uid
        /// </summary>
        [Key]
        [DeclareField(IsKey = true)]
        public Guid Uid { get; set; }

        /// <summary>
        /// Gets or sets the user login
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Gets or sets the user password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the user roles
        /// </summary>
        public List<Role> Roles { get; set; }
    }
}
