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
    using ClusterKit.NodeManager.Client.ORM;
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
        [PrivilegeDescription("User management", "Get", "GetList", "Create", "Delete", "Update")]
        public const string User = "ClusterKit.NodeManager.User";

        /// <summary>
        /// The privilege to manage <see cref="Role"/>
        /// </summary>
        [PrivilegeDescription("Role management", "Get", "GetList", "Create", "Delete", "Update")]
        public const string RoleRead = "ClusterKit.NodeManager.Role";

        /// <summary>
        /// The privilege to manage <see cref="NodeTemplate"/>
        /// </summary>
        [PrivilegeDescription("Node template management", "Get", "GetList", "Create", "Delete", "Update")]
        public const string NodeTemplate = "ClusterKit.NodeManager.NodeTemplate";

        /// <summary>
        /// The privilege to manage <see cref="NugetFeed"/>
        /// </summary>
        [PrivilegeDescription("Nuget feed management", "Get", "GetList", "Create", "Delete", "Update")]
        public const string NugetFeed = "ClusterKit.NodeManager.NugetFeed";

        /// <summary>
        /// The privilege to manage <see cref="SeedAddress"/>
        /// </summary>
        [PrivilegeDescription("Seed addresses management", "Get", "GetList", "Create", "Delete", "Update")]
        public const string SeedAddress = "ClusterKit.NodeManager.SeedAddress";

        /// <summary>
        /// The privilege to get swagger link list
        /// </summary>
        [PrivilegeDescription("Gets discovered swagger links")]
        public const string GetSwaggerList = "ClusterKit.NodeManager.GetSwaggerList";

        /// <summary>
        /// The privilege to get the list of all registered privileges
        /// </summary>
        [PrivilegeDescription("Get the list of all system defined privilege descriptions")]
        public const string GetPrivilegesList = "ClusterKit.NodeManager.GetPrivilegesList";

        /// <summary>
        /// The privilege to get actual nodes list
        /// </summary>
        [PrivilegeDescription("Gets active nodes list")]
        public const string GetActiveNodeDescriptions = "ClusterKit.NodeManager.GetActiveNodeDescriptions";

        /// <summary>
        /// Gets current cluster node template usage for debug purposes
        /// </summary>
        [PrivilegeDescription("Gets current cluster node template usage for debug purposes")]
        public const string GetTemplateStatistics = "ClusterKit.NodeManager.GetTemplateStatistics";

        /// <summary>
        /// Gets the list of available packages from local cluster repository
        /// </summary>
        [PrivilegeDescription("Manually initiate update nuget packages cache list")]
        public const string GetPackages = "ClusterKit.NodeManager.GetPackages";

        /// <summary>
        /// The privilege to initiate nuget cache update
        /// </summary>
        [PrivilegeDescription("Manually initiate update nuget packages cache list")]
        public const string ReloadPackages = "ClusterKit.NodeManager.ReloadPackages";

        /// <summary>
        /// The privilege to manually initiate node update (restart node)
        /// </summary>
        [PrivilegeDescription("Manually initiate node update (restart node)")]
        public const string UpgradeNode = "ClusterKit.NodeManager.UpgradeNode";
    }
}
