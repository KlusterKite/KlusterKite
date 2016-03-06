// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeTemplate.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Node template description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource
{
    using System.ComponentModel.DataAnnotations.Schema;

    using JetBrains.Annotations;

    /// <summary>
    /// Node template description
    /// </summary>
    [UsedImplicitly]
    public class NodeTemplate
    {
        /// <summary>
        /// Gets or sets the program readable node template name
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets unique template identification number
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the human readable node template name
        /// </summary>
        public string Name { get; set; }
    }
}