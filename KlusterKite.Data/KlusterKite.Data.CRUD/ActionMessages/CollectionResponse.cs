// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionResponse.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The standard response to the <see cref="CollectionRequest" /> message
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Data.CRUD.ActionMessages
{
    using System.Collections.Generic;

    /// <summary>
    /// The standard response to the <see cref="CollectionRequest"/> message
    /// </summary>
    /// <typeparam name="TObject">The type of entity</typeparam>
    public class CollectionResponse<TObject>
    {
        /// <summary>
        /// Gets or sets the number of entities, that satisfies the query
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the items for the query
        /// </summary>
        public List<TObject> Items { get; set; }
    }
}
