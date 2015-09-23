// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindsorDependencyResolver.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The dependency resolver for owin service
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http.Dependencies;

    using Castle.Windsor;

    /// <summary>
    /// The dependency resolver for owin service
    /// </summary>
    internal sealed class WindsorDependencyResolver : IDependencyResolver
    {
        /// <summary>
        /// The windsor container.
        /// </summary>
        private readonly IWindsorContainer container;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindsorDependencyResolver"/> class.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// In case of null parameter
        /// </exception>
        public WindsorDependencyResolver(IWindsorContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            this.container = container;
        }

        /// <summary>
        /// Starts a resolution scope.
        /// </summary>
        /// <returns>
        /// The dependency scope.
        /// </returns>
        public IDependencyScope BeginScope()
        {
            return new WindsorDependencyScope(this.container);
        }

        /// <summary>
        /// Disposes all resources
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Retrieves a service from the scope.
        /// </summary>
        /// <returns>
        /// The retrieved service.
        /// </returns>
        /// <param name="serviceType">The service to be retrieved.</param>
        public object GetService(Type serviceType)
        {
            return this.container.Kernel.HasComponent(serviceType) ? this.container.Resolve(serviceType) : null;
        }

        /// <summary>
        /// Retrieves a collection of services from the scope.
        /// </summary>
        /// <returns>
        /// The retrieved collection of services.
        /// </returns>
        /// <param name="serviceType">The collection of services to be retrieved.</param>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return this.container.ResolveAll(serviceType).Cast<object>().ToArray();
        }
    }
}