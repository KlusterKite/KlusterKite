// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RoleUser.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Role to user link
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Data.Tests.Mock
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    using JetBrains.Annotations;

    /// <summary>
    /// Role to user link
    /// </summary>
    public class RoleUser
    {
        /// <summary>
        /// Gets or sets the role uid
        /// </summary>
        [UsedImplicitly]
        [ForeignKey(nameof(Role))]
        public Guid RoleUid { get; set; }

        /// <summary>
        /// Gets or sets the role
        /// </summary>
        [UsedImplicitly]
        public Role Role { get; set; }

        /// <summary>
        /// Gets or sets the user uid
        /// </summary>
        [UsedImplicitly]
        [ForeignKey(nameof(User))]
        public Guid UserUid { get; set; }

        /// <summary>
        /// Gets or sets the user
        /// </summary>
        [UsedImplicitly]
        public User User { get; set; }
    }
}