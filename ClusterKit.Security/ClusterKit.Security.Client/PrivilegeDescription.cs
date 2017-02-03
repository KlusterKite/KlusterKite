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
        /// <param name="action">the exact controller action name</param>
        public PrivilegeDescription(
            [NotNull]string assemblyName, 
            [CanBeNull]string description, 
            [NotNull]string privilege,
            [CanBeNull]string action = null)
        {
            this.AssemblyName = assemblyName;
            this.Description = description ?? privilege;
            this.Privilege = privilege;
            this.Action = action;
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
        /// Gets the exact controller action name
        /// </summary>
        [CanBeNull]
        [UsedImplicitly]
        public string Action { get; }

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