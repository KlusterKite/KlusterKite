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
    using System.ComponentModel;
    using System.Configuration;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Akka.Configuration;

    using Autofac;

    using ClusterKit.Core;
    using ClusterKit.Core.TestKit;
    using ClusterKit.Security.Attributes;
    using ClusterKit.Web.Authorization;
    using ClusterKit.Web.Authorization.Attributes;

    using RestSharp;
    using RestSharp.Authenticators;

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
        /// The type of test authentication
        /// </summary>
        public enum EnAuthenticationType
        {
            /// <summary>
            /// There is no authentication
            /// </summary>
            None,

            /// <summary>
            /// There is user authentication
            /// </summary>
            User,

            /// <summary>
            /// There is client authentication
            /// </summary>
            Client
        }

        /// <summary>
        /// Current owin bind port
        /// </summary>
        private int OwinPort => this.Sys.Settings.Config.GetInt("ClusterKit.Web.OwinPort");

        /// <summary>
        /// Checks various authorization combinations
        /// </summary>
        /// <param name="authenticationTypeString">The authentication type</param>
        /// <param name="method">The requested method</param>
        /// <param name="expectedResult">The expected response code</param>
        /// <returns>The async task</returns>
        [Theory]
        [InlineData("None", "session", (int)HttpStatusCode.Unauthorized)]
        [InlineData("User", "session", (int)HttpStatusCode.OK)]
        [InlineData("Client", "session", (int)HttpStatusCode.OK)]
        [InlineData("None", "user", (int)HttpStatusCode.Unauthorized)]
        [InlineData("User", "user", (int)HttpStatusCode.OK)]
        [InlineData("Client", "user", (int)HttpStatusCode.Unauthorized)]
        [InlineData("User", "AuthorizedUserAction", (int)HttpStatusCode.NoContent)]
        [InlineData("Client", "AuthorizedUserAction", (int)HttpStatusCode.Unauthorized)]
        [InlineData("User", "UnauthorizedUserAction", (int)HttpStatusCode.Unauthorized)]
        [InlineData("Client", "UnauthorizedUserAction", (int)HttpStatusCode.Unauthorized)]
        [InlineData("User", "UnauthorizedClientUserAction", (int)HttpStatusCode.Unauthorized)]
        [InlineData("Client", "UnauthorizedClientUserAction", (int)HttpStatusCode.Unauthorized)]
        [InlineData("User", "AuthorizedEitherClientUserAction", (int)HttpStatusCode.NoContent)]
        [InlineData("Client", "AuthorizedEitherClientUserAction", (int)HttpStatusCode.NoContent)]
        [InlineData("User", "authorizedUserExactAction", (int)HttpStatusCode.NoContent)]
        [InlineData("User", "unauthorizedUserExactAction", (int)HttpStatusCode.Unauthorized)]
        public async Task CheckAuthorization(string authenticationTypeString, string method, HttpStatusCode expectedResult)
        {
            EnAuthenticationType authenticationType =
                (EnAuthenticationType)Enum.Parse(typeof(EnAuthenticationType), authenticationTypeString);

            this.ExpectNoMsg();

            var client = new RestClient($"http://localhost:{this.OwinPort}/test") { Timeout = 5000 };
            switch (authenticationType)
            {
                case EnAuthenticationType.User:
                    client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(await this.SetUserSession(), "Bearer");
                    break;
                case EnAuthenticationType.Client:
                    client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(await this.SetClientSession(), "Bearer");
                    break;
            }

            var request = new RestRequest { Method = Method.GET, Resource = method };
            request.AddHeader("Accept", "application/json, text/json");
            var result = await client.ExecuteTaskAsync(request);
            this.Sys.Log.Info("Result: {Result}", result.Content);
            Assert.Equal(expectedResult, result.StatusCode);
        }

        /// <summary>
        /// In case of invalid token, controller should return 401
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task ForbiddenOnInvalidToken()
        {
            this.ExpectNoMsg();
            var client = new RestClient($"http://localhost:{this.OwinPort}/public") { Timeout = 5000 };
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(
                Guid.NewGuid().ToString("N"),
                "Bearer");

            var request = new RestRequest { Method = Method.GET, Resource = "test" };
            request.AddHeader("Accept", "application/json, text/json");
            var result = await client.ExecuteTaskAsync(request);

            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        /// <summary>
        /// In case of invalid token, controller should return 401
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task ValidOnNoToken()
        {
            this.ExpectNoMsg();
            var client = new RestClient($"http://localhost:{this.OwinPort}/public") { Timeout = 5000 };
           
            var request = new RestRequest { Method = Method.GET, Resource = "test" };
            request.AddHeader("Accept", "application/json, text/json");
            var result = await client.ExecuteTaskAsync(request);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        /// <summary>
        /// Sets the user authenticated session
        /// </summary>
        /// <returns>The user token</returns>
        private async Task<string> SetUserSession()
        {
            var session = new AccessTicket(
                new User { UserId = "testUser" },
                new[] { "User1", "User.AuthorizedUserExactAction" },
                "testClient",
                "testClientType",
                new[] { "Client1", "Client2", "Client.AuthorizedUserExactAction" },
                DateTimeOffset.Now,
                DateTimeOffset.Now.AddMinutes(1),
                "hello world");

            return await this.Container.Resolve<ITokenManager>().CreateAccessToken(session);
        }

        /// <summary>
        /// Sets the client authenticated session
        /// </summary>
        /// <returns>The user token</returns>
        private async Task<string> SetClientSession()
        {
            var session = new AccessTicket(
                null,
                null,
                "testClient-2",
                "testClientType",
                new[] { "Client1-2", "Client2-2" },
                DateTimeOffset.Now,
                DateTimeOffset.Now.AddMinutes(1),
                "hello world");

            return await this.Container.Resolve<ITokenManager>().CreateAccessToken(session);
        }

        /// <summary>
        /// The test configurator
        /// </summary>
        public class Configurator : TestConfigurator
        {
            /// <inheritdoc />
            public override bool RunPostStart => true;

            /// <inheritdoc />
            public override Config GetAkkaConfig(ContainerBuilder containerBuilder)
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
").WithFallback(base.GetAkkaConfig(containerBuilder));
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
        [RequireSession]
        public class TestController : ApiController
        {
            /// <summary>
            /// Tests user authentication
            /// </summary>
            /// <returns>The user name</returns>
            [HttpGet]
            [Route("session")]
            public string GetUserSession()
            {
                var session = this.GetSession();
                return session.User?.UserId ?? session.ClientId;
            }

            /// <summary>
            /// Tests user authentication
            /// </summary>
            /// <returns>The user name</returns>
            [HttpGet]
            [RequireUser]
            [Route("user")]
            public string GetUser()
            {
                var session = this.GetSession();
                if (session.User == null)
                {
                    throw new ArgumentNullException(nameof(AccessTicket.User));
                }

                return session.User.UserId;
            }

            /// <summary>
            /// User action authorized for default user on default client
            /// </summary>
            [HttpGet]
            [Route("AuthorizedUserAction")]
            [RequireUserPrivilege("User1")]
            [RequireClientPrivilege("Client1")]
            public void AuthorizedUserAction()
            {
            }

            /// <summary>
            /// User action unauthorized for default user, but authorized for default client
            /// </summary>
            [HttpGet]
            [Route("UnauthorizedUserAction")]
            [RequireUserPrivilege("User2")]
            [RequireClientPrivilege("Client1")]
            public void UnauthorizedUserAction()
            {
            }

            /// <summary>
            /// User action authorized for default user, but unauthorized for default client
            /// </summary>
            [HttpGet]
            [Route("UnauthorizedClientUserAction")]
            [RequireUserPrivilege("User1")]
            [RequireClientPrivilege("Client1-2")]
            public void UnauthorizedClientUserAction()
            {
            }

            /// <summary>
            /// User action authorized for either client or user
            /// </summary>
            [HttpGet]
            [Route("AuthorizedEitherClientUserAction")]
            [RequireUserPrivilege("User1", IgnoreOnClientOwnBehalf = true)]
            [RequireClientPrivilege("Client1-2", IgnoreOnUserPresent = true)]
            public void AuthorizedEitherClientUserAction()
            {
            }

            /// <summary>
            /// User action authorized with action name
            /// </summary>
            [HttpGet]
            [Route("authorizedUserExactAction")]
            [RequireUserPrivilege("User", CombinePrivilegeWithActionName = true)]
            [RequireClientPrivilege("Client", CombinePrivilegeWithActionName = true)]
            public void AuthorizedUserExactAction()
            {
            }

            /// <summary>
            /// User action authorized with action name
            /// </summary>
            [HttpGet]
            [Route("unauthorizedUserExactAction")]
            [RequireUserPrivilege("User1", CombinePrivilegeWithActionName = true)]
            [RequireClientPrivilege("Client1", CombinePrivilegeWithActionName = true)]
            public void UnauthorizedUserExactAction()
            {
            }
        }

        /// <summary>
        /// The testing web api controller
        /// </summary>
        [RoutePrefix("public")]
        public class PublicTestController : ApiController
        {
            /// <summary>
            /// Tests user authentication
            /// </summary>
            /// <returns>The user name</returns>
            [HttpGet]
            [Route("test")]
            public string GetUserSession()
            {
                return "Hello world";
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
            protected override void RegisterComponents(ContainerBuilder container)
            {
                container.RegisterType<TestController>();
                container.RegisterType<PublicTestController>();
                container.RegisterType<MoqTokenManager>().As<ITokenManager>().SingleInstance();
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