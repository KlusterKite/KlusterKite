namespace ClusterKit.Web
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Akka.Actor;
    using Akka.Configuration;
    using Akka.Event;

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
        private ActorSystem system;

        /// <summary>
        /// The system configuration
        /// </summary>
        private Config config;

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
            if (this.config.GetBoolean("ClusterKit.Web.Debug.Trace"))
            {
                appBuilder.Use<TraceMiddleware>();
            }
        }

        /// <summary>
        /// Creates the new instance of <see cref="WebTracer"/>
        /// </summary>
        /// <param name="system">The actor system</param>
        /// <param name="config">The system configuration</param>
        public WebTracer(ActorSystem system, Config config)
        {
            this.system = system;
            this.config = config;
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
            /// Creates the new instance of the <see cref="TraceMiddleware"/>
            /// </summary>
            /// <param name="next">The next middleware in the conveyor</param>
            public TraceMiddleware(OwinMiddleware next)
                : base(next)
            {
            }

            /// <summary>Process an individual request.</summary>
            /// <param name="context">The request context</param>
            /// <returns>The async process task</returns>
            public override async Task Invoke(IOwinContext context)
            {
                var n = Interlocked.Increment(ref requestNumber);
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                Console.WriteLine($@"Owin started Request {n} {context.Request.Path}");
                await this.Next.Invoke(context);
                Console.WriteLine($@"Owin finished Request {n} {context.Request.Path} in {stopwatch.ElapsedMilliseconds}ms");
            }
        }
    }
}