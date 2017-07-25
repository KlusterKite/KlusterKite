// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestObject.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Test object
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Tests.Mock
{
    using System;
    using System.Collections.Generic;

    using JetBrains.Annotations;

    using KlusterKite.API.Attributes;

    /// <summary>
    /// Test object
    /// </summary>
    [ApiDescription(Description = "test object", Name = "TestObject")]
    public class TestObject
    {
        /// <summary>
        /// The type of object as enum
        /// </summary>
        [ApiDescription(Description = "The type of object as enum", Name = "EnObjectType")]
        public enum EnObjectType
        {
            /// <summary>
            /// This is good object
            /// </summary>
            [ApiDescription(Description = "This is good object")]
            Good = 1,

            /// <summary>
            /// This is bad object
            /// </summary>
            [ApiDescription(Description = "This is bad object")]
            Bad = 2
        }

        /// <summary>
        /// Gets or sets the name of the object
        /// </summary>
        [DeclareField]
        [UsedImplicitly]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the object uid
        /// </summary>
        [UsedImplicitly]
        [DeclareField(IsKey = true)]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets some value
        /// </summary>
        [DeclareField]
        [UsedImplicitly]
        public decimal Value { get; set; }

        /// <summary>
        /// Gets or sets some value
        /// </summary>
        [DeclareField]
        [UsedImplicitly]
        public EnObjectType Type { get; set; } = EnObjectType.Good;

        /// <summary>
        /// Gets or sets the nested object
        /// </summary>
        [DeclareField]
        [UsedImplicitly]
        public TestObject Recursion { get; set; }

        /// <summary>
        /// Gets or sets the list of nested object
        /// </summary>
        [DeclareField]
        [UsedImplicitly]
        public List<TestObject> RecursionArray { get; set; }
    }
}