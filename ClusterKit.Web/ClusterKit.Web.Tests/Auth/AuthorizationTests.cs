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
    using ClusterKit.Security.Client;
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
        /// <param name="authenticationType">The authentication type</param>
        /// <param name="method">The requested method</param>
        /// <param name="expectedResult">The expected response code</param>
        /// <returns>The async task</returns>
        [Theory]
        [InlineData(EnAuthenticationType.None, "session", HttpStatusCode.Unauthorized)]
        [InlineData(EnAuthenticationType.User, "session", HttpStatusCode.OK)]
        [InlineData(EnAuthenticationType.Client, "session", HttpStatusCode.OK)]
        [InlineData(EnAuthenticationType.None, "user", HttpStatusCode.Unauthorized)]
        [InlineData(EnAuthenticationType.User, "user", HttpStatusCode.OK)]
        [InlineData(EnAuthenticationType.Client, "user", HttpStatusCode.Unauthorized)]
        [InlineData(EnAuthenticationType.User, "AuthorizedUserAction", HttpStatusCode.NoContent)]
        [InlineData(EnAuthenticationType.Client, "AuthorizedUserAction", HttpStatusCode.Unauthorized)]
        [InlineData(EnAuthenticationType.User, "UnauthorizedUserAction", HttpStatusCode.Unauthorized)]
        [InlineData(EnAuthenticationType.Client, "UnauthorizedUserAction", HttpStatusCode.Unauthorized)]
        [InlineData(EnAuthenticationType.User, "UnauthorizedClientUserAction", HttpStatusCode.Unauthorized)]
        [InlineData(EnAuthenticationType.Client, "UnauthorizedClientUserAction", HttpStatusCode.Unauthorized)]
        [InlineData(EnAuthenticationType.User, "AuthorizedEitherClientUserAction", HttpStatusCode.NoContent)]
        [InlineData(EnAuthenticationType.Client, "AuthorizedEitherClientUserAction", HttpStatusCode.NoContent)]
        [InlineData(EnAuthenticationType.User, "authorizedUserExactAction", HttpStatusCode.NoContent)]
        [InlineData(EnAuthenticationType.User, "unauthorizedUserExactAction", HttpStatusCode.Unauthorized)]
        public async Task CheckAuthorization(EnAuthenticationType authenticationType, string method, HttpStatusCode expectedResult) 
        {
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

            Assert.Equal(expectedResult, result.StatusCode);
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

            return await this.WindsorContainer.Resolve<ITokenManager>().CreateAccessToken(session);
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

            return await this.WindsorContainer.Resolve<ITokenManager>().CreateAccessToken(session);
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
                container.Register(Component.For<ITokenManager>().ImplementedBy<MoqTokenManager>().LifestyleSingleton());
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