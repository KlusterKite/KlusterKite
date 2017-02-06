// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Privileges.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The list of defined module privileges
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Swagger.Messages
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
        /// The privilege to get current cluster defined swagger links
        /// </summary>
        [PrivilegeDescription("Get current cluster defined swagger links", Target = EnPrivilegeTarget.User)]
        public const string GetServices = "ClusterKit.Web.Swagger.GetServices";
    }
}
