// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrivilegeDescription.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The description of the privilege
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Security.Client
{
    using JetBrains.Annotations;

    /// <summary>
    /// The description of the privilege
    /// </summary>
    public class PrivilegeDescription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrivilegeDescription"/> class.
        /// </summary>
        /// <param name="assemblyName">
        /// The assembly name.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        /// <param name="privilege">
        /// The privilege.
        /// </param>
        public PrivilegeDescription([NotNull]string assemblyName, [CanBeNull]string description, [NotNull]string privilege)
        {
            this.AssemblyName = assemblyName;
            this.Description = description ?? privilege;
            this.Privilege = privilege;
        }

        /// <summary>
        /// Gets the container assembly name
        /// </summary>
        [NotNull]
        [UsedImplicitly]
        public string AssemblyName { get; }

        /// <summary>
        /// Gets the privilege description
        /// </summary>
        [NotNull]
        [UsedImplicitly]
        public string Description { get; }

        /// <summary>
        /// Gets the privilege
        /// </summary>
        [NotNull]
        [UsedImplicitly]
        public string Privilege { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.AssemblyName}: {this.Privilege}";
        }
    }
}