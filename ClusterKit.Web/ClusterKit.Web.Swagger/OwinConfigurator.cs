﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OwinConfigurator.cs" company="ClusterKit">
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
    using System.Linq;
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

            var publishDocUrl = akkaConfig.GetString("ClusterKit.Web.Swagger.Publish.publishDocPath", "swagger");
            var publishUiUrl = akkaConfig.GetString("ClusterKit.Web.Swagger.Publish.publishUiPath", "swagger/ui");

            var useOAuth2 = akkaConfig.GetBoolean("ClusterKit.Web.Swagger.OAuth2Enabled");

            Log.Information("Swagger: setting up swagger on {SwaggerUrl}", publishUiUrl);
            config.EnableSwagger(
                $"{publishDocUrl}/{{apiVersion}}",
                c =>
                    {
                        c.SingleApiVersion(
                            akkaConfig.GetString("ClusterKit.Web.Swagger.Publish.apiVersion", "v1"),
                            akkaConfig.GetString("ClusterKit.Web.Swagger.Publish.apiTitle", "Cluster API"));
                        var commentFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory);
                        foreach (
                            var commentFile in
                                commentFiles.Where(
                                    cf =>
                                    ".xml".Equals(
                                        Path.GetExtension(Path.GetFileName(cf)),
                                        StringComparison.InvariantCultureIgnoreCase)))
                        {
                            c.IncludeXmlComments(Path.GetFullPath(commentFile));
                        }

                        c.UseFullTypeNameInSchemaIds();
                        if (useOAuth2)
                        {
                            c.OAuth2("oauth2").Flow("implicit").TokenUrl(akkaConfig.GetString("ClusterKit.Web.Swagger.OAuth2TokenUrl"));
                        }
                    }).EnableSwaggerUi(
                        $"{publishUiUrl}/{{*assetPath}}",
                        c =>
                            {
                                c.DisableValidator();
                                if (useOAuth2)
                                {
                                    c.EnableOAuth2Support(
                                        akkaConfig.GetString("ClusterKit.Web.Swagger.OAuth2ClientId"),
                                        "swagger",
                                        "Swagger UI");
                                }
                            });

            Log.Information("Swagger was set up successfully");
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