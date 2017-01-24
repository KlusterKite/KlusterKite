// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageDescription.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Short description of NuGet package
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Launcher.Messages
{
    /// <summary>
    /// Short description of NuGet package
    /// </summary>
    public class PackageDescription
    {
        /// <summary>
        /// Gets or sets build time of assembly (if specified)
        /// </summary>
        public string BuildDate { get; set; }

        /// <summary>
        /// Gets or sets the package Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the package latest version
        /// </summary>
        public string Version { get; set; }


        /// <summary>Determines whether the specified object is not equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public static bool operator !=(PackageDescription left, PackageDescription right)
        {
            return !Equals(left, right);
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public static bool operator ==(PackageDescription left, PackageDescription right)
        {
            return Equals(left, right);
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((PackageDescription)obj);
        }

        /// <summary>Serves as the default hash function. </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.BuildDate?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (this.Id?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (this.Version?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="other">The object to compare with the current object. </param>
        private bool Equals(PackageDescription other)
        {
            return string.Equals(this.BuildDate, other.BuildDate) && string.Equals(this.Id, other.Id) && string.Equals(this.Version, other.Version);
        }
    }
}