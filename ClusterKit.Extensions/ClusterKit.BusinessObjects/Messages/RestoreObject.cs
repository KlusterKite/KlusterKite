namespace ClusterKit.BusinessObjects.Messages
{
    using JetBrains.Annotations;

    /// <summary>
    /// The command, that is sent to child actor to initialize it with id.
    /// </summary>
    [UsedImplicitly]
    public class RestoreObject : IMessageToBusinessObjectActor
    {
        [UsedImplicitly]
        public string Id { get; set; }
    }
}