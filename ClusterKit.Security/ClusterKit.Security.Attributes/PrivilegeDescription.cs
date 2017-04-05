// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrivilegeDescription.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The description of the privilege
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Security.Attributes
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
        /// <param name="action">
        /// the exact controller action name
        /// </param>
        /// <param name="target">
        /// The target to grant privilege to
        /// </param>
        public PrivilegeDescription(
            [NotNull]string assemblyName, 
            [CanBeNull]string description, 
            [NotNull]string privilege,
            [CanBeNull]string action = null,
            EnPrivilegeTarget target = EnPrivilegeTarget.ClientAndUser)
        {
            this.AssemblyName = assemblyName;
            this.Description = description ?? privilege;
            this.Privilege = privilege;
            this.Action = action;
            this.Target = target;
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

        /// <summary>
        /// Gets the target to grant privilege to
        /// </summary>
        [UsedImplicitly]
        public EnPrivilegeTarget Target { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.AssemblyName}: {this.Privilege}";
        }
    }
}