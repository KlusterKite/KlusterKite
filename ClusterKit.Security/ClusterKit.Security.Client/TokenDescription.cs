// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TokenDescription.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The token description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Security.Client
{
    using Newtonsoft.Json;

    /// <summary> 
    /// The token description
    /// </summary>
    public class TokenDescription
    {
        /// <summary>
        /// Gets or sets the token value
        /// </summary>
        [JsonProperty("access_token")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the token type
        /// </summary>
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        /// <summary>
        /// Gets or sets the token expiration in seconds
        /// </summary>
        [JsonProperty("expires_in")]
        public int? Expires { get; set; }
    }
}
