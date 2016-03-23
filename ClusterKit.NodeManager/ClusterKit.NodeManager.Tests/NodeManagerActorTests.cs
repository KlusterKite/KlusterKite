// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeManagerActorTests.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing node manager actor
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
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
    using ClusterKit.Core.Data;
    using ClusterKit.Core.EF;
    using ClusterKit.Core.Ping;
    using ClusterKit.Core.TestKit;
    using ClusterKit.NodeManager.Client.Messages;
    using ClusterKit.NodeManager.ConfigurationSource;

    using Moq;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Testing node manager actor
    /// </summary>
    public class NodeManagerActorTests : BaseActorTest<NodeManagerActorTests.Configurator>
    {
        /// <summary>
        /// Access to xunit output
        /// </summary>
        private ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeManagerActorTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public NodeManagerActorTests(ITestOutputHelper output)
            : base(output)
        {
            this.output = output;
        }

        /// <summary>
        /// Tests actor start
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Fact]
        public async Task ActorStartTest()
        {
            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>());
            var response = await testActor.Ask<PongMessage>(new PingMessage(), TimeSpan.FromSeconds(1));
            Assert.NotNull(response);
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
                var pluginInstallers = base.GetPluginInstallers();
                pluginInstallers.Add(new TestInstaller());
                return pluginInstallers;
            }
        }

        public class MoqConnection : DbConnection
        {
            private string database;

            public override string ConnectionString { get; set; }

            public override string Database => this.database;

            public override string DataSource
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override string ServerVersion
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override ConnectionState State => ConnectionState.Open;

            public override void ChangeDatabase(string databaseName)
            {
                this.database = databaseName;
            }

            public override void Close()
            {
            }

            public override void Open()
            {
            }

            protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
            {
                throw new NotImplementedException();
            }

            protected override DbCommand CreateDbCommand()
            {
                throw new NotImplementedException();
            }
        }

        public class TestConnectionManager : BaseConnectionManager
        {
            public override void CheckCreateDatabase(DbConnection connection, string databaseName)
            {
            }

            public override DbConnection CreateConnection(string connectionString)
            {
                return new MoqConnection();
            }
        }

        public class TestContextFactory<TContext>
            : IContextFactory<TContext>
            where TContext : new()
        {
            public Task<TContext> CreateAndUpgradeContext(string connectionString, string databaseName)
            {
                return Task.FromResult(new TContext());
            }

            public Task<TContext> CreateContext(string connectionString, string databaseName)
            {
                return Task.FromResult(new TContext());
            }
        }

        public abstract class TestFactory<TContext, TObject, TId>
                    : DataFactory<TContext, TObject, TId>
            where TObject : class
        {
            private Dictionary<TId, TObject> storage = new Dictionary<TId, TObject>();

            public TestFactory(TContext context)
                : base(context)
            {
            }

            public override Task<TObject> Delete(TId id)
            {
                TObject obj;
                if (this.storage.TryGetValue(id, out obj))
                {
                    this.storage.Remove(id);
                    return Task.FromResult(obj);
                }

                return Task.FromResult<TObject>(null);
            }

            public override Task<TObject> Get(TId id)
            {
                TObject obj;
                return this.storage.TryGetValue(id, out obj) ? Task.FromResult(obj) : Task.FromResult<TObject>(null);
            }

            public override Task<List<TObject>> GetList(int skip, int? count)
            {
                var objects = this.storage.Values.Skip(skip);
                if (count.HasValue)
                {
                    objects = objects.Take(count.Value);
                }

                return Task.FromResult(objects.ToList());
            }

            public override Task Insert(TObject obj)
            {
                if (this.storage.ContainsKey(this.GetId(obj)))
                {
                    throw new InvalidOperationException("Duplicate insert");
                }

                this.storage[this.GetId(obj)] = obj;
                return Task.FromResult<object>(null);
            }

            public override Task Update(TObject obj)
            {
                if (!this.storage.ContainsKey(this.GetId(obj)))
                {
                    throw new InvalidOperationException("Duplicate insert");
                }

                this.storage[this.GetId(obj)] = obj;
                return Task.FromResult<object>(null);
            }
        }

        public class TestInstaller : BaseInstaller
        {
            protected override decimal AkkaConfigLoadPriority => -1M;

            protected override Config GetAkkaConfig() => ConfigurationFactory.ParseString(@"
            {
                ClusterKit.NodeManager.ConfigurationDatabaseName = ""TestConfigurationDatabase""
            }");

            protected override void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store)
            {
                container.Register(Classes.FromAssemblyContaining<NodeManagerActor>().Where(t => t.IsSubclassOf(typeof(ActorBase))).LifestyleTransient());
                container.Register(Component.For<BaseConnectionManager>().Instance(new TestConnectionManager()).LifestyleSingleton());

                container.Register(Component.For<DataFactory<ConfigurationContext, NodeTemplate, int>>()
                    .Instance(new UniversalTestFactory<ConfigurationContext, NodeTemplate, int>(null, o => o.Id)).LifestyleSingleton());
                container.Register(Component.For<DataFactory<ConfigurationContext, NugetFeed, int>>()
                    .Instance(new UniversalTestFactory<ConfigurationContext, NugetFeed, int>(null, o => o.Id)).LifestyleSingleton());
                container.Register(Component.For<DataFactory<ConfigurationContext, SeedAddress, int>>()
                    .Instance(new UniversalTestFactory<ConfigurationContext, SeedAddress, int>(null, o => o.Id)).LifestyleSingleton());

                container.Register(Component.For<DataFactory<string, PackageDescription, string>>()
                    .Instance(new UniversalTestFactory<string, PackageDescription, string>(null, o => o.Id)).LifestyleSingleton());

                container.Register(Component.For<IContextFactory<ConfigurationContext>>().Instance(new TestContextFactory<ConfigurationContext>()).LifestyleSingleton());
            }
        }

        public class UniversalTestFactory<TContext, TObject, TId> : TestFactory<TContext, TObject, TId>
                    where TObject : class
        {
            private readonly Func<TObject, TId> getIdFunc;

            public UniversalTestFactory(TContext context, Func<TObject, TId> getIdFunc)
                : base(context)
            {
                this.getIdFunc = getIdFunc;
            }

            public override TId GetId(TObject obj)
            {
                return this.getIdFunc(obj);
            }
        }
    }
}