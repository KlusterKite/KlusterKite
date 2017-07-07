// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompatibleTemplate.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the CompatibleTemplate type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.ORM
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using ClusterKit.API.Attributes;

    using JetBrains.Annotations;

    /// <summary>
    /// The compatible node template
    /// </summary>
    [ApiDescription("The compatible node template", Name = "CompatibleTemplate")]
    public class CompatibleTemplate
    {
        /// <summary>
        /// Gets or sets the relation id
        /// </summary>
        [Key]
        [UsedImplicitly]
        [DeclareField("the relation id", IsKey = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(TypeName = "serial")] // TODO: check and remove that Npgsql.EntityFrameworkCore.PostgreSQL can generate serial columns on migration without this kludge
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the compatible release id
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the compatible release id")]
        public int CompatibleReleaseId { get; set; }

        /// <summary>
        /// Gets or sets the parent release id
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the parent release id")]
        public int ReleaseId { get; set; }

        /// <summary>
        /// Gets or sets the template code
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the template code")]
        public string TemplateCode { get; set; }

        /// <summary>
        /// Gets or sets the compatible release
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the compatible release")]
        [ForeignKey(nameof(CompatibleReleaseId))]
        public Release CompatibleRelease { get; set; }

        /// <summary>
        /// Gets or sets the parent release
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the parent release")]
        [ForeignKey(nameof(ReleaseId))]
        public Release Release { get; set; }
    }
}
