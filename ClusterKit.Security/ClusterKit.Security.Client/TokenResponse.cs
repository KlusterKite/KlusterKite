// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TokenResponse.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The token response description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Security.Client
{
    using JetBrains.Annotations;

    using Newtonsoft.Json;

    /// <summary> 
    /// The token description
    /// </summary>
    [UsedImplicitly]
    public class TokenResponse
    {
        /// <summary>
        /// Gets or sets the access token value
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

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

        /// <summary>
        /// Gets or sets the refresh token value
        /// </summary>
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the access scope
        /// </summary>
        [JsonProperty("scope")]
        public string Scope { get; set; }

        /// <summary>
        /// Gets or sets the access scope
        /// </summary>
        [JsonProperty("state")]
        public string State { get; set; }
    }
}
