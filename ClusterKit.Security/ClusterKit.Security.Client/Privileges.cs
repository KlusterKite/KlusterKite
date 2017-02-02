// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Privileges.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The list of default privileges
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Security.Client
{
    /// <summary>
    /// The list of default privileges
    /// </summary>
    public static class Privileges
    {
        /// <summary>
        /// The privilege that should be accepted only! in test environment to allow any operation
        /// </summary>
        public const string TestGrantAll = "ClusterKit.Security.TestGrantAll";
    }
}
