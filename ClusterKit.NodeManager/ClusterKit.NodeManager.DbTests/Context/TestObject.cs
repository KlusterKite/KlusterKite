// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestObject.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The test database object
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.DbTests.Context
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The test database object
    /// </summary>
    public class TestObject
    {
        /// <summary>
        /// Gets or sets the object id
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the object name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the object value
        /// </summary>
        public decimal Value { get; set; }
    }
}
