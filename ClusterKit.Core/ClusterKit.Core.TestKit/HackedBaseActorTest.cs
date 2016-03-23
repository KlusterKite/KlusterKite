// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HackedBaseActorTest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Some strange workaround to solve class creation order problem
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.TestKit
{
    using Akka.Actor;
    using Akka.TestKit.Xunit2;

    using Castle.MicroKernel.Registration;
    using Castle.Windsor;

    using CommonServiceLocator.WindsorAdapter;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// Some strange workaround to solve class creation order problem
    /// </summary>
    public abstract class HackedBaseActorTest : TestKit
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HackedBaseActorTest"/> class.
        /// </summary>
        /// <param name="description">
        /// The description.
        /// </param>
        protected HackedBaseActorTest(TestDescription description) : base(description.System)
        {
            description.Container.Register(Component.For<IActorRef>().Instance(this.TestActor).Named("testActor"));
            description.Container.Register(Component.For<IWindsorContainer>().Instance(description.Container).LifestyleSingleton());
            this.WindsorContainer = description.Container;
            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(this.WindsorContainer));
        }

        /// <summary>
        /// Gets the Windsor container
        /// </summary>
        protected IWindsorContainer WindsorContainer { get; }

        /// <summary>
        /// Base test class initialization data
        /// </summary>
        protected class TestDescription
        {
            /// <summary>
            /// Gets or sets DI container
            /// </summary>
            public WindsorContainer Container { get; set; }

            /// <summary>
            /// Gets or sets ready actor system
            /// </summary>
            public ActorSystem System { get; set; }
        }
    }
}