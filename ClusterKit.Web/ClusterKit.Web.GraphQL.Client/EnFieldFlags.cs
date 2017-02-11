// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnFieldFlags.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The list of field flags
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Client
{
    using System;

    /// <summary>
    /// The list of field flags
    /// </summary>
    [Flags]
    public enum EnFieldFlags
    {
        /// <summary>
        /// No special flags were set
        /// </summary>
        None = 0,

        /// <summary>
        /// Field type is a primitive type
        /// </summary>
        IsScalar = 1,

        /// <summary>
        /// This is an array of elements
        /// </summary>
        IsArray = 2,

        /// <summary>
        /// The field is an object key
        /// </summary>
        IsKey = 4
    }
}