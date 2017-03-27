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
    using ClusterKit.Data.CRUD;

    using JetBrains.Annotations;

    /// <summary>
    /// The address of fixed cluster seed for new node configuration
    /// </summary>
    [ApiDescription(Description = "The address of fixed cluster seed for new node configuration", Name = "ClusterKitSeedAddress")]
    public class SeedAddress : IObjectWithId<int>
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
        [UsedImplicitly]
        public int Id { get; set; }

        /// <inheritdoc />
        int IObjectWithId<int>.GetId()
        {
            return this.Id;
        }
    }
}