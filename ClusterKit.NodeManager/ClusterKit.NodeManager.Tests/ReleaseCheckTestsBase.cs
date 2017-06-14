// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReleaseCheckTestsBase.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Prepares the test environment to test  <see cref="ReleaseExtensions" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Runtime.Versioning;
    using System.Threading.Tasks;

    using ClusterKit.API.Client;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.ConfigurationSource;
    using ClusterKit.NodeManager.Launcher.Messages;

    using NuGet.Frameworks;
    using NuGet.Packaging;
    using NuGet.Packaging.Core;
    using NuGet.Protocol.Core.Types;
    using NuGet.Versioning;

    using Xunit.Abstractions;

    /// <summary>
    /// Prepares the test environment to test  <see cref="ReleaseExtensions"/>
    /// </summary>
    public abstract class ReleaseCheckTestsBase
    {
        /// <summary>
        /// The .NET Framework 4.6 name
        /// </summary> 
        public const string Net46 = ".NETFramework,Version=v4.6";

        /// <summary>
        /// The .NET Standard 1.1 name
        /// </summary>
        public const string NetStandard = ".NET Standard,Version=v1.1";

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseCheckTestsBase"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        protected ReleaseCheckTestsBase(ITestOutputHelper output)
        {
            this.Output = output;
        }

        /// <summary>
        /// Gets the test output stream
        /// </summary>
        protected ITestOutputHelper Output { get; }

        /// <summary>
        /// Creates a <see cref="PackageDependencySet"/> from string definition
        /// </summary>
        /// <param name="framework">The framework name</param>
        /// <param name="definition">The dependencies definition</param>
        /// <returns>The dependency set</returns>
        internal static PackageDependencyGroup CreatePackageDependencySet(string framework, params string[] definition)
        {
            return new PackageDependencyGroup(
                NuGetFramework.ParseFolder(framework),
                definition.Select(
                    d =>
                        {
                            var parts = d.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            return new PackageDependency(parts[0], CreateVersionRange(parts[1]));
                        }));
        }

        /// <summary>
        /// Creates the version range
        /// </summary>
        /// <param name="minVersion">The minimum required version</param>
        /// <returns>The version range</returns>
        internal static VersionRange CreateVersionRange(string minVersion)
        {
            return new VersionRange(NuGetVersion.Parse(minVersion));
        }

        /// <summary>
        /// Creates the list of package descriptions.
        /// </summary>
        /// <param name="packages">
        /// The string descriptions.
        /// </param>
        /// <returns>
        /// The list of package descriptions.
        /// </returns>
        internal static IEnumerable<PackageDescription> CreatePackageDescriptions(params string[] packages)
        {
            return packages.Select(
                p =>
                    {
                        var parts = p.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        return new PackageDescription(parts[0], parts[1]);
                    });
        }

        /// <summary>
        /// Creates the list of package requirements.
        /// </summary>
        /// <param name="packages">
        /// The string descriptions.
        /// </param>
        /// <returns>
        /// The list of package requirements.
        /// </returns>
        internal static List<NodeTemplate.PackageRequirement> CreatePackageRequirement(params string[] packages)
        {
            return packages.Select(
                p =>
                    {
                        var parts = p.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        return new NodeTemplate.PackageRequirement(parts[0], parts.Length > 1 ? parts[1] : null);
                    }).ToList();
        }

        /// <summary>
        /// Creates the default release
        /// </summary>
        /// <param name="packages">The list of defined packages to override</param>
        /// <param name="templatePackageRequirements">The template package requirements</param>
        /// <returns>The release</returns>
        internal static Release CreateRelease(string[] packages = null, string[] templatePackageRequirements = null)
        {
            if (packages == null)
            {
                packages = new[] { "p1 1.0.0", "p2 1.0.0", "dp1 1.0.0", "dp2 1.0.0" };
            }

            if (templatePackageRequirements == null)
            {
                templatePackageRequirements = new[] { "p1", "p2 1.0.0" };
            }

            var packageDescriptions = new List<PackageDescription>(CreatePackageDescriptions(packages));

            var nodeTemplates = new List<NodeTemplate>();
            var t1 = new NodeTemplate
                         {
                             Code = "t1",
                             Configuration = "t1",
                             PackageRequirements = CreatePackageRequirement(templatePackageRequirements),
                             ContainerTypes = new List<string> { "test" }
                         };
            nodeTemplates.Add(t1);

            var migratorTemplates = new List<MigratorTemplate>();
            var m1 = new MigratorTemplate
                         {
                             Code = "m1",
                             Configuration = "m1",
                             PackageRequirements =
                                 CreatePackageRequirement(templatePackageRequirements)
                         };
            migratorTemplates.Add(m1);

            var releaseConfiguration =
                new ReleaseConfiguration
                    {
                        Packages = packageDescriptions,
                        NodeTemplates = nodeTemplates,
                        MigratorTemplates = migratorTemplates
                    };

            return new Release { Configuration = releaseConfiguration };
        }

        /// <summary>
        /// Creates a test repository
        /// </summary>
        /// <returns>The test repository</returns>
        internal static TestRepository CreateRepository()
        {
            var p1 = new TestPackage("p1", "1.0.0")
                         {
                             DependencySets = new[] { CreatePackageDependencySet(Net46, "dp1 1.0.0") }
                         };

            var p2 = new TestPackage("p2", "1.0.0")
            {
                             DependencySets = new[] { CreatePackageDependencySet(Net46, "dp2 1.0.0") }
                         };

            var p3 = new TestPackage("p3", "1.0.0")
            {
                             DependencySets = new[] { CreatePackageDependencySet(Net46, "dp3 2.0.0") }
                         };
            var dp1 = new TestPackage("dp1", "1.0.0");

            var dp2 = new TestPackage("dp2", "1.0.0");

            var dp3 = new TestPackage("dp3", "1.0.0");

            return new TestRepository(p1, p2, p3, dp1, dp2, dp3);
        }

        /// <summary>
        /// Writes the error list to the output
        /// </summary>
        /// <param name="errors">The output list</param>
        internal void WriteErrors(IEnumerable<ErrorDescription> errors)
        {
            foreach (var error in errors)
            {
                this.Output.WriteLine($"{error.Field}: {error.Message}");
            }
        }

        /// <summary>
        /// The test package representation
        /// </summary>
        [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty", Justification = "This is test implementation")]
        internal class TestPackage : IPackageSearchMetadata
        {
            /// <inheritdoc />
            public TestPackage(string id, string version)
            {
                this.Identity = new PackageIdentity(id, NuGetVersion.Parse(version));
                this.DependencySets = new List<PackageDependencyGroup>();
            }

            /// <inheritdoc />
            public string Authors { get; }

            /// <inheritdoc />
            public IEnumerable<PackageDependencyGroup> DependencySets { get; set; }

            /// <inheritdoc />
            public string Description { get; }

            /// <inheritdoc />
            public long? DownloadCount { get; }

            /// <inheritdoc />
            public Uri IconUrl { get; }

            /// <inheritdoc />
            public PackageIdentity Identity { get; set;  }

            /// <inheritdoc />
            public bool IsListed { get; }

            /// <inheritdoc />
            public Uri LicenseUrl { get; }

            /// <inheritdoc />
            public string Owners { get; }

            /// <inheritdoc />
            public Uri ProjectUrl { get; }

            /// <inheritdoc />
            public DateTimeOffset? Published { get; }

            /// <inheritdoc />
            public Uri ReportAbuseUrl { get; }

            /// <inheritdoc />
            public bool RequireLicenseAcceptance { get; }

            /// <inheritdoc />
            public string Summary { get; }

            /// <inheritdoc />
            public string Tags { get; }

            /// <inheritdoc />
            public string Title { get; }

            /// <inheritdoc />
            public Task<IEnumerable<VersionInfo>> GetVersionsAsync()
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// The test package file  representation
        /// </summary>
        internal class TestPackageFile : IPackageFile
        {
            /// <inheritdoc />
            public string EffectivePath { get; set; }

            /// <summary>
            /// Gets or sets a delegate for <see cref="IPackageFile.GetStream"/>
            /// </summary>
            public Func<Stream> GetStreamAction { get; set; }

            /// <inheritdoc />
            public string Path { get; set; }

            /// <inheritdoc />
            public IEnumerable<FrameworkName> SupportedFrameworks { get; set; }

            /// <inheritdoc />
            public FrameworkName TargetFramework { get; set; }

            /// <inheritdoc />
            public Stream GetStream()
            {
                if (this.GetStreamAction == null)
                {
                    throw new InvalidOperationException();
                }

                return this.GetStreamAction();
            }
        }

        /// <summary>
        /// The test nuget repository
        /// </summary>
        internal class TestRepository : IPackageRepository
        {
            /// <inheritdoc />
            public TestRepository(params TestPackage[] packages)
            {
            }

            /// <inheritdoc />
            public Task<IPackageSearchMetadata> GetAsync(string id)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public Task<IPackageSearchMetadata> GetAsync(string id, NuGetVersion version)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public Task<IEnumerable<IPackageSearchMetadata>> SearchAsync(string terms, bool includePreRelease)
            {
                throw new NotImplementedException();
            }
        }
    }
}