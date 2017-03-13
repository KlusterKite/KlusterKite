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
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Configuration;
    using Akka.DI.Core;

    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ClusterKit.Core;
    using ClusterKit.Core.TestKit;
    using ClusterKit.Data.CRUD.ActionMessages;
    using ClusterKit.Data.Tests.Mock;

    using Moq;

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
            var users = this.WindsorContainer.Resolve<List<User>>();
            var user = users.First();
            
            var actor = this.Sys.ActorOf(this.Sys.DI().Props<TestDataActor>(), "data");

            this.ExpectNoMsg();

            var request = new CrudActionMessage<User, Guid>
                              {
                                  ActionType = EnActionType.Get,
                                  Id = user.Uid
                              };

            var result = await actor.Ask<CrudActionResponse<User>>(request, TimeSpan.FromSeconds(1));
            Assert.NotNull(result);
            if (result.Exception != null)
            {
                this.Sys.Log.Error(result.Exception, "Exception");
            }

            Assert.Null(result.Exception);
            Assert.NotNull(result.Data);
            Assert.Equal(result.Data.Login, user.Login);
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
                var usersMock = new Mock<DbSet<User>>();
                var rolesMock = new Mock<DbSet<Role>>();
                var context = new Mock<TestDataContext>();
                context.Setup(m => m.Roles).Returns(rolesMock.Object);
                context.Setup(m => m.Users).Returns(usersMock.Object);

                var user1 = new User { Login = "User1", Password = "123", Uid = Guid.NewGuid() };
                var user2 = new User { Login = "User2", Password = "123", Uid = Guid.NewGuid() };
                var users = new List<User> { user1, user2 };
                var usersSet = users.AsQueryable();
                usersMock.As<IQueryable<User>>().Setup(m => m.Provider).Returns(usersSet.Provider);
                usersMock.As<IQueryable<User>>().Setup(m => m.Expression).Returns(usersSet.Expression);
                usersMock.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(usersSet.ElementType);
                usersMock.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(() => usersSet.GetEnumerator());

                container.Register(Classes.FromThisAssembly().Where(t => t.IsSubclassOf(typeof(ActorBase))).LifestyleTransient());

                container.Register(Component.For<List<User>>().Instance(users));
                container.Register(Component.For<TestDataContext>().Instance(context.Object));
                container.Register(Component.For<Mock<DbSet<User>>>().Instance(usersMock));
                container.Register(Component.For<Mock<DbSet<Role>>>().Instance(rolesMock));
                container.Register(Component.For<Mock<TestDataContext>>().Instance(context));

                container.Register(
                    Component.For<DataFactory<TestDataContext, User, Guid>>()
                        .ImplementedBy<TestUserFactory>()
                        .LifestyleTransient());
                container.Register(
                    Component.For<DataFactory<TestDataContext, Role, Guid>>()
                        .ImplementedBy<TestRolesFactory>()
                        .LifestyleTransient());
            }
        }
    }
}
