// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetAssemblyReferences.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Reads the <see cref="Assembly" /> references
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.AppDomainHelper
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Reads the <see cref="Assembly"/> references
    /// </summary>
    [Serializable]
    public class GetAssemblyReferences
    {
        /// <summary>
        /// Gets or sets the file name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the list of referenced assemblies
        /// </summary>
        public AssemblyName[] Result { get; set; }

        /// <summary>
        /// Loads the assembly and gets it's references
        /// </summary>
        public void Execute()
        {
            var assembly = Assembly.ReflectionOnlyLoadFrom(this.FileName);
            this.Result = assembly.GetReferencedAssemblies();
        }
    }
}