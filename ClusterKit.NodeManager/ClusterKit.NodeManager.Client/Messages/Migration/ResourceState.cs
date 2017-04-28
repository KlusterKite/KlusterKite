// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceState.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The overall resource migration state
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.Messages.Migration
{
    using ClusterKit.API.Attributes;
    
    /// <summary>
    /// The overall resource migration state
    /// </summary>
    [ApiDescription("The overall resource migration state", Name = "ResourceState")]
    public class ResourceState
    {
        /// <summary>
        /// Gets or sets the current resource state with active migration
        /// </summary>
        [DeclareField("the current resource state with active migration")]
        public MigrationActorMigrationState MigrationState { get; set; }

        /// <summary>
        /// Gets or sets the current resource state without active migration
        /// </summary>
        [DeclareField("the current resource state without active migration")]
        public MigrationActorReleaseState ReleaseState { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether some resource operation is in progress
        /// </summary>
        [DeclareField("a value indicating whether some resource operation is in progress")]
        public bool OperationIsInProgress { get; set; }
    }
}
