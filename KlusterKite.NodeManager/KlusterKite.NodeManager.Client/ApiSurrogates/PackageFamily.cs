// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageFamily.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Represents the <see cref="PackageFamily" /> for public API
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.ApiSurrogates
{
    using System.Collections.Generic;

    using JetBrains.Annotations;

    using KlusterKite.API.Attributes;

    using Newtonsoft.Json;

    /// <summary>
    /// Represents the <see cref="Launcher.Messages.PackageDescription"/> for public API
    /// </summary>
    [ApiDescription(Description = "Short description of NuGet package family", Name = "NugetPackageFamily")]
    public class PackageFamily
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageFamily"/> class.
        /// </summary>
        public PackageFamily()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageFamily"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="version">
        /// The version.
        /// </param>
        public PackageFamily(string name, string version)
        {
            this.Name = name;
            this.Version = version;
        }

        /// <summary>
        /// Gets the package id (name and version)
        /// </summary>
        [UsedImplicitly]
        [JsonIgnore]
        [DeclareField(Description = "The package id (name and version)", IsKey = true, Access = EnAccessFlag.Queryable)]
        public string Id => $"{this.Name} {this.Version}";

        /// <summary>
        /// Gets or sets the package name
        /// </summary>
        [UsedImplicitly]
        [DeclareField(Description = "The package name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the package latest version
        /// </summary>
        [UsedImplicitly]
        [DeclareField(Description = "The package version")]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the list of available versions
        /// </summary>
        [UsedImplicitly]
        [JsonIgnore]
        [DeclareField(Description = "The list of available versions")]
        public List<string> AvailableVersions { get; set; }

        /// <summary>
        /// Converts <see cref="Launcher.Messages.PackageDescription"/> to <see cref="AkkaAddressSurrogate"/>
        /// </summary>
        public class Converter : IValueConverter<PackageFamily>
        {
            /// <inheritdoc />
            public PackageFamily Convert(object source)
            {
                var packageDescription = source as Launcher.Messages.PackageDescription;
                if (packageDescription == null)
                {
                    return null;
                }

                return new PackageFamily
                {
                    Name = packageDescription.Id,
                    Version = packageDescription.Version
                };
            }
        }
    }
}
