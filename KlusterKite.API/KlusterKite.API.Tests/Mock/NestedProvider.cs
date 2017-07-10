// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NestedProvider.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Nested API
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Tests.Mock
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using KlusterKite.API.Attributes;
    using KlusterKite.API.Client;

    /// <summary>
    /// Nested API
    /// </summary>
    [UsedImplicitly]
    [ApiDescription(Description = "Nested API", Name = "NestedProvider")]
    public class NestedProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NestedProvider"/> class.
        /// </summary>
        public NestedProvider()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NestedProvider"/> class.
        /// </summary>
        /// <param name="objects">
        /// The list of pre-stored objects.
        /// </param>
        public NestedProvider(List<TestObject> objects)
        {
            this.Connection = new TestObjectConnection(
                objects?.ToDictionary(o => o.Id) ?? new Dictionary<Guid, TestObject>());
        }

        /// <summary>
        /// Gets or sets the key field
        /// </summary>
        [DeclareField(IsKey = true)]
        public Guid Uid { get; set; }

        /// <summary>
        /// Gets or sets the field that is visible only in "input" objects
        /// </summary>
        [DeclareField(Access = EnAccessFlag.Writable)]
        [UsedImplicitly]
        public string ArgumentField { get; set; }

        /// <summary>
        /// Async scalar field
        /// </summary>
        [DeclareField]
        [UsedImplicitly]
        public Task<string> AsyncScalarField => Task.FromResult("AsyncScalarField");

        /// <summary>
        /// Gets the test objects connection
        /// </summary>
        [DeclareConnection(CanCreate = true, CanDelete = true, CanUpdate = true)]
        [UsedImplicitly]
        public TestObjectConnection Connection { get; }

        /// <summary>
        /// Gets or sets the field that is not visible "input" objects, but can be queried
        /// </summary>
        [DeclareField(Access = EnAccessFlag.Queryable)]
        [UsedImplicitly]
        public string ReadOnlyField { get; set; }

        /// <summary>
        /// Gets or sets the sync scalar field
        /// </summary>
        [DeclareField]
        [UsedImplicitly]
        public string SyncScalarField { get; set; } = "SyncScalarField";

        /// <summary>
        /// Gets this for infinite recursive requests
        /// </summary>
        [DeclareField]
        [UsedImplicitly]
        public NestedProvider This => this;

        /// <summary>
        /// Test nested mutation
        /// </summary>
        /// <param name="name">The new name of current test object</param>
        /// <returns>Test object after mutation</returns>
        [DeclareMutation]
        [UsedImplicitly]
        public MutationResult<bool> Check(string name)
        {
            return new MutationResult<bool> { Result = true };
        }

        /// <summary>
        /// Test nested mutation
        /// </summary>
        /// <param name="name">The new name of current test object</param>
        /// <returns>Test object after mutation</returns>
        [DeclareMutation]
        [UsedImplicitly]
        public TestObject SetName(string name)
        {
            return new TestObject { Name = name };
        }

        /// <summary>
        /// Tests the object updater mutation
        /// </summary>
        /// <param name="input">The input object</param>
        /// <param name="request">The request</param>
        /// <returns>The output object</returns>
        [DeclareMutation]
        [UsedImplicitly]
        public TestObject SetObject(TestObject input, ApiRequest request)
        {
            var initialObject = new TestObject
                                    {
                                        Name = "SourceName",
                                        Type = TestObject.EnObjectType.Good,
                                        Value = 1M,
                                        Recursion =
                                            new TestObject
                                                {
                                                    Name = "NestedSourceName",
                                                    Type = TestObject.EnObjectType.Good,
                                                    Value = 10M,
                                                },
                                        RecursionArray =
                                            new List<TestObject>
                                                {
                                                    new TestObject
                                                        {
                                                            Name =
                                                                "NestedListedSourceName",
                                                            Type =
                                                                TestObject
                                                                    .EnObjectType
                                                                    .Good,
                                                            Value = 100M,
                                                        }
                                                }
                                    };

            DataUpdater<TestObject>.Update(initialObject, input, request, "input");
            return initialObject;
        }
    }
}