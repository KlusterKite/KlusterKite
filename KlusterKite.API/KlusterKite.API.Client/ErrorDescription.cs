// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorDescription.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The error description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Client
{
    using KlusterKite.API.Attributes;

    using JetBrains.Annotations;

    /// <summary>
    /// The error description
    /// </summary>
    [ApiDescription(Description = "The mutation error description", Name = "ErrorDescription")]
    public class ErrorDescription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorDescription"/> class.
        /// </summary>
        [UsedImplicitly]
        public ErrorDescription()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorDescription"/> class.
        /// </summary>
        /// <param name="field">
        /// The field.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        public ErrorDescription(string field, string message)
        {
            this.Field = field;
            this.Message = message;
        }

        /// <summary>
        /// Gets or sets the error number
        /// </summary>
        [UsedImplicitly]
        [DeclareField(Description = "The error number", IsKey = true)]
        public int Number { get; set; }

        /// <summary>
        /// Gets or sets the related field name
        /// </summary>
        [UsedImplicitly]
        [DeclareField(Description = "The related field name")]
        public string Field { get; set; }

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        [UsedImplicitly]
        [DeclareField(Description = "The error message")]
        public string Message { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.Field}: {this.Message}";
        }
    }
}