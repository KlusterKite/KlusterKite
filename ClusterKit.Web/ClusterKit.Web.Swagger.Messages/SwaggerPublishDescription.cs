// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SwaggerPublishDescription.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Description of published swagger site
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Swagger.Messages
{
    /// <summary>
    /// Description of published swagger site
    /// </summary>
    public struct SwaggerPublishDescription
    {
        /// <summary>
        /// Gets or sets path to the local swagger description
        /// </summary>
        public string DocUrl { get; set; }

        /// <summary>
        /// Gets or sets path to the local swagger ui
        /// </summary>
        public string Url { get; set; }
    }
}