// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnResourcePosition.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The possible resource migration position according to current migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.MigrationStates
{
    /// <summary>
    /// The possible resource migration position according to current migration
    /// </summary>
    public enum EnResourcePosition
    {
        /// <summary>
        /// The resource is not created yet
        /// </summary>
        NotCreated,

        /// <summary>
        /// The resource is in the source point migration
        /// </summary>
        Source,

        /// <summary>
        /// The migration position of the resource is neither source nor destination
        /// </summary>
        Undefined,

        /// <summary>
        /// The resource is in the destination point migration
        /// </summary>
        Destination, 

        /// <summary>
        /// The resource is not modified during current migration
        /// </summary>
        SourceAndDestination,

        /// <summary>
        /// The resource is no more supported in the target release
        /// </summary>
        Obsolete
    }
}