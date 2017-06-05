﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Installer.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Installing components from current library
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ClusterKit.Web.GraphQL.Publisher
{
    using System.Collections.Generic;

    using Akka.Actor;
    using Akka.Configuration;

    using Autofac;

    using ClusterKit.Core;

    using global::GraphQL;
    using global::GraphQL.Http;

    /// <summary>
    /// Installing components from current library
    /// </summary>
    public class Installer : BaseInstaller
    {
        /// <summary>
        /// Gets priority for ordering akka configurations. Highest priority will override lower priority.
        /// </summary>
        /// <remarks>Consider using <seealso cref="BaseInstaller"/> integrated constants</remarks>
        protected override decimal AkkaConfigLoadPriority => PrioritySharedLib;

        /// <summary>
        /// Gets default akka configuration for current module
        /// </summary>
        /// <returns>Akka configuration</returns>
        protected override Config GetAkkaConfig() => ConfigurationFactory.ParseString(Configuration.AkkaConfig);

        /// <summary>
        /// Gets list of roles, that would be assign to cluster node with this plugin installed.
        /// </summary>
        /// <returns>The list of roles</returns>
        protected override IEnumerable<string> GetRoles() => new[]
                                                                 {
                                                                     "ClusterKit.Web.GraphQL.Publisher"
                                                                 };

        /// <inheritdoc />
        protected override void RegisterComponents(ContainerBuilder container)
        {
            container.RegisterType<OwinConfigurator>().As<IOwinStartupConfigurator>();
            container.RegisterAssemblyTypes(typeof(Installer).Assembly).Where(t => t.IsSubclassOf(typeof(ActorBase)));
            container.RegisterType<SchemaProvider>().SingleInstance();

            var documentExecuter = new DocumentExecuter();
            var writer = new DocumentWriter(false);
            container.RegisterInstance(documentExecuter).As<IDocumentExecuter>();
            container.RegisterInstance(writer).As<IDocumentWriter>();
        }
    }
}