// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceMigrationCommand.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The description of migrator action to resource
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.RemoteDomain
{
    /// <summary>
    /// The description of migrator action to resource
    /// </summary>
    public class ResourceMigrationCommand
    {
        /// <summary>
        /// Gets or sets the resource code
        /// </summary>
        public string ResourceCode { get; set; }

        /// <summary>
        /// Gets or sets the destination point
        /// </summary>
        public string MigrationPoint { get; set; }
    }
}