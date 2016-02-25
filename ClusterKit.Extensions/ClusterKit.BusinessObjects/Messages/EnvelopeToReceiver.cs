namespace ClusterKit.BusinessObjects.Messages
{
    using Akka.Actor;

    public class EnvelopeToReceiver
    {
        public IMessageToBusinessObjectActor Message { get; set; }
        public IActorRef Sender { get; set; }
    }
}