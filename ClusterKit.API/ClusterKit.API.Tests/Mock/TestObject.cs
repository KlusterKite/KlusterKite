// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestObject.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Test object
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Tests.Mock
{
    using System;

    using ClusterKit.API.Client.Attributes;

    using JetBrains.Annotations;

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
            Good = 1,

            /// <summary>
            /// This is bad object
            /// </summary>
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
    }
}