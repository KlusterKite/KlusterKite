// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Experiments.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The list of migration experimenting tests
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.DbTests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Versioning;
    using System.Security.Policy;

    using ClusterKit.Core;
    using ClusterKit.NodeManager.DbTests.Context;
    using ClusterKit.NodeManager.DbTests.Migrations;

    using NuGet;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// The list of migration experimenting tests
    /// </summary>
    public class Experiments
    {
        /// <summary>
        /// The test output
        /// </summary>
        private ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="Experiments"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public Experiments(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// Test 1
        /// </summary>
        [Fact]
        public void EntityFrameworkTest()
        {
            Database.SetInitializer(new NullDatabaseInitializer<TestContext>());
            using (var connection = TempConnection.Create(this.output))
            {
                var configuration = new Configuration
                                        {
                                            TargetDatabase = new DbConnectionInfo(
                                                connection.ConnectionString,
                                                "Npgsql")
                                        };
                var migrator = new DbMigrator(configuration);
                migrator.Update("Update");
                
                using (var context = new TestContext(connection, false))
                { 
                    var testObject = new TestObject { Name = "test1" };
                    context.TestObjects.Add(testObject);
                    context.SaveChanges();
                }

                using (var context = new TestContext(connection, false))
                {
                    Assert.Equal(1, context.TestObjects.Count());
                    Assert.Equal("test1", context.TestObjects.First().Name);
                }

                migrator.Update("Init");

                using (var context = new TestContext(connection, false))
                {
                    Assert.Equal(1, context.TestObjects.Count());
                    Assert.Equal("test1", context.TestObjects.First().Name);
                }
            }
        }

        /// <summary>
        /// Testing work with nuget
        /// </summary>
        [Fact]
        public void NugetTest()
        {
            var repository = PackageRepositoryFactory.Default.CreateRepository("http://nuget/");

            var package = repository
                .Search("ClusterKit.Core", true)
                .ToList()
                .First(p => p.Id == "ClusterKit.Core" && p.IsLatestVersion);

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var executionDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var tempDirectoryInfo = Directory.CreateDirectory(tempDir);
            var executionDirectoryInfo = Directory.CreateDirectory(executionDir);
            var frameworkName = new FrameworkName(".NETFramework,Version=v4.6");

            AppDomainSetup domainSetup = new AppDomainSetup();
            domainSetup.ApplicationBase = executionDir;
            Evidence evidence = AppDomain.CurrentDomain.Evidence;
            var appDomain = AppDomain.CreateDomain("TestDomain", evidence, domainSetup);

            try
            {
                this.ExtractPackage(package, frameworkName, tempDir, executionDir);
                var dependencies = package.DependencySets.FirstOrDefault(
                    s => s.SupportedFrameworks == null
                    || !s.SupportedFrameworks.Any()
                         || s.SupportedFrameworks.Any(f => f.FullName == ".NETFramework,Version=v4.5"));
                if (dependencies != null)
                {
                    foreach (var dependency in dependencies.Dependencies)
                    {
                        // check for missing
                        var dependencyPackage = repository.Search(dependency.Id, true)
                            .ToList()
                            .Where(p => p.Id == dependency.Id && dependency.VersionSpec.Satisfies(p.Version))
                            .OrderBy(p => p.Version)
                            .First();

                        this.ExtractPackage(dependencyPackage, frameworkName, tempDir, executionDir);
                    }
                }

                foreach (var lib in executionDirectoryInfo.GetFiles("*.dll"))
                {
                    appDomain.Load(File.ReadAllBytes(lib.FullName));
                }

                var caller = new Caller();
                appDomain.DoCallBack(() => { });
                this.output.WriteLine(caller.Result);
                
            }
            finally 
            {
                AppDomain.Unload(appDomain);
                tempDirectoryInfo.Delete(true);
                executionDirectoryInfo.Delete(true);
            }

            Assert.NotNull(package);
        }

        /// <summary>
        /// Extracts the lib files to execution directory
        /// </summary>
        /// <param name="package">The package to extract</param>
        /// <param name="frameworkName">The current framework name</param>
        /// <param name="tmpDir">The temp directory to extract packages</param>
        /// <param name="executionDir">The execution directory to load packages</param>
        private void ExtractPackage(IPackage package, FrameworkName frameworkName, string tmpDir, string executionDir)
        {
            var fileSystem = new PhysicalFileSystem(tmpDir);
            package.ExtractContents(fileSystem, package.Id);

            IEnumerable<IPackageFile> compatibleFiles;
            if (VersionUtility.TryGetCompatibleItems(frameworkName, package.GetLibFiles(), out compatibleFiles))
            {
                foreach (var compatibleFile in compatibleFiles)
                {
                    File.Copy(
                        Path.Combine(tmpDir, package.Id, compatibleFile.Path),
                        Path.Combine(executionDir, Path.GetFileName(compatibleFile.Path)),
                        true);
                }
            }
        }

        /// <summary>
        /// Draws directory contents recursively
        /// </summary>
        /// <param name="directoryInfo">The directory</param>
        private void DrawDirContents(DirectoryInfo directoryInfo)
        {
            foreach (var fileSystemInfo in directoryInfo.GetFileSystemInfos())
            {
                this.output.WriteLine(fileSystemInfo.FullName);
                var subDir = fileSystemInfo as DirectoryInfo;
                if (subDir != null)
                {
                    this.DrawDirContents(subDir);
                }
            }
        }

        [Serializable]
        public class Caller
        {
            public string Result { get; set; }

            public void CallMethod()
            {
                var installer = Activator.CreateInstance(null, "ClusterKit.Core.Installer").Unwrap();
                this.Result = (string)installer.GetType().GetMethod("GetAkkaConfig").Invoke(installer, new object[0]);
            }
        }
    }
}
