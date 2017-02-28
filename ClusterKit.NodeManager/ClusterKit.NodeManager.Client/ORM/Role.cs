// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Role.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The amount of privileges assigned to the user
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.ORM
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Xml.Serialization;

    using ClusterKit.API.Client.Attributes;

    using JetBrains.Annotations;

    using Newtonsoft.Json;

    /// <summary>
    /// The amount of privileges assigned to the user
    /// </summary>
    [ApiDescription(Description = "Security role. The amount of privileges assigned to the user", Name = "ClusterKitRole")]
    public class Role
    {
        /// <summary>
        /// Gets or sets the role uid
        /// </summary>
        [Key]
        [DeclareField(Description = "The role uid", IsKey = true)]
        public Guid Uid { get; set; }

        /// <summary>
        /// Gets or sets the role name
        /// </summary>
        [DeclareField(Description = "The role name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the list of granted privileges
        /// </summary>
        [NotMapped]
        [NotNull]
        [DeclareField(Description = "The list of granted privileges")]
        public List<string> AllowedScope { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets <see cref="AllowedScope"/> serialized to JSON
        /// </summary>
        [JsonIgnore]
        [XmlIgnore]
        [UsedImplicitly]
        public string AllowedScopeJson
        {
            get
            {
                return JsonConvert.SerializeObject(this.AllowedScope);
            }

            set
            {
                this.AllowedScope = JsonConvert.DeserializeObject<List<string>>(value);
            }
        }

        /// <summary>
        /// Gets or sets the list of denied privileges (the user will not acquire this privileges, even if they will be granted via other roles)
        /// </summary>
        [NotMapped]
        [NotNull]
        [DeclareField(Description = "The list of denied privileges (the user will not acquire this privileges, even if they will be granted via other roles)")]
        public List<string> DeniedScope { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets <see cref="AllowedScope"/> serialized to JSON
        /// </summary>
        [JsonIgnore]
        [XmlIgnore]
        [UsedImplicitly]
        public string DeniedScopeJson
        {
            get
            {
                return JsonConvert.SerializeObject(this.DeniedScope);
            }

            set
            {
                this.DeniedScope = JsonConvert.DeserializeObject<List<string>>(value);
            }
        }

        /// <summary>
        /// Gets or sets the list of users assigned to this role
        /// </summary>
        /// TODO: support recursive type refs
        /// [DeclareField(Description = "The list of users assigned to this role")]
        public List<User> Users { get; set; }
    }
}
