// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatasourceInnerExceptin.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   There was internal datasource exception during operation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.CRUD.Exceptions
{
    using System;

    /// <summary>
    /// There was internal datasource exception during operation
    /// </summary>
    public class DatasourceInnerExceptin : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatasourceInnerExceptin"/> class.
        /// </summary>
        public DatasourceInnerExceptin()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasourceInnerExceptin"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public DatasourceInnerExceptin(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasourceInnerExceptin"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="innerException">
        /// The inner exception.
        /// </param>
        public DatasourceInnerExceptin(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}