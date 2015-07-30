// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AkkaClusterTest.cs" company="">
//
// </copyright>
// <summary>
//   Akka cluster capability testing
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TaxiKit.Core.Tests.ConceptProof
{
    using System.Configuration;

    using Akka.Actor;
    using Akka.Configuration;
    using Akka.Configuration.Hocon;

    using TaxiKit.Core.TestKit;

    using Xunit;
    using Xunit.Abstractions;

    using static Serilog.Log;

    /// <summary>
    /// Akka cluster capability testing
    /// </summary>
    public class AkkaClusterTest : TestWithSerilog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AkkaClusterTest"/> class.
        /// </summary>
        /// <param name="output">
        /// The Xunit output.
        /// </param>
        public AkkaClusterTest(ITestOutputHelper output) : base(output)
        {
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
    }
}