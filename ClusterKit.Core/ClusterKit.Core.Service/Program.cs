// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Service main entry point
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.Service
{
    using System;
    using System.Collections.Generic;

    using Castle.Windsor;

    using DocoptNet;

    using JetBrains.Annotations;

    using Serilog;

    /// <summary>
    /// Service main entry point
    /// </summary>
    [UsedImplicitly]
    public class Program
    {
        /// <summary>
        /// Help description for command run
        /// </summary>
        private const string CommandUsage =
@"Usage: ClusterKit.Core.Service.exe [--config=<file>]
--config=<file> file to load as top-level config
";

        /// <summary>
        /// Gets or sets the dependency injection container
        /// </summary>
        private static IWindsorContainer Container { get; set; }

        /// <summary>
        /// Service main entry point
        /// </summary>
        /// <param name="args">
        /// Startup parameters
        /// </param>
        public static void Main(string[] args)
        {
            // preset logger
            var loggerConfig = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.ColoredConsole();
            var logger = loggerConfig.CreateLogger();
            Log.Logger = logger;

            var arguments = new Docopt().Apply(CommandUsage, args, exit: true);
            var configurations = new List<string>();

            ValueObject config;
            if (arguments.TryGetValue("--config", out config) && config != null)
            {
                configurations.Add(config.ToString());
            }

            Container = new WindsorContainer();

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                {
                    Log.Logger.Error(
                        eventArgs.ExceptionObject as Exception,
                        "{Type}: Unhandled domain exception from {SenderType}, terminating: {IsTerminating}", 
                        "System",
                        sender?.GetType().Name ?? "unknown",
                        eventArgs.IsTerminating);
                };

            var system = Bootstrapper.ConfigureAndStart(Container, configurations.ToArray());
            Log.Logger.Warning("{Type}: Started", "System");
            Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    Log.Logger.Warning("{Type}: Shutdown sequence initiated", "System");
                    var cluster = Akka.Cluster.Cluster.Get(system);

                    var timeout = TimeSpan.FromSeconds(10);
                    if (cluster.IsTerminated || cluster.State.Members.Count == 0)
                    {
                        system.Terminate().Wait(timeout);
                    }
                    else
                    {
                        cluster.LeaveAsync().Wait(timeout);
                        system.Terminate().Wait(timeout);
                    }

                    Log.Logger.Warning("{Type}: Hard stopped", "System");
                    eventArgs.Cancel = true;
                };

            system.StartNameSpaceActorsFromConfiguration();
            BaseInstaller.RunPostStart(Container);

            var waitedTask = new System.Threading.Tasks.TaskCompletionSource<bool>();
            system.WhenTerminated.ContinueWith(task => waitedTask.SetResult(true));
            waitedTask.Task.Wait();
            Log.Logger.Warning("{Type}: Stopped", "System");
        }
    }
}