// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Navigation.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Gets the set of actor paths
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client
{
    using Akka.Actor;
    using Akka.Configuration;

    using JetBrains.Annotations;

    /// <summary>
    /// Gets the set of configured actor paths
    /// </summary>
    public static class Navigation
    {
        /// <summary>
        /// Multithread lock for managing cache
        /// </summary>
        private static readonly object LockObject = new object();

        /// <summary>
        /// The cached node manager path
        /// </summary>
        private static string nodeManagerPath;

        /// <summary>
        /// Gets or sets a value indicating whether path values should be cached in statics
        /// </summary>
        [UsedImplicitly]
        public static bool UseCacheForPaths { get; set; } = true;

        /// <summary>
        /// Gets the current node manager actor path
        /// </summary>
        /// <param name="context">
        /// The current context
        /// </param>
        /// <returns>
        /// The <see cref="ICanTell"/>.
        /// </returns>
        [UsedImplicitly]
        public static ICanTell GetNodeManager([NotNull] this IActorContext context)
        {
            var path = GetNodeManagerPath(context.System.Settings.Config);
            return context.ActorSelection(path);
        }

        /// <summary>
        /// Gets the current node manager actor path
        /// </summary>
        /// <param name="system">
        /// The current actor system
        /// </param>
        /// <returns>
        /// The <see cref="ICanTell"/>.
        /// </returns>
        [UsedImplicitly]
        public static ICanTell GetNodeManager([NotNull] this ActorSystem system)
        {
            var path = GetNodeManagerPath(system.Settings.Config);
            return system.ActorSelection(path);
        }

        /// <summary>
        /// Gets the current node manager actor path
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        /// <returns>
        /// The current node manager path
        /// </returns>
        private static string GetNodeManagerPath(Config config)
        {
            var path = GetPath(
                ref nodeManagerPath,
                config,
                "KlusterKite.NodeManager.NodeManagerPath",
                "/user/NodeManager/NodeManagerProxy");
            return path;
        }

        /// <summary>
        /// Gets the path to some static actor with cache
        /// </summary>
        /// <param name="cachedValue">
        /// The reference to static variable with cached value
        /// </param>
        /// <param name="config">
        /// The config.
        /// </param>
        /// <param name="configPath">
        /// The path to the configured value
        /// </param>
        /// <param name="defaultValue">
        /// The default value
        /// </param>
        /// <returns>
        /// The path
        /// </returns>
        private static string GetPath(ref string cachedValue, Config config, string configPath, string defaultValue)
        {
            if (!string.IsNullOrEmpty(cachedValue) && UseCacheForPaths)
            {
                return cachedValue;
            }

            if (!UseCacheForPaths)
            {
                return config.GetString(configPath, defaultValue);
            }

            lock (LockObject)
            {
                if (!string.IsNullOrEmpty(cachedValue))
                {
                    return cachedValue;
                }

                cachedValue = config.GetString(configPath, defaultValue);
                return cachedValue;
            }
        }
    }
}
