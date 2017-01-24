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
    using System.IO.Compression;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Serialization;

    using ClusterKit.NodeManager.Launcher.Messages;


    using Newtonsoft.Json;

    using NuGet;

    using Formatting = Newtonsoft.Json.Formatting;

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




            var description = JsonConvert.DeserializeObject<NodeStartUpConfiguration>(File.ReadAllText(fileName));


            var installedMetadata = 
                args
                .Skip(1)
                .SelectMany(d => Directory.GetFiles(d, "*.nupkg", SearchOption.AllDirectories))
                .Select(GetMetadata)
                .Where(m => m!= null).ToList();



            var localPackages = installedMetadata
                .Where(rp => description.Packages.Any(lp => lp.Id == rp.Id))
                .Where(rp => rp.Version == installedMetadata.Where(p => p.Id == rp.Id).Max(p => p.Version))
                .ToList();

            Console.WriteLine("Packages found: ");
            foreach (var package in localPackages)
            {
                Console.WriteLine($"\t{package.Id} {package.Version}");
            }
            Console.WriteLine("");

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
                    Version = allDependencies.Where(dd => dd.Id == id).Max(dd => SemanticVersion.Parse(dd.Version)).ToString()
                }).ToList();

            var result = localPackages
                .Select(p => new PackageDescription { Id = p.Id, Version = p.Version.ToString() })
                .OrderBy(p => p.Id)
                .Union(dependencies)
                .Distinct()
                .ToList();

            Console.WriteLine("Packages: ");
            foreach (var package in result)
            {
                Console.WriteLine($"\t{package.Id} {package.Version}");
            }

            description.Packages = result;
            File.WriteAllText(fileName, JsonConvert.SerializeObject(description, Formatting.Indented));
        }

        private static ManifestMetadata GetMetadata(string packageName)
        {
            using (var file = File.OpenRead(packageName))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
            {
                var nuspecFile = zip.Entries.FirstOrDefault(e => e.Name.EndsWith(".nuspec"));
                if (nuspecFile == null)
                {
                    Console.WriteLine($"Could not find metadata for package {packageName}");
                    return null;
                }
                using (var stream = nuspecFile.Open())
                {
                    return Manifest.ReadFrom(stream, false).Metadata;
                }
            }
        }


    }
}