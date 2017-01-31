// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthorizationTests.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing authorization process
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ClusterKit.Web.Tests.Auth
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Akka.Configuration;

    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ClusterKit.Core;
    using ClusterKit.Core.TestKit;
    using ClusterKit.Core.Utils;
    using ClusterKit.Security.Client;
    using ClusterKit.Web.Authorization;

    using Newtonsoft.Json;

    using RestSharp;
    using RestSharp.Authenticators;

    using StackExchange.Redis;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Testing authorization process
    /// </summary>
    public class AuthorizationTests : BaseActorTest<AuthorizationTests.Configurator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public AuthorizationTests(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Current owin bind port
        /// </summary>
        private int OwinPort => this.Sys.Settings.Config.GetInt("ClusterKit.Web.OwinPort");

        /// <summary>
        /// Checks user authorization
        /// </summary>
        /// <returns>Async task</returns>
        [Fact]
        public async Task CheckUserSession()
        {
            this.ExpectNoMsg();

            var client = new RestClient($"http://localhost:{this.OwinPort}") { Timeout = 5000 };
            var token = await this.SetUserSession();
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(token, "Bearer");
            var request = new RestRequest { Method = Method.GET, Resource = "/test/user" };
            request.AddHeader("Accept", "application/json, text/json");
            var result = await client.ExecuteTaskAsync(request);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Content);
            Assert.Equal("testUser", JsonConvert.DeserializeObject<string>(result.Content));
        }

        /// <summary>
        /// Checks unauthorized user
        /// </summary>
        /// <returns>Async task</returns>
        [Fact]
        public async Task CheckUnauthorizedUser()
        {
            this.ExpectNoMsg();
            var client = new RestClient($"http://localhost:{this.OwinPort}") { Timeout = 5000 };
            var request = new RestRequest { Method = Method.GET, Resource = "/test/user" };
            request.AddHeader("Accept", "application/json, text/json");
            var result = await client.ExecuteTaskAsync(request);

            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        /// <summary>
        /// Sets the user authenticated session
        /// </summary>
        /// <returns>The user token</returns>
        private async Task<string> SetUserSession()
        {
            var session = new UserSession(
                new User { UserId = "testUser" },
                new[] { "User1" },
                "testClient",
                "testClientType",
                new[] { "Client1", "Client2" },
                DateTimeOffset.Now,
                DateTimeOffset.Now.AddMinutes(1),
                "hello world");

            var redisConnectionString =
                this.Sys.Settings.Config.GetString("ClusterKit.Web.Authentication.RedisConnection");
            var redisDb = this.Sys.Settings.Config.GetInt("ClusterKit.Web.Authentication.RedisDb");
            var tokenKeyPrefix = this.Sys.Settings.Config.GetString("ClusterKit.Web.Authentication.TokenKeyPrefix");

            var token = Guid.NewGuid().ToString("N");

            using (var connection = await ConnectionMultiplexer.ConnectAsync(redisConnectionString))
            {
                var db = connection.GetDatabase(redisDb);
                await db.StringSetAsync(
                    $"{tokenKeyPrefix}{token}",
                    session.SerializeToAkkaString(this.Sys),
                    TimeSpan.FromMinutes(1));
            }

            return token;
        }

        /// <summary>
        /// The test configurator
        /// </summary>
        public class Configurator : TestConfigurator
        {
            /// <inheritdoc />
            public override bool RunPostStart => true;

            /// <summary>
            /// Gets the akka system config
            /// </summary>
            /// <param name="windsorContainer">
            /// The windsor Container.
            /// </param>
            /// <returns>
            /// The config
            /// </returns>
            public override Config GetAkkaConfig(IWindsorContainer windsorContainer)
            {
                var listener = new TcpListener(IPAddress.Any, 0);
                listener.Start();
                var port = ((IPEndPoint)listener.LocalEndpoint).Port;
                listener.Stop();

                return ConfigurationFactory.ParseString($@"
                {{
                    ClusterKit {{
 		                Web {{
                            Authentication.RedisConnection = ""{ConfigurationManager.ConnectionStrings["redis"]
                    .ConnectionString}""
                            OwinPort = {port},
 			                OwinBindAddress = ""http://*:{port}"",
                            Debug.Trace = true
                        }}
                    }}
                }}
").WithFallback(base.GetAkkaConfig(windsorContainer));
            }

            /// <summary>
            /// Gets list of all used plugin installers
            /// </summary>
            /// <returns>The list of installers</returns>
            public override List<BaseInstaller> GetPluginInstallers()
            {
                var installers = base.GetPluginInstallers();
                installers.Add(new Descriptor.Installer());
                installers.Add(new Web.Installer());
                installers.Add(new Web.Authorization.Installer());
                installers.Add(new TestInstaller());
                return installers;
            }
        }

        /// <summary>
        /// The testing web api controller
        /// </summary>
        [RoutePrefix("test")]
        public class TestController : ApiController
        {
            [HttpGet]
            [Route("user")]
            public string GetUserSession()
            {
                var session = this.GetSession();
                if (session == null)
                {
                    throw new HttpResponseException(HttpStatusCode.Unauthorized);
                }

                return session.User?.UserId ?? session.ClientId;
            }
        }

        /// <summary>
        /// The test installer to register components
        /// </summary> 
        public class TestInstaller : BaseInstaller
        {
            /// <inheritdoc />
            protected override decimal AkkaConfigLoadPriority => PriorityClusterRole;

            /// <inheritdoc />
            protected override Config GetAkkaConfig()
            {
                return Config.Empty;
            }

            /// <inheritdoc />
            protected override void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store)
            {
                container.Register(Classes.From(typeof(TestController)));
            }
        }

        /// <summary>
        /// The test user
        /// </summary>
        private class User : IUser
        {
            /// <inheritdoc />
            public string UserId { get; set; }
        }
    }
}