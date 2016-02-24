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
        /// <param name="internalDependencies">
        ///     The internal dependencies.
        /// </param>
        public ProjectDescription(string projectFileName, string[] internalDependencies = null)
        {
            this.InternalDependencies = internalDependencies ?? new string[0];
            this.ProjectFileName = projectFileName;

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
        /// Gets directory name for clean build
        /// </summary>
        public string CleanBuildDirectory => Path.Combine(BuildUtils.BuildClean, this.ProjectName);

        /// <summary>
        /// Gets list of internal (global across all modules bundle) dependencies
        /// </summary>
        public string[] InternalDependencies { get; }

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
        /// Gets directory name for temporary build
        /// </summary>
        public string TempBuildDirectory => Path.Combine(BuildUtils.BuildClean, this.ProjectName);
    }
}