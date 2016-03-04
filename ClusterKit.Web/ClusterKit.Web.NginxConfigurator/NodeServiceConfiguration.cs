namespace ClusterKit.Web.NginxConfigurator
{
    using Akka.Actor;

    using ClusterKit.Web.Client.Messages;

    /// <summary>
    /// Node service description
    /// </summary>
    public class NodeServiceConfiguration
    {
        /// <summary>
        /// Node address
        /// </summary>
        public Address NodeAddress { get; set; }

        /// <summary>
        /// Link to node listening server root
        /// </summary>
        public string NodeUrl => $"{this.NodeAddress.Host}:{this.ServiceDescription.ListeningPort}";

        /// <summary>
        /// Local node service desription
        /// </summary>
        public ServiceDescription ServiceDescription { get; set; }

        public static bool operator !=(NodeServiceConfiguration left, NodeServiceConfiguration right)
        {
            return !Equals(left, right);
        }

        public static bool operator ==(NodeServiceConfiguration left, NodeServiceConfiguration right)
        {
            return Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((NodeServiceConfiguration)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.NodeAddress != null ? this.NodeAddress.GetHashCode() : 0) * 397) ^ this.ServiceDescription.GetHashCode();
            }
        }

        protected bool Equals(NodeServiceConfiguration other)
        {
            return Equals(this.NodeAddress, other.NodeAddress) && this.ServiceDescription.Equals(other.ServiceDescription);
        }
    }
}