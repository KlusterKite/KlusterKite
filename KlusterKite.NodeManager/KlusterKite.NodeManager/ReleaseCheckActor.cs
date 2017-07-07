// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReleaseCheckActor.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The actor to work with releases
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
    /// The actor to work with releases
    /// </summary>
    public class ReleaseCheckActor : BaseCrudActor<ConfigurationContext>
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
        private string connectionString;

        /// <summary>
        /// The database name
        /// </summary>
        private string databaseName;

        /// <summary>
        /// The database provider name
        /// </summary>
        private string databaseProviderName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseCheckActor"/> class.
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
        public ReleaseCheckActor(
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
            this.ReceiveAsync<ReleaseCheckRequest>(this.OnReleaseCheck);
            this.ReceiveAsync<ReleaseSetReadyRequest>(this.OnReleaseSetReady);
            this.Receive<ReleaseSetObsoleteRequest>(r => this.OnReleaseSetObsolete(r));
            this.Receive<ReleaseSetStableRequest>(r => this.OnReleaseSetStable(r));
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
        /// Process the <see cref="ReleaseSetReadyRequest"/>
        /// </summary>
        /// <param name="request">
        /// The request
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task OnReleaseCheck(ReleaseCheckRequest request)
        {
            try
            {
                using (var ds = this.GetContext())
                {
                    Context.GetLogger().Info("{Type}: checking release {ReleaseId}", this.GetType().Name, request.Id);
                    var release = ds.Releases.FirstOrDefault(r => r.Id == request.Id);
                    if (release == null)
                    {
                        this.Sender.Tell(CrudActionResponse<Configuration>.Error(new EntityNotFoundException(), null));
                        Context.GetLogger().Info(
                            "{Type}: checking release {ReleaseId} - not found",
                            this.GetType().Name,
                            request.Id);
                        return;
                    }

                    if (release.State != EnReleaseState.Draft)
                    {
                        this.Sender.Tell(
                            CrudActionResponse<Configuration>.Error(
                                new Exception("Only draft releases can be checked"),
                                null));
                        Context.GetLogger().Info(
                            "{Type}: checking release {ReleaseId} - not draft",
                            this.GetType().Name,
                            request.Id);
                        return;
                    }

                    var supportedFrameworks =
                        Context.System.Settings.Config.GetStringList("KlusterKite.NodeManager.SupportedFrameworks");

                    Context.GetLogger().Info(
                        "{Type}: checking release {ReleaseId} against frameworks {Frameworks}",
                        this.GetType().Name,
                        request.Id,
                        string.Join(", ", supportedFrameworks));
                    var errors = await release.CheckAll(ds, this.nugetRepository, supportedFrameworks.ToList());
                    if (errors.Count > 0)
                    {
                        this.Sender.Tell(
                            CrudActionResponse<Configuration>.Error(new MutationException(errors.ToArray()), null));
                        Context.GetLogger().Info(
                            "{Type}: checking release {ReleaseId} completed",
                            this.GetType().Name,
                            request.Id);
                        return;
                    }

                    this.Sender.Tell(CrudActionResponse<Configuration>.Success(release, null));
                }
            }
            catch (Exception exception)
            {
                this.Sender.Tell(CrudActionResponse<Configuration>.Error(exception, null));
            }
        }

        /// <summary>
        /// Process the <see cref="ReleaseSetObsoleteRequest"/>
        /// </summary>
        /// <param name="request">The request</param>
        private void OnReleaseSetObsolete(ReleaseSetObsoleteRequest request)
        {
            try
            {
                using (var ds = this.GetContext())
                {
                    var release = ds.Releases.FirstOrDefault(r => r.Id == request.Id);
                    if (release == null)
                    {
                        this.Sender.Tell(CrudActionResponse<Configuration>.Error(new EntityNotFoundException(), null));
                        return;
                    }

                    if (release.State != EnReleaseState.Ready)
                    {
                        this.Sender.Tell(
                            CrudActionResponse<Configuration>.Error(
                                new Exception("Only ready releases can be made obsolete manually"),
                                null));
                        return;
                    }

                    release.State = EnReleaseState.Obsolete;
                    ds.SaveChanges();
                    this.Sender.Tell(CrudActionResponse<Configuration>.Success(release, null));
                    SecurityLog.CreateRecord(
                        EnSecurityLogType.OperationGranted,
                        EnSeverity.Crucial,
                        request.Context,
                        "Release {ReleaseId} marked as obsolete",
                        release.Id);
                }
            }
            catch (Exception exception)
            {
                this.Sender.Tell(CrudActionResponse<Configuration>.Error(exception, null));
            }
        }

        /// <summary>
        /// Process the <see cref="ReleaseSetReadyRequest"/>
        /// </summary>
        /// <param name="request">
        /// The request
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task OnReleaseSetReady(ReleaseSetReadyRequest request)
        {
            try
            {
                using (var ds = this.GetContext())
                {
                    var release = ds.Releases.FirstOrDefault(r => r.Id == request.Id);
                    if (release == null)
                    {
                        this.Sender.Tell(CrudActionResponse<Configuration>.Error(new EntityNotFoundException(), null));
                        return;
                    }

                    if (release.State != EnReleaseState.Draft)
                    {
                        this.Sender.Tell(
                            CrudActionResponse<Configuration>.Error(
                                new Exception("Only draft releases can be made ready"),
                                null));
                        return;
                    }

                    if (ds.Releases.Any(r => r.State == EnReleaseState.Ready))
                    {
                        this.Sender.Tell(
                            CrudActionResponse<Configuration>.Error(
                                new Exception(
                                    "There is an already defined ready release. Please remove the previous one."),
                                null));
                        return;
                    }

                    var supportedFrameworks =
                        Context.System.Settings.Config.GetStringList("KlusterKite.NodeManager.SupportedFrameworks");
                    var errors = await release.CheckAll(ds, this.nugetRepository, supportedFrameworks.ToList());
                    if (errors.Count > 0)
                    {
                        this.Sender.Tell(CrudActionResponse<Configuration>.Error(new MutationException(errors.ToArray()), null));
                        return;
                    }

                    release.State = EnReleaseState.Ready;
                    ds.SaveChanges();
                    this.Sender.Tell(CrudActionResponse<Configuration>.Success(release, null));
                    SecurityLog.CreateRecord(
                        EnSecurityLogType.OperationGranted,
                        EnSeverity.Crucial,
                        request.Context,
                        "Release {ReleaseId} marked as Ready",
                        release.Id);
                }
            }
            catch (Exception exception)
            {
                this.Sender.Tell(CrudActionResponse<Configuration>.Error(exception, null));
            }
        }

        /// <summary>
        /// Process the <see cref="ReleaseSetStableRequest"/>
        /// </summary>
        /// <param name="request">The request</param>
        private void OnReleaseSetStable(ReleaseSetStableRequest request)
        {
            try
            {
                using (var ds = this.GetContext())
                {
                    var release = ds.Releases.FirstOrDefault(r => r.Id == request.Id);
                    if (release == null)
                    {
                        this.Sender.Tell(CrudActionResponse<Configuration>.Error(new EntityNotFoundException(), null));
                        return;
                    }

                    if (release.State != EnReleaseState.Active)
                    {
                        this.Sender.Tell(
                            CrudActionResponse<Configuration>.Error(
                                new Exception("Only active releases can be marked as stable"),
                                null));
                        return;
                    }

                    if (release.IsStable != request.IsStable)
                    {
                        var error = new ErrorDescription("isStable", "The value is not changed");
                        var mutationException = new MutationException(error);
                        this.Sender.Tell(CrudActionResponse<Configuration>.Error(mutationException, null));
                        return;
                    }

                    release.IsStable = request.IsStable;
                    ds.SaveChanges();
                    this.Sender.Tell(CrudActionResponse<Configuration>.Success(release, null));
                }
            }
            catch (Exception exception)
            {
                this.Sender.Tell(CrudActionResponse<Configuration>.Error(exception, null));
            }
        }
    }
}
