// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClusterExtensions.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Bundle of extension methods to fake cluster internal messages
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.TestKit
{
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    using Akka.Cluster;

    /// <summary>
    /// Bundle of extension methods to fake cluster internal messages
    /// </summary>
    public static class ClusterExtensions
    {
        /// <summary>
        /// Creates a member from internal Akka method
        /// </summary>
        /// <param name="uniqueAddress">
        /// The unique Address.
        /// </param>
        /// <param name="upNumber">
        /// The up number of gossip message.
        /// </param>
        /// <param name="status">
        /// The member status.
        /// </param>
        /// <param name="roles">
        /// The list of roles.
        /// </param>
        /// <returns>
        /// The new member
        /// </returns>
        // ReSharper disable once StyleCop.SA1305
        public static Member MemberCreate(UniqueAddress uniqueAddress, int upNumber, MemberStatus status, ImmutableHashSet<string> roles)
        {
            var createMethod =
                typeof(Member).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .First(m => m.Name == "Create" && m.GetParameters().Length == 4);

            return (Member)createMethod.Invoke(null, new object[] { uniqueAddress, upNumber, status, roles });
        }
    }
}