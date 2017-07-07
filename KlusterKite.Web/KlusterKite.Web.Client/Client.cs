// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Client.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Access to client methods
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Web.Client
{
    using System;

    using Akka.Actor;

    using JetBrains.Annotations;

    /// <summary>
    /// Access to client methods
    /// </summary>
    public static class Client
    {
        /// <summary>
        /// Gets path to web descriptor for node with role "web"
        /// </summary>
        /// <param name="system">The actor system</param>
        /// <param name="nodeAddress">The web node</param>
        /// <returns>Path to web descriptor</returns>
        public static ICanTell GetWebDescriptor(this ActorSystem system, [NotNull] Address nodeAddress)
        {
            if (nodeAddress == null)
            {
                throw new ArgumentNullException(nameof(nodeAddress));
            }

            return system.ActorSelection($"{nodeAddress}/user/Web/Descriptor");
        }
    }
}