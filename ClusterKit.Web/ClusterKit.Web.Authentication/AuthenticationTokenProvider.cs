// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthenticationTokenProvider.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Creates authentication tokens
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Authentication
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Akka.Actor;

    using ClusterKit.Core.Utils;
    using ClusterKit.Security.Client;

    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Infrastructure;

    using Newtonsoft.Json;

    using StackExchange.Redis;

    /// <summary>
    /// Creates authentication tokens
    /// </summary>
    public class AuthenticationTokenProvider : Microsoft.Owin.Security.Infrastructure.AuthenticationTokenProvider
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
        /// Initializes a new instance of the <see cref="AuthenticationTokenProvider"/> class.
        /// </summary>
        /// <param name="system">
        /// The system.
        /// </param>
        public AuthenticationTokenProvider(ActorSystem system)
        {
            this.system = system;
            this.redisConnectionString = system.Settings.Config.GetString("ClusterKit.Web.Authentication.RedisConnection");
            this.redisDb = system.Settings.Config.GetInt("ClusterKit.Web.Authentication.RedisDb");
            this.tokenKeyPrefix = system.Settings.Config.GetString("ClusterKit.Web.Authentication.TokenKeyPrefix");
        }

        /// <inheritdoc />
        public override async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            var tokenUid = Guid.NewGuid();
            var tokenValue = tokenUid.ToString("N");
            
            var userSession = context.OwinContext.Get<UserSession>(AuthorizationServerProvider.OwinContextUserSessionKey);

            var data = userSession.SerializeToAkkaString(this.system);
            using (var connection = await ConnectionMultiplexer.ConnectAsync(this.redisConnectionString))
            {
                var db = connection.GetDatabase(this.redisDb);
                
                var result = await db.StringSetAsync(
                    $"{this.tokenKeyPrefix}{tokenValue}",
                    data,
                    userSession.Expiring.HasValue ? (TimeSpan?)(userSession.Expiring.Value - DateTimeOffset.Now) : null);

                if (result)
                {
                    context.SetToken(tokenValue);
                }
                else
                {
                    var exception = new Exception("Session data server is unavailable");
                    this.system.Log.Error(exception, "{Type}: Session data server is unavailable", this.GetType().Name);
                    throw exception;
                }
            }
        }

        /// <inheritdoc />
        public override async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            var tokenValue = context.Token;
            using (var connection = await ConnectionMultiplexer.ConnectAsync(this.redisConnectionString))
            {
                var db = connection.GetDatabase(this.redisDb);
                var data = await db.StringGetAsync($"{this.tokenKeyPrefix}{tokenValue}");
                if (data.HasValue)
                {
                    var session = data.ToString().DeserializeFromAkkaString<UserSession>(this.system);
                    context.SetTicket(new AuthenticationTicket(new ClaimsIdentity(session.User.UserId), new AuthenticationProperties { ExpiresUtc = session.Expiring, IssuedUtc = session.Created }));
                }
            }
        }
    }
}
