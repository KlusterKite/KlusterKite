// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Application.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Main application class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.Service
{
    using Akka.Actor;
    using Castle.Windsor;

    /// <summary>
    /// Main application class
    /// </summary>
    public static class Application
    {
        /// <summary>
        /// Gets the actor system
        /// </summary>
        public static ActorSystem ActorSystem { get; private set; }

        /// <summary>
        /// Gets a value indicating whether system is up an ready
        /// </summary>
        public static bool IsReady { get; private set; }

        /// <summary>
        /// Service start
        /// </summary>
        /// <param name="actorSystem">
        /// the actor system
        /// </param>
        /// <param name="container">
        /// The dependency injection container
        /// </param>
        public static void Start(ActorSystem actorSystem, IWindsorContainer container)
        {
            ActorSystem = actorSystem;
            IsReady = true;
        }

        /// <summary>
        /// Service stop
        /// </summary>
        public static void Stop()
        {
            IsReady = false;
        }
    }
}