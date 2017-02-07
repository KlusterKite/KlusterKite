﻿// --------------------------------------------------------------------------------------------------------------------
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

    using JetBrains.Annotations;

    using Newtonsoft.Json;

    /// <summary>
    /// The amount of privileges assigned to the user
    /// </summary>
    public class Role
    {
        /// <summary>
        /// Gets or sets the role uid
        /// </summary>
        [Key]
        public Guid Uid { get; set; }

        /// <summary>
        /// Gets or sets the role name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the list of granted privileges
        /// </summary>
        [NotMapped]
        [NotNull]
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
        public List<User> Users { get; set; }
    }
}