// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuildUtils.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Bundle of utilities to build project
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Build
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Xml;

    using Fake;
    using Fake.Testing;

    using Microsoft.FSharp.Core;

    using Test.FAKECore;

    /// <summary>
    /// Bundle of utilities to build project
    /// </summary>
    public static class BuildUtils
    {
        /// <summary>
        /// Gets temporary directory to store clean builds of projects
        /// </summary>
        public static string BuildClean { get; private set; }

        /// <summary>
        /// Gets temporary directory to build projects
        /// </summary>
        public static string BuildDirectory { get; private set; }

        /// <summary>
        /// Gets temporary directory to use in build procedures
        /// </summary>
        public static string BuildTemp { get; private set; }

        /// <summary>
        /// Gets output directory to put prepared nuget packages
        /// </summary>
        public static string PackageDirecotry { get; private set; }

        /// <summary>
        /// Gets global output packages version
        /// </summary>
        public static string Version { get; private set; }

        /// <summary>
        /// Builds current project
        /// </summary>
        /// <param name="project">Project to build</param>
        public static void Build(ProjectDescription project)
        {
            TraceHelper.trace($"Building {project.ProjectName}");

            if (Directory.Exists(project.TempBuildDirectory))
            {
                Directory.Delete(project.TempBuildDirectory, true);
            }

            if (Directory.Exists(project.CleanBuildDirectory))
            {
                Directory.Delete(project.CleanBuildDirectory, true);
            }

            // restoring packages for project with respec to global configuration file
            Func<Unit, Unit> failFunc = u => u;
            var nugetPath = RestorePackageHelper.findNuget("./");
            RestorePackageHelper.runNuGet(
                nugetPath,
                TimeSpan.FromMinutes(10),
                $"restore {project.ProjectFileName} -ConfigFile ./nuget.config",
                failFunc.ToFSharpFunc());

            // modifying project file to substitute internal nute-through dependencies to currently built files
            var projDoc = new XmlDocument();
            projDoc.Load(project.ProjectFileName);
            var originalProjDoc = new XmlDocument();
            originalProjDoc.Load(project.ProjectFileName);

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(projDoc.NameTable);
            namespaceManager.AddNamespace("def", "http://schemas.microsoft.com/developer/msbuild/2003");

            foreach (var dependency in project.InternalDependencies)
            {
                var refNode = projDoc
                    .DocumentElement
                    .SelectNodes("//def:Reference", namespaceManager)
                    .Cast<XmlElement>()
                    .FirstOrDefault(e => e.HasAttribute("Include") && Regex.IsMatch(e.Attributes["Include"].Value, $"^{Regex.Escape(dependency)}((, )|$)"));

                if (refNode != null)
                {
                    ConsoleLog($"ref linked to {dependency} was updated");
                    refNode.Attributes["Include"].Value = $"{dependency}";
                    refNode.SelectSingleNode("./def:HintPath", namespaceManager).InnerText = Path.Combine(
                        Path.GetFullPath(BuildClean),
                        $"{dependency}\\{dependency}.dll");
                }
                else
                {
                    ConsoleLog($"{dependency} ref node was not found");
                }
            }

            projDoc.Save(project.ProjectFileName);

            try
            {
                MSBuildHelper.MSBuildRelease(project.TempBuildDirectory, "Rebuild", new[] { project.ProjectFileName });
            }
            finally
            {
                // restoring original project file
                originalProjDoc.Save(project.ProjectFileName);
                projDoc.Save(Path.Combine(project.TempBuildDirectory, $"{project.ProjectName}.csproj.modified"));
            }

            Directory.CreateDirectory(project.CleanBuildDirectory);
            var buildFiles = Directory.GetFiles(project.TempBuildDirectory, $"{project.ProjectName}.*");
            foreach (var file in buildFiles)
            {
                var fileName = Path.GetFileName(file);
                ConsoleLog($"Copying {fileName}");
                File.Copy(Path.Combine(project.TempBuildDirectory, fileName), Path.Combine(project.CleanBuildDirectory, fileName));
            }
        }

        /// <summary>
        /// Builds all projects
        /// </summary>
        /// <param name="projects">The list of projects to build</param>
        public static void Build(IEnumerable<ProjectDescription> projects)
        {
            foreach (var project in projects)
            {
                Build(project);
            }
        }

        /// <summary>
        /// Configures current build execution
        /// </summary>
        /// <param name="version">Global output packages version</param>
        /// <param name="buildDirectory">Temp directory to build projects</param>
        /// <param name="packageDirecotry">Output directory to put prepared nuget packages</param>
        public static void Configure(
            // ReSharper disable ParameterHidesMember
            string version,
            string buildDirectory,
            string packageDirecotry /* ReSharper restore ParameterHidesMember */)
        {
            BuildUtils.Version = version;
            BuildUtils.BuildDirectory = buildDirectory;
            BuildUtils.PackageDirecotry = packageDirecotry;

            BuildTemp = Path.Combine(buildDirectory, "tmp");
            BuildClean = Path.Combine(buildDirectory, "clean");
        }

        /// <summary>
        /// Creates nuget package
        /// </summary>
        /// <param name="project">
        /// The project to publish
        /// </param>
        public static void CreateNuget(ProjectDescription project)
        {
            if (!project.ProjectType.HasFlag(ProjectDescription.EnProjectType.NugetPackage))
            {
                return;
            }

            TraceHelper.trace($"Packing {project.ProjectName}");

            var nuspecData = new XmlDocument();
            var nuspecDataFileName = $"{project.ProjectName}.nuspec";
            nuspecData.Load(Path.Combine(project.ProjectDirectory, nuspecDataFileName));
            // ReSharper disable PossibleNullReferenceException
            var metaData = nuspecData.DocumentElement.SelectSingleNode("/package/metadata");
            metaData.SelectSingleNode("id").InnerText = project.ProjectName;
            metaData.SelectSingleNode("version").InnerText = Version;
            metaData.SelectSingleNode("title").InnerText = project.ProjectName;
            metaData.SelectSingleNode("authors").InnerText = "ClusterKit Team";  // todo: @m_kantarovsky publish correct data
            metaData.SelectSingleNode("owners").InnerText = "ClusterKit Team";  // todo: @m_kantarovsky publish correct data
            metaData.SelectSingleNode("description").InnerText = "ClusterKit lib"; // todo: @m_kantarovsky publish correct data

            var dependenciesRootElement = metaData.AppendChild(nuspecData.CreateElement("dependencies"));
            var dependenciesDoc = new XmlDocument();
            dependenciesDoc.Load(Path.Combine(project.ProjectDirectory, "packages.config"));

            foreach (XmlElement dependency in dependenciesDoc.DocumentElement.SelectNodes("/packages/package"))
            {
                if (project.InternalDependencies.Contains(dependency.Attributes["id"].Value))
                {
                    continue;
                }

                var dependencyElement = dependenciesRootElement.AppendChild(nuspecData.CreateElement("dependency"));
                dependencyElement.Attributes.Append(nuspecData.CreateAttribute("id")).Value =
                    dependency.Attributes["id"].Value;
                dependencyElement.Attributes.Append(nuspecData.CreateAttribute("version")).Value =
                    dependency.Attributes["version"].Value;
            }

            foreach (var dependency in project.InternalDependencies)
            {
                var dependencyElement = dependenciesRootElement.AppendChild(nuspecData.CreateElement("dependency"));
                dependencyElement.Attributes.Append(nuspecData.CreateAttribute("id")).Value =
                    dependency;
                dependencyElement.Attributes.Append(nuspecData.CreateAttribute("version")).Value = Version;
            }

            var filesRootElement = nuspecData.DocumentElement.SelectSingleNode("/package").AppendChild(nuspecData.CreateElement("files"));
            foreach (var file in Directory.GetFiles(project.CleanBuildDirectory))
            {
                var fileElement = filesRootElement.AppendChild(nuspecData.CreateElement("file"));
                fileElement.Attributes.Append(nuspecData.CreateAttribute("src")).Value = Path.GetFileName(file);
                fileElement.Attributes.Append(nuspecData.CreateAttribute("target")).Value = $"./lib/{Path.GetFileName(file)}";
            }

            var generatedNuspecFile = Path.Combine(project.CleanBuildDirectory, nuspecDataFileName);
            nuspecData.Save(generatedNuspecFile);

            if (!Directory.Exists(PackageDirecotry))
            {
                Directory.CreateDirectory(PackageDirecotry);
            }

            Func<NuGetHelper.NuGetParams, NuGetHelper.NuGetParams> provider = defaults =>
                {
                    defaults.SetFieldValue("Version", Version);
                    defaults.SetFieldValue("WorkingDir", project.CleanBuildDirectory);
                    defaults.SetFieldValue("OutputPath", PackageDirecotry);
                    return defaults;
                };

            Fake.NuGetHelper.NuGetPackDirectly(provider.ToFSharpFunc(), generatedNuspecFile);

            // ReSharper restore PossibleNullReferenceException
        }

        /// <summary>
        /// Creates nuget package for all projects
        /// </summary>
        /// <param name="projects">The list of projects to package</param>
        public static void CreateNuget(IEnumerable<ProjectDescription> projects)
        {
            foreach (var project in projects)
            {
                CreateNuget(project);
            }
        }

        /// <summary>
        /// Runs defined unit tests on all projects
        /// </summary>
        /// <param name="projects">The list of projects to test</param>
        public static void RunXUnitTest(IEnumerable<ProjectDescription> projects)
        {
            var testAssemblies =
                projects.Where(p => p.ProjectType.HasFlag(ProjectDescription.EnProjectType.XUnitTests))
                    .Select(p => Path.Combine(p.TempBuildDirectory, $"{p.ProjectName}.dll"));

            var runnerLocation = Directory.GetDirectories(Path.Combine(Directory.GetCurrentDirectory(), "packages"))
                .OrderByDescending(d => d)
                .First();

            Func<XUnit2.XUnit2Params, XUnit2.XUnit2Params> testParameters = p =>
            {
                p.SetFieldValue("ToolPath", Path.Combine(runnerLocation, "tools", "xunit.console.exe"));
                return p;
            };

            XUnit2.xUnit2(testParameters.ToFSharpFunc(), testAssemblies);
        }

        /// <summary>
        /// Debugger method to write sum console output
        /// </summary>
        /// <param name="message"></param>
        private static void ConsoleLog(string message)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(message);
            Console.ForegroundColor = currentColor;
        }

        /// <summary>
        /// Kludge to work with F# objects
        /// </summary>
        /// <param name="obj">Object to modify</param>
        /// <param name="fieldName">Field name</param>
        /// <param name="value">Value to store</param>
        private static void SetFieldValue(this object obj, string fieldName, object value)
        {
            var fieldInfo = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?? obj.GetType().GetField($"{fieldName}\u0040", BindingFlags.Instance | BindingFlags.NonPublic);
            fieldInfo.SetValue(obj, value);
        }
    }
}