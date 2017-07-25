// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationCheckActor.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The actor to work with configurations
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Event;

    using Autofac;

    using KlusterKite.API.Client;
    using KlusterKite.Data;
    using KlusterKite.Data.CRUD.ActionMessages;
    using KlusterKite.Data.CRUD.Exceptions;
    using KlusterKite.Data.EF;
    using KlusterKite.NodeManager.Client.ORM;
    using KlusterKite.NodeManager.ConfigurationSource;
    using KlusterKite.NodeManager.Launcher.Utils;
    using KlusterKite.NodeManager.Messages;
    using KlusterKite.Security.Attributes;
    using KlusterKite.Security.Client;

    /// <summary>
    /// The actor to work with configurations
    /// </summary>
    public class ConfigurationCheckActor : BaseCrudActor<ConfigurationContext>
    {
        /// <summary>
        /// The data source context factory
        /// </summary>
        private readonly UniversalContextFactory contextFactory;

        /// <summary>  
        /// The nuget repository
        /// </summary>
        private readonly IPackageRepository nugetRepository;

        /// <summary>
        /// The database connection string
        /// </summary>
        private readonly string connectionString;

        /// <summary>
        /// The database name
        /// </summary>
        private readonly string databaseName;

        /// <summary>
        /// The database provider name
        /// </summary>
        private readonly string databaseProviderName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationCheckActor"/> class.
        /// </summary>
        /// <param name="componentContext">
        /// The DI context
        /// </param>
        /// <param name="contextFactory">
        /// Configuration context factory
        /// </param>
        /// <param name="nugetRepository">
        /// The nuget repository
        /// </param>
        public ConfigurationCheckActor(
            IComponentContext componentContext,
            UniversalContextFactory contextFactory,
            IPackageRepository nugetRepository) : base(componentContext)
        {
            this.contextFactory = contextFactory;
            this.nugetRepository = nugetRepository;
            this.connectionString = Context.System.Settings.Config.GetString(NodeManagerActor.ConfigConnectionStringPath);
            this.databaseName = Context.System.Settings.Config.GetString(NodeManagerActor.ConfigDatabaseNamePath);
            this.databaseProviderName = Context.System.Settings.Config.GetString(NodeManagerActor.ConfigDatabaseProviderNamePath);

            this.ReceiveAsync<CrudActionMessage<Configuration, int>>(this.OnRequest);
            this.ReceiveAsync<ConfigurationCheckRequest>(this.OnConfigurationCheck);
            this.ReceiveAsync<ConfigurationSetReadyRequest>(this.OnConfigurationSetReady);
            this.Receive<ConfigurationSetObsoleteRequest>(r => this.OnConfigurationSetObsolete(r));
            this.Receive<ConfigurationSetStableRequest>(r => this.OnConfigurationSetStable(r));
        }

        /// <inheritdoc />
        protected override ConfigurationContext GetContext()
        {
            return this.contextFactory.CreateContext<ConfigurationContext>(
                this.databaseProviderName,
                this.connectionString,
                this.databaseName);
        }

        /// <summary>
        /// Process the <see cref="ConfigurationSetReadyRequest"/>
        /// </summary>
        /// <param name="request">
        /// The request
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task OnConfigurationCheck(ConfigurationCheckRequest request)
        {
            try
            {
                using (var ds = this.GetContext())
                {
                    Context.GetLogger().Info("{Type}: checking configuration {ConfigurationId}", this.GetType().Name, request.Id);
                    var configuration = ds.Configurations.FirstOrDefault(r => r.Id == request.Id);
                    if (configuration == null)
                    {
                        this.Sender.Tell(CrudActionResponse<Configuration>.Error(new EntityNotFoundException(), null));
                        Context.GetLogger().Info(
                            "{Type}: checking configuration {ConfigurationId} - not found",
                            this.GetType().Name,
                            request.Id);
                        return;
                    }

                    if (configuration.State != EnConfigurationState.Draft)
                    {
                        this.Sender.Tell(
                            CrudActionResponse<Configuration>.Error(
                                new Exception("Only draft configurations can be checked"),
                                null));
                        Context.GetLogger().Info(
                            "{Type}: checking configuration {ConfigurationId} - not draft",
                            this.GetType().Name,
                            request.Id);
                        return;
                    }

                    var supportedFrameworks =
                        Context.System.Settings.Config.GetStringList("KlusterKite.NodeManager.SupportedFrameworks");

                    Context.GetLogger().Info(
                        "{Type}: checking configuration {ConfigurationId} against frameworks {Frameworks}",
                        this.GetType().Name,
                        request.Id,
                        string.Join(", ", supportedFrameworks));
                    var errors = await configuration.CheckAll(ds, this.nugetRepository, supportedFrameworks.ToList());
                    if (errors.Count > 0)
                    {
                        this.Sender.Tell(
                            CrudActionResponse<Configuration>.Error(new MutationException(errors.ToArray()), null));
                        Context.GetLogger().Info(
                            "{Type}: checking configuration {ConfigurationId} completed",
                            this.GetType().Name,
                            request.Id);
                        return;
                    }

                    this.Sender.Tell(CrudActionResponse<Configuration>.Success(configuration, null));
                }
            }
            catch (Exception exception)
            {
                this.Sender.Tell(CrudActionResponse<Configuration>.Error(exception, null));
            }
        }

        /// <summary>
        /// Process the <see cref="ConfigurationSetObsoleteRequest"/>
        /// </summary>
        /// <param name="request">The request</param>
        private void OnConfigurationSetObsolete(ConfigurationSetObsoleteRequest request)
        {
            try
            {
                using (var ds = this.GetContext())
                {
                    var configuration = ds.Configurations.FirstOrDefault(r => r.Id == request.Id);
                    if (configuration == null)
                    {
                        this.Sender.Tell(CrudActionResponse<Configuration>.Error(new EntityNotFoundException(), null));
                        return;
                    }

                    if (configuration.State != EnConfigurationState.Ready)
                    {
                        this.Sender.Tell(
                            CrudActionResponse<Configuration>.Error(
                                new Exception("Only ready configurations can be made obsolete manually"),
                                null));
                        return;
                    }

                    configuration.State = EnConfigurationState.Obsolete;
                    ds.SaveChanges();
                    this.Sender.Tell(CrudActionResponse<Configuration>.Success(configuration, null));
                    SecurityLog.CreateRecord(
                        EnSecurityLogType.OperationGranted,
                        EnSeverity.Crucial,
                        request.Context,
                        "Configuration {ConfigurationId} marked as obsolete",
                        configuration.Id);
                }
            }
            catch (Exception exception)
            {
                this.Sender.Tell(CrudActionResponse<Configuration>.Error(exception, null));
            }
        }

        /// <summary>
        /// Process the <see cref="ConfigurationSetReadyRequest"/>
        /// </summary>
        /// <param name="request">
        /// The request
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task OnConfigurationSetReady(ConfigurationSetReadyRequest request)
        {
            try
            {
                using (var ds = this.GetContext())
                {
                    var configuration = ds.Configurations.FirstOrDefault(r => r.Id == request.Id);
                    if (configuration == null)
                    {
                        this.Sender.Tell(CrudActionResponse<Configuration>.Error(new EntityNotFoundException(), null));
                        return;
                    }

                    if (configuration.State != EnConfigurationState.Draft)
                    {
                        this.Sender.Tell(
                            CrudActionResponse<Configuration>.Error(
                                new Exception("Only draft configurations can be made ready"),
                                null));
                        return;
                    }

                    if (ds.Configurations.Any(r => r.State == EnConfigurationState.Ready))
                    {
                        this.Sender.Tell(
                            CrudActionResponse<Configuration>.Error(
                                new Exception(
                                    "There is an already defined ready configuration. Please remove the previous one."),
                                null));
                        return;
                    }

                    var supportedFrameworks =
                        Context.System.Settings.Config.GetStringList("KlusterKite.NodeManager.SupportedFrameworks");
                    var errors = await configuration.CheckAll(ds, this.nugetRepository, supportedFrameworks.ToList());
                    if (errors.Count > 0)
                    {
                        this.Sender.Tell(CrudActionResponse<Configuration>.Error(new MutationException(errors.ToArray()), null));
                        return;
                    }

                    configuration.State = EnConfigurationState.Ready;
                    ds.SaveChanges();
                    this.Sender.Tell(CrudActionResponse<Configuration>.Success(configuration, null));
                    SecurityLog.CreateRecord(
                        EnSecurityLogType.OperationGranted,
                        EnSeverity.Crucial,
                        request.Context,
                        "Configuration {ConfigurationId} marked as Ready",
                        configuration.Id);
                }
            }
            catch (Exception exception)
            {
                this.Sender.Tell(CrudActionResponse<Configuration>.Error(exception, null));
            }
        }

        /// <summary>
        /// Process the <see cref="ConfigurationSetStableRequest"/>
        /// </summary>
        /// <param name="request">The request</param>
        private void OnConfigurationSetStable(ConfigurationSetStableRequest request)
        {
            try
            {
                using (var ds = this.GetContext())
                {
                    var configuration = ds.Configurations.FirstOrDefault(r => r.Id == request.Id);
                    if (configuration == null)
                    {
                        this.Sender.Tell(CrudActionResponse<Configuration>.Error(new EntityNotFoundException(), null));
                        return;
                    }

                    if (configuration.State != EnConfigurationState.Active)
                    {
                        this.Sender.Tell(
                            CrudActionResponse<Configuration>.Error(
                                new Exception("Only active configurations can be marked as stable"),
                                null));
                        return;
                    }

                    if (configuration.IsStable != request.IsStable)
                    {
                        var error = new ErrorDescription("isStable", "The value is not changed");
                        var mutationException = new MutationException(error);
                        this.Sender.Tell(CrudActionResponse<Configuration>.Error(mutationException, null));
                        return;
                    }

                    configuration.IsStable = request.IsStable;
                    ds.SaveChanges();
                    this.Sender.Tell(CrudActionResponse<Configuration>.Success(configuration, null));
                }
            }
            catch (Exception exception)
            {
                this.Sender.Tell(CrudActionResponse<Configuration>.Error(exception, null));
            }
        }
    }
}
