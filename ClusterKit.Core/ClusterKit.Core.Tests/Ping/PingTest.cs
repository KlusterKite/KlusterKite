﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PingTest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing <seealso cref="PingActor" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.Tests.Ping
{
    using System;

    using Akka.Actor;
    using Akka.Cluster;

    using ClusterKit.Core.Ping;
    using ClusterKit.Core.TestKit;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Testing <seealso cref="PingActor"/>
    /// </summary>
    public class PingTest : BaseActorTest<TestConfigurator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PingTest"/> class.
        /// The test ping.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public PingTest(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Testing ping actor
        /// </summary>
        [Fact]
        public void TestNetworkPing()
        {
            this.Sys.StartNameSpaceActorsFromConfiguration();
            var now = DateTime.UtcNow;
            var address = Cluster.Get(this.Sys).SelfAddress;

            this.Sys.ActorSelection($"{address.ToString()}/user/Core/Ping").Ask<PongMessage>(new PingMessage(), TimeSpan.FromMilliseconds(200));
            var elapsed = (DateTime.UtcNow - now).TotalMilliseconds;
            this.Sys.Log.Info("Ping in {0}ms", elapsed);
        }

        /// <summary>
        /// Testing ping actor
        /// </summary>
        [Fact]
        public void TestPing()
        {
            this.Sys.StartNameSpaceActorsFromConfiguration();
            var now = DateTime.UtcNow;
            this.Sys.ActorSelection("/user /Core/Ping").Ask<PongMessage>(new PingMessage(), TimeSpan.FromMilliseconds(200));
            var elapsed = (DateTime.UtcNow - now).TotalMilliseconds;
            this.Sys.Log.Info("Ping in {0}ms", elapsed);
        }
    }
}