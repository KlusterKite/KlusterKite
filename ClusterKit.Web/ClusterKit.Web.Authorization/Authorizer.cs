// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Authorizer.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Checks the access token for validness and extracts the user session data
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ClusterKit.Web.Authorization
{
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Akka.Actor;

    using ClusterKit.Core.Utils;
    using ClusterKit.Security.Client;

    using JetBrains.Annotations;

    using Microsoft.Owin;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Infrastructure;

    using StackExchange.Redis;

    /// <summary>
    /// Checks the access token for validness and extracts the user session data
    /// </summary>
    [UsedImplicitly]
    public class Authorizer : AuthenticationMiddleware<Authorizer.AuthorizerOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Authorizer"/> class.
        /// </summary>
        /// <param name="next">
        /// The next.
        /// </param>
        /// <param name="options">
        /// The options.
        /// </param>
        public Authorizer(OwinMiddleware next, AuthorizerOptions options)
            : base(next, options)
        {
        }

        /// <inheritdoc />
        protected override AuthenticationHandler<AuthorizerOptions> CreateHandler()
        {
            return new AuthorizerHandler();
        }

        /// <summary>
        /// The authentication handler
        /// </summary>
        public class AuthorizerHandler : AuthenticationHandler<AuthorizerOptions>
        {
            /// <inheritdoc />
            protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
            {
                var authorizationHeader = this.Context.Request.Headers["Authorization"];
                if (string.IsNullOrWhiteSpace(authorizationHeader))
                {
                    return null;
                }

                var authParts = authorizationHeader.Split(' ');
                if (authParts.Length != 2 || authParts[0].ToLower() != "bearer"
                    || string.IsNullOrWhiteSpace(authParts[1]))
                {
                    return null;
                }

                var token = authParts[1];

                using (var connection = await ConnectionMultiplexer.ConnectAsync(this.Options.RedisConnectionString))
                {
                    var db = connection.GetDatabase(this.Options.RedisDb);
                    var key = $"{this.Options.TokenKeyPrefix}{token}";
                    var data = await db.StringGetAsync(key);
                    if (!data.HasValue)
                    {
                        return null;
                    }

                    var session = data.ToString().DeserializeFromAkkaString<UserSession>(this.Options.System);
                    this.Context.Set("UserSession", session);

                    return new AuthenticationTicket(
                        new ClaimsIdentity(session.User?.UserId ?? session.ClientId),
                        new AuthenticationProperties { IssuedUtc = session.Created, ExpiresUtc = session.Expiring });
                }
            }
        }

        /// <summary>
        /// The list of authentication options
        /// </summary>
        public class AuthorizerOptions : AuthenticationOptions
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AuthorizerOptions"/> class.
            /// </summary>
            /// <param name="authenticationType">
            /// The authentication type.
            /// </param>
            /// <param name="system">
            /// The system.
            /// </param>
            public AuthorizerOptions(string authenticationType, ActorSystem system)
                : base(authenticationType)
            {
                this.System = system;
                this.RedisConnectionString =
                    system.Settings.Config.GetString("ClusterKit.Web.Authentication.RedisConnection");
                this.RedisDb = system.Settings.Config.GetInt("ClusterKit.Web.Authentication.RedisDb");
                this.TokenKeyPrefix = system.Settings.Config.GetString("ClusterKit.Web.Authentication.TokenKeyPrefix");
            }

            /// <summary>
            /// Gets the redis connection string
            /// </summary>
            public string RedisConnectionString { get; }

            /// <summary>
            /// Gets the token database number
            /// </summary>
            public int RedisDb { get; }

            /// <summary>
            /// Gets the actor system
            /// </summary>
            public ActorSystem System { get; }

            /// <summary>
            /// Gets the token key prefix
            /// </summary>
            public string TokenKeyPrefix { get; }
        }
    }
}