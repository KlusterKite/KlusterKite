// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceDescription.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Description of single published local service
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Client.Messages
{
    using JetBrains.Annotations;

    /// <summary>
    /// Description of single published local service
    /// </summary>
    [UsedImplicitly]
    public struct ServiceDescription
    {
        /// <summary>
        /// Gets or sets the port, where web service is listening connections
        /// </summary>
        [UsedImplicitly]
        public int ListeningPort { get; set; }

        /// <summary>
        /// Gets or sets local hostname that proxy should path
        /// </summary>
        /// <remarks>
        /// This should be used to support virtual hosting inside single node
        /// </remarks>
        [UsedImplicitly]
        public string LocalHostName { get; set; }

        /// <summary>
        ///  Gets or sets public host name of service
        /// </summary>
        /// <remarks>
        /// It doesn't supposed (but is not prohibited) that this should be real public service hostname.
        /// It's just used to distinguish services with identical url paths to be correctly published on frontend web servers. Real expected hostname should be configured in NginxConfigurator or similar publisher
        /// </remarks>
        [UsedImplicitly]
        public string PublicHostName { get; set; }

        /// <summary>
        ///  Gets or sets route (aka directory) path to service
        /// </summary>
        [UsedImplicitly]
        public string Route { get; set; }

        public static bool operator !=(ServiceDescription left, ServiceDescription right)
        {
            return !left.Equals(right);
        }

        public static bool operator ==(ServiceDescription left, ServiceDescription right)
        {
            return left.Equals(right);
        }

        public bool Equals(ServiceDescription other)
        {
            return this.ListeningPort == other.ListeningPort && string.Equals(this.LocalHostName, other.LocalHostName) && string.Equals(this.PublicHostName, other.PublicHostName) && string.Equals(this.Route, other.Route);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is ServiceDescription && Equals((ServiceDescription)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.ListeningPort;
                hashCode = (hashCode * 397) ^ (this.LocalHostName != null ? this.LocalHostName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.PublicHostName != null ? this.PublicHostName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.Route != null ? this.Route.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}