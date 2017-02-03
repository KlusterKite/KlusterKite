// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrivilegeDescriptionTests.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing the privilege description collection
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Security.Tests
{
    using System.Linq;

    using Akka.Configuration;

    using ClusterKit.Security.Client;
    using ClusterKit.Security.Client.Attributes;

    using JetBrains.Annotations;

    using Xunit;

    /// <summary>
    /// Testing the privilege description collection
    /// </summary>
    public class PrivilegeDescriptionTests
    {
        /// <summary>
        /// Test privilege discovering
        /// </summary>
        [Fact]
        public void PrivilegeDiscoverTest()
        {
            new Installer().PreCheck(Config.Empty);
            Assert.Equal(7, Utils.DefinedPrivileges.Count);
        }

        /// <summary>
        /// Test privilege extraction
        /// </summary>
        [Fact]
        public void PrivilegeGetListTest()
        {
            var description = Utils.GetDefinedPrivileges(typeof(TestPrivileges)).ToList();
            Assert.Equal(5, description.Count);

            var userPrivilege = description.First(d => d.Privilege == TestPrivileges.DescribedUserPrivilege);
            Assert.Equal(EnPrivilegeTarget.User, userPrivilege.Target);
        }

        /// <summary>
        /// Nested privileges will be discovered
        /// </summary>
        [PrivilegesContainer]
        public static class NestedPrivileges
        {
            /// <summary>
            /// Some privilege with description
            /// </summary>
            [UsedImplicitly]
            [PrivilegeDescription("Some nested privilege")]
            public const string NestedPrivilege = "NestedPrivilege";
        }

        /// <summary>
        /// Private privileges will be discovered
        /// </summary>
        [PrivilegesContainer]
        private static class PrivatePrivileges
        {
            /// <summary>
            /// Some privilege with description
            /// </summary>
            [UsedImplicitly]
            [PrivilegeDescription("Some private privilege")]
            public const string PrivatePrivilege = "PrivatePrivilege";
        }
    }
}
