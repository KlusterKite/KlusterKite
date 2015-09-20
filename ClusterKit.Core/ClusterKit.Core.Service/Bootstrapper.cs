// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Bootstrapper.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Dependency injection configuration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.Service
{
    using System.Configuration;

    using Akka.Actor;
    using Akka.DI.CastleWindsor;
    using Akka.DI.Core;

    using Castle.Facilities.TypedFactory;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.Resolvers.SpecializedResolvers;
    using Castle.Windsor;

    using CommonServiceLocator.WindsorAdapter;

    using JetBrains.Annotations;

    using Microsoft.Practices.ServiceLocation;

    using StackExchange.Redis;

    /// <summary>
    /// Dependency injection configuration
    /// </summary>
    [UsedImplicitly]
    public class Bootstrapper
    {
        /// <summary>
        ///  Dependency injection configuration
        /// </summary>
        /// <param name="container">Dependency injection container</param>
        public static void Configure(IWindsorContainer container)
        {
            container.AddFacility<TypedFactoryFacility>();
            container.Kernel.Resolver.AddSubResolver(new ArrayResolver(container.Kernel, true));
            container.Register(Component.For<Controller>().LifestyleTransient());
            container.Register(Component.For<IWindsorContainer>().Instance(container));

            // registering redis connection
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(ConfigurationManager.ConnectionStrings["redis"].ConnectionString);
            container.Register(
                Component
                .For<IConnectionMultiplexer>()
                .Instance(redis).LifestyleSingleton());

            container.RegisterWindsorInstallers();

            // starting akka system
            var config = BaseInstaller.GetStackedConfig(container);
            var actorSystem = ActorSystem.Create("ClusterKit", config);
            actorSystem.AddDependencyResolver(new WindsorDependencyResolver(container, actorSystem));
            actorSystem.StartNameSpaceActorsFromConfiguration();

            container.Register(Component.For<ActorSystem>().Instance(actorSystem).LifestyleSingleton());
            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));
            BaseInstaller.RunPostStart(container);
        }
    }
}