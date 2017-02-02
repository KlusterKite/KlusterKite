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
        /// The privilege to get list of <see cref="User"/>
        /// </summary>
        public const string UserRead = "ClusterKit.NodeManager.UserRead";

        /// <summary>
        /// The privilege to create <see cref="User"/>
        /// </summary>
        public const string UserCreate = "ClusterKit.NodeManager.UserCreate";

        /// <summary>
        /// The privilege to update <see cref="User"/>
        /// </summary>
        public const string UserUpdate = "ClusterKit.NodeManager.UserUpdate";

        /// <summary>
        /// The privilege to reset <see cref="User"/> password
        /// </summary>
        public const string UserResetPassword = "ClusterKit.NodeManager.UserUpdate";

        /// <summary>
        /// The privilege to get list of <see cref="Role"/>
        /// </summary>
        public const string RoleRead = "ClusterKit.NodeManager.RoleRead";

        /// <summary>
        /// The privilege to create <see cref="Role"/>
        /// </summary>
        public const string RoleCreate = "ClusterKit.NodeManager.RoleCreate";

        /// <summary>
        /// The privilege to update <see cref="Role"/>
        /// </summary>
        public const string RoleUpdate = "ClusterKit.NodeManager.RoleUpdate";

        /// <summary>
        /// The privilege to delete <see cref="Role"/>
        /// </summary>
        public const string RoleDelete = "ClusterKit.NodeManager.RoleDelete";

        /// <summary>
        /// The privilege to create <see cref="NodeTemplate"/>
        /// </summary>
        public const string NodeTemplateCreate = "ClusterKit.NodeManager.NodeTemplateCreate";

        /// <summary>
        /// The privilege to update <see cref="NodeTemplate"/>
        /// </summary>
        public const string NodeTemplateUpdate = "ClusterKit.NodeManager.NodeTemplateUpdate";

        /// <summary>
        /// The privilege to delete <see cref="NodeTemplate"/>
        /// </summary>
        public const string NodeTemplateDelete = "ClusterKit.NodeManager.NodeTemplateDelete";

        /// <summary>
        /// The privilege to create <see cref="NugetFeed"/>
        /// </summary>
        public const string NugetFeedCreate = "ClusterKit.NodeManager.NugetFeedCreate";

        /// <summary>
        /// The privilege to update <see cref="NugetFeed"/>
        /// </summary>
        public const string NugetFeedUpdate = "ClusterKit.NodeManager.NugetFeedUpdate";

        /// <summary>
        /// The privilege to delete <see cref="NugetFeed"/>
        /// </summary>
        public const string NugetFeedDelete = "ClusterKit.NodeManager.NugetFeedDelete";

        /// <summary>
        /// The privilege to create <see cref="SeedAddress"/>
        /// </summary>
        public const string SeedAddressCreate = "ClusterKit.NodeManager.SeedAddressCreate";

        /// <summary>
        /// The privilege to update <see cref="SeedAddress"/>
        /// </summary>
        public const string SeedAddressUpdate = "ClusterKit.NodeManager.SeedAddressUpdate";

        /// <summary>
        /// The privilege to delete <see cref="SeedAddress"/>
        /// </summary>
        public const string SeedAddressDelete = "ClusterKit.NodeManager.SeedAddressDelete";

        /// <summary>
        /// The privilege to get actual nodes list
        /// </summary>
        public const string GetNodeList = "ClusterKit.NodeManager.GetNodeList";

        /// <summary>
        /// The privilege to get swagger link list
        /// </summary>
        public const string GetSwaggerList = "ClusterKit.NodeManager.GetSwaggerList";

        /// <summary>
        /// The privilege to initiate nuget cache update
        /// </summary>
        public const string UpdateNugetCache = "ClusterKit.NodeManager.UpdateNugetCache";

        /// <summary>
        /// The privilege to manually initiate node update (restart node)
        /// </summary>
        public const string ManualNodeUpdate = "ClusterKit.NodeManager.ManualNodeUpdate";

        /// <summary>
        /// Gets all defined privileges list
        /// </summary>
        /// <returns>The privileges list</returns>
        public static string[] GetAllPrivilegesList()
        {
            return new[]
                       {
                           GetNodeList, GetSwaggerList, ManualNodeUpdate, NodeTemplateCreate, NodeTemplateDelete,
                           NodeTemplateUpdate, NugetFeedCreate, NugetFeedDelete, NugetFeedUpdate, RoleCreate, RoleDelete,
                           RoleRead, RoleUpdate, SeedAddressCreate, SeedAddressDelete, SeedAddressUpdate,
                           UpdateNugetCache, UserCreate, UserRead, UserResetPassword, UserUpdate
                       };
        }
    }
}
