// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GrantTokenResponse.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The response for the successful authentication token grant
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Web.Authentication
{
    using JetBrains.Annotations;

    using Newtonsoft.Json;

    /// <summary>
    /// The response for the successful authentication token grant
    /// </summary>
    [UsedImplicitly]
    public class GrantTokenResponse
    {
        /// <summary>
        /// Gets or sets the access token value
        /// </summary>
        [UsedImplicitly]
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the token type
        /// </summary>
        [UsedImplicitly]
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds before access token will expire
        /// </summary>
        [UsedImplicitly]
        [JsonProperty("expires_in")]
        public int? ExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets the refresh token value
        /// </summary>
        [UsedImplicitly]
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the provided scope
        /// </summary>
        [UsedImplicitly]
        [JsonProperty("scope")]
        public string Scope { get; set; }
    }
}
