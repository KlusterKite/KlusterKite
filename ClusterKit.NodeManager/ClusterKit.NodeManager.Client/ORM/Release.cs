// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Release.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The cluster configuration with all program modules versions, node templates and configurations
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.ORM
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using ClusterKit.API.Attributes;
    using ClusterKit.Data.CRUD;

    using JetBrains.Annotations;

    using Newtonsoft.Json;

    /// <summary>
    /// The cluster configuration with all program modules versions, node templates and configurations
    /// </summary>
    [ApiDescription("The cluster configuration with all program modules versions, node templates and configurations", Name = "Release")]
    public class Release : IObjectWithId<int>
    {
        /// <summary>
        /// Gets or sets the release id
        /// </summary>
        [DeclareField("The release id", IsKey = true)]
        [Key]
        [UsedImplicitly]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the release major version number
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the release major version number")]
        public int MajorVersion { get; set; }

        /// <summary>
        /// Gets or sets the release minor version number
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the release minor version number")]
        public int MinorVersion { get; set; }

        /// <summary>
        /// Gets or sets the release name
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the release name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the release name
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the release notes")]
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets the release creation date
        /// </summary>
        [UsedImplicitly]
        [DeclareField("Release creation date", Access = EnAccessFlag.Queryable)]
        public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// Gets or sets the release start date (the date when cluster was switched on this configuration first time)
        /// </summary>
        [UsedImplicitly]
        [DeclareField("Release start date (the date when cluster was switched on this configuration first time)", Access = EnAccessFlag.Queryable)]
        public DateTimeOffset? Started { get; set; }

        /// <summary>
        /// Gets or sets the release  finish date (the date when cluster was switched from this configuration last time)
        /// </summary>
        [UsedImplicitly]
        [DeclareField("Release finish date (the date when cluster was switched from this configuration last time)", Access = EnAccessFlag.Queryable)]
        public DateTimeOffset? Finished { get; set; }

        /// <summary>
        /// Gets or sets the release state
        /// </summary>
        [UsedImplicitly]
        [DeclareField("The release state", Access = EnAccessFlag.Queryable)]
        public EnReleaseState State { get; set; } = EnReleaseState.Draft;

        /// <summary>
        /// Gets or sets a value indicating whether this release was considered stable
        /// </summary>
        [UsedImplicitly]
        [DeclareField("A value indicating whether this release was considered stable", Access = EnAccessFlag.Queryable)]
        public bool IsStable { get; set; }

        /// <summary>
        /// Gets or sets the release configuration
        /// </summary>
        [DeclareField("the release configuration")]
        [NotMapped]
        public ReleaseConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets or sets the list of compatible node templates
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the list of compatible node templates", Access = EnAccessFlag.Queryable)]
        [ForeignKey(nameof(CompatibleTemplate.ReleaseId))]
        public List<CompatibleTemplate> CompatibleTemplates { get; set; }
        
        /// <summary>
        /// Gets or sets the list of migration operations
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the list of migration operations")]
        public List<MigrationOperation> Operations { get; set; }

        /// <summary>
        /// Gets or sets the list of migration errors
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the list of migration errors")]
        public List<MigrationError> Errors { get; set; }

        /// <summary>
        ///  Gets or sets the release configuration as json to store in database
        /// </summary>
        [UsedImplicitly]
        public string ConfigurationJson
        {
            get
            {
                return this.Configuration != null
                           ? JsonConvert.SerializeObject(this.Configuration, Formatting.None)
                           : null;
            }

            set
            {
                this.Configuration = value == null ? null : JsonConvert.DeserializeObject<ReleaseConfiguration>(value);
            }
        }

        /// <inheritdoc />
        public int GetId()
        {
            return this.Id;
        }
    }
}
