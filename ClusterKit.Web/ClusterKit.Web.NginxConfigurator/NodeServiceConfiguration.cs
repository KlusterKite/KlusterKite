namespace ClusterKit.Web.NginxConfigurator
{
    using Akka.Actor;

    using ClusterKit.Web.Client.Messages;

    using JetBrains.Annotations;

    /// <summary>
    /// Node service description
    /// </summary>
    public class NodeServiceConfiguration
    {
        /// <summary>
        /// Node address
        /// </summary>
        [UsedImplicitly]
        public Address NodeAddress { get; set; }

        /// <summary>
        /// Link to node listening server root
        /// </summary>
        public string NodeUrl => $"{this.NodeAddress.Host}:{this.ServiceDescription.ListeningPort}";

        /// <summary>
        /// Local node service description
        /// </summary>
        public ServiceDescription ServiceDescription { get; set; }

        /// <summary>
        /// Compares two <seealso cref="NodeServiceConfiguration"/> for non-equality
        /// </summary>
        /// <param name="left">The left service configuration</param>
        /// <param name="right">The right service configuration</param>
        /// <returns>Whether two <seealso cref="NodeServiceConfiguration"/> are not equal</returns>
        public static bool operator !=(NodeServiceConfiguration left, NodeServiceConfiguration right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Compares two <seealso cref="NodeServiceConfiguration"/> for equality
        /// </summary>
        /// <param name="left">The left service configuration</param>
        /// <param name="right">The right service configuration</param>
        /// <returns>Whether two <seealso cref="NodeServiceConfiguration"/> are equal</returns>
        public static bool operator ==(NodeServiceConfiguration left, NodeServiceConfiguration right)
        {
            return Equals(left, right);
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
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
            return this.Equals((NodeServiceConfiguration)obj);
        }

        /// <summary>Serves as the default hash function. </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.NodeAddress?.GetHashCode() ?? 0) * 397) ^ this.ServiceDescription.GetHashCode();
            }
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="other">The object to compare with the current object. </param>
        private bool Equals(NodeServiceConfiguration other)
        {
            return Equals(this.NodeAddress, other.NodeAddress) && this.ServiceDescription.Equals(other.ServiceDescription);
        }
    }
}