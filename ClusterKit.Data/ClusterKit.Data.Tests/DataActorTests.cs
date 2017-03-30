// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataActorTests.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing the <see cref="BaseCrudActor{TContext}" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Configuration;
    using Akka.DI.Core;

    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ClusterKit.API.Client;
    using ClusterKit.Core;
    using ClusterKit.Core.TestKit;
    using ClusterKit.Data.CRUD.ActionMessages;
    using ClusterKit.Data.EF;
    using ClusterKit.Data.Tests.Mock;

    using Newtonsoft.Json.Linq;

    using Xunit;
    using Xunit.Abstractions;

    using Component = Castle.MicroKernel.Registration.Component;

    /// <summary>
    /// Testing the <see cref="BaseCrudActor{TContext}"/>
    /// </summary>
    public class DataActorTests : BaseActorTest<DataActorTests.Configurator>
    {
        /// <inheritdoc />
        public DataActorTests(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Testing the get by id request
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task GetByIdTest()
        {
            var actor = await this.InitContext();

            var request = new CrudActionMessage<User, Guid>
                              {
                                  ActionType = EnActionType.Get,
                                  Id = Guid.Parse("{72C23018-0C49-4419-8982-D7C0168E8DC2}")
            };

            var result = await actor.Ask<CrudActionResponse<User>>(request, TimeSpan.FromSeconds(1));
            Assert.NotNull(result);
            if (result.Exception != null)
            {
                this.Sys.Log.Error(result.Exception, "Exception");
            }

            Assert.Null(result.Exception);
            Assert.NotNull(result.Data);
            Assert.Equal("user1", result.Data.Login);
        }

        /// <summary>
        /// Testing the create request
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task CreateTest()
        {
            var actor = await this.InitContext();

            var uid = Guid.Parse("{72C23018-0C49-4419-8982-D7C0168E8DC3}");
            var request = new CrudActionMessage<User, Guid>
            {
                ActionType = EnActionType.Create,
                Data = new User
                {
                    Login = "new_user",
                    Password = "456",
                    Uid = uid
                }
            };

            var result = await actor.Ask<CrudActionResponse<User>>(request, TimeSpan.FromSeconds(1));
            Assert.NotNull(result);
            if (result.Exception != null)
            {
                this.Sys.Log.Error(result.Exception, "Exception");
            }

            Assert.Null(result.Exception);
            Assert.NotNull(result.Data);
            Assert.Equal("new_user", result.Data.Login);

            var contextFactory = this.WindsorContainer.Resolve<IContextFactory<TestDataContext>>();
            using (var context = await contextFactory.CreateContext(null, null))
            {
                Assert.Equal(3, context.Users.Count());
            }
        }

        /// <summary>
        /// Testing update request
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task UpdateTest()
        {
            var actor = await this.InitContext();

            var uid = Guid.Parse("{72C23018-0C49-4419-8982-D7C0168E8DC2}");
            var request = new CrudActionMessage<User, Guid>
            {
                ActionType = EnActionType.Update,
                Id = uid,
                Data = new User
                           {
                               Login = "new_user",
                               Password = "456",
                               Uid = uid
                }
            };

            var result = await actor.Ask<CrudActionResponse<User>>(request, TimeSpan.FromSeconds(1));
            Assert.NotNull(result);
            if (result.Exception != null)
            {
                this.Sys.Log.Error(result.Exception, "Exception");
            }

            Assert.Null(result.Exception);
            Assert.NotNull(result.Data);
            Assert.Equal("new_user", result.Data.Login);
            Assert.Equal("456", result.Data.Password);

            var contextFactory = this.WindsorContainer.Resolve<IContextFactory<TestDataContext>>();
            using (var context = await contextFactory.CreateContext(null, null))
            {
                var user =
                    context.Users.FirstOrDefault(u => u.Uid == uid);
                Assert.NotNull(user);
                Assert.Equal("new_user", user.Login);
                Assert.Equal("456", user.Password);
            }
        }

        /// <summary>
        /// Testing update with api request
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task UpdateWithApiRequestTest()
        {
            var actor = await this.InitContext();

            var uid = Guid.Parse("{72C23018-0C49-4419-8982-D7C0168E8DC2}");
            var argumentsJson = @"
                {
                    ""id"": ""72C23018-0C49-4419-8982-D7C0168E8DC2"",
                    ""newNode"": {
                        ""login"": ""new_user""                     
                    }
                }
            ";
            var apiRequest = new ApiRequest { Arguments = JObject.Parse(argumentsJson) };

            var request = new CrudActionMessage<User, Guid>
            {
                ActionType = EnActionType.Update,
                Id = uid,
                Data = new User
                {
                    Login = "new_user",
                    Password = "456",
                    Uid = uid
                },
                ApiRequest = apiRequest
            };

            var result = await actor.Ask<CrudActionResponse<User>>(request, TimeSpan.FromSeconds(1));
            Assert.NotNull(result);
            if (result.Exception != null)
            {
                this.Sys.Log.Error(result.Exception, "Exception");
            }

            Assert.Null(result.Exception);
            Assert.NotNull(result.Data);
            Assert.Equal("new_user", result.Data.Login);
            Assert.Equal("123", result.Data.Password);

            var contextFactory = this.WindsorContainer.Resolve<IContextFactory<TestDataContext>>();
            using (var context = await contextFactory.CreateContext(null, null))
            {
                var user =
                    context.Users.FirstOrDefault(u => u.Uid == uid);
                Assert.NotNull(user);
                Assert.Equal("new_user", user.Login);
                Assert.Equal("123", user.Password);
            }
        }

        /// <summary>
        /// Testing the delete request
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task DeleteTest()
        {
            var actor = await this.InitContext();

            var uid = Guid.Parse("{72C23018-0C49-4419-8982-D7C0168E8DC2}");
            var request = new CrudActionMessage<User, Guid>
            {
                ActionType = EnActionType.Delete,
                Id = uid
            };

            var result = await actor.Ask<CrudActionResponse<User>>(request, TimeSpan.FromSeconds(1));
            Assert.NotNull(result);
            if (result.Exception != null)
            {
                this.Sys.Log.Error(result.Exception, "Exception");
            }

            Assert.Null(result.Exception);
            Assert.NotNull(result.Data);
            Assert.Equal("user1", result.Data.Login);

            var contextFactory = this.WindsorContainer.Resolve<IContextFactory<TestDataContext>>();
            using (var context = await contextFactory.CreateContext(null, null))
            {
                Assert.Equal(1, context.Users.Count());
            }
        }

        /// <summary>
        /// Performs context initialization
        /// </summary>
        /// <returns>The data actor</returns>
        private async Task<IActorRef> InitContext()
        {
            var contextFactory = this.WindsorContainer.Resolve<IContextFactory<TestDataContext>>();
            await contextFactory.CreateAndUpgradeContext(null, null);
            var actor = this.Sys.ActorOf(this.Sys.DI().Props<TestDataActor>(), "data");
            this.ExpectNoMsg();
            return actor;
        }

        /// <summary>
        /// Configures current test system
        /// </summary>
        public class Configurator : TestConfigurator
        {
            /// <summary>
            /// Gets list of all used plugin installers
            /// </summary>
            /// <returns>The list of installers</returns>
            public override List<BaseInstaller> GetPluginInstallers()
            {
                var pluginInstallers = new List<BaseInstaller>
                                           {
                                               new Core.Installer(),
                                               new Core.TestKit.Installer(),
                                               new TestInstaller()
                                           };
                return pluginInstallers;
            }
        }

        /// <summary>
        /// Replaces production data sources with the test ones
        /// </summary>
        private class TestInstaller : BaseInstaller
        {
            /// <summary>
            /// Gets priority for ordering akka configurations. Highest priority will override lower priority.
            /// </summary>
            /// <remarks>Consider using <seealso cref="BaseInstaller"/> integrated constants</remarks>
            protected override decimal AkkaConfigLoadPriority => -1M;

            /// <summary>
            /// Gets default akka configuration for current module
            /// </summary>
            /// <returns>Akka configuration</returns>
            protected override Config GetAkkaConfig() => ConfigurationFactory.Empty;

            /// <summary>
            /// Registering DI components
            /// </summary>
            /// <param name="container">The container.</param>
            /// <param name="store">The configuration store.</param>
            protected override void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store)
            {
                container.Register(Component.For<DatabaseInstanceName>().Instance(new DatabaseInstanceName()));
                container.Register(Classes.FromThisAssembly().Where(t => t.IsSubclassOf(typeof(ActorBase))).LifestyleTransient());

                container.Register(
                    Component.For<DataFactory<TestDataContext, User, Guid>>()
                        .ImplementedBy<TestUserFactory>()
                        .LifestyleTransient());
                container.Register(
                    Component.For<DataFactory<TestDataContext, Role, Guid>>()
                        .ImplementedBy<TestRolesFactory>()
                        .LifestyleTransient());

                container.Register(
                    Component.For<BaseConnectionManager>()
                        .ImplementedBy<TestConnectionManager>()
                        .LifestyleSingleton());

                container.Register(
                    Component.For<IContextFactory<TestDataContext>>()
                        .ImplementedBy<TestContextFactory>()
                        .LifestyleTransient());
            }
        }
    }
}
