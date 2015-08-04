// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="TaxiKit">
//   All rights reserved
// </copyright>
// <summary>
//   Service main entry point
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TaxiKit.Core.Service
{
    using Castle.Windsor;

    using Serilog;

    using Topshelf;

    /// <summary>
    /// Service main entry point
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Gets the dependency injection container
        /// </summary>
        public static IWindsorContainer Container { get; private set; }

        /// <summary>
        /// Service main entry point
        /// </summary>
        /// <param name="args">
        /// Startup parameters
        /// </param>
        public static void Main(string[] args)
        {
            Container = new WindsorContainer();

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.ColoredConsole();

            var logger = loggerConfig.CreateLogger();
            Log.Logger = logger;
            Bootstrapper.Configure(Container);
            HostFactory.Run(
                            x =>
                            {
                                x.Service<Controller>(
                                    s =>
                                    {
                                        s.ConstructUsing(name => Container.Resolve<Controller>());
                                        s.WhenStarted((tc, hc) => tc.Start(Container));
                                        s.WhenStopped(
                                            tc =>
                                            {
                                                tc.Stop();
                                                Container.Release(tc);
                                                Container.Dispose();
                                            });
                                    });

                                x.EnableServiceRecovery(
                                    rc =>
                                    {
                                        rc.RestartService(1);
                                        rc.SetResetPeriod(7);
                                    });

                                x.UseSerilog(loggerConfig);
                                x.StartAutomatically();
                                x.RunAsLocalSystem();
                                x.SetDescription("TaxiKit Node service");
                                x.SetDisplayName("TaxiKitNode");
                                x.SetServiceName("TaxiKitNode");
                                x.UseLinuxIfAvailable();
                            });
        }
    }
}