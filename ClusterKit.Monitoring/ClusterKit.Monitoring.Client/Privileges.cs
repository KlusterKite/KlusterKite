// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Privileges.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The list of defined module privileges
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Monitoring.Client
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
        /// The privilege to get the last cluster scan result
        /// </summary>
        [PrivilegeDescription("Get the last cluster scan result", Target = EnPrivilegeTarget.User)]
        public const string GetClusterTree = "ClusterKit.Monitoring.GetClusterTree";

        /// <summary>
        /// The privilege to initiate the new actor system scan
        /// </summary>
        [PrivilegeDescription("Initiate the new actor system scan", Target = EnPrivilegeTarget.User)]
        public const string InitiateScan = "ClusterKit.Monitoring.InitiateScan";
    }
}
