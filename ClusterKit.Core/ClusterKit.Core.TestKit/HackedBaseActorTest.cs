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
    using Akka.DI.AutoFac;
    using Akka.DI.Core;
    using Akka.TestKit.Xunit2;

    using Autofac;
    using Autofac.Extras.CommonServiceLocator;

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
            var containerBuilder = description.ContainerBuilder;
            containerBuilder.RegisterInstance(this.TestActor).As<IActorRef>().Named("testActor", typeof(IActorRef));
            this.Container = containerBuilder.Build();
            this.Sys.AddDependencyResolver(new AutoFacDependencyResolver(this.Container, this.Sys));

            if (description.Configurator.RunPostStart)
            {
                BaseInstaller.RunPostStart(containerBuilder);
            }

            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocator(this.Container));
        }

        /// <summary>
        /// Gets DI container
        /// </summary>
        protected IContainer Container { get; }

        /// <summary>
        /// Base test class initialization data
        /// </summary>
        protected class TestDescription
        {
            /// <summary>
            /// Gets or sets DI container builder
            /// </summary> 
            public ContainerBuilder ContainerBuilder { get; set; }

            /// <summary>
            /// Gets or sets ready actor system
            /// </summary>
            public ActorSystem System { get; set; }

            /// <summary>
            /// Gets or sets the test configurator
            /// </summary>
            public TestConfigurator Configurator { get; set; }
        }
    }
}