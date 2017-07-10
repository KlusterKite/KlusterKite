// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeInterface.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The interface representing strict type from api
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Web.GraphQL.Publisher.GraphTypes
{
    /// <summary>
    /// The interface representing strict type from api
    /// </summary>
    public class TypeInterface : BaseTypeInterface
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeInterface"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        public TypeInterface(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }
    }
}
