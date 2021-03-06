﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnMigrationSteps.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The list of migration procedure steps
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.ORM
{
    using KlusterKite.API.Attributes;

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
        /// The resources are in the update stage
        /// </summary>
        [ApiDescription("The resources that should be updated prior to nodes are in the update stage")]
        PreNodesResourcesUpdating,

        /// <summary>
        /// The resources were successfully updated
        /// </summary>
        [ApiDescription("The resources that should be updated prior to nodes were successfully updated")]
        PreNodeResourcesUpdated,

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
        [ApiDescription("The resources that should be updated after nodes are in the update stage")]
        PostNodesResourcesUpdating,

        /// <summary>
        /// The migration is finished
        /// </summary>
        [ApiDescription("The migration is finished")]
        Finish,

        /// <summary>
        /// The migration is broken
        /// </summary>
        [ApiDescription("The migration is broken, requires manual recovery")]
        Broken,

        /// <summary>
        /// The migration is broken, but can be recovered
        /// </summary>
        [ApiDescription("The migration is broken, but can be recovered")]
        Recovery,
    }
}