// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Fixes the list of packages for dependenceis in fall-back files
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.FallbackPackageDependencyFixer
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using ClusterKit.NodeManager.Launcher.Messages;

    using Newtonsoft.Json;

    using NuGet;

    /// <summary>
    /// Fixes the list of packages for dependencies in fall-back files
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">List of command line arguments</param>
        private static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine($"Usage: {Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)} [fallback.json] [package directories...]");
                return;
            }

            var fileName = args[0];
            if (!File.Exists(fileName))
            {
                Console.WriteLine($"Could not find file {fileName}");
                return;
            }

            foreach (var packageDirectory in args.Skip(1).Where(packageDirectory => !Directory.Exists(packageDirectory)))
            {
                Console.WriteLine($"Could not find package directory {packageDirectory}");
                return;
            }

            NodeStartUpConfiguration description;

            using (var reader = File.OpenText(fileName))
            using (var jsonReader = new JsonTextReader(reader))
            {
                description = JsonSerializer.Create().Deserialize<NodeStartUpConfiguration>(jsonReader);
            }

            var getPackage = typeof(LocalPackageRepository).GetMethod("OpenPackage", BindingFlags.NonPublic | BindingFlags.Instance);

            var generatedPackages = args.Skip(1)
                .SelectMany(d => Directory.GetFiles(d, "*.nupkg"))
                .Select(directory => new LocalPackageRepository(Path.GetFullPath(directory), false))
                .Select(nugetRepository => (IPackage)getPackage.Invoke(nugetRepository, new object[] { nugetRepository.Source }))
                .Where(p => p != null)
                .ToList();

            var localPackages = generatedPackages
                .Where(rp => description.Packages.Any(lp => lp.Id == rp.Id))
                .Where(rp => rp.Version == generatedPackages.Where(p => p.Id == rp.Id).Max(p => p.Version))
                .ToList();

            var allDependencies = localPackages
                .SelectMany(p => p.DependencySets)
                .SelectMany(p => p.Dependencies)
                .ToList();
            var dependencies = allDependencies
                .Select(d => d.Id)
                .Where(id => localPackages.All(p => p.Id != id))
                .OrderBy(id => id)
                .Distinct()
                .Select(id => new PackageDescription
                {
                    Id = id,
                    Version = allDependencies.Where(dd => dd.Id == id).Max(dd => SemanticVersion.Parse(dd.VersionSpec.ToString())).ToString()
                }).ToList();

            var result = localPackages
                .Select(p => new PackageDescription { Id = p.Id, Version = p.Version.ToString() })
                .OrderBy(p => p.Id)
                .Union(dependencies)
                .ToList();

            Console.WriteLine("Packages: ");
            foreach (var package in result)
            {
                Console.WriteLine($"\t{package.Id} {package.Version}");
            }

            description.Packages = result;

            using (var writer = File.OpenWrite(fileName))
            using (var textWriter = new StreamWriter(writer))
            using (var jsonReader = new JsonTextWriter(textWriter))
            {
                var jsonSerializer = JsonSerializer.Create();
                jsonSerializer.Formatting = Formatting.Indented;
                jsonSerializer.Serialize(jsonReader, description);
            }
        }
    }
}