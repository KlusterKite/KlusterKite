// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeclareFieldAttribute.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Declare method as published to api and a data mutation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Attributes
{
    using System;

    /// <summary>
    /// Declare method as published to api
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class DeclareFieldAttribute : PublishToApiAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeclareFieldAttribute"/> class.
        /// </summary>
        public DeclareFieldAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeclareFieldAttribute"/> class.
        /// </summary>
        /// <param name="description">
        /// The description.
        /// </param>
        public DeclareFieldAttribute(string description)
            : base(description)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether current property is entity identification key.
        /// </summary>
        public bool IsKey { get; set; }

        /// <summary>
        /// Gets or sets the converter type. It will be used to transfer property / method value to the API representable values
        /// </summary>
        /// <remarks>
        /// Only implementations of <see cref="IValueConverter{T}"/> can be used
        /// </remarks>
        public Type Converter { get; set; } 

        /// <summary>
        /// Gets or sets the field access type. This is not checked for methods as all published methods are considered <see cref="EnAccessFlag.Queryable"/>
        /// </summary>
        public EnAccessFlag Access { get; set; } = EnAccessFlag.All;
    }
}