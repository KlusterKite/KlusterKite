// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OwinConfiguration.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   External additional owin configuration.
//   Should be registered in DI resolver
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Swagger
{
    using System;
    using System.IO;
    using System.Web.Http;

    using Akka.Configuration;

    using Microsoft.Practices.ServiceLocation;

    using Owin;

    using Serilog;

    using Swashbuckle.Application;

    /// <summary>
    /// External additional owin configuration.
    /// Should be registered in DI resolver
    /// </summary>
    public class OwinConfigurator : IOwinStartupConfigurator
    {
        /// <summary>
        /// Add additional http configuration
        /// </summary>
        /// <param name="config">The configuration</param>
        public void ConfigureApi(HttpConfiguration config)
        {
            var akkaConfig = ServiceLocator.Current.GetInstance<Config>();
            if (akkaConfig == null)
            {
                Log.Error("{Type}: akka config is null", this.GetType().FullName);
                return;
            }

            var publishDocUrl = akkaConfig.GetString("ClusterKit.Web.Swagger.Publish.publishDocPath", " /swagger");
            var publishUiUrl = akkaConfig.GetString("ClusterKit.Web.Swagger.Publish.publishUiPath", " /swagger/ui");

            config.EnableSwagger(
                publishDocUrl,
                c =>
                    {
                        c.SingleApiVersion(
                            akkaConfig.GetString("ClusterKit.Web.Swagger.Publish.apiVersion", "v1"),
                            akkaConfig.GetString("ClusterKit.Web.Swagger.Publish.apiTitle", "Cluster API"));
                        var commentFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.xml");
                        foreach (var commentFile in commentFiles)
                        {
                            c.IncludeXmlComments(Path.GetFullPath(commentFile));
                        }
                        c.UseFullTypeNameInSchemaIds();
                    }).EnableSwaggerUi(
                        publishUiUrl,
                        c =>
                            {
                                c.DisableValidator();
                            });
        }

        /// <summary>
        /// Add additional owin configuration
        /// </summary>
        /// <param name="appBuilder">The builder</param>
        public void ConfigureApp(IAppBuilder appBuilder)
        {
        }
    }
}