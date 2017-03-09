// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageDescriptionSurrogate.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Represents the <see cref="PackageDescription" /> for public API
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.ApiSurrogates
{
    using ClusterKit.API.Client;
    using ClusterKit.API.Client.Attributes;
    using ClusterKit.NodeManager.Launcher.Messages;

    using JetBrains.Annotations;

    /// <summary>
    /// Represents the <see cref="PackageDescription"/> for public API
    /// </summary>
    [ApiDescription(Description = "Short description of NuGet package", Name = "ClusterKitPackageDescription")]
    public class PackageDescriptionSurrogate
    {
        /// <summary>
        /// Gets or sets the package Id
        /// </summary>
        [UsedImplicitly]
        [DeclareField(Description = "The package Id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the package latest version
        /// </summary>
        [UsedImplicitly]
        [DeclareField(Description = "The package version")]
        public string Version { get; set; }

        /// <summary>
        /// Converts <see cref="PackageDescription"/> to <see cref="AkkaAddressSurrogate"/>
        /// </summary>
        public class Converter : IValueConverter<PackageDescriptionSurrogate>
        {
            /// <inheritdoc />
            public PackageDescriptionSurrogate Convert(object source)
            {
                var packageDescription = source as PackageDescription;
                if (packageDescription == null)
                {
                    return null;
                }

                return new PackageDescriptionSurrogate
                {
                    Id = packageDescription.Id,
                    Version = packageDescription.Version
                };
            }
        }
    }
}
