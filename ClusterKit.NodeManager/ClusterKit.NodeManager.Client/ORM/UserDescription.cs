// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserDescription.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The short public user description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.ORM
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    using ClusterKit.Security.Client;

    /// <summary>
    /// The short public user description
    /// </summary>
    public class UserDescription : IUser
    {
        /// <summary>
        /// Gets or sets a date, till which user is active. After this date the user will be blocked. Null for unlimited work.
        /// </summary>
        public DateTimeOffset? ActiveTill { get; set; }

        /// <summary>
        /// Gets or sets a date, till which user is temporary blocked. After this date the user will be blocked. Null for unlimited work.
        /// </summary>
        public DateTimeOffset? BlockedTill { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether current user is currently manually blocked
        /// </summary>
        public bool IsBlocked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether current user is currently manually blocked
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the user login
        /// </summary>
        [Index(IsUnique = true)]
        public string Login { get; set; }

        /// <inheritdoc />
        string IUser.UserId => this.Login;
    }
}