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
    using ClusterKit.API.Attributes;
    
    /// <summary>
    /// The possible resource migration position according to current migration
    /// </summary>
    [ApiDescription("The possible resource migration position according to current migration", Name = "EnResourcePosition")]
    public enum EnResourcePosition
    {
        /// <summary>
        /// The resource is not created yet
        /// </summary>
        [ApiDescription("The resource is not created yet")]
        NotCreated,

        /// <summary>
        /// The resource is in the source point migration
        /// </summary>
        [ApiDescription("The resource is in the source point migration")]
        Source,

        /// <summary>
        /// The migration position of the resource is neither source nor destination
        /// </summary>
        [ApiDescription("The migration position of the resource is neither source nor destination")]
        Undefined,

        /// <summary>
        /// The resource is in the destination point migration
        /// </summary>
        [ApiDescription("The resource is in the destination point migration")]
        Destination,

        /// <summary>
        /// The resource is not modified during current migration
        /// </summary>
        [ApiDescription("The resource is not modified during current migration")]
        SourceAndDestination,

        /// <summary>
        /// The resource is no more supported in the target release
        /// </summary>
        [ApiDescription("The resource is no more supported in the target release")]
        Obsolete
    }
}