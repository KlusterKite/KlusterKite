// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectDescription.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Short description of project to build
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Build
{
    using System;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Short description of project to build
    /// </summary>
    public class ProjectDescription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectDescription"/> class.
        /// </summary>
        /// <param name="projectFileName">
        ///     The project file name.
        /// </param>
        /// <param name="projectType">
        ///     The project type or types
        /// </param>
        /// <param name="internalDependencies">
        ///     The internal dependencies.
        /// </param>
        public ProjectDescription(string projectFileName, EnProjectType projectType, string[] internalDependencies = null)
            : this(null, projectFileName, projectType, internalDependencies)
        {
            if (!string.IsNullOrEmpty(projectFileName))
            {
                this.PackageName = string.Join(
                    ".",
                    projectFileName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Take(2));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectDescription"/> class.
        /// </summary>
        /// <param name="packageName">
        ///     The package name.
        /// </param>
        /// <param name="projectFileName">
        ///     The project file name.
        /// </param>
        /// <param name="projectType">
        ///     The project type or types
        /// </param>
        /// <param name="internalDependencies">
        ///     The internal dependencies.
        /// </param>
        public ProjectDescription(
            string packageName,
            string projectFileName,
            EnProjectType projectType,
            string[] internalDependencies = null)
        {
            this.PackageName = packageName;
            this.InternalDependencies = internalDependencies ?? new string[0];
            this.ProjectFileName = projectFileName;
            this.ProjectType = projectType;

            if (projectFileName == null)
            {
                throw new ArgumentNullException(nameof(projectFileName));
            }

            this.ProjectDirectory = Path.GetDirectoryName(Path.GetFullPath(projectFileName));

            if (this.ProjectDirectory == null)
            {
                throw new ArgumentNullException(nameof(projectFileName));
            }

            this.ProjectName = Path.GetFileNameWithoutExtension(projectFileName);
        }

        /// <summary>
        /// Types of projects
        /// </summary>
        [Flags]
        public enum EnProjectType
        {
            /// <summary>
            /// Project has no specific type
            /// </summary>
            None = 0,

            /// <summary>
            /// Project can be packed in nuget package
            /// </summary>
            NugetPackage = 1,

            /// <summary>
            /// Project contains xunit tests
            /// </summary>
            XUnitTests = 2,

            /// <summary>
            /// Project just need to be built and nothing else
            /// </summary>
            SimpleBuild = 3
        }

        /// <summary>
        /// Gets directory name for clean build
        /// </summary>
        public string CleanBuildDirectory => Path.Combine(BuildUtils.BuildClean, this.ProjectName);

        /// <summary>
        /// Gets list of internal (global across all modules bundle) dependencies
        /// </summary>
        public string[] InternalDependencies { get; }

        /// <summary>
        /// Gets or sets the name of projects bucket (all projects with the same package name are usually in one local solution
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// Gets current project absolute directory path
        /// </summary>
        public string ProjectDirectory { get; }

        /// <summary>
        /// Gets path to project file
        /// </summary>
        public string ProjectFileName { get; }

        /// <summary>
        /// Gets current projects name
        /// </summary>
        public string ProjectName { get; }

        /// <summary>
        /// Gets current project types
        /// </summary>
        public EnProjectType ProjectType { get; }

        /// <summary>
        /// Gets directory name for temporary build
        /// </summary>
        public string TempBuildDirectory => Path.Combine(BuildUtils.BuildTemp, this.ProjectName);
    }
}