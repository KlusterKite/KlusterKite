// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SeederActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Monitors for seed actors and repeatedly trying to connect with them
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.Cluster
{
    using Akka.Actor;
    using Akka.Cluster;

    /// <summary>
    /// Monitors for seed actors and repeatedly trying to connect with them
    /// </summary>
    public class SeederActor : ReceiveActor
    {
        public void AttachSeed(Address seedAddress)
        {
            var cluster = Cluster.Get(Context.System);
        }
    }
}