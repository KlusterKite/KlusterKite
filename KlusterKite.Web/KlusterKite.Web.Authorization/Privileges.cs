// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Privileges.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The list of defined privileges
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Web.Authorization
{
    /// <summary>
    /// The list of defined privileges
    /// </summary>
    public static class Privileges
    {
        /// <summary>
        /// The privilege to use "grant password" authentication
        /// </summary>
        public const string ImplicitGrantPassword = "KlusterKite.Web.Authorization.ImplicitGrantPassword";
    }
}
