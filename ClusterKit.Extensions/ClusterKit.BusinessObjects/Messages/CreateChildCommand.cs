namespace ClusterKit.BusinessObjects.Messages
{
    using JetBrains.Annotations;

    /// <summary>
    /// The command, that is sent to node supervisor in order to create child actor
    /// </summary>
    [UsedImplicitly]
    public class CreateChildCommand
    {
        public string Id { get; set; }
    }
}