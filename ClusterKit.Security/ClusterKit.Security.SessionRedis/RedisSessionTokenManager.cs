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
        public async Task<string> CreateAccessToken(AccessTicket session)
        {
            var tokenUid = Guid.NewGuid();
            var token = tokenUid.ToString("N");

            var data = session.SerializeToAkkaString(this.system);
            using (var connection = await ConnectionMultiplexer.ConnectAsync(this.redisConnectionString))
            {
                var db = connection.GetDatabase(this.redisDb);

                var result = await db.StringSetAsync(
                    this.GetRedisAccessKey(token),
                    data,
                    session.Expiring.HasValue ? (TimeSpan?)(session.Expiring.Value - DateTimeOffset.Now) : null);

                if (result)
                {
                    return token;
                }

                var exception = new Exception("Session data server is unavailable");
                this.system.Log.Error(exception, "{Type}: Session data server is unavailable", this.GetType().Name);
                throw exception;
            }
        }

        /// <inheritdoc />
        public async Task<AccessTicket> ValidateAccessToken(string token)
        {
            using (var connection = await ConnectionMultiplexer.ConnectAsync(this.redisConnectionString))
            {
                var db = connection.GetDatabase(this.redisDb);
                var data = await db.StringGetAsync(this.GetRedisAccessKey(token));
                return data.HasValue ? data.ToString().DeserializeFromAkkaString<AccessTicket>(this.system) : null;
            }
        }

        /// <inheritdoc />
        public async Task<bool> RevokeAccessToken(string token)
        {
            using (var connection = await ConnectionMultiplexer.ConnectAsync(this.redisConnectionString))
            {
                var db = connection.GetDatabase(this.redisDb);
                return await db.KeyDeleteAsync(this.GetRedisAccessKey(token));
            }
        }

        /// <inheritdoc />
        public async Task<string> CreateRefreshToken(RefreshTicket ticket)
        {
            var tokenUid = Guid.NewGuid();
            var token = tokenUid.ToString("N");

            var data = ticket.SerializeToAkkaString(this.system);
            using (var connection = await ConnectionMultiplexer.ConnectAsync(this.redisConnectionString))
            {
                var db = connection.GetDatabase(this.redisDb);

                var result = await db.StringSetAsync(
                    this.GetRedisRefreshKey(token),
                    data,
                    ticket.Expiring.HasValue ? (TimeSpan?)(ticket.Expiring.Value - DateTimeOffset.Now) : null);

                if (result)
                {
                    return token;
                }

                var exception = new Exception("Session data server is unavailable");
                this.system.Log.Error(exception, "{Type}: Session data server is unavailable", this.GetType().Name);
                throw exception;
            }
        }

        /// <inheritdoc />
        public async Task<RefreshTicket> ValidateRefreshToken(string token)
        {
            using (var connection = await ConnectionMultiplexer.ConnectAsync(this.redisConnectionString))
            {
                var db = connection.GetDatabase(this.redisDb);
                var redisRefreshKey = this.GetRedisRefreshKey(token);
                var data = await db.StringGetAsync(redisRefreshKey);
                await db.KeyDeleteAsync(redisRefreshKey);
                return data.HasValue ? data.ToString().DeserializeFromAkkaString<RefreshTicket>(this.system) : null;
            }
        }

        /// <inheritdoc />
        public async Task<bool> RevokeRefreshToken(string token)
        {
            using (var connection = await ConnectionMultiplexer.ConnectAsync(this.redisConnectionString))
            {
                var db = connection.GetDatabase(this.redisDb);
                return await db.KeyDeleteAsync(this.GetRedisRefreshKey(token));
            }
        }

        /// <summary>
        /// Creates the redis key from access token
        /// </summary>
        /// <param name="token">The token</param>
        /// <returns>The redis key</returns>
        private string GetRedisAccessKey(string token)
        {
            return $"{this.tokenKeyPrefix}Access:{token}";
        }

        /// <summary>
        /// Creates the redis key from refresh token
        /// </summary>
        /// <param name="token">The token</param>
        /// <returns>The redis key</returns>
        private string GetRedisRefreshKey(string token)
        {
            return $"{this.tokenKeyPrefix}Refresh:{token}";
        }
    }
}
