// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnConfigurationState.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The list of configuration states
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.ORM
{
    using KlusterKite.API.Attributes;

    /// <summary>
    /// The list of configuration states
    /// </summary>
    [ApiDescription("The list of configuration states", Name = "EnConfigurationState")]
    public enum EnConfigurationState
    {
        /// <summary>
        /// This is the draft of configuration and can be edited
        /// </summary>
        [ApiDescription("This is the draft of configuration and can be edited")]
        Draft,

        /// <summary>
        /// This is the new configuration, ready to be applied
        /// </summary>
        [ApiDescription("This is the new configuration, ready to be applied")]
        Ready,

        /// <summary>
        /// This is the current active configuration
        /// </summary>
        [ApiDescription("This is the current active configuration")]
        Active,

        /// <summary>
        /// This configuration was faulted and cluster roll-backed to the latest stable configuration (or some other)
        /// </summary>
        [ApiDescription("This configuration was faulted and cluster rollbacked to the latest stable configuration (or some other)")]
        Faulted,

        /// <summary>
        /// This configuration is obsolete and was replaced by some new one
        /// </summary>
        [ApiDescription("This configuration is obsolete and was replaced by some new one")]
        Obsolete
    }
}