// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DependOnAttribute.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Marks that current <see cref="IMigrator" /> depends on some other migrator.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Migrator
{
    using System;

    /// <summary>
    /// Marks that current <see cref="IMigrator"/> depends on some other migrator.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DependOnAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependOnAttribute"/> class.
        /// </summary>
        /// <param name="dependency">
        /// The dependency.
        /// </param>
        public DependOnAttribute(Type dependency)
        {
            this.Dependency = dependency;
        }

        /// <summary>
        /// Gets the target <see cref="IMigrator"/> that current migrator depends on
        /// </summary>
        public Type Dependency { get; }
    }
}
