// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeployerFallbackTest.cs" company="Kantora">
//   All rights reserved
// </copyright>
// <summary>
//   Proof of https://github.com/akkadotnet/akka.net/issues/1321
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.Tests.AkkaTest
{
    using System;
    using System.IO;
    using System.Text;

    using Akka.Actor;
    using Akka.Configuration;
    using Akka.Routing;

    using ClusterKit.Core.TestKit;

    using Serilog;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Proof of https://github.com/akkadotnet/akka.net/issues/1321
    /// </summary>
    public class DeployerFallbackTest
    {
        private ITestOutputHelper output;

        public DeployerFallbackTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// This test is failing
        /// </summary>
        //[Fact]
        public void FailingTest()
        {
            var loggerConfig = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.TextWriter(new LocalXunitOutputWriter(this.output));
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
            var loggerConfig = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.TextWriter(new LocalXunitOutputWriter(this.output));
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
        /// TextWriter to write logs to Xunit output
        /// </summary>
        public class LocalXunitOutputWriter : TextWriter
        {
            private readonly StringBuilder line;
            private readonly ITestOutputHelper output;

            public LocalXunitOutputWriter(ITestOutputHelper output)
            {
                this.output = output;
                this.line = new StringBuilder();
            }

            public override Encoding Encoding => Encoding.UTF8;

            public override void Write(char[] buffer)
            {
                var str = new string(buffer);
                if (!str.EndsWith(this.NewLine))
                {
                    this.line.Append(str);
                    return;
                }

                this.line.Append(str.Substring(0, str.Length - this.NewLine.Length));
                this.output.WriteLine(this.line.ToString());
                this.line.Clear();
            }

            public override void WriteLine()
            {
                this.output.WriteLine(this.line.ToString());
                this.line.Clear();
            }
        }

        public class MyActor : ReceiveActor
        {
            private IActorRef workers;

            public MyActor()
            {
                this.workers = Context.ActorOf(Props.Create(typeof(Worker)).WithRouter(FromConfig.Instance), "workers");

                //just simple bouncer
                this.Receive<object>(m => this.Sender.Tell(m));
            }

            private class Worker : ReceiveActor
            {
            }
        }
    }
}