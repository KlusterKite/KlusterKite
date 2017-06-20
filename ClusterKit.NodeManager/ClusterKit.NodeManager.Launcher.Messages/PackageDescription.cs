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
    using System;
    using System.Diagnostics.CodeAnalysis;

    using ClusterKit.API.Attributes;

    /// <summary>
    /// Short description of NuGet package
    /// </summary>
    [ApiDescription("Short description of NuGet package", Name = "PackageDescription")]
    public class PackageDescription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageDescription"/> class.
        /// </summary>
        public PackageDescription()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageDescription"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="version">
        /// The version.
        /// </param>
        public PackageDescription(string id, string version)
        {
            this.Id = id;
            this.Version = version;
        }

        /// <summary>
        /// Gets or sets the package Id
        /// </summary>
        [DeclareField("the package Id", IsKey = true)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the package version
        /// </summary>
        [DeclareField("the package version")]
        public string Version { get; set; }

        /// <summary>
        /// Determines whether the specified object is not equal to the current object.
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        public static bool operator !=(PackageDescription left, PackageDescription right)
        {
            return !Equals(left, right);
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public static bool operator ==(PackageDescription left, PackageDescription right)
        {
            return Equals(left, right);
        }

        /// <inheritdoc />
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

            return this.Equals((PackageDescription)obj);
        }

        /// <summary>Serves as the default hash function. </summary>
        /// <returns>A hash code for the current object.</returns>
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "Properties are non readonly for serialization purposes")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.Id?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (this.Version?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="other">The object to compare with the current object. </param>
        private bool Equals(PackageDescription other)
        {
            return string.Equals(this.Id, other.Id) && string.Equals(this.Version, other.Version);
        }
    }
}