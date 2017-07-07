// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatasourceInnerException.cs" company="ClusterKit">
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
    public class DatasourceInnerException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatasourceInnerException"/> class.
        /// </summary>
        public DatasourceInnerException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasourceInnerException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public DatasourceInnerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasourceInnerException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="innerException">
        /// The inner exception.
        /// </param>
        public DatasourceInnerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}