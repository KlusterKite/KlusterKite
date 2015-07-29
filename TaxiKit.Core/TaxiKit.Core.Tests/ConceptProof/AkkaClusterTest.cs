namespace TaxiKit.Core.Tests.ConceptProof
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Reactive.Linq;
    using System.Text;
    using System.Threading;
    using Akka.Actor;
    using Akka.Configuration;
    using Akka.Configuration.Hocon;

    using Serilog;

    using Xunit;
    using Xunit.Abstractions;

    using static Serilog.Log;

    /// <summary>
    /// Akka cluster capability testing
    /// </summary>
    public class AkkaClusterTest
    {
        public AkkaClusterTest(ITestOutputHelper output)
        {
            var loggerConfig = new LoggerConfiguration().WriteTo.TextWriter(new OutputWriter(output));
            Logger = loggerConfig.CreateLogger();
        }

        /// <summary>
        /// Experimenting with akka cluster initialization
        /// </summary>
        [Fact]
        public void SimpleClusterTest()
        {
            var section = (AkkaConfigurationSection)ConfigurationManager.GetSection("akka");
            var ports = new[] { "2551", "2552", "0", "0" };

            foreach (var port in ports)
            {
                // Override the configuration of the port
                var config =
                    ConfigurationFactory.ParseString("akka.remote.helios.tcp.port=" + port)
                        .WithFallback(section.AkkaConfig);

                // create an Akka system
                var system = ActorSystem.Create("ClusterSystem", config);
                Logger.Information("node {0} is up", port);
            }

            // Thread.Sleep(TimeSpan.FromSeconds(1));
        }

        protected class OutputWriter : TextWriter
        {
            private ITestOutputHelper output;

            public OutputWriter(ITestOutputHelper output)
            {
                this.output = output;
            }

            private string buffer = string.Empty;

            public override void Write(string value)
            {
                this.buffer += value;
                if (value.Contains("\n"))
                {
                    this.output.WriteLine(buffer.Replace("\n", string.Empty));
                    this.buffer = string.Empty;
                }
            }

            public override void WriteLine(string value)
            {
                this.output.WriteLine(value);
            }

            public override Encoding Encoding
            {
                get
                {
                    return Encoding.UTF8;
                }
            }
        }
    }
}