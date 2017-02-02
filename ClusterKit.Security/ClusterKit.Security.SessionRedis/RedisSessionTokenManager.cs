// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RedisSessionTokenManager.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Redis based token manager
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Security.SessionRedis
{
    using System;
    using System.Threading.Tasks;

    using Akka.Actor;

    using ClusterKit.Core.Utils;
    using ClusterKit.Security.Client;

    using JetBrains.Annotations;

    using StackExchange.Redis;

    /// <summary>
    /// Redis based token manager
    /// </summary>
    [UsedImplicitly]
    public class RedisSessionTokenManager : ITokenManager
    {
        /// <summary>
        /// The actor system
        /// </summary>
        private readonly ActorSystem system;

        /// <summary>
        /// The redis connection string
        /// </summary>
        private readonly string redisConnectionString;

        /// <summary>
        /// The redis database number
        /// </summary>
        private readonly int redisDb;

        /// <summary>
        /// The token key prefix to store in redis
        /// </summary>
        private readonly string tokenKeyPrefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisSessionTokenManager"/> class.
        /// </summary>
        /// <param name="system">
        /// The system.
        /// </param>
        public RedisSessionTokenManager(ActorSystem system)
        {
            this.system = system;
            this.redisConnectionString = system.Settings.Config.GetString("ClusterKit.Security.SessionRedis.RedisConnection");
            this.redisDb = system.Settings.Config.GetInt("ClusterKit.Security.SessionRedis.RedisDb");
            this.tokenKeyPrefix = system.Settings.Config.GetString("ClusterKit.Security.SessionRedis.TokenKeyPrefix");
        }

        /// <inheritdoc />
        public async Task<string> CreateAccessToken(UserSession session)
        {
            var tokenUid = Guid.NewGuid();
            var tokenValue = tokenUid.ToString("N");

            var data = session.SerializeToAkkaString(this.system);
            using (var connection = await ConnectionMultiplexer.ConnectAsync(this.redisConnectionString))
            {
                var db = connection.GetDatabase(this.redisDb);

                var result = await db.StringSetAsync(
                    $"{this.tokenKeyPrefix}{tokenValue}",
                    data,
                    session.Expiring.HasValue ? (TimeSpan?)(session.Expiring.Value - DateTimeOffset.Now) : null);

                if (result)
                {
                    return tokenValue;
                }

                var exception = new Exception("Session data server is unavailable");
                this.system.Log.Error(exception, "{Type}: Session data server is unavailable", this.GetType().Name);
                throw exception;
            }
        }

        /// <inheritdoc />
        public async Task<UserSession> ValidateAccessToken(string token)
        {
            using (var connection = await ConnectionMultiplexer.ConnectAsync(this.redisConnectionString))
            {
                var db = connection.GetDatabase(this.redisDb);
                var data = await db.StringGetAsync($"{this.tokenKeyPrefix}{token}");
                return data.HasValue ? data.ToString().DeserializeFromAkkaString<UserSession>(this.system) : null;
            }
        }
    }
}
