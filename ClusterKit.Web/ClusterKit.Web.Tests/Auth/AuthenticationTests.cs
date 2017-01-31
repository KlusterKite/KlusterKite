// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthenticationTests.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing authentication process
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

    using JetBrains.Annotations;

    using Newtonsoft.Json;

    using RestSharp;

    using StackExchange.Redis;

    using Xunit;
    using Xunit.Abstractions;

    using Installer = ClusterKit.Web.Installer;

    /// <summary>
    /// Testing authentication process
    /// </summary>
    public class AuthenticationTests : BaseActorTest<AuthenticationTests.Configurator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public AuthenticationTests(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Current owin bind port
        /// </summary>
        private int OwinPort => this.Sys.Settings.Config.GetInt("ClusterKit.Web.OwinPort");

        /// <summary>
        /// Basic authorization test
        /// </summary>
        /// <param name="clientId">
        /// The client Id.
        /// </param>
        /// <param name="clientSecret">
        /// The client Secret.
        /// </param>
        /// <param name="userName">
        /// The user Name.
        /// </param>
        /// <param name="userPassword">
        /// The user Password.
        /// </param>
        /// <param name="expectedResult">
        /// The expected Result.
        /// </param>
        /// <returns>
        /// Task to enable asynchronous execution
        /// </returns>
        [Theory]
        [InlineData("unknownClient", null, "testUser", "testPassword", HttpStatusCode.BadRequest)]
        [InlineData("unit-test-nosecret", null, "testUser", "testPassword", HttpStatusCode.OK)]
        [InlineData("unit-test-nosecret", "random", "testUser", "testPassword", HttpStatusCode.OK)]
        [InlineData("unit-test-nosecret", null, "testUser1", "testPassword", HttpStatusCode.BadRequest)]
        [InlineData("unit-test-nosecret", null, "testUser", "testPassword1", HttpStatusCode.BadRequest)]
        [InlineData("unit-test-secret", null, "testUser", "testPassword", HttpStatusCode.BadRequest)]
        [InlineData("unit-test-secret", "random", "testUser", "testPassword", HttpStatusCode.BadRequest)]
        [InlineData("unit-test-secret", "test-secret", "testUser", "testPassword", HttpStatusCode.OK)]
        [InlineData("unit-test-secret", "test-secret", "testUser1", "testPassword", HttpStatusCode.BadRequest)]
        [InlineData("unit-test-secret", "test-secret", "testUser", "testPassword1", HttpStatusCode.BadRequest)]
        public async Task GrantPasswordTest(
            string clientId,
            string clientSecret,
            string userName,
            string userPassword,
            HttpStatusCode expectedResult)
        {
            this.ExpectNoMsg();
            this.Log.Info("Owin port is {OwinPort}", this.OwinPort);

            var client = new RestClient($"http://localhost:{this.OwinPort}") { Timeout = 5000 };

            var request = new RestRequest { Method = Method.POST, Resource = "/api/1.x/security/token" };
            request.AddParameter("grant_type", "password");
            request.AddParameter("username", userName);
            request.AddParameter("password", userPassword);
            request.AddParameter("client_id", clientId);
            if (!string.IsNullOrWhiteSpace(clientSecret))
            {
                request.AddParameter("client_secret", clientSecret);
            }

            var result = await client.ExecuteTaskAsync(request);
            Assert.Equal(expectedResult, result.StatusCode);

            if (expectedResult == HttpStatusCode.OK)
            {
                var tokenDescription = JsonConvert.DeserializeObject<TokenDescription>(result.Content);
                var redisConnectionString = this.Sys.Settings.Config.GetString("ClusterKit.Web.Authentication.RedisConnection");
                var redisDb = this.Sys.Settings.Config.GetInt("ClusterKit.Web.Authentication.RedisDb");
                var tokenKeyPrefix = this.Sys.Settings.Config.GetString("ClusterKit.Web.Authentication.TokenKeyPrefix");

                using (var connection = await ConnectionMultiplexer.ConnectAsync(redisConnectionString))
                {
                    var db = connection.GetDatabase(redisDb);
                    var data = await db.StringGetAsync($"{tokenKeyPrefix}{tokenDescription.Token}");
                    Assert.True(data.HasValue);
                    var session = data.ToString().DeserializeFromAkkaString<UserSession>(this.Sys);

                    Assert.Equal(clientId, session.ClientId);
                    Assert.Equal(typeof(User), session.User.GetType());
                    Assert.Equal(userName, session.User.UserId);
                }
            }
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
                            Authentication.RedisConnection = ""{ConfigurationManager.ConnectionStrings["redis"].ConnectionString}""
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
                installers.Add(new Installer());
                installers.Add(new Authentication.Installer());
                installers.Add(new TestInstaller());
                return installers;
            }
        }

        /// <summary>
        /// The test installer to register components
        /// </summary> 
        public class TestInstaller : BaseInstaller
        {
            /// <inheritdoc />
            protected override decimal AkkaConfigLoadPriority => BaseInstaller.PriorityClusterRole;

            /// <inheritdoc />
            protected override Config GetAkkaConfig()
            {
                return Config.Empty;
            }

            /// <inheritdoc />
            protected override void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store)
            {
                container.Register(Component.For<IClientProvider>().ImplementedBy<TestClientProvider>());
            }
        }

        /// <summary> 
        /// The test client
        /// </summary>
        private class TestClient : IClient
        {
            /// <inheritdoc />
            public string ClientId { get; set; }

            /// <inheritdoc />
            public string Name { get; set; }

            /// <inheritdoc />
            public IEnumerable<string> OwnScope { get; set; }

            /// <inheritdoc />
            public string Type => this.GetType().Name;

            /// <inheritdoc />
            public Task<UserSession> AuthenticateUserAsync(string userName, string password)
            {
                if (userName == "testUser" && password == "testPassword")
                {
                    var session = new UserSession(
                        new User { UserId = userName },
                        new[] { "User" },
                        this.ClientId,
                        this.Type,
                        this.OwnScope,
                        DateTimeOffset.Now, 
                        DateTimeOffset.Now.AddSeconds(60),
                        null);

                    return Task.FromResult(session);
                }

                return Task.FromResult<UserSession>(null);
            }
        }

        /// <summary>
        /// The test client provider
        /// </summary>
        [UsedImplicitly]
        private class TestClientProvider : IClientProvider
        {
            /// <inheritdoc />
            public decimal Priority => 10M;

            /// <inheritdoc />
            public Task<IClient> GetClientAsync(string clientId, string secret)
            {
                switch (clientId)
                {
                    case "unit-test-nosecret":
                        return
                            Task.FromResult<IClient>(
                                new TestClient
                                    {
                                        ClientId = clientId,
                                        Name = "Test - no secret",
                                        OwnScope = new[] { "Test1", "Test2" }
                                    });
                    case "unit-test-secret":
                        if (secret == "test-secret")
                        {
                            return
                                Task.FromResult<IClient>(
                                    new TestClient
                                        {
                                            ClientId = clientId,
                                            Name = "Test - secret",
                                            OwnScope = new[] { "Test1", "Test3" }
                                        });
                        }

                        break;
                }

                return Task.FromResult<IClient>(null);
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