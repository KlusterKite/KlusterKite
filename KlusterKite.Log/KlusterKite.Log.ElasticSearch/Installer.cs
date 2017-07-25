// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Installer.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Installing components from current library
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Log.ElasticSearch
{
    using System.Reflection;

    using Akka.Configuration;

    using Autofac;

    using KlusterKite.Core;
    using KlusterKite.Core.Log;

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
        protected override Config GetAkkaConfig() => ConfigurationFactory.ParseString(ReadTextResource(typeof(Installer).GetTypeInfo().Assembly, "KlusterKite.Log.ElasticSearch.Resources.akka.hocon"));

        /// <inheritdoc />
        protected override void RegisterComponents(ContainerBuilder container, Config config)
        {
            container.RegisterType<Configurator>().As<ILoggerConfigurator>();
        }
    }
}