// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NugetTest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing package installation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Tests
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using NuGet;
    using NuGet.Frameworks;
    using NuGet.Packaging;
    using NuGet.Protocol;

    using Xunit;
    using Xunit.Abstractions;

    using NullLogger = NuGet.Common.NullLogger;

    /// <summary>
    /// Testing package installation
    /// </summary>
    public class NugetTest
    {
        /// <summary>
        /// The output.
        /// </summary>
        private readonly ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="NugetTest"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public NugetTest(ITestOutputHelper output)
        {
            this.output = output;
        }


        /// <summary>
        /// Testing files installation
        /// </summary>
        /// <param name="packageName">
        /// The package Name to check.
        /// </param>
        [Theory]
        [InlineData("Libuv")]
        [InlineData("Microsoft.AspNetCore.Cors")]
        public void FilesToInstallTest(string packageName)
        {
            var packagesDir = Path.GetFullPath("../../../../packages");
            var packageReader = new FindLocalPackagesResourcePackagesConfig(packagesDir);
            var package = packageReader.GetPackages(NullLogger.Instance, CancellationToken.None)
                .FirstOrDefault(p => p.Identity.Id == packageName);
            Assert.NotNull(package);

            var targetFramework = NuGetFramework.ParseFrameworkName(
                ".NETFramework,Version=v4.6",
                new DefaultFrameworkNameProvider());

            var runtime = "win7-x64";


            using (var reader = package.GetReader())
            {
                var directLib = NuGetFrameworkUtility.GetNearest(reader.GetLibItems(), targetFramework)?.Items.ToList()
                                ?? new List<string>();
                var runtimeSpecific = reader.GetFiles($"runtimes/{runtime}")
                    .Select(
                        f =>
                            {
                                var pathParts = f.Split('/');
                                return new
                                           {
                                               Runtime = pathParts[1],
                                               Framework = NuGetFramework.ParseFolder(pathParts[2]),
                                               File = f
                                           };
                            })
                    .Where(i => i.Runtime == runtime)
                    .GroupBy(f => f.Framework)
                    .Select(g => new FrameworkSpecificGroup(g.Key, g.Select(f => f.File)))
                    .ToList();
                var additionalFiles = NuGetFrameworkUtility.GetNearest(runtimeSpecific, targetFramework)?.Items.ToList()
                                      ?? new List<string>();
                var nativeFiles = runtimeSpecific
                                      .FirstOrDefault(f => f.TargetFramework.GetShortFolderName() == "native")
                                      ?.Items.ToList() ?? new List<string>();

                foreach (var file in directLib)
                {
                    this.output.WriteLine(file);
                }

                foreach (var file in additionalFiles)
                {
                    this.output.WriteLine(file);
                }

                foreach (var file in nativeFiles)
                {
                    this.output.WriteLine(file);
                }

                Assert.True(directLib.Count + additionalFiles.Count + nativeFiles.Count > 0);
            }
        }
    }
}
