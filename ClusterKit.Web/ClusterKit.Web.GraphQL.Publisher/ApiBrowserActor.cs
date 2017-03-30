// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiBrowserActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Scans cluster for the API publishers and generates the cluster schema
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Event;

    using ClusterKit.API.Client;
    using ClusterKit.API.Client.Messages;
    using ClusterKit.Core;

    using JetBrains.Annotations;

    /// <summary>
    /// Scans cluster for the API publishers and generates the cluster schema
    /// </summary>
    [UsedImplicitly]
    public class ApiBrowserActor : ReceiveActor
    {
        /// <summary>
        /// The publisher actor local path
        /// </summary>
        private const string PublisherActorLocalPath = "/user/ClusterKit/API/Publisher";

        /// <summary>
        /// The message router to mock discover requests in tests
        /// </summary>
        private readonly IMessageRouter messageRouter;

        /// <summary>
        /// The schema provider.
        /// </summary>
        private readonly SchemaProvider schemaProvider;

        /// <summary>
        /// The list of known api endpoints and their description
        /// </summary>
        private readonly Dictionary<string, ApiEndpoints> apiEndpoints = new Dictionary<string, ApiEndpoints>();

        /// <summary>
        /// The timeout for discover message response
        /// </summary>
        private readonly TimeSpan discoverTimeout;

        /// <summary>
        /// Gets the list of known active API nodes addresses and provided API names
        /// </summary>
        private readonly Dictionary<Address, List<string>> knownActiveNodes = new Dictionary<Address, List<string>>();

        /// <summary>
        /// Gets the list of unknown active API nodes addresses
        /// </summary>
        private readonly List<Address> unknownActiveNodes = new List<Address>();

        /// <summary>
        /// The discover message resending schedule to handle timeouts
        /// </summary>
        private ICancelable dicoverSchedule;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiBrowserActor"/> class.
        /// </summary>
        /// <param name="schemaProvider">
        /// The schema provider.
        /// </param>
        /// <param name="messageRouter">
        /// The message Router.
        /// </param>
        public ApiBrowserActor(SchemaProvider schemaProvider, IMessageRouter messageRouter)
        {
            this.messageRouter = messageRouter;
            this.schemaProvider = schemaProvider;
            this.discoverTimeout =
                Context.System.Settings.Config.GetTimeSpan(
                    "ClusterKit.Web.GraphQL.ApiDiscoverTimeout",
                    TimeSpan.FromSeconds(30));

            Cluster.Get(Context.System)
                .Subscribe(
                    this.Self,
                    ClusterEvent.InitialStateAsEvents,
                    typeof(ClusterEvent.MemberUp),
                    typeof(ClusterEvent.MemberRemoved));

            this.Receive<ClusterEvent.MemberUp>(
                m => m.Member.Roles.Contains("ClusterKit.API.Endpoint"),
                m => this.OnApiEndpointUp(m.Member.Address));

            this.Receive<ClusterEvent.MemberRemoved>(
                m => m.Member.Roles.Contains("ClusterKit.API.Endpoint"),
                m => this.OnApiEndpointDown(m.Member.Address));

            this.Receive<SendDiscoverRequest>(m => this.OnDiscoverRequestTimeout());
            this.Receive<List<ApiDiscoverResponse>>(m => this.OnDiscoverResponse(m));
        }

        /// <summary>
        /// Handles the api endpoint node down event
        /// </summary>
        /// <param name="address">The node address</param>
        private void OnApiEndpointDown(Address address)
        {
            bool needsSchemaRebuild = false;
            Context.GetLogger()
                .Info("{Type}: API endpoint node {NodeAddress} was removed", this.GetType().Name, address.ToString());

            List<string> apiNames;
            if (this.knownActiveNodes.TryGetValue(address, out apiNames))
            {
                foreach (var apiName in apiNames)
                {
                    ApiEndpoints endPoint;
                    if (!this.apiEndpoints.TryGetValue(apiName, out endPoint))
                    {
                        Context.GetLogger().Error(
                            "{Type}: on removing {NodeAddress} api {ApiName} provider encounterwed an error: api was not registered",
                            this.GetType().Name,
                            address.ToString(),
                            apiName);
                        continue;
                    }

                    IActorRef handler;
                    endPoint.Provider.Endpoints.TryRemove(address, out handler);

                    Version version;
                    if (!endPoint.HandlerDescriptions.TryGetValue(address, out version))
                    {
                        Context.GetLogger().Error(
                            "{Type}: on removing {NodeAddress} api {ApiName} provider encounterwed an error: api version was not registered",
                            this.GetType().Name,
                            address.ToString(),
                            apiName);
                        continue;
                    }

                    endPoint.HandlerDescriptions.Remove(address);
                    if (endPoint.HandlerDescriptions.Values.All(v => v != version))
                    {
                        endPoint.Descriptions.Remove(version);
                    }

                    if (!endPoint.Descriptions.Any())
                    {
                        needsSchemaRebuild = true;
                        this.apiEndpoints.Remove(apiName);
                        Context.GetLogger()
                            .Info("{Type}: API {ApiName} is now unsupported", this.GetType().Name, apiName);
                    }
                    else if (endPoint.Descriptions.Keys.Max() < version)
                    {
                        needsSchemaRebuild = true;
                    }
                }
            }

            this.knownActiveNodes.Remove(address);
            this.unknownActiveNodes.Remove(address);

            if (needsSchemaRebuild)
            {
                this.RebuildSchema();
            }
        }

        /// <summary>
        /// Handles the new api endpoint node up event
        /// </summary>
        /// <param name="address">The node address</param>
        private void OnApiEndpointUp(Address address)
        {
            Context.GetLogger()
                .Info("{Type}: new API endpoint node {NodeAddress} discovered", this.GetType().Name, address.ToString());

            this.unknownActiveNodes.Add(address);
            this.messageRouter.Tell(address, PublisherActorLocalPath, new ApiDiscoverRequest(), this.Self);

            if (this.dicoverSchedule == null)
            {
                this.dicoverSchedule = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(
                    this.discoverTimeout,
                    this.discoverTimeout,
                    this.Self,
                    new SendDiscoverRequest(),
                    this.Self);
            }
        }

        /// <summary>
        /// Handles discover request timeout
        /// </summary>
        private void OnDiscoverRequestTimeout()
        {
            if (this.unknownActiveNodes.Count == 0)
            {
                this.dicoverSchedule?.Cancel();
                this.dicoverSchedule = null;
                return;
            }

            foreach (var node in this.unknownActiveNodes)
            {
                this.messageRouter.Tell(node, PublisherActorLocalPath, new ApiDiscoverRequest(), this.Self);
            }
        }

        /// <summary>
        /// Api node responded with api descriptions
        /// </summary>
        /// <param name="descriptions">
        /// The API descriptions on the node.
        /// </param>
        private void OnDiscoverResponse(List<ApiDiscoverResponse> descriptions)
        {
            var nodeAddress = this.Sender.Path.Address;
            if (nodeAddress.Host == null)
            {
                // supposed this is local address
                nodeAddress = Cluster.Get(Context.System).SelfAddress;
            }

            if (!this.unknownActiveNodes.Remove(nodeAddress))
            {
                // node was removed or this is secondary response after a timeout
                return;
            }

            this.knownActiveNodes.Add(nodeAddress, descriptions.Select(d => d.Description.ApiName).ToList());

            Context.GetLogger()
                .Info(
                    "{Type}: API endpoint {NodeAddress} node handles {ApiNamesList}",
                    this.GetType().Name,
                    nodeAddress.ToString(),
                    string.Join(", ", descriptions.Select(d => d.Description.ApiName)));

            this.AddHandlers(nodeAddress, descriptions);
        }

        /// <summary>
        /// Performs current schema rebuild
        /// </summary>
        private void RebuildSchema()
        {
            if (!this.apiEndpoints.Any())
            {
                this.schemaProvider.CurrentSchema = null;
                Context.GetLogger().Info("{Type}: There is now api to support", this.GetType().Name);
                return;
            }

            var descriptions =
                this.apiEndpoints.Values.Select(
                    d =>
                        new
                            {
                                Description = d.Descriptions.Values.OrderByDescending(ad => ad.Version).FirstOrDefault(),
                                d.Provider
                            }).Where(d => d.Description != null).ToList();

            // renews the provider descriptions. This descriptions are used only in schema generation process
            foreach (var description in descriptions)
            {
                description.Provider.Description = description.Description;
            }

            try
            {
                var schema = SchemaGenerator.Generate(descriptions.Select(d => d.Provider).Cast<ApiProvider>().ToList());
                this.schemaProvider.CurrentSchema = schema;
            }
            catch (Exception exception)
            {
                Context.GetLogger().Error(exception, "{Type}: Failed to build GraphQL schema", this.GetType().Name);
            }
        }

        /// <summary>
        /// Adds the new  api handlers endpoints
        /// </summary>
        /// <param name="address">
        /// The handler node's address.
        /// </param>
        /// <param name="descriptions">
        /// The list of handles
        /// </param>
        private void AddHandlers(Address address, List<ApiDiscoverResponse> descriptions)
        {
            bool needsSchemaRebuild = false;
            foreach (var description in descriptions)
            {
                ApiEndpoints endpoints;
                if (!this.apiEndpoints.TryGetValue(description.Description.ApiName, out endpoints))
                {
                    needsSchemaRebuild = true;
                    endpoints = new ApiEndpoints(
                        description.Description,
                        Context.System);
                    this.apiEndpoints.Add(description.Description.ApiName, endpoints);
                    Context.GetLogger()
                        .Info(
                            "{Type}: API {ApiName} is now supported",
                            this.GetType().Name,
                            description.Description.ApiName);
                }

                var currentMaximumApiVersion = endpoints.Descriptions.Any()
                                                   ? endpoints.Descriptions.Keys.Max()
                                                   : new Version();

                if (description.Description.Version > currentMaximumApiVersion)
                {
                    needsSchemaRebuild = true;
                }

                if (!endpoints.Descriptions.ContainsKey(description.Description.Version))
                {
                    endpoints.Descriptions.Add(description.Description.Version, description.Description);
                }

                endpoints.Provider.Endpoints.TryAdd(address, description.Handler);
                endpoints.HandlerDescriptions[address] = description.Description.Version;
            }

            if (needsSchemaRebuild)
            {
                this.RebuildSchema();
            }
        }

        /// <summary>
        /// Current data of api handlers
        /// </summary>
        private class ApiEndpoints
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ApiEndpoints"/> class.
            /// </summary>
            /// <param name="description">
            /// The description.
            /// </param>
            /// <param name="system">
            /// The actor system
            /// </param>
            public ApiEndpoints(ApiDescription description, ActorSystem system)
            {
                this.Descriptions.Add(description.Version, description);
                this.Provider = new ActorSystemApiProvider(description, system);
            }

            /// <summary>
            /// Gets the list of versioned current api descriptions
            /// </summary>
            public Dictionary<Version, ApiDescription> Descriptions { get; } = new Dictionary<Version, ApiDescription>();

            /// <summary>
            /// Gets the description of handlers supported API version
            /// </summary>
            public Dictionary<Address, Version> HandlerDescriptions { get; } = new Dictionary<Address, Version>();

            /// <summary>
            /// Gets the api provider provider
            /// </summary>
            public ActorSystemApiProvider Provider { get; }
        }

        /// <summary>
        /// Auto message to resend discover request
        /// </summary>
        private class SendDiscoverRequest
        {
        }
    }
}