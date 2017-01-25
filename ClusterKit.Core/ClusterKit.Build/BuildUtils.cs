// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuildUtils.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Bundle of utilities to build project
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable ArrangeStaticMemberQualifier
namespace ClusterKit.Build
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading;
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
        /// The list of defined projects
        /// </summary>
        private static List<ProjectDescription> definedProjectDescriptions = new List<ProjectDescription>();

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
        /// Gets directory with sources nuget packages
        /// </summary>
        public static string PackageInputDirectory { get; private set; }

        /// <summary>
        /// Gets output directory to put prepared nuget packages
        /// </summary>
        public static string PackageOutputDirectory { get; private set; }

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
            var tempSrc = Path.Combine(Path.GetFullPath(BuildDirectory), "src");

            TraceHelper.trace($"Building {project.ProjectName}");

            if (Directory.Exists(project.TempBuildDirectory))
            {
                Directory.Delete(project.TempBuildDirectory, true);
            }

            if (Directory.Exists(project.CleanBuildDirectory))
            {
                Directory.Delete(project.CleanBuildDirectory, true);
            }

            if (Directory.Exists(tempSrc))
            {
                var attempt = 0;
                while (true)
                {
                    try
                    {
                        Directory.Delete(tempSrc, true);
                        continue;
                    }
                    catch (IOException)
                    {
                        ConsoleLog("Src directory is blocked. Retrying...");
                        attempt++;
                        Thread.Sleep(100);
                        if (attempt >= 10)
                        {
                            throw;
                        }
                    }

                    break;
                }
            }

            Directory.CreateDirectory(project.CleanBuildDirectory);
            Directory.CreateDirectory(project.TempBuildDirectory);

            RestoreProjectDependencies(project);

            Func<string, bool> filter = s => true;
            FileHelper.CopyDir(tempSrc, project.ProjectDirectory, filter.ToFSharpFunc());
            // ReSharper disable once AssignNullToNotNullAttribute
            var projectFileName = Path.Combine(tempSrc, Path.GetFileName(project.ProjectFileName));

            // modifying project file to substitute internal nute-through dependencies to currently built files
            var projDoc = new XmlDocument();
            projDoc.Load(projectFileName);

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(projDoc.NameTable);
            namespaceManager.AddNamespace("def", "http://schemas.microsoft.com/developer/msbuild/2003");

            var rootNode = projDoc.DocumentElement;
            if (rootNode == null)
            {
                ConsoleLog($"Could not read xml from {projectFileName}");
                return;
            }

            var refItemGroup = rootNode.SelectSingleNode("//def:ItemGroup[def:Reference]", namespaceManager);

            foreach (var dependency in project.InternalDependencies)
            {
                var refNode =
                    rootNode.SelectNodes("//def:Reference", namespaceManager)
                        ?.Cast<XmlElement>()
                        .FirstOrDefault(
                            e =>
                            e.HasAttribute("Include")
                            && Regex.IsMatch(e.Attributes["Include"].Value, $"^{Regex.Escape(dependency)}((, )|$)"))
                    ?? rootNode.SelectNodes("//def:ProjectReference", namespaceManager)
                        ?.Cast<XmlElement>()
                        .FirstOrDefault(
                            e =>
                            e.HasAttribute("Include")
                            && Regex.IsMatch(e.Attributes["Include"].Value, $"(^|[\\\\\\/]){Regex.Escape(dependency)}.csproj$"));

                if (refNode == null)
                {
                    var projectReference = rootNode.SelectNodes("//def:ProjectReference", namespaceManager)?.Cast<XmlElement>().Where(e => e.HasAttribute("Include"));
                    if (projectReference != null)
                    {
                        foreach (var element in projectReference)
                        {
                            ConsoleLog($"Comparing {element.Attributes["Include"].Value} and {dependency}");
                        }
                    }
                }

                if (refNode != null)
                {
                    var libPath = Path.Combine(Path.GetFullPath(BuildClean), dependency, $"{dependency}.dll");
                    refNode.ParentNode?.RemoveChild(refNode);
                    refNode = projDoc.CreateElement("Reference", "http://schemas.microsoft.com/developer/msbuild/2003");
                    refItemGroup?.AppendChild(refNode);
                    refNode.SetAttribute("Include", dependency);
                    refNode.InnerXml = $"<SpecificVersion>False</SpecificVersion><HintPath>{libPath}</HintPath><Private>True</Private>";
                    ConsoleLog($"ref linked to {dependency} was updated");
                    if (!File.Exists(libPath))
                    {
                        ConsoleLog($"!!!!!{libPath} does not exist!!!!");
                    }
                }
                else
                {
                    ConsoleLog($"{dependency} ref node was not found");
                }
            }

            // todo: @kantora update NuGet package references in case of directory structure change
            ConsoleLog($"Writing modified {projectFileName}");
            projDoc.Save(projectFileName);

            var assemblyInfoPath = Path.Combine(
                tempSrc,
                "Properties",
                "AssemblyInfo.cs");

            var assemblyText = File.ReadAllText(assemblyInfoPath);
            assemblyText += $"\n[assembly: AssemblyMetadata(\"NugetVersion\", \"{Version?.Replace("\"", "\\\"")}\")]\n";
            assemblyText += $"\n[assembly: AssemblyMetadata(\"BuildDate\", \"{DateTimeOffset.Now.ToString("s")}\")]\n";

            var assemblyVersion = Regex.Replace(Version ?? "1.0.0.0", "((\\d\\.?)+)(.*)", "$1");

            assemblyText = Regex.Replace(assemblyText, "AssemblyVersion\\(\"([^\\)]+)\"\\)", $"AssemblyVersion(\"{assemblyVersion}\")");
            assemblyText = Regex.Replace(assemblyText, "AssemblyFileVersion\\(\"([^\\)]+)\"\\)", $"AssemblyFileVersion(\"{assemblyVersion}\")");
            File.WriteAllText(assemblyInfoPath, assemblyText);

            MSBuildHelper.MSBuildRelease(project.TempBuildDirectory, "Clean", new[] { projectFileName });
            MSBuildHelper.MSBuildRelease(project.TempBuildDirectory, "Build", new[] { projectFileName });

            Directory.CreateDirectory(project.CleanBuildDirectory);

            var extensions = new[] { "dll", "nuspec", "pdb", "xml", "exe", "dll.config", "exe.config", "fsx" };

            var buildFiles = Directory.GetFiles(project.TempBuildDirectory)
                .Where(f => extensions.Any(e => $"{project.ProjectName}.{e}".Equals(Path.GetFileName(f), StringComparison.InvariantCultureIgnoreCase)))
                .ToList();

            foreach (var fileName in buildFiles.Select(Path.GetFileName).Where(fileName => fileName != null))
            {
                ConsoleLog($"Copying {fileName}");
                File.Copy(
                    Path.Combine(project.TempBuildDirectory, fileName),
                    Path.Combine(project.CleanBuildDirectory, fileName));
            }

            Func<string, bool> alwaysTrue = f => true;

            foreach (var directory in Directory.GetDirectories(project.TempBuildDirectory).Select(Path.GetFileName))
            {
                ConsoleLog($"Copying localization {directory}");
                FileHelper.CopyDir(
                    Path.Combine(project.CleanBuildDirectory, directory),
                    Path.Combine(project.TempBuildDirectory, directory),
                    alwaysTrue.ToFSharpFunc());
            }

            if (buildFiles.Any(f => f.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase) || f.EndsWith(".fsx", StringComparison.InvariantCultureIgnoreCase)))
            {
                var toolsDir = Path.Combine(project.CleanBuildDirectory, "tools");
                Directory.CreateDirectory(toolsDir);

                FileHelper.CopyDir(toolsDir, project.TempBuildDirectory, alwaysTrue.ToFSharpFunc());
            }

            TraceHelper.trace($"Build {project.ProjectName} finished");
        }

        /// <summary>
        /// Builds all projects
        /// </summary>
        /// <param name="projects">The list of projects to build</param>
        public static void Build(IEnumerable<ProjectDescription> projects)
        {
            var list = projects.ToList();

            foreach (var project in list)
            {
                Build(project);
            }
        }

        /// <summary>
        /// Configures current build execution
        /// </summary>
        /// <param name="version">Global output packages version</param>
        /// <param name="buildDirectory">Temp directory to build projects</param>
        /// <param name="packageOutputDirectory">Output directory to put prepared nuget packages</param>
        /// <param name="packageInputDirectory">directory with sources nuget packages</param>
        public static void Configure(
            string version,
            string buildDirectory,
            string packageOutputDirectory,
            string packageInputDirectory)
        {
            BuildUtils.Version = version;
            BuildUtils.BuildDirectory = buildDirectory;
            BuildUtils.PackageOutputDirectory = packageOutputDirectory;
            BuildUtils.PackageInputDirectory = packageInputDirectory;

            BuildTemp = Path.Combine(buildDirectory, "tmp");
            BuildClean = Path.Combine(buildDirectory, "clean");
        }

        /// <summary>
        /// Creates global solution file that includes all registered projects
        /// </summary>
        /// <param name="projects">The list of projects</param>
        public static void CreateGlobalSolution(IEnumerable<ProjectDescription> projects)
        {
            using (var writer = File.CreateText(Path.Combine(BuildDirectory, "global.sln")))
            {
                writer.Write($@"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio 14
VisualStudioVersion = 14.0.24720.0
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{{2150E333-8FDC-42A3-9474-1A3956D46DE8}}"") = ""_Solution Items"", ""_Solution Items"", ""{{{Guid.NewGuid().ToString().ToUpper()}}}""
	ProjectSection(SolutionItems) = preProject
");

                foreach (var filePath in Directory.GetFiles(Path.Combine(BuildDirectory, "..")))
                {
                    var file = Path.GetFileName(filePath);
                    writer.WriteLine($"\t\t..\\{file} = ..\\{file}");
                }

                writer.Write(@"	EndProjectSection
EndProject
                ");

                var folders = new List<string>();

                foreach (var package in projects.GroupBy(p => p.PackageName))
                {
                    var packageUid = Guid.NewGuid();
                    writer.Write(
                        $@"
Project(""{{2150E333-8FDC-42A3-9474-1A3956D46DE8}}"") = ""{package.Key}"", ""{package.Key}"", ""{{{packageUid}}}""
EndProject
");
                    foreach (var project in package)
                    {
                        try
                        {
                            var uid = GetProjectUid(project);
                            if (string.IsNullOrEmpty(uid))
                            {
                                throw new Exception("Could not parse project uid");
                            }


                            writer.Write(
                                $@"
Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{project.ProjectName}"", ""../{project.ProjectFileName}"", ""{uid}""
EndProject
");
                            folders.Add($"\t\t{uid} = {{{packageUid}}}\n");
                        }
                        catch (Exception e)
                        {
                            ConsoleLog(
                                $"Could not add {project.ProjectName} to global solution, {e.Message} \n {e.StackTrace}");
                        }
                    }
                }
                writer.Write($@"
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
	GlobalSection(NestedProjects) = preSolution
");
                foreach (var folder in folders)
                {
                    writer.Write(folder);
                }
                writer.Write($@"
	EndGlobalSection
EndGlobal
");
            }

            File.Copy(Path.Combine(BuildDirectory, "..", "global.sln.DotSettings"), Path.Combine(BuildDirectory, "global.sln.DotSettings"), true);
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

            if ("$author$".Equals(metaData.SelectSingleNode("authors").InnerText))
            {
                metaData.SelectSingleNode("authors").InnerText = "ClusterKit Team";
            }

            if ("$author$".Equals(metaData.SelectSingleNode("owners").InnerText))
            {
                metaData.SelectSingleNode("owners").InnerText = "ClusterKit Team";
            }

            if ("$description$".Equals(metaData.SelectSingleNode("description").InnerText))
            {
                metaData.SelectSingleNode("description").InnerText = "ClusterKit lib";
            }

            var dependenciesRootElement = metaData.AppendChild(nuspecData.CreateElement("dependencies"));
            var dependenciesDoc = new XmlDocument();
            dependenciesDoc.Load(Path.Combine(project.ProjectDirectory, "packages.config"));

            foreach (XmlElement dependency in dependenciesDoc.DocumentElement.SelectNodes("/packages/package"))
            {
                if (project.InternalDependencies.Contains(dependency.Attributes["id"].Value))
                {
                    continue;
                }

                if (!Directory.Exists(Path.Combine(PackageInputDirectory, $"{dependency.Attributes["id"].Value}.{dependency.Attributes["version"].Value}")))
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

            foreach (var directory in Directory.GetDirectories(project.CleanBuildDirectory).Select(Path.GetFileName).Where(d => d != "tools"))
            {
                foreach (var file in Directory.GetFiles(Path.Combine(project.CleanBuildDirectory, directory)))
                {
                    var fileElement = filesRootElement.AppendChild(nuspecData.CreateElement("file"));
                    fileElement.Attributes.Append(nuspecData.CreateAttribute("src")).Value = Path.Combine(directory, Path.GetFileName(file));
                    fileElement.Attributes.Append(nuspecData.CreateAttribute("target")).Value = $"./lib/{directory}/{Path.GetFileName(file)}";
                }
            }

            var toolsDir = Path.Combine(project.CleanBuildDirectory, "tools");
            if (Directory.Exists(toolsDir))
            {
                foreach (var file in Directory.GetFiles(toolsDir))
                {
                    var fileElement = filesRootElement.AppendChild(nuspecData.CreateElement("file"));
                    fileElement.Attributes.Append(nuspecData.CreateAttribute("src")).Value = Path.Combine("tools", Path.GetFileName(file));
                    fileElement.Attributes.Append(nuspecData.CreateAttribute("target")).Value = $"./tools/{Path.GetFileName(file)}";
                }
            }

            var generatedNuspecFile = Path.Combine(project.CleanBuildDirectory, nuspecDataFileName);
            nuspecData.Save(generatedNuspecFile);

            if (!Directory.Exists(PackageOutputDirectory))
            {
                Directory.CreateDirectory(PackageOutputDirectory);
            }

            Func<NuGetHelper.NuGetParams, NuGetHelper.NuGetParams> provider = defaults =>
                {
                    defaults.SetFieldValue("Version", Version);
                    defaults.SetFieldValue("WorkingDir", project.CleanBuildDirectory);
                    defaults.SetFieldValue("OutputPath", PackageOutputDirectory);
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
        /// Defines the project list
        /// </summary>
        /// <param name="projects">The project list</param>
        public static void DefineProjects(IEnumerable<ProjectDescription> projects)
        {
            definedProjectDescriptions = projects.ToList();
        }

        /// <summary>
        /// Gets the list of defined projects
        /// </summary>
        /// <returns>The list of defined projects</returns>
        public static IEnumerable<ProjectDescription> GetProjects()
        {
            return definedProjectDescriptions;
        }

        /// <summary>
        /// Removes local projects package references
        /// </summary>
        /// <param name="projects">List of local projects</param>
        public static void ReloadNuget(IEnumerable<ProjectDescription> projects)
        {
            var projectList = projects.ToList();

            if (!Directory.Exists(PackageInputDirectory))
            {
                return;
            }

            foreach (var installedPackageDir in projectList.Select(projectDescription => Path.Combine(
                PackageInputDirectory,
                $"{projectDescription.ProjectName}.{Version}")).Where(Directory.Exists))
            {
                Directory.Delete(installedPackageDir, true);
            }

            foreach (var projectDescription in projectList)
            {
                RestoreProjectDependencies(projectDescription);
            }
        }

        /// <summary>
        /// Runs defined unit tests on all projects
        /// </summary>
        /// <param name="project">The project to test</param>
        public static void RunXUnitTest(ProjectDescription project)
        {
            if (!project.ProjectType.HasFlag(ProjectDescription.EnProjectType.XUnitTests))
            {
                return;
            }

            var testAssembly = Path.Combine(project.TempBuildDirectory, $"{project.ProjectName}.dll");

            var runnerLocation = Directory.GetDirectories(Path.Combine(Directory.GetCurrentDirectory(), "packages"))
                .OrderByDescending(d => d)
                .First();

            Func<XUnit2.XUnit2Params, XUnit2.XUnit2Params> testParameters = p =>
            {
                p.SetFieldValue("ToolPath", Path.Combine(runnerLocation, "tools", "xunit.console.exe"));
                p.SetFieldValue("ErrorLevel", UnitTestCommon.TestRunnerErrorLevel.DontFailBuild);
                p.SetFieldValue("TimeOut", TimeSpan.FromHours(8));
                p.SetFieldValue("ForceTeamCity", true);
                return p;
            };

            XUnit2.xUnit2(testParameters.ToFSharpFunc(), new[] { testAssembly });
        }

        /// <summary>
        /// Runs defined unit tests on all projects
        /// </summary>
        /// <param name="projects">The list of projects to test</param>
        public static void RunXUnitTest(IEnumerable<ProjectDescription> projects)
        {
            /*
            var testAssemblies =
                projects.Where(p => p.ProjectType.HasFlag(ProjectDescription.EnProjectType.XUnitTests))
                    .Select(p => Path.Combine(p.TempBuildDirectory, $"{p.ProjectName}.dll"));

            var runnerLocation = Directory.GetDirectories(Path.Combine(Directory.GetCurrentDirectory(), "packages"))
                .OrderByDescending(d => d)
                .First();

            Func<XUnit2.XUnit2Params, XUnit2.XUnit2Params> testParameters = p =>
            {
                p.SetFieldValue("ToolPath", Path.Combine(runnerLocation, "tools", "xunit.console.exe"));
                p.SetFieldValue("ErrorLevel", UnitTestCommon.TestRunnerErrorLevel.DontFailBuild);
                p.SetFieldValue("TimeOut", TimeSpan.FromHours(8));
                p.SetFieldValue("ForceTeamCity", true);
                return p;
            };

            XUnit2.xUnit2(testParameters.ToFSharpFunc(), testAssemblies);
            */

            foreach (var project in projects)
            {
                RunXUnitTest(project);
            }
        }

        /// <summary>
        /// Updates current projects (csproj) to reference projects in other folders as Nuget Package
        /// </summary>
        /// <param name="projects">The list of projects</param>
        public static void SwitchToPackageRefs(IEnumerable<ProjectDescription> projects)
        {
            var projectList = projects.ToList();

            foreach (var project in projectList)
            {
                var projDoc = new XmlDocument();
                projDoc.Load(project.ProjectFileName);

                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(projDoc.NameTable);
                namespaceManager.AddNamespace("def", "http://schemas.microsoft.com/developer/msbuild/2003");

                var refItemGroup = projDoc.DocumentElement.SelectSingleNode(
                    "//def:ItemGroup[def:Reference]",
                    namespaceManager);

                foreach (var dependency in project.InternalDependencies)
                {
                    var refProject = projectList.FirstOrDefault(p => p.ProjectName == dependency);
                    if (refProject == null)
                    {
                        ConsoleLog($"{project.ProjectName} {dependency} ref project was not registered");
                        continue;
                    }

                    if (refProject.PackageName == project.PackageName)
                    {
                        continue;
                    }

                    //ConsoleLog($"{project.ProjectName} {dependency} updating...");

                    var refNode =
                        projDoc.DocumentElement.SelectNodes("//def:Reference", namespaceManager)
                            .Cast<XmlElement>()
                            .FirstOrDefault(
                                e =>
                                e.HasAttribute("Include")
                                && Regex.IsMatch(e.Attributes["Include"].Value, $"^{Regex.Escape(dependency)}((, )|$)"))
                        ?? projDoc.DocumentElement.SelectNodes("//def:ProjectReference", namespaceManager)
                               .Cast<XmlElement>()
                               .FirstOrDefault(
                                   e =>
                                   e.HasAttribute("Include")
                                   && Regex.IsMatch(
                                       e.Attributes["Include"].Value,
                                       $"(^|[\\\\\\/]){Regex.Escape(dependency)}.csproj$"));

                    if (refNode != null)
                    {
                        refNode.ParentNode.RemoveChild(refNode);
                        refNode = projDoc.CreateElement(
                                                    "Reference",
                                                    "http://schemas.microsoft.com/developer/msbuild/2003");
                        refItemGroup.AppendChild(refNode);
                        refNode.SetAttribute("Include", $"{refProject.ProjectName}, Version=0.0.0, Culture=neutral, processorArchitecture=MSIL");
                        refNode.InnerXml = $"\n     <SpecificVersion>False</SpecificVersion>\r\n      <HintPath>..\\..\\packages\\{refProject.ProjectName}.0.0.0-local\\lib\\{refProject.ProjectName}.dll</HintPath>\r\n      <Private>True</Private>\r\n";

                        //ConsoleLog($"{project.ProjectName} {dependency} {refNode.Name}");
                    }
                    else
                    {
                        ConsoleLog($"{project.ProjectName} {dependency} ref node was not found");
                    }
                }

                projDoc.Save(project.ProjectFileName);
                NugetAddLinksToLocalPackages(project, projectList);
            }
        }

        /// <summary>
        /// Adds links to internal dependencies from packages.config
        /// </summary>
        /// <param name="project">The project description</param>
        /// <param name="projects">The list of all projects</param>
        private static void NugetAddLinksToLocalPackages(ProjectDescription project, List<ProjectDescription> projects)
        {
            var packagesFileName = Path.Combine(project.ProjectDirectory, "packages.config");
            if (File.Exists(packagesFileName))
            {
                var packagesDoc = new XmlDocument();
                packagesDoc.Load(packagesFileName);

                var documentNode = packagesDoc.DocumentElement;
                foreach (var dependency in project.InternalDependencies.Where(d => documentNode.SelectNodes($"./package[@id=\"{d}\"]").Count == 0))
                {
                    var dependencyProject = projects.FirstOrDefault(p => p.ProjectName == dependency);
                    if (dependencyProject == null)
                    {
                        ConsoleLog($"{project.ProjectName} linked dependency {dependency} is not registered");
                        continue;
                    }

                    if (dependencyProject.PackageName == project.PackageName)
                    {
                        continue;
                    }

                    ConsoleLog($"{project.ProjectName} adding nuget ref to {dependency}");
                    var packageElement = packagesDoc.CreateElement("package");
                    packageElement.SetAttribute("id", dependency);
                    packageElement.SetAttribute("version", "0.0.0-local");
                    packageElement.SetAttribute("targetFramework", "net45");
                    documentNode.AppendChild(packageElement);
                }

                packagesDoc.Save(packagesFileName);
                
            }
        }

        /// <summary>
        /// Adds links to internal dependencies from packages.config
        /// </summary>
        /// <param name="project">The project description</param>
        /// <param name="projects">The list of all projects</param>
        private static void NugetRemoveLinksToLocalPackages(ProjectDescription project, List<ProjectDescription> projects)
        {
            var packagesFileName = Path.Combine(project.ProjectDirectory, "packages.config");
            if (File.Exists(packagesFileName))
            {
                var packagesDoc = new XmlDocument();
                packagesDoc.Load(packagesFileName);

                var documentNode = packagesDoc.DocumentElement;
                foreach (var dependency in project.InternalDependencies.Where(d => documentNode.SelectNodes($"./package[@id=\"{d}\"]").Count != 0))
                {
                    var dependencyProject = projects.FirstOrDefault(p => p.ProjectName == dependency);
                    if (dependencyProject == null)
                    {
                        ConsoleLog($"{project.ProjectName} linked dependency {dependency} is not registered");
                        continue;
                    }

                    ConsoleLog($"{project.ProjectName} removing nuget ref to {dependency}");
                    var packageElement = documentNode.SelectSingleNode($"./package[@id=\"{dependency}\"]");
                    if (packageElement == null)
                    {
                        ConsoleLog($"{project.ProjectName} linked dependency {dependency} is not found in package.json from second read O_O");
                        continue;
                    }

                    documentNode.RemoveChild(packageElement);
                }

                packagesDoc.Save(packagesFileName);

            }
        }

        /// <summary>
        /// Updates current projects (csproj) to reference projects in other folders as projects
        /// </summary>
        /// <remarks>
        /// Usefull while working in global solution
        /// </remarks>
        /// <param name="projects">The list of projects</param>
        public static void SwitchToProjectRefs(IEnumerable<ProjectDescription> projects)
        {
            var projectList = projects.ToList();
            foreach (var project in projectList)
            {
                var projDoc = new XmlDocument();
                projDoc.Load(project.ProjectFileName);

                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(projDoc.NameTable);
                namespaceManager.AddNamespace("def", "http://schemas.microsoft.com/developer/msbuild/2003");

                var refItemGroup = projDoc.DocumentElement.SelectSingleNode(
                    "//def:ItemGroup[def:Reference]",
                    namespaceManager);

                foreach (var dependency in project.InternalDependencies)
                {
                    var refProject = projectList.FirstOrDefault(p => p.ProjectName == dependency);
                    if (refProject == null)
                    {
                        ConsoleLog($"{project.ProjectName} {dependency} ref project was not registered");
                        continue;
                    }

                    if (refProject.PackageName == project.PackageName)
                    {
                        continue;
                    }

                    //ConsoleLog($"{project.ProjectName} {dependency} updating...");

                    var refNode =
                        projDoc.DocumentElement.SelectNodes("//def:Reference", namespaceManager)
                            .Cast<XmlElement>()
                            .FirstOrDefault(
                                e =>
                                e.HasAttribute("Include")
                                && Regex.IsMatch(e.Attributes["Include"].Value, $"^{Regex.Escape(dependency)}((, )|$)"))
                        ?? projDoc.DocumentElement.SelectNodes("//def:ProjectReference", namespaceManager)
                               .Cast<XmlElement>()
                               .FirstOrDefault(
                                   e =>
                                   e.HasAttribute("Include")
                                   && Regex.IsMatch(
                                       e.Attributes["Include"].Value,
                                       $"(^|[\\\\\\/]){Regex.Escape(dependency)}.csproj$"));

                    if (refNode != null)
                    {
                        refNode.ParentNode.RemoveChild(refNode);
                        refNode = projDoc.CreateElement(
                                                    "ProjectReference",
                                                    "http://schemas.microsoft.com/developer/msbuild/2003");
                        refItemGroup.AppendChild(refNode);
                        refNode.SetAttribute("Include", "../../" + refProject.ProjectFileName);
                        refNode.InnerXml = $"\n      <Project>{GetProjectUid(refProject)}</Project>\r\n      <Name>{refProject.ProjectName}</Name>\r\n";

                        //ConsoleLog($"{project.ProjectName} {dependency} {refNode.Name}");
                    }
                    else
                    {
                        ConsoleLog($"{project.ProjectName} {dependency} ref node was not found");
                    }
                }

                projDoc.Save(project.ProjectFileName);
                NugetRemoveLinksToLocalPackages(project, projectList);
            }
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
        /// Extracts project uid from project file
        /// </summary>
        /// <param name="project">The project description</param>
        /// <returns>The project's uid</returns>
        private static string GetProjectUid(ProjectDescription project)
        {
            var doc = new XmlDocument();
            doc.Load(project.ProjectFileName);
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("def", "http://schemas.microsoft.com/developer/msbuild/2003");
            var uid =
                doc.DocumentElement?.SelectSingleNode("/def:Project/def:PropertyGroup/def:ProjectGuid", namespaceManager)?
                    .InnerText;
            return uid;
        }

        /// <summary>
        /// Executes nuget restore for project
        /// </summary>
        /// <param name="project">Project to restore</param>
        private static void RestoreProjectDependencies(ProjectDescription project)
        {
            // restoring packages for project with respec to global configuration file
            Func<Unit, Unit> failFunc = u => u;
            var nugetPath = RestorePackageHelper.findNuget("./");
            RestorePackageHelper.runNuGet(
                nugetPath,
                TimeSpan.FromMinutes(10),
                $"restore {project.ProjectFileName} -ConfigFile ./nuget.config",
                failFunc.ToFSharpFunc());
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