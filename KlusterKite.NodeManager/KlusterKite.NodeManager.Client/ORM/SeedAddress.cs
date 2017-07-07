// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SeedAddress.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The address of fixed cluster seed for new node configuration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.ORM
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using JetBrains.Annotations;

    using KlusterKite.API.Attributes;
    using KlusterKite.Data.CRUD;

    /// <summary>
    /// The address of fixed cluster seed for new node configuration
    /// </summary>
    [ApiDescription("The address of fixed cluster seed for new node configuration", Name = "SeedAddress")]
    public class SeedAddress : IObjectWithId<int>
    {
        /// <summary>
        /// Gets or sets seed address in format of akka url
        /// </summary>
        [DeclareField("The seed address in format of akka url")]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets unique address identification number
        /// </summary>
        [DeclareField("The unique address identification number", IsKey = true)]
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