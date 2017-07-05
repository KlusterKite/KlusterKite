// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnMigrationSteps.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The list of migration procedure steps
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.ORM
{
    using ClusterKit.API.Attributes;

    /// <summary>
    /// The list of migration procedure steps
    /// </summary>
    [ApiDescription("The list of migration procedure steps", Name = "EnMigrationSteps")]
    public enum EnMigrationSteps
    {
        /// <summary>
        /// The initial migration position, nothing is done
        /// </summary>
        [ApiDescription("The initial migration position, nothing is done")]
        Start,

        /// <summary>
        /// The nodes are in the update state
        /// </summary>
        [ApiDescription("The nodes are in the update state")]
        NodesUpdating,

        /// <summary>
        /// The nodes were successfully updated
        /// </summary>
        [ApiDescription("The nodes were successfully updated")]
        NodesUpdated,

        /// <summary>
        /// The resources are in the update stage
        /// </summary>
        [ApiDescription("The resources are in the update stage")]
        ResourcesUpdating,

        /// <summary>
        /// The resources were successfully updated
        /// </summary>
        [ApiDescription("The resources were successfully updated")]
        ResourcesUpdated,

        /// <summary>
        /// The migration is finished
        /// </summary>
        [ApiDescription("The migration is finished")]
        Finish,

        /// <summary>
        /// The migration is broken
        /// </summary>
        [ApiDescription("The migration is broken")]
        Broken
    }
}