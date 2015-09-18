namespace TaxiKit.Core.Cluster.Messages
{
    using Akka.Cluster;

    using JetBrains.Annotations;

    /// <summary>
    /// The command, that is sent to node supervisor in order to notifiy that child was removed from cluster
    /// </summary>
    [UsedImplicitly]
    public class ChildRemoved
    {
        public string Id { get; set; }

        public UniqueAddress NodeAddress { get; set; }
    }
}