// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Privileges.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The list of defined module privileges
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client
{
    using KlusterKite.NodeManager.Client.ORM;
    using KlusterKite.Security.Attributes;

    /// <summary>
    /// The list of defined module privileges
    /// </summary>
    [PrivilegesContainer]
    public static class Privileges
    {
        /// <summary>
        /// The privilege to manage <see cref="User"/>
        /// </summary>
        [PrivilegeDescription("User management", "Get", "GetList", "Create", "Delete", "Update", "Query", Target = EnPrivilegeTarget.User)]
        public const string User = "KlusterKite.NodeManager.User";

        /// <summary>
        /// The privilege to reset any <see cref="User"/> password
        /// </summary>
        [PrivilegeDescription("Reset any user password", Target = EnPrivilegeTarget.User)]
        public const string UserResetPassword = "KlusterKite.NodeManager.UserResetPassword";

        /// <summary>
        /// The privilege to manage <see cref="ORM.Role"/>
        /// </summary>
        [PrivilegeDescription("Role management", "Get", "GetList", "Create", "Delete", "Update", "Query", Target = EnPrivilegeTarget.User)]
        public const string Role = "KlusterKite.NodeManager.Role";

        /// <summary>
        /// The privilege to grant and withdraw roles to users
        /// </summary>
        [PrivilegeDescription("The privilege to grant and withdraw roles to users", Target = EnPrivilegeTarget.User)]
        public const string UserRole = "KlusterKite.NodeManager.UserRole";

        /// <summary>
        /// The privilege to manage <see cref="ORM.Configuration"/>
        /// </summary>
        [PrivilegeDescription("Configuration management", "Get", "GetList", "Create", "Delete", "Update", "Query", Target = EnPrivilegeTarget.User)]
        public const string Configuration = "KlusterKite.NodeManager.Configuration";

        /// <summary>
        /// The privilege to set <see cref="EnConfigurationState.Ready"/> state for draft configurations
        /// </summary>
        [PrivilegeDescription("The privilege to set ready state for draft configurations", Target = EnPrivilegeTarget.User)]
        public const string ConfigurationFinish = "KlusterKite.NodeManager.ConfigurationFinish";

        /// <summary>
        /// The privilege to manage global cluster configuration
        /// </summary>
        [PrivilegeDescription("Cluster configuration management", Target = EnPrivilegeTarget.User)]
        public const string ClusterUpdate = "KlusterKite.NodeManager.ClusterUpdate";

        /// <summary>
        /// The privilege to get the list of all registered privileges
        /// </summary>
        [PrivilegeDescription("Get the list of all system defined privilege descriptions", Target = EnPrivilegeTarget.User)]
        public const string GetPrivilegesList = "KlusterKite.NodeManager.GetPrivilegesList";

        /// <summary>
        /// The privilege to get actual nodes list
        /// </summary>
        [PrivilegeDescription("Gets active nodes list", Target = EnPrivilegeTarget.User)]
        public const string GetActiveNodeDescriptions = "KlusterKite.NodeManager.GetActiveNodeDescriptions";

        /// <summary>
        /// Gets current cluster node template usage for debug purposes
        /// </summary>
        [PrivilegeDescription("Gets current cluster node template usage for debug purposes", Target = EnPrivilegeTarget.User)]
        public const string GetTemplateStatistics = "KlusterKite.NodeManager.GetTemplateStatistics";

        /// <summary>
        /// Gets the list of available packages from local cluster repository
        /// </summary>
        [PrivilegeDescription("Manually initiate update nuget packages cache list", Target = EnPrivilegeTarget.User)]
        public const string GetPackages = "KlusterKite.NodeManager.GetPackages";

        /// <summary>
        /// The privilege to initiate nuget cache update
        /// </summary>
        [PrivilegeDescription("Manually initiate update nuget packages cache list", Target = EnPrivilegeTarget.User)]
        public const string ReloadPackages = "KlusterKite.NodeManager.ReloadPackages";

        /// <summary>
        /// The privilege to get the list of available templates for specified container type for current cluster state
        /// </summary>
        [PrivilegeDescription("Get the list of available templates for specified container type for current cluster state", Target = EnPrivilegeTarget.ClientAndUser)]
        public const string GetAvailableTemplates = "KlusterKite.NodeManager.GetAvailableTemplates";

        /// <summary>
        /// The privilege to manually initiate node update (restart node)
        /// </summary>
        [PrivilegeDescription("Manually initiate node update (restart node)", Target = EnPrivilegeTarget.User)]
        public const string UpgradeNode = "KlusterKite.NodeManager.UpgradeNode";

        /// <summary>
        /// The privilege to get the configuration for the new empty node
        /// </summary>
        [PrivilegeDescription("Gets the configuration for the new empty node", Target = EnPrivilegeTarget.Client)]
        public const string GetConfiguration = "KlusterKite.NodeManager.GetConfiguration";

        /// <summary>
        /// View the migration history and its log
        /// </summary>
        [PrivilegeDescription("View the migration history and its log", "Get", "Query", Target = EnPrivilegeTarget.User)]
        public const string ClusterMigration = "KlusterKite.NodeManager.Migration";

        /// <summary>
        /// Get the current cluster resources states
        /// </summary>
        [PrivilegeDescription("Get the current cluster resources states", Target = EnPrivilegeTarget.User)]
        public const string GetResourceState = "KlusterKite.NodeManager.GetResourceState";

        /// <summary>
        /// Perform cluster migration operations
        /// </summary>
        [PrivilegeDescription("Perform cluster migration operations", Target = EnPrivilegeTarget.User)]
        public const string MigrateCluster = "KlusterKite.NodeManager.GetResourceState";
    }
}
