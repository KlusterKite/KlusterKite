// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestDataActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing working with data
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.Tests.Mock
{
    using System;
    using System.Threading.Tasks;

    using ClusterKit.Data.CRUD.ActionMessages;

    using JetBrains.Annotations;

    /// <summary>
    /// Testing working with data
    /// </summary>
    [UsedImplicitly]
    public class TestDataActor : BaseCrudActor<TestDataContext>
    {
        /// <summary>
        /// The mocked data context
        /// </summary>
        private readonly IContextFactory<TestDataContext> contextFactory;

        /// <inheritdoc />
        public TestDataActor(IContextFactory<TestDataContext> contextFactory)
        {
            this.contextFactory = contextFactory;

            this.ReceiveAsync<CrudActionMessage<User, Guid>>(this.OnRequest);
            this.ReceiveAsync<CrudActionMessage<Role, Guid>>(this.OnRequest);
            this.ReceiveAsync<CollectionRequest<User>>(this.OnCollectionRequest<User, Guid>);
            this.ReceiveAsync<CollectionRequest<Role>>(this.OnCollectionRequest<Role, Guid>);
        }

        /// <inheritdoc />
        protected override Task<TestDataContext> GetContext()
        {
            return this.contextFactory.CreateContext(null, "test");
        }
    }
}
