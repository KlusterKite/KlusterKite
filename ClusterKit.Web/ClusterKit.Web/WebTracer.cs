// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebTracer.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Debug configuration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Akka.Actor;
    using Akka.Configuration;

    using JetBrains.Annotations;

    using Microsoft.Owin;

    using Owin;

    /// <summary>
    /// Debug configuration
    /// </summary>
    [UsedImplicitly]
    public class WebTracer : IOwinStartupConfigurator
    {
        /// <summary>
        /// The actor system
        /// </summary>
        /// <remarks>Just for debugging</remarks>
        // ReSharper disable once NotAccessedField.Local
        private readonly ActorSystem system;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebTracer"/> class.
        /// </summary>
        /// <param name="system">The actor system</param>
        /// <param name="config">The system configuration</param>
        public WebTracer(ActorSystem system, Config config)
        {
            this.system = system;
        }

        /// <summary>
        /// Add additional http configuration
        /// </summary>
        /// <param name="httpConfiguration">The configuration</param>
        public void ConfigureApi(HttpConfiguration httpConfiguration)
        {
        }

        /// <summary>
        /// Add additional owin configuration
        /// </summary>
        /// <param name="appBuilder">The builder</param>
        public void ConfigureApp(IAppBuilder appBuilder)
        {
            appBuilder.Use<TraceMiddleware>(this.system);
        }

        /// <summary>
        /// Request process logging. Used to catch request loss
        /// </summary>
        [UsedImplicitly]
        public class TraceMiddleware : OwinMiddleware
        {
            /// <summary>
            /// The current request number
            /// </summary>
            private static long requestNumber;

            /// <summary>
            /// The actor system
            /// </summary>
            private readonly ActorSystem system;

            /// <summary>
            /// A value to trace every web request
            /// </summary>
            private readonly bool trace;

            /// <summary>
            /// Initializes a new instance of the <see cref="TraceMiddleware"/> class.
            /// </summary>
            /// <param name="next">
            /// The next.
            /// </param>
            /// <param name="system">
            /// The system.
            /// </param>
            public TraceMiddleware(OwinMiddleware next, ActorSystem system)
                : base(next)
            {
                this.system = system;
                this.trace = system.Settings.Config.GetBoolean("ClusterKit.Web.Debug.Trace");
            }

            /// <summary>Process an individual request.</summary>
            /// <param name="context">The request context</param>
            /// <returns>The async process task</returns>
            public override async Task Invoke(IOwinContext context)
            {
                if (this.trace)
                {
                    var n = Interlocked.Increment(ref requestNumber);
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    this.system.Log.Info(
                        "{Type}: started Request {RequestNumber} {RequestPath}",
                        this.GetType().Name,
                        n,
                        context.Request.Path);


                    try
                    {
                        await this.Next.Invoke(context);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine($@"Web exception: {exception.Message} \n {exception.StackTrace}");

                        this.system.Log.Error(
                            exception,
                            "{Type}: error resolving {RequestNumber} {RequestPath}",
                            this.GetType().Name,
                            n,
                            context.Request.Path);
                        throw;
                    }

                    this.system.Log.Info(
                        "{Type}: finished Request {RequestNumber} {RequestPath} in {ElapsedMilliseconds}ms",
                        this.GetType().Name,
                        n,
                        context.Request.Path,
                        stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    try
                    {
                        await this.Next.Invoke(context);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine($@"Web exception: {exception.Message} \n {exception.StackTrace}");

                        this.system.Log.Error(
                            exception,
                            "{Type}: error resolving {RequestPath}",
                            this.GetType().Name,
                            context.Request.Path);
                        throw;
                    }
                }
            }
        }
    }
}