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
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using ClusterKit.API.Client;
    using ClusterKit.API.Client.Attributes;
    using ClusterKit.Data.CRUD;

    using Newtonsoft.Json;

    /// <summary>
    /// The cluster configuration with all program modules versions, node templates and configurations
    /// </summary>
    [ApiDescription("The cluster configuration with all program modules versions, node templates and configurations", Name = "Release")]
    public class Release : IObjectWithId<int>
    {
        /// <summary>
        /// The list of release states
        /// </summary>
        [ApiDescription("The list of release states", Name = "EnReleaseState")]
        public enum EnState
        {
            /// <summary>
            /// This is the draft of release and can be edited
            /// </summary>
            [ApiDescription("This is the draft of release and can be edited")]
            Draft,

            /// <summary>
            /// This is the new release, ready to be applied
            /// </summary>
            [ApiDescription("This is the new release, ready to be applied")]
            Ready,

            /// <summary>
            /// This is the current active release
            /// </summary>
            [ApiDescription("This is the current active release")]
            Active,

            /// <summary>
            /// This release was faulted and cluster roll-backed to the latest stable release (or some other)
            /// </summary>
            [ApiDescription("This release was faulted and cluster rollbacked to the latest stable release (or some other)")]
            Faulted,

            /// <summary>
            /// This release is obsolete and was replaced by some new one
            /// </summary>
            [ApiDescription("This release is obsolete and was replaced by some new one")]
            Obsolete
        }

        /// <summary>
        /// Gets or sets the release id
        /// </summary>
        [DeclareField("The release id", IsKey = true)]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the release major version number
        /// </summary>
        [DeclareField("the release major version number")]
        public int MajorVersion { get; set; }

        /// <summary>
        /// Gets or sets the release minor version number
        /// </summary>
        [DeclareField("the release minor version number")]
        public int MinorVersion { get; set; }

        /// <summary>
        /// Gets or sets the release name
        /// </summary>
        [DeclareField("the release name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the release name
        /// </summary>
        [DeclareField("the release notes")]
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets the release creation date
        /// </summary>
        [DeclareField("Release creation date", Access = EnAccessFlag.Queryable)]
        public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// Gets or sets the release start date (the date when cluster was switched on this configuration first time)
        /// </summary>
        [DeclareField("Release start date (the date when cluster was switched on this configuration first time)", Access = EnAccessFlag.Queryable)]
        public DateTimeOffset? Started { get; set; }

        /// <summary>
        /// Gets or sets the release  finish date (the date when cluster was switched from this configuration last time)
        /// </summary>
        [DeclareField("Release finish date (the date when cluster was switched from this configuration last time)", Access = EnAccessFlag.Queryable)]
        public DateTimeOffset? Finished { get; set; }

        /// <summary>
        /// Gets or sets the release state
        /// </summary>
        [DeclareField("The release state", Access = EnAccessFlag.Queryable)]
        public EnState State { get; set; } = EnState.Draft;

        /// <summary>
        /// Gets or sets a value indicating whether this release was considered stable
        /// </summary>
        [DeclareField("A value indicating whether this release was considered stable", Access = EnAccessFlag.Queryable)]
        public bool IsStable { get; set; }

        /// <summary>
        /// Gets or sets the release configuration
        /// </summary>
        [DeclareField("the release configuration")]
        [NotMapped]
        public ReleaseConfiguration Configuration { get; set; }

        /// <summary>
        ///  Gets or sets the release configuration as json to store in database
        /// </summary>
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
