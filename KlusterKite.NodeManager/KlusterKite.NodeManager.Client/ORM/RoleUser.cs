// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RoleUser.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Role to user link
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.ORM
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using JetBrains.Annotations;

    using KlusterKite.API.Attributes;

    /// <summary>
    /// Role to user link
    /// </summary>
    public class RoleUser
    {
        /// <summary>
        /// Gets or sets the link id
        /// </summary>
        [Key]
        [UsedImplicitly]
        [DeclareField("The link id", IsKey = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(TypeName = "serial")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the role uid
        /// </summary>
        [UsedImplicitly]
        [ForeignKey(nameof(Role))]
        [DeclareField("the role uid")]
        public Guid RoleUid { get; set; }

        /// <summary>
        /// Gets or sets the role
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the role")]
        public Role Role { get; set; }

        /// <summary>
        /// Gets or sets the user uid
        /// </summary>
        [UsedImplicitly]
        [ForeignKey(nameof(User))]
        [DeclareField("the user uid")]
        public Guid UserUid { get; set; }

        /// <summary>
        /// Gets or sets the user
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the user")]
        public User User { get; set; }
    }
}
