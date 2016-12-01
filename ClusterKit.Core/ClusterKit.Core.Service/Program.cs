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

    using JetBrains.Annotations;

    using Serilog;

    using Topshelf;

    /// <summary>
    /// Service main entry point
    /// </summary>
    [UsedImplicitly]
    public class Program
    {
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
            Container = new WindsorContainer();
            HostFactory.Run(
                x =>
                    {
                        var configurations = new List<string>();
                        x.AddCommandLineDefinition("config", fileName => configurations.Add(fileName));
                        x.ApplyCommandLine();
                        
                        // preset logger
                        var loggerConfig = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.ColoredConsole();
                        var logger = loggerConfig.CreateLogger();
                        Log.Logger = logger;

                        Bootstrapper.Configure(Container, configurations.ToArray());

                        x.Service<Controller>(
                            s =>
                                {
                                    s.ConstructUsing(name => Container.Resolve<Controller>());
                                    s.WhenStarted(
                                        (tc, hc) =>
                                            {
                                                Log.Logger.Warning("{Type}: Service started", "Topshelf");
                                                return tc.Start(Container, hc);
                                            });
                                    s.WhenStopped(
                                        tc =>
                                            {
                                                Log.Logger.Warning("{Type}: Service stopped", "Topshelf");
                                                tc.Stop();
                                                Container.Release(tc);
                                                Container.Dispose();
                                            });
                                });

                        x.StartAutomatically();
                        x.RunAsLocalSystem();
                        x.SetDescription("ClusterKit Node service");
                        x.SetDisplayName("ClusterKitNode");
                        x.SetServiceName("ClusterKitNode");
                        x.UseLinuxIfAvailable();
                        x.OnException(e => Log.Logger.Error(e, "{Type}: exception", "Topshelf"));
                        x.UseSerilog();
                    });
        }
    }
}