// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestPrivileges.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The test privileges
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Security.Tests
{
    using KlusterKite.Security.Attributes;

    using JetBrains.Annotations;

    /// <summary>
    /// The test privileges
    /// </summary>
    [PrivilegesContainer]
    [UsedImplicitly]
    public static class TestPrivileges
    {
        /// <summary>
        /// Some privilege with no description
        /// </summary>
        [UsedImplicitly]
        public const string NoDescriptionPrivilege = "NoDescriptionPrivilege";

        /// <summary>
        /// Some privilege with description
        /// </summary>
        [UsedImplicitly]
        [PrivilegeDescription("Some privilege with description")]
        public const string DescribedPrivilege = "DescribedPrivilege";

        /// <summary>
        /// Some privilege with description
        /// </summary>
        [UsedImplicitly]
        [PrivilegeDescription("Some privilege with description", Target = EnPrivilegeTarget.User)]
        public const string DescribedUserPrivilege = "DescribedUserPrivilege";

        /// <summary>
        /// Some privilege with description and actions
        /// </summary>
        [UsedImplicitly]
        [PrivilegeDescription("Action privilege", "Get", "Set")]
        public const string ActionsPrivilege = "ActionsPrivilege";
    }
}
