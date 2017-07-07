// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Role.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The amount of privileges assigned to the user
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.ORM
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Xml.Serialization;

    using KlusterKite.API.Attributes;
    using KlusterKite.Data.CRUD;

    using JetBrains.Annotations;

    using Newtonsoft.Json;

    /// <summary>
    /// The amount of privileges assigned to the user
    /// </summary>
    [ApiDescription("Security role. The amount of privileges assigned to the user", Name = "Role")]
    public class Role : IObjectWithId<Guid>
    {
        /// <summary>
        /// Gets or sets the role uid
        /// </summary>
        [Key]
        [DeclareField("The role uid", IsKey = true)]
        public Guid Uid { get; set; }

        /// <summary>
        /// Gets or sets the role name
        /// </summary>
        [DeclareField("The role name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the list of granted privileges
        /// </summary>
        [NotMapped]
        [NotNull]
        [DeclareField("The list of granted privileges")]
        public List<string> AllowedScope { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets <see cref="AllowedScope"/> serialized to JSON
        /// </summary>
        [JsonIgnore]
        [XmlIgnore]
        [UsedImplicitly]
        public string AllowedScopeJson
        {
            get => JsonConvert.SerializeObject(this.AllowedScope);

            set => this.AllowedScope = JsonConvert.DeserializeObject<List<string>>(value);
        }

        /// <summary>
        /// Gets or sets the list of denied privileges (the user will not acquire this privileges, even if they will be granted via other roles)
        /// </summary>
        [NotMapped]
        [NotNull]
        [DeclareField("The list of denied privileges (the user will not acquire this privileges, even if they will be granted via other roles)")]
        [UsedImplicitly]
        public List<string> DeniedScope { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets <see cref="AllowedScope"/> serialized to JSON
        /// </summary>
        [JsonIgnore]
        [XmlIgnore]
        [UsedImplicitly]
        public string DeniedScopeJson
        {
            get => JsonConvert.SerializeObject(this.DeniedScope);

            set => this.DeniedScope = JsonConvert.DeserializeObject<List<string>>(value);
        }

        /// <summary>
        /// Gets or sets the list of users assigned to this role
        /// </summary>
        [UsedImplicitly]
        [DeclareField("The list of users assigned to this role", Access = EnAccessFlag.Queryable)]
        public List<RoleUser> Users { get; set; }

        /// <inheritdoc />
        Guid IObjectWithId<Guid>.GetId()
        {
            return this.Uid;
        }
    }
}
