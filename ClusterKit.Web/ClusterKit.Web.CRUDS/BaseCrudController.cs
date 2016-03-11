// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseCrudController.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Base class to provide basic CRUD operation for REST service
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.CRUDS
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Akka.Actor;

    using ClusterKit.Core.Rest.ActionMessages;

    using JetBrains.Annotations;

    /// <summary>
    /// Base class to provide basic CRUD operation for REST service
    /// </summary>
    /// <typeparam name="TObject">
    /// The type of ef object
    /// </typeparam>
    /// <typeparam name="TId">
    /// The type of object identity field
    /// </typeparam>
    [UsedImplicitly]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    public abstract class BaseCrudController<TObject, TId> : ApiController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCrudController{TObject,TId}"/> class.
        /// </summary>
        /// <param name="system">
        /// The system.
        /// </param>
        protected BaseCrudController(ActorSystem system)
        {
            this.System = system;
            this.AkkaTimeout = ConfigurationUtils.GetRestTimeout(system);
        }

        /// <summary>
        /// Gets timeout for actor system requests
        /// </summary>
        protected virtual TimeSpan AkkaTimeout { get; }

        /// <summary>
        /// Gets the actor system
        /// </summary>
        protected ActorSystem System { get; }

        /// <summary>
        /// Updates or creates new Node template
        /// </summary>
        /// <param name="request">Node template data</param>
        /// <returns>Updated node template</returns>
        [HttpPost]
        [Route("")]
        public async Task<TObject> Create(TObject request)
        {
            var template =
               await
               this.System.ActorSelection(this.GetDbActorProxyPath())
                   .Ask<TObject>(
                       new RestActionMessage<TObject, TId> { ActionType = EnActionType.Create, Request = request },
                       this.AkkaTimeout);

            if (template == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return template;
        }

        /// <summary>
        /// Removes Node template
        /// </summary>
        /// <param name="id">Node template unique id</param>
        /// <returns>Execution task</returns>
        [HttpDelete]
        [Route("")]
        public async Task Delete(TId id)
        {
            var result =
                await
                this.System.ActorSelection(this.GetDbActorProxyPath())
                    .Ask<bool>(
                        new RestActionMessage<TObject, TId> { ActionType = EnActionType.Delete, Id = id },
                        this.AkkaTimeout);

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
        [HttpGet]
        [Route("")]
        public async Task<TObject> Get(TId id)
        {
            var template =
                await
                this.System.ActorSelection(this.GetDbActorProxyPath())
                    .Ask<TObject>(
                        new RestActionMessage<TObject, TId> { ActionType = EnActionType.Get, Id = id },
                        this.AkkaTimeout);

            if (template == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return template;
        }

        /// <summary>
        /// Updates or creates new Node template
        /// </summary>
        /// <param name="request">Node template data</param>
        /// <returns>Updated node template</returns>
        [HttpPatch, HttpPut]
        [Route("")]
        public async Task<TObject> Update(TObject request)
        {
            var template =
               await
               this.System.ActorSelection(this.GetDbActorProxyPath())
                   .Ask<TObject>(
                       new RestActionMessage<TObject, TId> { ActionType = EnActionType.Update, Request = request },
                       this.AkkaTimeout);

            if (template == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return template;
        }

        /// <summary>
        /// Gets akka actor path for database worker
        /// </summary>
        /// <returns>Akka actor path</returns>
        protected abstract string GetDbActorProxyPath();
    }
}