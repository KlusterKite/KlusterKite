// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeployerFallbackTest.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Proof of <see href="https://github.com/akkadotnet/akka.net/issues/1321" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Core.Tests.AkkaTest
{
    using System;
    
    using Akka.Actor;
    using Akka.Configuration;
    using Akka.Routing;

    using KlusterKite.Core.TestKit;

    using Serilog;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Proof of <see href="https://github.com/akkadotnet/akka.net/issues/1321"/> 
    /// </summary>
    public class DeployerFallbackTest
    {
        /// <summary>
        /// The xunit output
        /// </summary>
        private ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeployerFallbackTest"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public DeployerFallbackTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// This test is failing - bug fixed
        /// </summary>
        [Fact]
        public void FailingTest()
        {
            var loggerConfig = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.TextWriter(new XunitOutputWriter(this.output));
            Log.Logger = loggerConfig.CreateLogger();

            var config =
                ConfigurationFactory.ParseString(@"
                    akka : {
                        stdout-loglevel : INFO
                        loggers : [""Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog""]
                        log-config-on-start : on
                        loglevel : INFO

                        actor.deployment {
                            /SomeActor/workers {
                                    router = round-robin-pool
                                    nr-of-instances = 5
                                }
                        }
                    }").WithFallback(ConfigurationFactory.ParseString(@"
                        akka.actor.deployment {
                            /MyActor/workers {
                                    router = round-robin-pool
                                    nr-of-instances = 5
                                }
                        }
                    "));

            ActorSystem system = ActorSystem.Create("Test", config);
            try
            {
                var actor = system.ActorOf(Props.Create(() => new MyActor()), "MyActor");
                var result = actor.Ask<string>("hello world", TimeSpan.FromSeconds(1)).Result;
                Assert.Equal("hello world", result);
            }
            finally
            {
                system.Terminate().Wait(TimeSpan.FromSeconds(5));
            }
        }

        /// <summary>
        /// This test is passing
        /// </summary>
        [Fact]
        public void WorkingTest()
        {
            var loggerConfig = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.TextWriter(new XunitOutputWriter(this.output));
            Log.Logger = loggerConfig.CreateLogger();

            var config =
                ConfigurationFactory.ParseString(
                    @"
                    akka : {
                        stdout-loglevel : INFO
                        loggers : [""Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog""]
                        log-config-on-start : on
                        loglevel : INFO

                        actor.deployment {
                            /MyActor/workers {
                                    router = round-robin-pool
                                    nr-of-instances = 5
                                }
                        }
                    }");

            ActorSystem system = ActorSystem.Create("Test", config);
            try
            {
                var actor = system.ActorOf(Props.Create(() => new MyActor()), "MyActor");
                var result = actor.Ask<string>("hello world", TimeSpan.FromSeconds(1)).Result;
                Assert.Equal("hello world", result);
            }
            finally
            {
                system.Terminate().Wait(TimeSpan.FromSeconds(5));
            }
        }

        /// <summary>
        /// Mock actor
        /// </summary>
        public class MyActor : ReceiveActor
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MyActor"/> class.
            /// </summary>
            public MyActor()
            {
                Context.ActorOf(Props.Create(typeof(Worker)).WithRouter(FromConfig.Instance), "workers");

                // just simple bouncer
                this.Receive<object>(m => this.Sender.Tell(m));
            }

            /// <summary>
            /// The mock actor worker
            /// </summary>
            private class Worker : ReceiveActor
            {
            }
        }
    }
}