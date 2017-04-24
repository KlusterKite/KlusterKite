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

    using ClusterKit.API.Client;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.ConfigurationSource;
    using ClusterKit.NodeManager.Launcher.Messages;

    using NuGet;

    using Xunit.Abstractions;

    /// <summary>
    /// Prepares the test environment to test  <see cref="ReleaseExtensions"/>
    /// </summary>
    public abstract class ReleaseCheckTestsBase
    {
        /// <summary>
        /// The .NET Framework 4.5 name
        /// </summary> 
        public const string Net45 = ".NET Framework,Version=v4.5";

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
        internal static PackageDependencySet CreatePackageDependencySet(string framework, params string[] definition)
        {
            var frameworkName = new FrameworkName(framework);
            return new PackageDependencySet(
                frameworkName,
                definition.Select(
                    d =>
                        {
                            var parts = d.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            return new PackageDependency(parts[0], CreateVersionSpec(parts[1]));
                        }));
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
                        })
                .ToList();
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

            var releaseConfiguration =
                new ReleaseConfiguration { Packages = packageDescriptions, NodeTemplates = nodeTemplates };

            return new Release { Configuration = releaseConfiguration };
        }

        /// <summary>
        /// Creates a test repository
        /// </summary>
        /// <returns>The test repository</returns>
        internal static TestRepository CreateRepository()
        {
            var p1 = new TestPackage
                         {
                             Id = "p1",
                             Version = SemanticVersion.Parse("1.0.0"),
                             DependencySets = new[] { CreatePackageDependencySet(Net45, "dp1 1.0.0") }
                         };

            var p2 = new TestPackage
                         {
                             Id = "p2",
                             Version = SemanticVersion.Parse("1.0.0"),
                             DependencySets = new[] { CreatePackageDependencySet(Net45, "dp2 1.0.0") }
                         };

            var p3 = new TestPackage
                         {
                             Id = "p3",
                             Version = SemanticVersion.Parse("1.0.0"),
                             DependencySets = new[] { CreatePackageDependencySet(Net45, "dp3 2.0.0") }
                         };
            var dp1 = new TestPackage
                          {
                              Id = "dp1",
                              Version = SemanticVersion.Parse("1.0.0"),
                              DependencySets = new PackageDependencySet[0]
                          };

            var dp2 = new TestPackage
                          {
                              Id = "dp2",
                              Version = SemanticVersion.Parse("1.0.0"),
                              DependencySets = new PackageDependencySet[0]
                          };

            var dp3 = new TestPackage
                          {
                              Id = "dp3",
                              Version = SemanticVersion.Parse("1.0.0"),
                              DependencySets = new PackageDependencySet[0]
                          };

            return new TestRepository(p1, p2, p3, dp1, dp2, dp3);
        }

        /// <summary>
        /// Creates the <see cref="VersionSpec"/> with specified version as minimum version inclusive
        /// </summary>
        /// <param name="version">
        /// The version.
        /// </param>
        /// <returns>
        /// The <see cref="VersionSpec"/>.
        /// </returns>
        internal static VersionSpec CreateVersionSpec(string version)
        {
            return new VersionSpec { IsMinInclusive = true, MinVersion = SemanticVersion.Parse(version) };
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
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "This is the test class")]
        internal class TestPackage : IPackage
        {
            /// <inheritdoc />
            public IEnumerable<IPackageAssemblyReference> AssemblyReferences { get; set; }

            /// <inheritdoc />
            public IEnumerable<string> Authors { get; set; }

            /// <inheritdoc />
            public string Copyright { get; set; }

            /// <inheritdoc />
            public IEnumerable<PackageDependencySet> DependencySets { get; set; }

            /// <inheritdoc />
            public string Description { get; set; }

            /// <inheritdoc />
            public bool DevelopmentDependency { get; set; }

            /// <inheritdoc />
            public int DownloadCount { get; set; }

            /// <summary>
            /// Gets or sets a delegate to implement <see cref="IPackage.ExtractContents"/>
            /// </summary>
            public Action<IFileSystem, string> ExtractContentsAction { get; set; }

            /// <inheritdoc />
            public IEnumerable<FrameworkAssemblyReference> FrameworkAssemblies { get; set; }

            /// <summary>
            /// Gets or sets a delegate to implement <see cref="IPackage.GetFiles"/>
            /// </summary>
            public Func<IEnumerable<IPackageFile>> GetFilesAction { get; set; }

            /// <summary>
            /// Gets or sets a delegate to implement <see cref="IPackage.ExtractContents"/>
            /// </summary>
            public Func<Stream> GetStreamAction { get; set; }

            /// <summary>
            /// Gets or sets a delegate to implement <see cref="IPackage.ExtractContents"/>
            /// </summary>
            public Func<IEnumerable<FrameworkName>> GetSupportedFrameworksAction { get; set; }

            /// <inheritdoc />
            public Uri IconUrl { get; set; }

            /// <inheritdoc />
            public string Id { get; set; }

            /// <inheritdoc />
            public bool IsAbsoluteLatestVersion { get; set; }

            /// <inheritdoc />
            public bool IsLatestVersion { get; set; }

            /// <inheritdoc />
            public string Language { get; set; }

            /// <inheritdoc />
            public Uri LicenseUrl { get; set; }

            /// <inheritdoc />
            public bool Listed { get; set; }

            /// <inheritdoc />
            public Version MinClientVersion { get; set; }

            /// <inheritdoc />
            public IEnumerable<string> Owners { get; set; }

            /// <inheritdoc />
            public ICollection<PackageReferenceSet> PackageAssemblyReferences { get; set; }

            /// <inheritdoc />
            public Uri ProjectUrl { get; set; }

            /// <inheritdoc />
            public DateTimeOffset? Published { get; set; }

            /// <inheritdoc />
            public string ReleaseNotes { get; set; }

            /// <inheritdoc />
            public Uri ReportAbuseUrl { get; set; }

            /// <inheritdoc />
            public bool RequireLicenseAcceptance { get; set; }

            /// <inheritdoc />
            public string Summary { get; set; }

            /// <inheritdoc />
            public string Tags { get; set; }

            /// <inheritdoc />
            public string Title { get; set; }

            /// <inheritdoc />
            public SemanticVersion Version { get; set; }

            /// <inheritdoc />
            public void ExtractContents(IFileSystem fileSystem, string extractPath)
            {
                if (this.ExtractContentsAction == null)
                {
                    throw new InvalidOperationException();
                }

                this.ExtractContentsAction(fileSystem, extractPath);
            }

            /// <inheritdoc />
            public IEnumerable<IPackageFile> GetFiles()
            {
                if (this.GetFilesAction == null)
                {
                    throw new InvalidOperationException();
                }

                return this.GetFilesAction();
            }

            /// <inheritdoc />
            public Stream GetStream()
            {
                if (this.GetStreamAction == null)
                {
                    throw new InvalidOperationException();
                }

                return this.GetStreamAction();
            }

            /// <inheritdoc />
            public IEnumerable<FrameworkName> GetSupportedFrameworks()
            {
                if (this.GetSupportedFrameworksAction == null)
                {
                    throw new InvalidOperationException();
                }

                return this.GetSupportedFrameworksAction();
            }
        }

        /// <summary>
        /// The test package file  representation
        /// </summary>
        internal class TestPackageFile : IPackageFile
        {
            /// <inheritdoc />
            public string EffectivePath { get; set; }

            /// <inheritdoc />
            public string Path { get; set; }

            /// <inheritdoc />
            public IEnumerable<FrameworkName> SupportedFrameworks { get; set; }

            /// <inheritdoc />
            public FrameworkName TargetFramework { get; set; }

            /// <summary>
            /// Gets or sets a delegate for <see cref="IPackageFile.GetStream"/>
            /// </summary>
            public Func<Stream> GetStreamAction { get; set; }

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
            /// <summary>
            /// Initializes a new instance of the <see cref="TestRepository"/> class.
            /// </summary>
            /// <param name="packages">
            /// The packages.
            /// </param>
            public TestRepository(params IPackage[] packages)
            {
                this.Packages = packages.ToImmutableDictionary(p => $"{p.Id} {p.Version}");
            }

            /// <summary>
            /// Gets the list of stored packages
            /// </summary>
            public IReadOnlyDictionary<string, IPackage> Packages { get; }

            /// <inheritdoc />
            public PackageSaveModes PackageSaveMode { get; set; }

            /// <inheritdoc />
            public string Source => string.Empty;

            /// <inheritdoc />
            public bool SupportsPrereleasePackages => true;

            /// <inheritdoc />
            public void AddPackage(IPackage package)
            {
                throw new InvalidOperationException();
            }

            /// <inheritdoc />
            public IQueryable<IPackage> GetPackages()
            {
                return this.Packages.Values.AsQueryable();
            }

            /// <inheritdoc />
            public void RemovePackage(IPackage package)
            {
                throw new InvalidOperationException();
            }
        }
    }
}