// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationCheckTests.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Checks the work of <see cref="ConfigurationExtensions" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using KlusterKite.NodeManager.Client.ORM;
    using KlusterKite.NodeManager.ConfigurationSource;
    using KlusterKite.NodeManager.Launcher.Messages;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Checks the work of <see cref="ConfigurationExtensions"/>
    /// </summary>
    public class ConfigurationCheckTests : ConfigurationCheckTestsBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationCheckTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public ConfigurationCheckTests(ITestOutputHelper output)
            : base(output)
        {
        }

#if APPDOMAIN
        /// <summary>
        /// Gets the current runtime
        /// </summary>
        private static string CurrentRuntime => Net46;
#elif CORECLR
        /// <summary>
        /// Gets the current runtime
        /// </summary>
        private static string CurrentRuntime => NetCore;
#endif

        /// <summary>
        /// Testing that default configuration has no errors
        /// </summary>
        /// <returns>
        /// The async task
        /// </returns>
        [Fact]
        public async Task DefaultConfigurationNoError()
        {
            var configuration = CreateConfiguration();
            var errors =
                (await configuration.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net46, NetCore })).ToList();
            this.WriteErrors(errors);
            Assert.Equal(0, errors.Count);

            var template = configuration.Settings.NodeTemplates.First();
            Assert.NotNull(template.PackagesToInstall);
            Assert.True(template.PackagesToInstall.ContainsKey(CurrentRuntime));
            var packagesToInstall = template.PackagesToInstall[CurrentRuntime];
            Assert.Equal(
                "dp1 1.0.0; dp2 1.0.0; p1 1.0.0; p2 1.0.0",
                string.Join("; ", packagesToInstall.OrderBy(p => p.Id).Select(p => $"{p.Id} {p.Version}")));
        }

        /// <summary>
        /// Tests the error of missed package definitions
        /// </summary>
        /// <returns>
        /// The async task
        /// </returns>
        [Fact]
        public async Task NoPackagesError()
        {
            var configuration = CreateConfiguration(new string[0]);
            var errors =
                (await configuration.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net46, NetCore })).ToList();
            this.WriteErrors(errors);
            Assert.Equal(1, errors.Count);
            Assert.Equal("configuration.packages", errors[0].Field);
        }

        /// <summary>
        /// Tests the error of missed template definitions
        /// </summary>
        /// <returns>
        /// The async task
        /// </returns>
        [Fact]
        public async Task NoTemplatesError()
        {
            var configuration = CreateConfiguration();
            configuration.Settings.NodeTemplates.Clear();
            var errors =
                (await configuration.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net46, NetCore })).ToList();
            this.WriteErrors(errors);
            Assert.Equal(1, errors.Count);
            Assert.Equal("configuration.nodeTemplates", errors[0].Field);
        }

        /// <summary>
        /// Test for correct workaround of incompatible package versions
        /// </summary>
        /// <returns>
        /// The async task
        /// </returns>
        [Fact]
        public async Task ConfigurationPackageDependencyInvalidVersionError()
        {
            var configuration = CreateConfiguration();
            configuration.Settings.Packages.Add(new PackageDescription("p3", "1.0.0"));
            configuration.Settings.Packages.Add(new PackageDescription("dp3", "1.0.0"));
            var errors =
                (await configuration.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net46, NetCore })).ToList();
            this.WriteErrors(errors);
            Assert.Equal(1, errors.Count);
            Assert.Equal("configuration.packages[\"dp3\"]", errors[0].Field);
            Assert.Equal($"Package doesn't satisfy other packages version requirements", errors[0].Message);
        }

        /// <summary>
        /// Test for correct workaround of missed definition of package dependencies
        /// </summary>
        /// <returns>
        /// The async task
        /// </returns>
        [Fact]
        public async Task ConfigurationPackageDependencyNotDefinedError()
        {
            var configuration = CreateConfiguration();
            configuration.Settings.Packages.Add(new PackageDescription("p3", "1.0.0"));
            var errors =
                (await configuration.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net46, NetCore })).ToList();
            this.WriteErrors(errors);
            Assert.Equal(1, errors.Count);
            Assert.Equal("configuration.packages[\"dp3\"]", errors[0].Field);
            Assert.Equal("Package is not defined", errors[0].Message);
        }

        /// <summary>
        /// Test for correct workaround of package definitions missed in nuget repository
        /// </summary>
        /// <returns>
        /// The async task
        /// </returns>
        [Fact]
        public async Task ConfigurationPackageNotFoundError()
        {
            var configuration = CreateConfiguration();
            configuration.Settings.Packages.Add(new PackageDescription("test", "1.0.0"));
            var errors =
                (await configuration.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net46, NetCore })).ToList();
            this.WriteErrors(errors);
            Assert.Equal(1, errors.Count);
            Assert.Equal("configuration.packages[\"test\"]", errors[0].Field);
            Assert.Equal("Package of specified version could not be found in the nuget repository", errors[0].Message);
        }

        /// <summary>
        /// Testing package version format error
        /// </summary>
        /// <returns>
        /// The async task
        /// </returns>
        [Fact]
        public async Task ConfigurationPackageVersionFormatError()
        {
            var configuration = CreateConfiguration();
            configuration.Settings.Packages.Add(new PackageDescription("test", "strange-version"));
            var errors =
                (await configuration.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net46, NetCore })).ToList();
            this.WriteErrors(errors);
            Assert.Equal(1, errors.Count);
            Assert.Equal("configuration.packages[\"test\"]", errors[0].Field);
            Assert.Equal("Package version could not be parsed", errors[0].Message);
        }

        /// <summary>
        /// Test for correct workaround of missed package requirements definition in defined template
        /// </summary>
        /// <returns>
        /// The async task
        /// </returns>
        [Fact]
        public async Task ConfigurationTemplateNoPackagesError()
        {
            var configuration = CreateConfiguration();
            configuration.Settings.NodeTemplates.Add(new NodeTemplate { Code = "t2" });
            var errors =
                (await configuration.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net46, NetCore })).ToList();
            this.WriteErrors(errors);
            Assert.Equal(1, errors.Count);
            Assert.Equal("configuration.nodeTemplates[\"t2\"]", errors[0].Field);
            Assert.Equal("Package requirements are not set", errors[0].Message);
        }

        /// <summary>
        /// Test for correct workaround of missed dependency of package requirements definition in defined template
        /// </summary>
        /// <returns>
        /// The async task
        /// </returns>
        [Fact]
        public async Task ConfigurationTemplatePackageRequirementDependencyNotFoundError()
        {
            var configuration = CreateConfiguration();
            configuration.Settings.NodeTemplates[0].PackageRequirements.Add(
                new NodeTemplate.PackageRequirement("p3", "1.0.0"));
            var errors =
                (await configuration.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net46, NetCore })).ToList();
            this.WriteErrors(errors);
            Assert.Equal(2, errors.Count);
            Assert.Equal("configuration.nodeTemplates[\"t1\"].packageRequirements[\"p3\"]", errors[0].Field);
            Assert.Equal($"Package dependency for {Net46} dp3 [2.0.0, ) is missing", errors[0].Message);
            Assert.Equal("configuration.nodeTemplates[\"t1\"].packageRequirements[\"p3\"]", errors[1].Field);
            Assert.Equal($"Package dependency for {NetCore} dp3 [2.0.0, ) is missing", errors[1].Message);
        }

        /// <summary>
        /// Test for correct workaround of incompatible dependency of package requirements definition in defined template
        /// </summary>
        /// <returns>
        /// The async task
        /// </returns>
        [Fact]
        public async Task ConfigurationTemplatePackageRequirementDependencyWrongVersionError()
        {
            var configuration = CreateConfiguration();
            configuration.Settings.NodeTemplates[0].PackageRequirements.Add(
                new NodeTemplate.PackageRequirement("p3", "1.0.0"));
            configuration.Settings.NodeTemplates[0].PackageRequirements.Add(
                new NodeTemplate.PackageRequirement("dp3", "1.0.0"));
            var errors =
                (await configuration.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net46, NetCore })).ToList();
            this.WriteErrors(errors);
            Assert.Equal(2, errors.Count);
            Assert.Equal("configuration.nodeTemplates[\"t1\"].packageRequirements[\"p3\"]", errors[0].Field);
            Assert.Equal($"Package dependency for {Net46} dp3.1.0.0 doesn't satisfy version requirements [2.0.0, ).", errors[0].Message);
            Assert.Equal("configuration.nodeTemplates[\"t1\"].packageRequirements[\"p3\"]", errors[1].Field);
            Assert.Equal($"Package dependency for {NetCore} dp3.1.0.0 doesn't satisfy version requirements [2.0.0, ).", errors[1].Message);
        }

        /// <summary>
        /// Test for correct workaround of  package requirements specific version format error in defined template
        /// </summary>
        /// <returns>
        /// The async task
        /// </returns>
        [Fact]
        public async Task ConfigurationTemplatePackageRequirementVersionFormatError()
        {
            var configuration = CreateConfiguration();
            configuration.Settings.NodeTemplates[0].PackageRequirements.Add(
                new NodeTemplate.PackageRequirement("test", "strange-version"));
            var errors =
                (await configuration.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net46, NetCore })).ToList();
            this.WriteErrors(errors);
            Assert.Equal(1, errors.Count);
            Assert.Equal("configuration.nodeTemplates[\"t1\"].packageRequirements[\"test\"]", errors[0].Field);
            Assert.Equal("Package version could not be parsed", errors[0].Message);
        }

        /// <summary>
        /// Test for correct workaround of template requirement for undefined package
        /// </summary>
        /// <returns>
        /// The async task
        /// </returns>
        [Fact]
        public async Task ConfigurationTemplateUnknownPackagesError()
        {
            var configuration = CreateConfiguration();
            configuration.Settings.NodeTemplates[0].PackageRequirements.Add(
                new NodeTemplate.PackageRequirement("test", null));
            var errors =
                (await configuration.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net46, NetCore })).ToList();
            this.WriteErrors(errors);
            Assert.Equal(1, errors.Count);
            Assert.Equal("configuration.nodeTemplates[\"t1\"].packageRequirements[\"test\"]", errors[0].Field);
            Assert.Equal("Package requirement is not defined in configuration packages", errors[0].Message);
        }

        /// <summary>
        /// Test for correct workaround of template requirement for package that is missed in nuget repository
        /// </summary>
        /// <returns>
        /// The async task
        /// </returns>
        [Fact]
        public async Task ConfigurationTemplateUnknownPackagesWithVersionError()
        {
            var configuration = CreateConfiguration();
            configuration.Settings.NodeTemplates[0].PackageRequirements.Add(
                new NodeTemplate.PackageRequirement("test", "1.0.0"));
            var errors =
                (await configuration.SetPackagesDescriptionsForTemplates(CreateRepository(), new List<string> { Net46, NetCore })).ToList();
            this.WriteErrors(errors);
            Assert.Equal(1, errors.Count);
            Assert.Equal("configuration.nodeTemplates[\"t1\"].packageRequirements[\"test\"]", errors[0].Field);
            Assert.Equal("Package could not be found in nuget repository", errors[0].Message);
        }
    }
}