// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindsorDependencyScope.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The windsor dependency scope.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http.Dependencies;

    using Castle.MicroKernel.Lifestyle;
    using Castle.Windsor;

    /// <summary>
    /// The windsor dependency scope.
    /// </summary>
    internal sealed class WindsorDependencyScope : IDependencyScope
    {
        /// <summary>
        /// The container.
        /// </summary>
        private readonly IWindsorContainer container;

        /// <summary>
        /// The scope.
        /// </summary>
        private readonly IDisposable scope;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindsorDependencyScope"/> class.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// In case of null parameter
        /// </exception>
        public WindsorDependencyScope(IWindsorContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            this.container = container;
            this.scope = container.BeginScope();
        }

        /// <summary>
        /// Releases used resources
        /// </summary>
        public void Dispose()
        {
            this.scope.Dispose();
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