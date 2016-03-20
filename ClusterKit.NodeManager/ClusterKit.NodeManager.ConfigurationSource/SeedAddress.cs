// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SeedAddress.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The address of fixed cluster seed for new node configuration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// The address of fixed cluster seed for new node configuration
    /// </summary>
    public class SeedAddress
    {
        /// <summary>
        /// Gets or sets seed address in format of akka url
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets unique address identification number
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key]
        public int Id { get; set; }
    }
}