// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiMutation.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Describes some mutation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client
{
    using System;
    using System.Collections.Generic;

    using JetBrains.Annotations;

    /// <summary>
    /// Describes some mutation
    /// </summary>
    public class ApiMutation : ApiField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiMutation"/> class.
        /// </summary>
        [UsedImplicitly]
        [Obsolete("Can be used by serializers only", true)]
        public ApiMutation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiMutation"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        protected ApiMutation(string name, EnFieldFlags flags)
            : base(name, flags)
        {
        }

        /// <summary>
        /// The type of defined mutation
        /// </summary>
        public enum EnType
        {
            /// <summary>
            /// Some mutation that is not directly related to some node connection
            /// </summary>
            Untyped,

            /// <summary>
            /// The node creation in the connection
            /// </summary>
            ConnectionCreate,

            /// <summary>
            /// The node update in the connection
            /// </summary>
            ConnectionUpdate,

            /// <summary>
            /// The node deletion in the connection
            /// </summary>
            ConnectionDelete
        }

        /// <summary>
        /// Gets or sets the type of a mutation
        /// </summary>
        public EnType Type { get; set; }

        /// <summary>
        /// Creates the mutation from field
        /// </summary>
        /// <param name="field">The field description</param>
        /// <param name="type">The mutation type</param>
        /// <returns>The new mutation description</returns>
        public static ApiMutation CreateFromField(ApiField field, EnType type)
        {
            return new ApiMutation(field.Name, field.Flags)
            {
                Arguments = new List<ApiField>(field.Arguments),
                Description = field.Description,
                ScalarType = field.ScalarType,
                TypeName = field.TypeName,
                Type = type,
                AuthorizationRules = field.AuthorizationRules,
                LogAccessRules = field.LogAccessRules,
                RequireAuthenticatedSession = field.RequireAuthenticatedSession,
                RequireAuthenticatedUserSession = field.RequireAuthenticatedUserSession
            };
        }
    }
}