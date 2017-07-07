// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Privileges.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The list of defined privileges
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Authorization
{
    /// <summary>
    /// The list of defined privileges
    /// </summary>
    public static class Privileges
    {
        /// <summary>
        /// The privilege to use "grant password" authentication
        /// </summary>
        public const string ImplicitGrantPassword = "ClusterKit.Web.Authorization.ImplicitGrantPassword";
    }
}
