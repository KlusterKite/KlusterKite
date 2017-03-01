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

    /// <summary>
    /// Test object
    /// </summary>
    public class TestObject
    {
        /// <summary>
        /// The type of object as enum
        /// </summary>
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
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the object uid
        /// </summary>
        [DeclareField(IsKey = true)]
        public Guid Uid { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets some value
        /// </summary>
        [DeclareField]
        public decimal Value { get; set; }

        /// <summary>
        /// Gets or sets some value
        /// </summary>
        [DeclareField]
        public EnObjectType Type { get; set; } = EnObjectType.Good;
    }
}