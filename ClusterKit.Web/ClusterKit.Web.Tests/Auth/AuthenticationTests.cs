﻿// --------------------------------------------------------------------------------------------------------------------
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

    using Akka.Configuration;

    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ClusterKit.Core;
    using ClusterKit.Core.TestKit;
    using ClusterKit.Security.Client;

    using JetBrains.Annotations;

    using Newtonsoft.Json;

    using RestSharp;

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
                var tokenDescription = JsonConvert.DeserializeObject<TokenResponse>(result.Content);
                var tokenManager = this.WindsorContainer.Resolve<ITokenManager>();
                var accessTicket = await tokenManager.ValidateAccessToken(tokenDescription.AccessToken);
                Assert.NotNull(accessTicket);
                Assert.NotNull(accessTicket.User);
                Assert.Equal(userName, accessTicket.User.UserId);

                var refreshTicket = await tokenManager.ValidateRefreshToken(tokenDescription.RefreshToken);
                Assert.NotNull(refreshTicket);
                Assert.NotNull(refreshTicket.UserId);
                Assert.Equal(userName, refreshTicket.UserId);
            }
        }

        /// <summary>
        /// Tests the refreshing access tokens process
        /// </summary>
        /// <param name="createTicket">A value indicating whether to create refresh tokens in the test</param>
        /// <param name="ticketUserId">The user id in the refresh ticket</param>
        /// <param name="ticketClientId">The client id in the ticket</param>
        /// <param name="clientId">The client id in the request</param>
        /// <param name="clientSecret">The client secret in the request</param>
        /// <param name="expectedResult">The expected response code</param>
        /// <returns>Async task</returns>
        [Theory]
        [InlineData(false, null, null, "unit-test-secret", "test-secret", HttpStatusCode.BadRequest)]
        [InlineData(false, null, null, "unit-test-nosecret", null, HttpStatusCode.BadRequest)]
        [InlineData(true, null, "unit-test-secret", "unit-test-secret", "test-secret", HttpStatusCode.BadRequest)]
        [InlineData(true, "testUser", "unit-test-secret", "unit-test-secret", "test-secret", HttpStatusCode.OK)]
        [InlineData(true, "testUser", "unit-test-secret", "unit-test-secret", "random", HttpStatusCode.BadRequest)]
        [InlineData(true, "testUser", "unit-test-nosecret", "unit-test-secret", "test-secret", HttpStatusCode.BadRequest)]
        [InlineData(true, null, "unit-test-secret", "unit-test-secret", "test-secret", HttpStatusCode.BadRequest)]
        [InlineData(true, "testUser", "unit-test-nosecret", "unit-test-nosecret", null, HttpStatusCode.OK)]
        [InlineData(true, "testUser", "unit-test-secret", "unit-test-nosecret", null, HttpStatusCode.BadRequest)]
        public async Task GrantRefreshToken(
            bool createTicket,
            string ticketUserId,
            string ticketClientId,
            string clientId,
            string clientSecret,
            HttpStatusCode expectedResult)
        {
            this.ExpectNoMsg();
            var tokenManager = this.WindsorContainer.Resolve<ITokenManager>();
            var token = createTicket
                            ? await tokenManager.CreateRefreshToken(
                                  new RefreshTicket(
                                      ticketUserId,
                                      ticketClientId,
                                      "TestClient",
                                      DateTimeOffset.Now,
                                      DateTimeOffset.Now.AddMinutes(1)))
                            : Guid.NewGuid().ToString("N");

            var client = new RestClient($"http://localhost:{this.OwinPort}") { Timeout = 5000 };

            var request = new RestRequest { Method = Method.POST, Resource = "/api/1.x/security/token" };
            request.AddParameter("grant_type", "refresh_token");
            request.AddParameter("refresh_token", token);
            request.AddParameter("client_id", clientId);
            if (!string.IsNullOrWhiteSpace(clientSecret))
            {
                request.AddParameter("client_secret", clientSecret);
            }

            var result = await client.ExecuteTaskAsync(request);
            Assert.Equal(expectedResult, result.StatusCode);
            if (expectedResult == HttpStatusCode.OK)
            {
                var tokenDescription = JsonConvert.DeserializeObject<TokenResponse>(result.Content);
                var accessTicket = await tokenManager.ValidateAccessToken(tokenDescription.AccessToken);
                Assert.NotNull(accessTicket);
                Assert.NotNull(accessTicket.User);
                Assert.Equal(ticketUserId, accessTicket.User.UserId);

                var refreshTicket = await tokenManager.ValidateRefreshToken(tokenDescription.RefreshToken);
                Assert.NotNull(refreshTicket);
                Assert.NotNull(refreshTicket.UserId);
                Assert.Equal(ticketUserId, refreshTicket.UserId);
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
            protected override decimal AkkaConfigLoadPriority => PriorityClusterRole;

            /// <inheritdoc />
            protected override Config GetAkkaConfig()
            {
                return Config.Empty;
            }

            /// <inheritdoc />
            protected override void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store)
            {
                container.Register(Component.For<IClientProvider>().ImplementedBy<TestClientProvider>());
                container.Register(Component.For<ITokenManager>().ImplementedBy<MoqTokenManager>().LifestyleSingleton());
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
            public Task<AuthenticationResult> AuthenticateSelf()
            {
                return Task.FromResult<AuthenticationResult>(null);
            }

            /// <inheritdoc />
            public Task<AuthenticationResult> AuthenticateUserAsync(string userName, string password)
            {
                if (userName == "testUser" && password == "testPassword")
                {
                    return Task.FromResult(this.CreateAuthenticationResult(userName));
                }

                return Task.FromResult<AuthenticationResult>(null);
            }

            /// <inheritdoc />
            public Task<AuthenticationResult> AuthenticateWithRefreshTicket(RefreshTicket refreshTicket)
            {
                return refreshTicket.UserId == "testUser"
                           ? Task.FromResult(this.CreateAuthenticationResult(refreshTicket.UserId))
                           : Task.FromResult<AuthenticationResult>(null);
            }

            /// <summary>
            /// Creates the authentication result
            /// </summary>
            /// <param name="userName">The user name</param>
            /// <returns>The authentication result</returns>
            private AuthenticationResult CreateAuthenticationResult(string userName)
            {
                var accessTicket = new AccessTicket(
                    new User { UserId = userName },
                    new[] { "User" },
                    this.ClientId,
                    this.Type,
                    this.OwnScope,
                    DateTimeOffset.Now,
                    DateTimeOffset.Now.AddSeconds(60),
                    null);
                var refreshTicket = new RefreshTicket(
                    userName,
                    this.ClientId,
                    this.Type,
                    DateTimeOffset.Now,
                    DateTimeOffset.Now.AddSeconds(60));

                var authenticationResult = new AuthenticationResult(accessTicket, refreshTicket);
                return authenticationResult;
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