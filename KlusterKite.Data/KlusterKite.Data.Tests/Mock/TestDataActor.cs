// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestDataActor.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Testing working with data
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Data.Tests.Mock
{
    using System;

    using Autofac;

    using KlusterKite.Data.CRUD.ActionMessages;
    using KlusterKite.Data.EF;

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
        private readonly UniversalContextFactory contextFactory;

        /// <inheritdoc />
        public TestDataActor(UniversalContextFactory contextFactory, IComponentContext componentContext) : base(componentContext)
        {
            this.contextFactory = contextFactory;

            this.ReceiveAsync<CrudActionMessage<User, Guid>>(this.OnRequest);
            this.ReceiveAsync<CrudActionMessage<Role, Guid>>(this.OnRequest);
            this.ReceiveAsync<CollectionRequest<User>>(this.OnCollectionRequest<User, Guid>);
            this.ReceiveAsync<CollectionRequest<Role>>(this.OnCollectionRequest<Role, Guid>);
        }

        /// <inheritdoc />
        protected override TestDataContext GetContext()
        {
            return this.contextFactory.CreateContext<TestDataContext>("InMemory", null, "test");
        }
    }
}
