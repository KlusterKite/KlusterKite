namespace ClusterKit.BusinessObjects.Messages
{
    using Akka.Actor;
    using Akka.Routing;

    public class EnvelopeToSender : IConsistentHashable
    {
        public object ConsistentHashKey => this.Message == null ? null : this.Message.Id;

        public IMessageToBusinessObjectActor Message { get; set; }
        public ICanTell Receiver { get; set; }
        public IActorRef Sender { get; set; }
    }
}