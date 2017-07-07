// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnReleaseState.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The list of release states
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.ORM
{
    using KlusterKite.API.Attributes;

    /// <summary>
    /// The list of release states
    /// </summary>
    [ApiDescription("The list of release states", Name = "EnReleaseState")]
    public enum EnReleaseState
    {
        /// <summary>
        /// This is the draft of release and can be edited
        /// </summary>
        [ApiDescription("This is the draft of release and can be edited")]
        Draft,

        /// <summary>
        /// This is the new release, ready to be applied
        /// </summary>
        [ApiDescription("This is the new release, ready to be applied")]
        Ready,

        /// <summary>
        /// This is the current active release
        /// </summary>
        [ApiDescription("This is the current active release")]
        Active,

        /// <summary>
        /// This release was faulted and cluster roll-backed to the latest stable release (or some other)
        /// </summary>
        [ApiDescription("This release was faulted and cluster rollbacked to the latest stable release (or some other)")]
        Faulted,

        /// <summary>
        /// This release is obsolete and was replaced by some new one
        /// </summary>
        [ApiDescription("This release is obsolete and was replaced by some new one")]
        Obsolete
    }
}