// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeTemplateRestController.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   All rest actions with <see cref="NodeTemplate" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Akka.Actor;

    using ClusterKit.Core.Rest.ActionMessages;
    using ClusterKit.NodeManager.ConfigurationSource;
    using ClusterKit.Web;

    /// <summary>
    /// All rest actions with <see cref="NodeTemplate"/>
    /// </summary>
    public class NodeTemplateRestController : ApiController
    {
        /// <summary>
        /// Path to database worker actor
        /// </summary>
        private const string UserNodemanagerDbworkerproxy = "/user/NodeManager/DbWorkerProxy";

        /// <summary>
        /// Default timeout for actor system requests
        /// </summary>
        private readonly TimeSpan akkaTimeout;

        /// <summary>
        /// Access to actor system
        /// </summary>
        private readonly ActorSystem system;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeTemplateRestController"/> class.
        /// </summary>
        /// <param name="system">
        /// The akka actor system.
        /// </param>
        public NodeTemplateRestController(ActorSystem system)
        {
            this.system = system;
            this.akkaTimeout = ConfigurationUtils.GetRestTimeout(system);
        }

        /// <summary>
        /// Removes Node template
        /// </summary>
        /// <param name="id">Node template unique id</param>
        /// <returns>Execution task</returns>
        [Route("/nodemanager/template")]
        [HttpDelete]
        public async Task Delete(int id)
        {
            var result =
                await
                this.system.ActorSelection(UserNodemanagerDbworkerproxy)
                    .Ask<bool>(
                        new RestActionMessage<NodeTemplate, int> { ActionType = EnActionType.Delete, Id = id },
                        this.akkaTimeout);

            if (!result)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// Removes Node template
        /// </summary>
        /// <param name="code">Node template string id</param>
        /// <returns>Execution task</returns>
        [Route("/nodemanager/template")]
        [HttpDelete]
        public async Task Delete(string code)
        {
            var result =
               await
               this.system.ActorSelection(UserNodemanagerDbworkerproxy)
                   .Ask<bool>(
                       new RestActionMessage<NodeTemplate, string> { ActionType = EnActionType.Delete, Id = code },
                       this.akkaTimeout);

            if (!result)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// Gets node template by its id
        /// </summary>
        /// <param name="id">Node template unique id</param>
        /// <returns>Node template</returns>
        [Route("/nodemanager/template")]
        [HttpGet]
        public async Task<NodeTemplate> Get(int id)
        {
            var template =
                await
                this.system.ActorSelection(UserNodemanagerDbworkerproxy)
                    .Ask<NodeTemplate>(
                        new RestActionMessage<NodeTemplate, int> { ActionType = EnActionType.Get, Id = id },
                        this.akkaTimeout);

            if (template == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return template;
        }

        /// <summary>
        /// Gets node template by its id
        /// </summary>
        /// <param name="code">Node template string id</param>
        /// <returns>Node template</returns>
        [Route("/nodemanager/template")]
        [HttpGet]
        public async Task<NodeTemplate> Get(string code)
        {
            var template =
                await
                this.system.ActorSelection(UserNodemanagerDbworkerproxy)
                    .Ask<NodeTemplate>(
                        new RestActionMessage<NodeTemplate, string> { ActionType = EnActionType.Get, Id = code },
                        this.akkaTimeout);

            if (template == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return template;
        }

        /// <summary>
        /// Gets the list of all node templates
        /// </summary>
        /// <param name="count">
        /// The count of elements to return.
        /// </param>
        /// <param name="skip">
        /// The count of elements to skip.
        /// </param>
        /// <returns>
        /// list of node templates
        /// </returns>
        [Route("/nodemanager/template")]
        [HttpGet]
        public async Task<List<NodeTemplate>> GetList(
            int count = 100,
            int skip = 0)
        {
            return await this.system.ActorSelection(UserNodemanagerDbworkerproxy)
                    .Ask<List<NodeTemplate>>(
                        new CollectionRequest { Count = count, Skip = skip },
                        this.akkaTimeout);
        }

        /// <summary>
        /// Updates or creates new Node template
        /// </summary>
        /// <param name="request">Node template data</param>
        /// <returns>Updated node template</returns>
        [Route("/nodemanager/template")]
        [HttpPost, HttpPut, HttpPatch]
        public async Task<NodeTemplate> Put(NodeTemplate request)
        {
            if (string.IsNullOrEmpty(request?.Code))
            {
                throw new HttpResponseException(HttpStatusCode.NoContent);
            }

            var template =
               await
               this.system.ActorSelection(UserNodemanagerDbworkerproxy)
                   .Ask<NodeTemplate>(
                       new RestActionMessage<NodeTemplate, int> { ActionType = request.Id == 0 ? EnActionType.Create : EnActionType.Update, Request = request },
                       this.akkaTimeout);

            if (template == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return template;
        }
    }
}