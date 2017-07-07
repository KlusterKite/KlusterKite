// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Installer.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Installing components from current library
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KlusterKite.Web.Authorization
{
    using System.Collections.Generic;
    using System.Reflection;

    using Akka.Actor;
    using Akka.Configuration;

    using Autofac;

    using KlusterKite.Core;

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
        protected override Config GetAkkaConfig() => ConfigurationFactory.ParseString(ReadTextResource(typeof(Installer).GetTypeInfo().Assembly, "KlusterKite.Web.Authorization.Resources.akka.hocon"));

        /// <summary>
        /// Gets list of roles, that would be assign to cluster node with this plugin installed.
        /// </summary>
        /// <returns>The list of roles</returns>
        protected override IEnumerable<string> GetRoles() => new string[0];

        /// <inheritdoc />
        protected override void RegisterComponents(ContainerBuilder container, Config config)
        {
            container.RegisterAssemblyTypes(typeof(Installer).GetTypeInfo().Assembly).Where(t => t.GetTypeInfo().IsSubclassOf(typeof(ActorBase)));
            container.RegisterType<WebHostingConfigurator>().As<IWebHostingConfigurator>();
        }
    }
}