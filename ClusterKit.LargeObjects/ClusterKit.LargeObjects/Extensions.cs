// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Extensions.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Bundle of methods to provide access to class extensions
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.LargeObjects
{
    using Akka.Actor;

    using JetBrains.Annotations;

    /// <summary>
    /// Bundle of methods to provide access to class extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets the current parcel manager
        /// </summary>
        /// <param name="system">The actor system</param>
        /// <returns>The current parcel manager</returns>
        [UsedImplicitly]
        public static ICanTell GetParcelManager([NotNull] this ActorSystem system)
        {
            var path = GetParcelManagerPath(system);
            return system.ActorSelection(path);
        }

        /// <summary>
        /// Gets the current parcel manager
        /// </summary>
        /// <param name="context">The actor context</param>
        /// <returns>The current parcel manager</returns>
        [UsedImplicitly]
        public static ICanTell GetParcelManager([NotNull] this IActorContext context)
        {
            var path = GetParcelManagerPath(context.System);
            return context.ActorSelection(path);
        }

        /// <summary>
        /// Gets the current parcel manager path
        /// </summary>
        /// <param name="system">The actor system</param>
        /// <returns>The current parcel manager path</returns>
        private static string GetParcelManagerPath(ActorSystem system)
        {
            return system.Settings.Config.GetString(
                "ClusterKit.ParcelsPath",
                "/user/ClusterKit/Parcels");
        }
    }
}
