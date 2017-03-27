// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SortingCondition.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The sorting condition to pass in messages
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client
{
    using JetBrains.Annotations;

    /// <summary>
    /// The sorting condition to pass in messages
    /// </summary>
    public class SortingCondition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SortingCondition"/> class.
        /// </summary>
        [UsedImplicitly]
        public SortingCondition()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SortingCondition"/> class.
        /// </summary>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <param name="direction">
        /// The direction.
        /// </param>
        [UsedImplicitly]
        public SortingCondition(string propertyName, EnDirection direction)
        {
            this.PropertyName = propertyName;
            this.Direction = direction;
        }

        /// <summary>
        /// The sorting direction
        /// </summary>
        public enum EnDirection
        {
            /// <summary>
            /// Ascending order
            /// </summary>
            Asc,

            /// <summary>
            /// Descending order
            /// </summary>
            Desc
        }

        /// <summary>
        /// Gets or sets the property name to sort by
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the sorting direction
        /// </summary>
        public EnDirection Direction { get; set; }
    }
}
