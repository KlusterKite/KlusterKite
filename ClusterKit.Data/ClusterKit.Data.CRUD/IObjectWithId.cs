﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IObjectWithId.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Interface on objects with id
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.CRUD
{
    /// <summary>
    /// Interface on objects with id
    /// </summary>
    /// <typeparam name="TId">The type of id</typeparam>
    public interface IObjectWithId<TId>
    {
        /// <summary>
        /// Gets the object id
        /// </summary>
        /// <returns>The object's od</returns>
        TId GetId();
    }
}