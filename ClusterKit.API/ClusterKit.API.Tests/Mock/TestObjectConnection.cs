// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestObjectConnection.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The <see cref="TestObject" /> connection provider
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Tests.Mock
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using ClusterKit.API.Client;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The <see cref="TestObject"/> connection provider
    /// </summary>
    public class TestObjectConnection : INodeConnection<TestObject, Guid>
    {
        /// <summary>
        /// Gets the stored objects (virtual database)
        /// </summary>
        private readonly Dictionary<Guid, TestObject> objects;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestObjectConnection"/> class.
        /// </summary>
        /// <param name="objects">
        /// The initial objects state.
        /// </param>
        public TestObjectConnection(Dictionary<Guid, TestObject> objects)
        {
            this.objects = objects;
        }

        /// <inheritdoc />
        public Task<MutationResult<TestObject>> Create(TestObject newNode)
        {
            if (newNode == null)
            {
                var errors = new List<ErrorDescription>
                                 {
                                     new ErrorDescription(null, "Create failed"),
                                     new ErrorDescription(null, "object data was not provided")
                                 };

                return Task.FromResult(new MutationResult<TestObject> { Errors = errors });
            }

            if (string.IsNullOrWhiteSpace(newNode.Name))
            {
                var errors = new List<ErrorDescription>
                                 {
                                     new ErrorDescription(null, "Create failed"),
                                     new ErrorDescription("name", "name should be set")
                                 };

                return Task.FromResult(new MutationResult<TestObject> { Errors = errors });
            }

            newNode.Uid = Guid.NewGuid();
            this.objects.Add(newNode.Uid, newNode);
            return Task.FromResult(new MutationResult<TestObject> { Result = newNode });
        }

        /// <inheritdoc />
        public Task<MutationResult<TestObject>> Delete(Guid id)
        {
            TestObject obj;
            if (!this.objects.TryGetValue(id, out obj))
            {
                var errors = new List<ErrorDescription>
                                 {
                                     new ErrorDescription(null, "Delete failed"),
                                     new ErrorDescription("id", "Node not found")
                                 };

                return Task.FromResult(new MutationResult<TestObject> { Errors = errors });
            }

            this.objects.Remove(id);
            return Task.FromResult(new MutationResult<TestObject> { Result = obj });
        }

        /// <inheritdoc />
        public Task<TestObject> GetById(Guid id)
        {
            TestObject obj;
            if (this.objects.TryGetValue(id, out obj))
            {
                return Task.FromResult(obj);
            }

            throw new Exception("not found");
        }

        /// <inheritdoc />
        public Task<QueryResult<TestObject>> Query(
            Expression<Func<TestObject, bool>> filter,
            Expression<Func<IQueryable<TestObject>, IOrderedQueryable<TestObject>>> sort,
            int? limit,
            int? offset)
        {
            var query = this.objects.Values.AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var count = query.Count();
            if (sort != null)
            {
                query = sort.Compile().Invoke(query);
            }

            if (offset.HasValue)
            {
                query = query.Skip(offset.Value);
            }

            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }

            return Task.FromResult(new QueryResult<TestObject> { Count = count, Items = query });
        }

        /// <inheritdoc />
        public Task<MutationResult<TestObject>> Update(Guid id, TestObject newNode, ApiRequest request)
        {
            TestObject obj;
            if (!this.objects.TryGetValue(id, out obj))
            {
                var errors = new List<ErrorDescription>
                                 {
                                     new ErrorDescription(null, "Update failed"),
                                     new ErrorDescription("id", "Node not found")
                                 };

                return Task.FromResult(new MutationResult<TestObject> { Errors = errors });
            }

            var description = request?.Arguments.Property("newNode").Value as JObject;
            if (description == null)
            {
                var errors = new List<ErrorDescription>
                                 {
                                     new ErrorDescription(null, "Update failed"),
                                     new ErrorDescription(null, "Update description not found")
                                 };

                return Task.FromResult(new MutationResult<TestObject> { Errors = errors });
            }

            if (description.Property("uid") != null)
            {
                if (newNode.Uid != obj.Uid && this.objects.ContainsKey(newNode.Uid))
                {
                    var errors = new List<ErrorDescription>
                                     {
                                         new ErrorDescription(null, "Update failed"),
                                         new ErrorDescription("uid", "Duplicate key")
                                     };

                    return Task.FromResult(new MutationResult<TestObject> { Errors = errors });
                }

                obj.Uid = newNode.Uid;
            }

            if (description.Property("name") != null)
            {
                obj.Name = newNode.Name;
            }

            if (description.Property("value") != null)
            {
                obj.Value = newNode.Value;
            }

            this.objects.Remove(id);
            this.objects[obj.Uid] = obj;

            return Task.FromResult(new MutationResult<TestObject> { Result = obj });
        }
    }
}