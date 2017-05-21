// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompatibleTemplate.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the CompatibleTemplate type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Castle.Components.DictionaryAdapter;

namespace ClusterKit.NodeManager.Client.ORM
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using ClusterKit.API.Attributes;

    /// <summary>
    /// The compatible node template
    /// </summary>
    [ApiDescription("The compatible node template", Name = "CompatibleTemplate")]
    [Serializable]
    public class CompatibleTemplate
    {
        /// <summary>
        /// Gets or sets the relation id
        /// </summary>
        [Key]
        [DeclareField("the relation id", IsKey = true)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the compatible release id
        /// </summary>
        [DeclareField("the compatible release id")]
        [ForeignKey(nameof(CompatibleRelease))]
        public int CompatibleReleaseId { get; set; }

        /// <summary>
        /// Gets or sets the parent release id
        /// </summary>
        [DeclareField("the parent release id")]
        [ForeignKey(nameof(Release))]
        public int ReleaseId { get; set; }

        /// <summary>
        /// Gets or sets the template code
        /// </summary>
        [DeclareField("the template code")]
        public string TemplateCode { get; set; }

        /// <summary>
        /// Gets or sets the compatible release
        /// </summary>
        [DeclareField("the compatible release")]
        public Release CompatibleRelease { get; set; }

        /// <summary>
        /// Gets or sets the parent release
        /// </summary>
        [DeclareField("the parent release")]
        public Release Release { get; set; }
    }
}
