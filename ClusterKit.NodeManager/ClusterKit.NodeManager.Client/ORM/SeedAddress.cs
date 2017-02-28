// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SeedAddress.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The address of fixed cluster seed for new node configuration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.ORM
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using ClusterKit.API.Client.Attributes;

    /// <summary>
    /// The address of fixed cluster seed for new node configuration
    /// </summary>
    [ApiDescription(Description = "The address of fixed cluster seed for new node configuration")]
    public class SeedAddress
    {
        /// <summary>
        /// Gets or sets seed address in format of akka url
        /// </summary>
        [DeclareField(Description = "The seed address in format of akka url")]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets unique address identification number
        /// </summary>
        [DeclareField(Description = "The unique address identification number", IsKey = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key]
        public int Id { get; set; }
    }
}