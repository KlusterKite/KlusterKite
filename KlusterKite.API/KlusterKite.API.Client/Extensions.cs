// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Extensions.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the Extensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client
{
    using System;

    using Akka.Actor;

    using JetBrains.Annotations;

    /// <summary>
    /// Grants access to API utilities
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets path to web descriptor for node with role "web"
        /// </summary>
        /// <param name="context">The actor context</param>
        /// <param name="nodeAddress">The web node</param>
        /// <returns>Path to web descriptor</returns>
        public static ICanTell GetApiPublisher(this IActorContext context, [NotNull] Address nodeAddress)
        {
            if (nodeAddress == null)
            {
                throw new ArgumentNullException(nameof(nodeAddress));
            }

            return context.ActorSelection($"{nodeAddress}/user/ClusterKit/API/Publisher");
        }
    }
}
