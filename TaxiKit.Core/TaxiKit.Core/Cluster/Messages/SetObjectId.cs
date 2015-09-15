namespace TaxiKit.Core.Cluster.Messages
{
    using JetBrains.Annotations;

    /// <summary>
    /// The command, that is sent to child actor to initialize it with id.
    /// </summary>
    [UsedImplicitly]
    public class SetObjectId
    {
        [UsedImplicitly]
        public string Id { get; set; }
    }
}