// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Token.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The token response description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Launcher
{
    using System;

    using JetBrains.Annotations;

    using Newtonsoft.Json;

    /// <summary> 
    /// The token description
    /// </summary>
    [UsedImplicitly]
    public class Token
    {
        /// <summary>
        /// The token creation date
        /// </summary>
        private readonly DateTimeOffset created = DateTimeOffset.Now;

        /// <summary>
        /// Gets or sets the access token value
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the token expiration in seconds
        /// </summary>
        [JsonProperty("expires_in")]
        [UsedImplicitly]
        public int? Expires { get; set; }

        /// <summary>
        /// Gets a value indicating whether current token was expired
        /// </summary>
        public bool IsExpired
            => !this.Expires.HasValue || (DateTimeOffset.Now - this.created).TotalSeconds > this.Expires.Value;
    }
}