// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigrationCollector.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the MigrationCollector type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.RemoteDomain
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    
    using Akka.Configuration;

    using Castle.Facilities.TypedFactory;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.Resolvers.SpecializedResolvers;
    using Castle.Windsor;

    using ClusterKit.Core;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.Migrator;

    using CommonServiceLocator.WindsorAdapter;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// Base class to launch <see cref="IMigrator"/> in remote domain
    /// </summary>
    /// <typeparam name="T">
    /// The type of expected result
    /// </typeparam>
    public abstract class MigrationCollector<T> : MarshalByRefObject
    {
        /// <summary>
        /// Gets or sets the configuration
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// Gets the list of errors
        /// </summary>
        public List<MigrationError> Errors { get; } = new List<MigrationError>();

        /// <summary>
        /// Gets or sets the result
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// Executes the collector
        /// </summary>
        public void Execute()
        {
            try
            {
                this.Result = this.GetResult();
            }
            catch (Exception e)
            {
                this.Errors.Add(new MigrationError { ErrorMessage = e.Message, Exception = e });
            }
        }

        /// <summary>
        /// Gets the list of migrators
        /// </summary>
        /// <returns>The list of migrators</returns>
        protected IEnumerable<IMigrator> GetMigrators()
        {
            var container = this.Initialize();
            var config = container.Resolve<Config>();
            var migratorTypeNames = config.GetStringList("ClusterKit.NodeManager.Migrators");

            if (migratorTypeNames == null)
            {
                yield break;
            }

            foreach (var typeName in migratorTypeNames)
            {
                var type = Type.GetType(typeName, false);
                if (type == null)
                {
                    this.Errors.Add(
                        new MigrationError
                            {
                                ErrorMessage = $"Migrator type {typeName} was not found",
                                MigratorTypeName = typeName
                            });
                    continue;
                }

                if (!type.GetInterfaces().Contains(typeof(IMigrator)))
                {
                    this.Errors.Add(
                        new MigrationError
                            {
                                ErrorMessage = $"Type {typeName} doesn't implement IMigrator",
                                MigratorTypeName = typeName
                            });
                    continue;
                }

                container.Register(Component.For(type));
                yield return (IMigrator)container.Resolve(type);
            }
        }

        /// <summary>
        /// Creates the result value
        /// </summary>
        /// <returns>The result</returns>
        protected abstract T GetResult();

        /// <summary>
        /// Creates and initializes the <see cref="IWindsorContainer"/>
        /// </summary>
        /// <returns>The DI container</returns>
        protected virtual IWindsorContainer Initialize()
        {
            foreach (var file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll"))
            {
                try
                {
                    var name = AssemblyName.GetAssemblyName(file);
                    Assembly.Load(name);
                }
                catch (Exception exception)
                {
                    this.Errors.Add(
                        new MigrationError
                            {
                                ErrorMessage = $"Error while loading assembly for domain {AppDomain.CurrentDomain.FriendlyName} in {AppDomain.CurrentDomain.BaseDirectory}",
                                Exception = exception
                            });
                }
            }

            var container = new WindsorContainer();
            container.AddFacility<TypedFactoryFacility>();
            container.Kernel.Resolver.AddSubResolver(new ArrayResolver(container.Kernel, true));
            container.Register(Component.For<IWindsorContainer>().Instance(container));
            container.RegisterWindsorInstallers(false);
            var config = BaseInstaller.GetStackedConfig(
                container,
                ConfigurationFactory.ParseString(this.Configuration));
            container.Register(Component.For<Config>().Instance(config));
            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));
            return container;
        }
    }
}