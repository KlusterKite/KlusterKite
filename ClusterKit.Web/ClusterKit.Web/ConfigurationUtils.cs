// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationUtils.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Bundle of web related configuration utils
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web
{
    using System;

    using Akka.Actor;
    using Akka.Configuration;

    /// <summary>
    /// Bundle of web related configuration utils
    /// </summary>
    public static class ConfigurationUtils
    {
        /// <summary>
        /// Gets the default timeout from rest api to actor system
        /// </summary>
        /// <param name="actorSystem">The actor system</param>
        /// <returns>Default timeout</returns>
        public static TimeSpan GetRestTimeout(ActorSystem actorSystem)
        {
            return GetRestTimeout(actorSystem.Settings.Config);
        }

        /// <summary>
        /// Gets the default timeout from rest api to actor system
        /// </summary>
        /// <param name="config">The actor system configuration (assumed that this should be the root of configuration)</param>
        /// <returns>Default timeout</returns>
        public static TimeSpan GetRestTimeout(Config config)
        {
            return config.GetTimeSpan("ClusterKit.Web.RestTimeout", TimeSpan.FromSeconds(10), false);
        }
    }
}