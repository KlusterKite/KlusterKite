// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Controller.cs" company="TaxiKit">
//   All rights reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace TaxiKit.Core.Service
{
    using System.Threading;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Event;
    using Castle.Windsor;

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
        /// Service start cancellation token (in case of service stop, before service start was actualy completed)
        /// </summary>
        private readonly CancellationTokenSource startCancellationToken = new CancellationTokenSource();

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
        /// Main system logger
        /// </summary>
        private ILoggingAdapter Logger => this.actorSystem.Log;

        /// <summary>
        /// Запуск сервиса
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <returns>
        /// Успех запуска
        /// </returns>
        public bool Start(IWindsorContainer container)
        {
            Task.Factory.StartNew(
                () =>
                    {
                        Application.Start(this.actorSystem, container);
                        this.Logger.Info("Service started.");
                    },
                this.startCancellationToken.Token);
            this.Logger.Info("Service start initiated.");
            return true;
        }

        /// <summary>
        /// Остановка сервиса
        /// </summary>
        public void Stop()
        {
            this.startCancellationToken.Cancel(false);
            this.actorSystem.Shutdown();
            Application.Stop();
            this.Logger.Info("Service was stopped.");
        }
    }
}