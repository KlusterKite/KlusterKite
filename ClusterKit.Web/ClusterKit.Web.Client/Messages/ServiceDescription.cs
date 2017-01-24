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

        /// <summary>
        /// Not equals operator for <seealso cref="ServiceDescription"/>
        /// </summary>
        /// <param name="left">Left object</param>
        /// <param name="right">Right object</param>
        /// <returns>Whether both objects are not equal</returns>
        public static bool operator !=(ServiceDescription left, ServiceDescription right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Equals operator for <seealso cref="ServiceDescription"/>
        /// </summary>
        /// <param name="left">Left object</param>
        /// <param name="right">Right object</param>
        /// <returns>Whether both objects are equal</returns>
        public static bool operator ==(ServiceDescription left, ServiceDescription right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Equals operator for <seealso cref="ServiceDescription"/>
        /// </summary>
        /// <param name="other">The object to compare</param>
        /// <returns>Whether both objects are equal</returns>
        public bool Equals(ServiceDescription other)
        {
            return this.ListeningPort == other.ListeningPort && string.Equals(this.LocalHostName, other.LocalHostName) && string.Equals(this.PublicHostName, other.PublicHostName) && string.Equals(this.Route, other.Route);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current instance. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is ServiceDescription && this.Equals((ServiceDescription)obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.ListeningPort;
                hashCode = (hashCode * 397) ^ (this.LocalHostName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (this.PublicHostName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (this.Route?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}