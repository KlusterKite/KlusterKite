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

    using ClusterKit.API.Attributes;

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
        [DeclareField]
        public string Login { get; set; }

        /// <summary>
        /// Gets or sets the user password
        /// </summary>
        [DeclareField]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the user roles
        /// </summary>
        [DeclareField(Access = EnAccessFlag.Queryable)]
        public List<RoleUser> Roles { get; set; }
    }
}
