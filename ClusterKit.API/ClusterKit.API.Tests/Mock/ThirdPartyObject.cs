// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThirdPartyObject.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Mocking third party library class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Tests.Mock
{
    /// <summary>
    /// Mocking third party library class
    /// </summary>
    public class ThirdPartyObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThirdPartyObject"/> class.
        /// </summary>
        public ThirdPartyObject()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThirdPartyObject"/> class.
        /// </summary>
        /// <param name="contents">
        /// The contents.
        /// </param>
        public ThirdPartyObject(string contents)
        {
            this.Contents = contents;
        }

        /// <summary>
        /// Gets or sets content data
        /// </summary>
        public string Contents { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Contents;
        }
    }
}
