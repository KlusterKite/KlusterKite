// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WatcherActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Watcher actor. It's main purpose to monitor any cluster changes and store complete
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Monitoring.Actors
{
    using Akka.Actor;

    /// <summary>
    /// Watcher actor. It's main purpose to monitor any cluster changes and store complete data about current cluster health
    /// </summary>
    public class WatcherActor : ReceiveActor
    {
    }
}