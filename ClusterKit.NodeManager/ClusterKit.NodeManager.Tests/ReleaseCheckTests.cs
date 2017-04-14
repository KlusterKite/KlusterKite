// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReleaseCheckTests.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Checks the work of <see cref="ReleaseExtensions" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.ConfigurationSource;
    using ClusterKit.NodeManager.Launcher.Messages;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Checks the work of <see cref="ReleaseExtensions"/>
    /// </summary>
    public class ReleaseCheckTests : ReleaseCheckTestsBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseCheckTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public ReleaseCheckTests(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Testing that default release has no errors
        /// </summary>
        [Fact]
        public void DefaultReleaseNoError()
        {
            var release = CreateRelease();
            var errors =
                release.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net45 }).ToList();
            this.WriteErrors(errors);
            Assert.Equal(0, errors.Count);

            var template = release.Configuration.NodeTemplates.First();
            Assert.NotNull(template.PackagesToInstall);
            Assert.True(template.PackagesToInstall.ContainsKey(Net45));
            var packagesToInstall = template.PackagesToInstall[Net45];
            Assert.Equal(
                "dp1 1.0.0; dp2 1.0.0; p1 1.0.0; p2 1.0.0",
                string.Join("; ", packagesToInstall.OrderBy(p => p.Id).Select(p => $"{p.Id} {p.Version}")));
        }

        /// <summary>
        /// Tests the error of missed package definitions
        /// </summary>
        [Fact]
        public void NoPackagesError()
        {
            var release = CreateRelease(new string[0]);
            var errors =
                release.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net45 }).ToList();
            this.WriteErrors(errors);
            Assert.Equal(1, errors.Count);
            Assert.Equal("configuration.packages", errors[0].Field);
        }

        /// <summary>
        /// Tests the error of missed template definitions
        /// </summary>
        [Fact]
        public void NoTemplatesError()
        {
            var release = CreateRelease();
            release.Configuration.NodeTemplates.Clear();
            var errors =
                release.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net45 }).ToList();
            this.WriteErrors(errors);
            Assert.Equal(1, errors.Count);
            Assert.Equal("configuration.nodeTemplates", errors[0].Field);
        }

        /// <summary>
        /// Test for correct workaround of incompatible package versions
        /// </summary>
        [Fact]
        public void ReleasePackageDependencyInvalidVersionError()
        {
            var release = CreateRelease();
            release.Configuration.Packages.Add(new PackageDescription("p3", "1.0.0"));
            release.Configuration.Packages.Add(new PackageDescription("dp3", "1.0.0"));
            var errors =
                release.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net45 }).ToList();
            this.WriteErrors(errors);
            Assert.Equal(1, errors.Count);
            Assert.Equal("configuration.packages[\"p3\"]", errors[0].Field);
            Assert.Equal($"Package dependency for {Net45} dp3 doesn't satisify version requirements", errors[0].Message);
        }

        /// <summary>
        /// Test for correct workaround of missed definition of package dependencies
        /// </summary>
        [Fact]
        public void ReleasePackageDependencyNotDefinedError()
        {
            var release = CreateRelease();
            release.Configuration.Packages.Add(new PackageDescription("p3", "1.0.0"));
            var errors =
                release.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net45 }).ToList();
            this.WriteErrors(errors);
            Assert.Equal(1, errors.Count);
            Assert.Equal("configuration.packages[\"p3\"]", errors[0].Field);
            Assert.Equal($"Package dependency for {Net45} dp3 is not defined", errors[0].Message);
        }

        /// <summary>
        /// Test for correct workaround of package definitions missed in nuget repository
        /// </summary>
        [Fact]
        public void ReleasePackageNotFoundError()
        {
            var release = CreateRelease();
            release.Configuration.Packages.Add(new PackageDescription("test", "1.0.0"));
            var errors =
                release.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net45 }).ToList();
            this.WriteErrors(errors);
            Assert.Equal(1, errors.Count);
            Assert.Equal("configuration.packages[\"test\"]", errors[0].Field);
            Assert.Equal("Package of specified version could not be found in the nuget repository", errors[0].Message);
        }

        /// <summary>
        /// Testing package version format error
        /// </summary>
        [Fact]
        public void ReleasePackageVersionFormatError()
        {
            var release = CreateRelease();
            release.Configuration.Packages.Add(new PackageDescription("test", "strange-version"));
            var errors =
                release.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net45 }).ToList();
            this.WriteErrors(errors);
            Assert.Equal(1, errors.Count);
            Assert.Equal("configuration.packages[\"test\"]", errors[0].Field);
            Assert.Equal("Package version could not be parsed", errors[0].Message);
        }

        /// <summary>
        /// Test for correct workaround of missed package requirements definition in defined template
        /// </summary>
        [Fact]
        public void ReleaseTemplateNoPackagesError()
        {
            var release = CreateRelease();
            release.Configuration.NodeTemplates.Add(new NodeTemplate { Code = "t2" });
            var errors =
                release.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net45 }).ToList();
            this.WriteErrors(errors);
            Assert.Equal(1, errors.Count);
            Assert.Equal("configuration.nodeTemplates[\"t2\"]", errors[0].Field);
            Assert.Equal("Package requirements are not set", errors[0].Message);
        }

        /// <summary>
        /// Test for correct workaround of missed dependency of package requirements definition in defined template
        /// </summary>
        [Fact]
        public void ReleaseTemplatePackageRequirementDependencyNotFoundError()
        {
            var release = CreateRelease();
            release.Configuration.NodeTemplates[0].PackageRequirements.Add(
                new NodeTemplate.PackageRequirement("p3", "1.0.0"));
            var errors =
                release.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net45 }).ToList();
            this.WriteErrors(errors);
            Assert.Equal(1, errors.Count);
            Assert.Equal("configuration.nodeTemplates[\"t1\"].packageRequirements[\"p3\"]", errors[0].Field);
            Assert.Equal($"Package dependency for {Net45} dp3 is not defined", errors[0].Message);
        }

        /// <summary>
        /// Test for correct workaround of incompatible dependency of package requirements definition in defined template
        /// </summary>
        [Fact]
        public void ReleaseTemplatePackageRequirementDependencyWrongVersionError()
        {
            var release = CreateRelease();
            release.Configuration.NodeTemplates[0].PackageRequirements.Add(
                new NodeTemplate.PackageRequirement("p3", "1.0.0"));
            release.Configuration.NodeTemplates[0].PackageRequirements.Add(
                new NodeTemplate.PackageRequirement("dp3", "1.0.0"));
            var errors =
                release.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net45 }).ToList();
            this.WriteErrors(errors);
            Assert.Equal(1, errors.Count);
            Assert.Equal("configuration.nodeTemplates[\"t1\"].packageRequirements[\"p3\"]", errors[0].Field);
            Assert.Equal($"Package dependency for {Net45} dp3 doesn't satisfy version requirements", errors[0].Message);
        }

        /// <summary>
        /// Test for correct workaround of  package requirements specific version format error in defined template
        /// </summary>
        [Fact]
        public void ReleaseTemplatePackageRequirementVersionFormatError()
        {
            var release = CreateRelease();
            release.Configuration.NodeTemplates[0].PackageRequirements.Add(
                new NodeTemplate.PackageRequirement("test", "strange-version"));
            var errors =
                release.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net45 }).ToList();
            this.WriteErrors(errors);
            Assert.Equal(1, errors.Count);
            Assert.Equal("configuration.nodeTemplates[\"t1\"].packageRequirements[\"test\"]", errors[0].Field);
            Assert.Equal("Package version could not be parsed", errors[0].Message);
        }

        /// <summary>
        /// Test for correct workaround of template requirement for undefined package
        /// </summary>
        [Fact]
        public void ReleaseTemplateUnknownPackagesError()
        {
            var release = CreateRelease();
            release.Configuration.NodeTemplates[0].PackageRequirements.Add(
                new NodeTemplate.PackageRequirement("test", null));
            var errors =
                release.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net45 }).ToList();
            this.WriteErrors(errors);
            Assert.Equal(1, errors.Count);
            Assert.Equal("configuration.nodeTemplates[\"t1\"].packageRequirements[\"test\"]", errors[0].Field);
            Assert.Equal("Package requirement is not defined in release packages", errors[0].Message);
        }

        /// <summary>
        /// Test for correct workaround of template requirement for package that is missed in nuget repository
        /// </summary>
        [Fact]
        public void ReleaseTemplateUnknownPackagesWithVersionError()
        {
            var release = CreateRelease();
            release.Configuration.NodeTemplates[0].PackageRequirements.Add(
                new NodeTemplate.PackageRequirement("test", "1.0.0"));
            var errors =
                release.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net45 }).ToList();
            this.WriteErrors(errors);
            Assert.Equal(1, errors.Count);
            Assert.Equal("configuration.nodeTemplates[\"t1\"].packageRequirements[\"test\"]", errors[0].Field);
            Assert.Equal("Package could not be found in nuget repository", errors[0].Message);
        }
    }
}