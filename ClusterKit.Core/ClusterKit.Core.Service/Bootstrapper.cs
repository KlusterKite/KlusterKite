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
    using Akka.Configuration;
    using Akka.DI.CastleWindsor;
    using Akka.DI.Core;

    using Castle.Facilities.TypedFactory;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.Resolvers.SpecializedResolvers;
    using Castle.Windsor;

    using CommonServiceLocator.WindsorAdapter;

    using JetBrains.Annotations;

    using Microsoft.Practices.ServiceLocation;

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

            container.RegisterWindsorInstallers();
            var config = BaseInstaller.GetStackedConfig(container);
            container.Register(Component.For<Config>().Instance(config));

            // performing prestart checks
            BaseInstaller.RunPrecheck(container, config);

            // starting akka system
            var actorSystem = ActorSystem.Create("ClusterKit", config);
            actorSystem.AddDependencyResolver(new WindsorDependencyResolver(container, actorSystem));

            container.Register(Component.For<ActorSystem>().Instance(actorSystem).LifestyleSingleton());
            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));
        }
    }
}