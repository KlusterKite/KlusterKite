// --------------------------------------------------------------------------------------------------------------------
// <copyright file="User.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the User type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ClusterKit.NodeManager.Client.ORM
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    using BCrypt.Net;

    using ClusterKit.API.Client;
    using ClusterKit.API.Client.Attributes;
    using ClusterKit.Data.CRUD;

    using JetBrains.Annotations;

    /// <summary>
    /// The web ui user
    /// </summary>
    [ApiDescription(Description = "The web ui user", Name = "ClusterKitUser")]
    public class User : UserDescription, IObjectWithId<Guid>
    {
        /// <summary>
        /// Gets or sets the password hash
        /// </summary>
        [UsedImplicitly]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the list of roles assigned to this user
        /// </summary>
        [DeclareField(Description = "The list of roles assigned to this user", Access = EnAccessFlag.Queryable)]
        public List<Role> Roles { get; set; } = new List<Role>();

        /// <summary>
        /// Gets or sets the user uid
        /// </summary>
        [Key]
        [UsedImplicitly]
        [DeclareField(Description = "The user uid", IsKey = true)]
        public Guid Uid { get; set; }

        /// <summary>
        /// Checks the user password
        /// </summary>
        /// <param name="password">The password to check</param>
        /// <returns>Whether the password is correct</returns>
        public bool CheckPassword(string password)
        {
            return BCrypt.Verify(password, this.Password);
        }

        /// <summary>
        /// Sets the new password for the user
        /// </summary>
        /// <param name="password">The new password</param>
        public void SetPassword(string password)
        {
            this.Password = BCrypt.HashPassword(password, BCrypt.GenerateSalt());
        }

        /// <summary>
        /// Gets the list of assigned privileges
        /// </summary>
        /// <returns>The list of privileges</returns>
        public IEnumerable<string> GetScope()
        {
            if (this.Roles == null)
            {
                throw new InvalidOperationException("Roles were not loaded");
            }

            var assignedScope = this.Roles.SelectMany(r => r.AllowedScope).Distinct();
            var deniedScope = this.Roles.SelectMany(r => r.DeniedScope).Distinct().ToList();
            return assignedScope.Where(s => !deniedScope.Contains(s));
        }

        /// <summary>
        /// Gets the short user description
        /// </summary>
        /// <returns>The user description</returns>
        public UserDescription GetDescription()
        {
            return new UserDescription
                       {
                           Login = this.Login,
                           ActiveTill = this.ActiveTill,
                           BlockedTill = this.BlockedTill,
                           IsBlocked = this.IsBlocked,
                           IsDeleted = this.IsDeleted
                       };
        }

        /// <inheritdoc />
        Guid IObjectWithId<Guid>.GetId()
        {
            return this.Uid;
        }
    }
}