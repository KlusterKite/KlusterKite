// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Controller.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.Service
{
    using System.Threading;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Event;
    using Castle.Windsor;

    using Serilog;

    using Topshelf;

    /// <summary>
    /// Service controller
    /// </summary>
    public class Controller
    {
        /// <summary>
        /// The actor system
        /// </summary>
        private readonly ActorSystem actorSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="Controller"/> class.
        /// </summary>
        /// <param name="actorSystem">
        /// The actor system.
        /// </param>
        public Controller(ActorSystem actorSystem)
        {
            this.actorSystem = actorSystem;
        }

        /// <summary>
        /// Service startup
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <param name="hostControl">The running service control</param>
        /// <returns>
        /// The success of service startup
        /// </returns>
        public bool Start(IWindsorContainer container, HostControl hostControl)
        {
            Log.Logger.Information("Service starting");
            this.actorSystem.WhenTerminated.ContinueWith(task => hostControl.Stop());
            this.actorSystem.StartNameSpaceActorsFromConfiguration();
            BaseInstaller.RunPostStart(container);
            Log.Logger.Information("Service started");
            return true;
        }

        /// <summary>
        /// Service stop
        /// </summary>
        public void Stop()
        {
            this.actorSystem.Terminate().Wait();
            Log.Logger.Information("Service was stopped.");
        }
    }
}