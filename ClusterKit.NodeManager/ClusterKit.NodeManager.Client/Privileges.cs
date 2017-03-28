// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Privileges.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The list of defined module privileges
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client
{
    using ClusterKit.Security.Client;
    using ClusterKit.Security.Client.Attributes;

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
        public const string User = "ClusterKit.NodeManager.User";

        /// <summary>
        /// The privilege to manage <see cref="ORM.Role"/>
        /// </summary>
        [PrivilegeDescription("Role management", "Get", "GetList", "Create", "Delete", "Update", "Query", Target = EnPrivilegeTarget.User)]
        public const string Role = "ClusterKit.NodeManager.Role";

        /// <summary>
        /// The privilege to manage <see cref="ORM.Role"/>
        /// </summary>
        [PrivilegeDescription("Release management", "Get", "GetList", "Create", "Delete", "Update", "Query", Target = EnPrivilegeTarget.User)]
        public const string Release = "ClusterKit.NodeManager.Role";

        /// <summary>
        /// The privilege to manage <see cref="NodeTemplate"/>
        /// </summary>
        [PrivilegeDescription("Node template management", "Get", "GetList", "Create", "Delete", "Update", "Query", Target = EnPrivilegeTarget.User)]
        public const string NodeTemplate = "ClusterKit.NodeManager.NodeTemplate";

        /// <summary>
        /// The privilege to manage <see cref="NugetFeed"/>
        /// </summary>
        [PrivilegeDescription("Nuget feed management", "Get", "GetList", "Create", "Delete", "Update", "Query", Target = EnPrivilegeTarget.User)]
        public const string NugetFeed = "ClusterKit.NodeManager.NugetFeed";

        /// <summary>
        /// The privilege to manage <see cref="SeedAddress"/>
        /// </summary>
        [PrivilegeDescription("Seed addresses management", "Get", "GetList", "Create", "Delete", "Update", "Query", Target = EnPrivilegeTarget.User)]
        public const string SeedAddress = "ClusterKit.NodeManager.SeedAddress";

        /// <summary>
        /// The privilege to get the list of all registered privileges
        /// </summary>
        [PrivilegeDescription("Get the list of all system defined privilege descriptions", Target = EnPrivilegeTarget.User)]
        public const string GetPrivilegesList = "ClusterKit.NodeManager.GetPrivilegesList";

        /// <summary>
        /// The privilege to get actual nodes list
        /// </summary>
        [PrivilegeDescription("Gets active nodes list", Target = EnPrivilegeTarget.User)]
        public const string GetActiveNodeDescriptions = "ClusterKit.NodeManager.GetActiveNodeDescriptions";

        /// <summary>
        /// Gets current cluster node template usage for debug purposes
        /// </summary>
        [PrivilegeDescription("Gets current cluster node template usage for debug purposes", Target = EnPrivilegeTarget.User)]
        public const string GetTemplateStatistics = "ClusterKit.NodeManager.GetTemplateStatistics";

        /// <summary>
        /// Gets the list of available packages from local cluster repository
        /// </summary>
        [PrivilegeDescription("Manually initiate update nuget packages cache list", Target = EnPrivilegeTarget.User)]
        public const string GetPackages = "ClusterKit.NodeManager.GetPackages";

        /// <summary>
        /// The privilege to initiate nuget cache update
        /// </summary>
        [PrivilegeDescription("Manually initiate update nuget packages cache list", Target = EnPrivilegeTarget.User)]
        public const string ReloadPackages = "ClusterKit.NodeManager.ReloadPackages";

        /// <summary>
        /// The privilege to get the list of available templates for specified container type for current cluster state
        /// </summary>
        [PrivilegeDescription("Get the list of available templates for specified container type for current cluster state", Target = EnPrivilegeTarget.ClientAndUser)]
        public const string GetAvailableTemplates = "ClusterKit.NodeManager.GetAvailableTemplates";

        /// <summary>
        /// The privilege to manually initiate node update (restart node)
        /// </summary>
        [PrivilegeDescription("Manually initiate node update (restart node)", Target = EnPrivilegeTarget.User)]
        public const string UpgradeNode = "ClusterKit.NodeManager.UpgradeNode";

        /// <summary>
        /// The privilege to get the configuration for the new empty node
        /// </summary>
        [PrivilegeDescription("Gets the configuration for the new empty node", Target = EnPrivilegeTarget.Client)]
        public const string GetConfiguration = "ClusterKit.NodeManager.GetConfiguration";
    }
}
