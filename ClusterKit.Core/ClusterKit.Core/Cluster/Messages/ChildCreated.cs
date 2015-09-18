namespace ClusterKit.Core.Cluster.Messages
{
    using Akka.Cluster;

    using JetBrains.Annotations;

    /// <summary>
    /// Notification, that new child actor spawned
    /// </summary>
    [UsedImplicitly]
    public class ChildCreated
    {
        public string Id { get; set; }
        public UniqueAddress NodeAddress { get; set; }
    }
}