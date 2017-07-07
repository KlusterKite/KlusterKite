// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestObjectNoId.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing object with id of other name
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Tests.Mock
{
    using ClusterKit.API.Attributes;

    using JetBrains.Annotations;

    /// <summary>
    /// Testing object with id of other name
    /// </summary>
    public class TestObjectNoId
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestObjectNoId"/> class.
        /// </summary>
        public TestObjectNoId()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestObjectNoId"/> class.
        /// </summary>
        /// <param name="code">
        /// The code.
        /// </param>
        public TestObjectNoId(string code)
        {
            this.Code = code;
        }

        /// <summary>
        /// Gets or sets the object code
        /// </summary>
        [UsedImplicitly]
        [DeclareField(IsKey = true)]
        public string Code { get; set; }
    }
}
