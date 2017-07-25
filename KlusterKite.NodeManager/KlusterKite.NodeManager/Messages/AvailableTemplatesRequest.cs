// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AvailableTemplatesRequest.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Debug request - checks containers for the node
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Messages
{
    /// <summary>
    /// Debug request - checks containers for the node
    /// </summary>
    public class AvailableTemplatesRequest
    {
        /// <summary>
        /// Gets or sets the container type
        /// </summary>
        public string ContainerType { get; set; }

        /// <summary>
        /// Gets or sets the framework runtime type name
        /// </summary>
        public string FrameworkRuntimeType { get; set; }
    }
}