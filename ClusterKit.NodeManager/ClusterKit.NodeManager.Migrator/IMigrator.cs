// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMigrator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Base class for all cluster system migrations
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Migrator
{
    using System.Collections.Generic;

    /// <summary>
    /// Base class for all cluster system migrations
    /// </summary>
    public interface IMigrator
    {
        /// <summary>
        /// Gets the latest known migration point identification for this assembly
        /// </summary>
        string LatestPoint { get; }

        /// <summary>
        /// Gets the human-readable migrator name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the list of cluster resources that this migrator can migrate
        /// </summary>
        /// <returns>The list of migratable resources</returns>
        IEnumerable<ResourceId> GetMigratableResources();

        /// <summary>
        /// Gets the current configuration migration point for specified resource
        /// </summary>
        /// <param name="resourceId">The resource id</param>
        /// <returns>The current resource point</returns>
        string GetCurrentPoint(ResourceId resourceId);

        /// <summary>
        /// Performs the migration procedure
        /// </summary>
        /// <param name="resourceId">
        /// The resource id.
        /// </param>
        /// <param name="pointToMigrate">
        /// The point to migrate.
        /// </param>
        /// <returns>The list of log messages</returns>
        IEnumerable<string> Migrate(ResourceId resourceId, string pointToMigrate);

        /// <summary>
        /// Gets all possible migration points defined in this assembly
        /// </summary>
        /// <returns>The list of migration points</returns>
        IEnumerable<string> GetAllPoints();
    }
}
