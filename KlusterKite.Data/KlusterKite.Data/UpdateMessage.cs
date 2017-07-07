// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateMessage.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   NodeTemplate update notification
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Data
{
    using KlusterKite.Data.CRUD.ActionMessages;

    /// <summary>
    /// NodeTemplate update notification
    /// </summary>
    /// <typeparam name="TObject">
    /// The type of ef object
    /// </typeparam>
    public class UpdateMessage<TObject>
    {
        /// <summary>
        /// Gets or sets update action
        /// </summary>
        public EnActionType ActionType { get; set; }

        /// <summary>
        /// Gets or sets object after modification
        /// </summary>
        public TObject NewObject { get; set; }

        /// <summary>
        /// Gets or sets object before modification
        /// </summary>
        public TObject OldObject { get; set; }
    }
}