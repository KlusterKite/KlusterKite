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
        /// The field is an object key
        /// </summary>
        IsKey = 1,

        /// <summary>
        /// This is an array of connected objects
        /// </summary>
        IsConnection = 2,

        /// <summary>
        /// This is an array of objects
        /// </summary>
        IsArray = 4
    }
}