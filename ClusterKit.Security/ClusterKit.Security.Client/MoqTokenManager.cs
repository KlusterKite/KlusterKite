// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MoqTokenManager.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Token manager for test purposes
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Security.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    /// <summary>
    /// Token manager for test purposes
    /// </summary>
    public class MoqTokenManager : ITokenManager
    {
        /// <summary>
        /// The list of stored tokens
        /// </summary>
        private readonly Dictionary<string, UserSession> storedTokens = new Dictionary<string, UserSession>();

        /// <summary>
        /// Gets or sets virtual current time to test token invalidation
        /// </summary>
        [UsedImplicitly]
        public DateTimeOffset? CurrentTime { get; set; }

        /// <inheritdoc />
        public Task<string> CreateAccessToken(UserSession session)
        {
            var token = Guid.NewGuid().ToString("N");
            this.storedTokens[token] = session;
            return Task.FromResult(token);
        }

        /// <inheritdoc />
        public Task<UserSession> ValidateAccessToken(string token)
        {
            UserSession session;
            if (!this.storedTokens.TryGetValue(token, out session))
            {
                return Task.FromResult<UserSession>(null);
            }

            if (session.Expiring.HasValue && session.Expiring.Value < (this.CurrentTime ?? DateTimeOffset.Now))
            {
                return Task.FromResult<UserSession>(null);
            }

            return Task.FromResult(session);
        }
    }
}
