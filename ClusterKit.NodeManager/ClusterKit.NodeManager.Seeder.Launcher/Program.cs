// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Service main entry point
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Seeder.Launcher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Versioning;
    using System.Threading;
    using System.Xml;

    using Akka.Configuration;

    using JetBrains.Annotations;

    using NuGet;

    /// <summary>
    /// Service main entry point
    /// </summary>
    [UsedImplicitly]
    public class Program
    {
        /// <summary>
        /// Service main entry point
        /// </summary>
        /// <param name="args">
        /// Startup parameters
        /// </param>
        public static void Main(string[] args)
        {
            var configuration = ReadConfiguration(args);
            if (configuration == null)
            {
                return;
            }

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var executionDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var tempDirectoryInfo = Directory.CreateDirectory(tempDir);
            var executionDirectoryInfo = Directory.CreateDirectory(executionDir);
            try
            {
                var repository = PackageRepositoryFactory.Default.CreateRepository(configuration.Nuget);
                while (true)
                {
                    try
                    {
                        var requiredPackages = configuration.RequiredPackages.Select(
                            name => repository.Search(name, true)
                                .ToList()
                                .FirstOrDefault(p => p.Id == name && p.IsLatestVersion)).ToList();

                        if (requiredPackages.Any(p => p == null))
                        {
                            Console.WriteLine("Could not find required packages in repository... Retrying...");
                            Thread.Sleep(configuration.NugetCheckPeriod);
                            continue;
                        }

                        var dependencies = requiredPackages
                            .Select(
                                p => p.DependencySets.FirstOrDefault(
                                    s => s.SupportedFrameworks == null || !s.SupportedFrameworks.Any()
                                         || s.SupportedFrameworks.Any(f => f == configuration.ExecutionFramework)))
                            .Where(ds => ds != null)
                            .SelectMany(ds => ds.Dependencies)
                            .GroupBy(d => d.Id)
                            .Select(
                                g => repository.Search(g.Key, true)
                                    .ToList()
                                    .Where(p => p.Id == g.Key && g.All(pd => pd.VersionSpec.Satisfies(p.Version)))
                                    .OrderByDescending(p => p.Version)
                                    .FirstOrDefault())
                            .ToList();

                        if (dependencies.Any(d => d == null))
                        {
                            Console.WriteLine("Could not find required package dependencies in repository... Retrying...");
                            Thread.Sleep(configuration.NugetCheckPeriod);
                            continue;
                        }

                        foreach (var package in requiredPackages.Union(dependencies.Where(d => requiredPackages.All(rp => rp.Id != d.Id))))
                        {
                            ExtractPackage(package, configuration.ExecutionFramework, tempDir, executionDir);
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Error while connecting to the nuget repository... Retrying...");
                        Thread.Sleep(configuration.NugetCheckPeriod);
                        continue;
                    }

                    try
                    {
                        File.Copy(configuration.ConfigFile, Path.Combine(executionDir, "seeder.hocon"), true);
                        FixAssemblyVersions(executionDir);
                        Console.WriteLine($"Seeder prepared in {executionDir}");
                        var process = new Process
                                          {
                                              StartInfo =
                                                  {
                                                      UseShellExecute = false,
                                                      WorkingDirectory = executionDir,
                                                      FileName = Path.Combine(executionDir, "ClusterKit.NodeManager.Seeder.exe")
                                                  }
                                          };
                        process.Start();
                        process.WaitForExit();
                        process.Dispose();
                        Console.WriteLine("Seeder stopped");
                        break;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                        Console.ReadLine();
                        break;
                    }
                }
            }
            finally
            {
                tempDirectoryInfo.Delete(true);
                executionDirectoryInfo.Delete(true);
            }
        }

        /// <summary>
        /// Fixes service configuration file to pass possible version conflicts in dependent assemblies
        /// </summary>
        /// <param name="executionDirectory">
        /// The execution Directory.
        /// </param>
        private static void FixAssemblyVersions(string executionDirectory)
        {
            var configName = Path.Combine(executionDirectory, "ClusterKit.NodeManager.Seeder.exe.config");

            XmlDocument document = new XmlDocument();
            document.Load(configName);
            var documentElement = document.DocumentElement;
            if (documentElement == null)
            {
                Console.WriteLine($@"Configuration file {configName} is broken");
                return;
            }

            documentElement = (XmlElement)documentElement.SelectSingleNode("/configuration");
            if (documentElement == null)
            {
                Console.WriteLine($@"Configuration file {configName} is broken");
                return;
            }

            var runTimeNode = documentElement.SelectSingleNode("./runtime")
                              ?? documentElement.AppendChild(document.CreateElement("runtime"));

            var nameTable = document.NameTable;
            var namespaceManager = new XmlNamespaceManager(nameTable);
            const string Uri = "urn:schemas-microsoft-com:asm.v1";
            namespaceManager.AddNamespace("urn", Uri);

            var assemblyBindingNode = runTimeNode.SelectSingleNode("./urn:assemblyBinding", namespaceManager)
                                      ?? runTimeNode.AppendChild(document.CreateElement("assemblyBinding", Uri));

            foreach (var lib in Directory.GetFiles(executionDirectory, "*.dll"))
            {
                var parameters = AssemblyName.GetAssemblyName(lib);
                var dependentNode =
                    assemblyBindingNode?.SelectSingleNode(
                        $"./urn:dependentAssembly[./urn:assemblyIdentity/@name='{parameters.Name}']",
                        namespaceManager)
                    ?? assemblyBindingNode?.AppendChild(document.CreateElement("dependentAssembly", Uri));

                if (dependentNode == null)
                {
                    continue;
                }

                dependentNode.RemoveAll();
                var assemblyIdentityNode =
                    (XmlElement)dependentNode.AppendChild(document.CreateElement("assemblyIdentity", Uri));
                assemblyIdentityNode.SetAttribute("name", parameters.Name);
                var publicKeyToken =
                    BitConverter.ToString(parameters.GetPublicKeyToken())
                        .Replace("-", string.Empty)
                        .ToLower(CultureInfo.InvariantCulture);
                assemblyIdentityNode.SetAttribute("publicKeyToken", publicKeyToken);
                var bindingRedirectNode =
                    (XmlElement)dependentNode.AppendChild(document.CreateElement("bindingRedirect", Uri));
                bindingRedirectNode.SetAttribute("oldVersion", $"0.0.0.0-{parameters.Version}");
                bindingRedirectNode.SetAttribute("newVersion", parameters.Version.ToString());
            }

            document.Save(configName);
        }

        /// <summary>
        /// Reads the configuration
        /// </summary>
        /// <param name="args">The program command line arguments</param>
        /// <returns>The configuration</returns>
        private static Configuration ReadConfiguration(string[] args)
        {
            var configuration = new Configuration();
            configuration.ConfigFile = args != null && args.Length > 0 ? args[0] : "seeder.hocon";
            if (configuration.ConfigFile == null || !File.Exists(configuration.ConfigFile))
            {
                Console.WriteLine("Configuration file was not found");
                return null;
            }

            configuration.Config = ConfigurationFactory.ParseString(File.ReadAllText(configuration.ConfigFile));
            configuration.RequiredPackages = configuration.Config.GetStringList("RequiredPackages");

            if (configuration.RequiredPackages == null || configuration.RequiredPackages.Count == 0
                || configuration.RequiredPackages.All(p => p != "ClusterKit.NodeManager.Seeder"))
            {
                Console.WriteLine("RequiredPackages is not properly defined");
                return null;
            }

            configuration.Nuget = configuration.Config.GetString("Nuget");
            if (string.IsNullOrWhiteSpace(configuration.Nuget))
            {
                Console.WriteLine("Nuget is not properly defined");
                return null;
            }

            configuration.Seeders = configuration.Config.GetStringList("Seeders");
            if (configuration.Seeders == null || configuration.Seeders.Count == 0)
            {
                Console.WriteLine("Seeders is not properly defined");
                return null;
            }

            configuration.NugetCheckPeriod = configuration.Config.GetTimeSpan("NugetCheckPeriod", TimeSpan.FromMinutes(1));
            configuration.ExecutionFramework = new FrameworkName(configuration.Config.GetString("ExecutionFramework"));
            return configuration;
        }

        /// <summary>
        /// Extracts the lib files to execution directory
        /// </summary>
        /// <param name="package">The package to extract</param>
        /// <param name="frameworkName">The current framework name</param>
        /// <param name="tmpDir">The temp directory to extract packages</param>
        /// <param name="executionDir">The execution directory to load packages</param>
        private static void ExtractPackage(IPackage package, FrameworkName frameworkName, string tmpDir, string executionDir)
        {
            Console.WriteLine($"Installing {package.Id} {package.Version}");
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
    }
}
