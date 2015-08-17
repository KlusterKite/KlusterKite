// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HackedBaseActorTest.cs" company="TaxiKit">
//   All rights reserved
// </copyright>
// <summary>
//   Some strange workaround to solve class creation order problem
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TaxiKit.Core.TestKit
{
    using Akka.Actor;
    using Akka.TestKit.Xunit2;

    using Castle.MicroKernel.Registration;
    using Castle.Windsor;

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
            this.WindsorContainer = description.Container;
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