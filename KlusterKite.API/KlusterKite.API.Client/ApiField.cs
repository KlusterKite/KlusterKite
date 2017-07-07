// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiField.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The filed provider
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using KlusterKite.API.Attributes;
    using KlusterKite.API.Attributes.Authorization;
    using KlusterKite.Security.Attributes;

    using JetBrains.Annotations;

    /// <summary>
    /// The field provider
    /// </summary>
    public class ApiField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiField"/> class.
        /// </summary>
        [UsedImplicitly]
        [Obsolete("Can be used by serializers only", true)]
        public ApiField()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiField"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        protected ApiField(string name, EnFieldFlags flags)
        {
            this.Name = name;
            this.Flags = flags;
        }

        /// <summary>
        /// Gets or sets the list of arguments (if is set - this field becomes a method)
        /// </summary>
        [NotNull]
        [UsedImplicitly]
        public List<ApiField> Arguments { get; set; } = new List<ApiField>();

        /// <summary>
        /// Gets or sets the human-readable type description for auto-publishing
        /// </summary>
        [UsedImplicitly]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the list of defined flags
        /// </summary>
        [UsedImplicitly]
        public EnFieldFlags Flags { get; set; }

        /// <summary>
        /// Gets or sets the field name
        /// </summary>
        [UsedImplicitly]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the scalar type of the field
        /// </summary>
        [UsedImplicitly]
        public EnScalarType ScalarType { get; set; }

        /// <summary>
        /// Gets or sets the field type name
        /// </summary>
        [UsedImplicitly]
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the list of authorization requirements
        /// </summary>
        public List<AuthorizationRule> AuthorizationRules { get; set; }

        /// <summary>
        /// Gets or sets the list of log access rules
        /// </summary>
        public List<LogAccessRule> LogAccessRules { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a valid authenticated session is required for access this field
        /// </summary>
        public bool RequireAuthenticatedSession { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a valid authenticated user session is required for access this field
        /// </summary>
        public bool RequireAuthenticatedUserSession { get; set; }

        /// <summary>
        /// Creates an object containing field
        /// </summary>
        /// <param name="name">
        /// The field name
        /// </param>
        /// <param name="typeName">
        /// The field type name
        /// </param>
        /// <param name="flags">
        /// The field flags
        /// </param>
        /// <param name="arguments">
        /// The arguments (if is set - this field becomes a method)
        /// </param>
        /// <param name="description">
        /// The field description
        /// </param>
        /// <returns>
        /// The new field
        /// </returns>
        public static ApiField Object(
            [NotNull] string name,
            [NotNull] string typeName,
            EnFieldFlags flags = EnFieldFlags.Queryable,
            IEnumerable<ApiField> arguments = null,
            string description = null)
        {
            if (flags.HasFlag(EnFieldFlags.IsKey))
            {
                throw new ArgumentException("Object field can't be used as key");
            }

            return new ApiField(name, flags)
                       {
                           TypeName = typeName,
                           ScalarType = EnScalarType.None,
                           Description = description,
                           Arguments =
                               arguments != null
                                   ? new List<ApiField>(arguments)
                                   : new List<ApiField>()
                       };
        }

        /// <summary>
        /// Creates a scalar containing field
        /// </summary>
        /// <param name="name">The field name</param>
        /// <param name="type">The field type</param>
        /// <param name="flags">The field flags</param>
        /// <param name="arguments">
        /// The arguments (if is set - this field becomes a method)
        /// </param>
        /// <param name="description">
        /// The field description
        /// </param>
        /// <returns>The new field</returns>
        public static ApiField Scalar(
            [NotNull] string name,
            EnScalarType type,
            EnFieldFlags flags = EnFieldFlags.Queryable,
            IEnumerable<ApiField> arguments = null,
            string description = null)
        {
            if (type == EnScalarType.None)
            {
                throw new ArgumentException("Type cannot be None");
            }

            if (flags.HasFlag(EnFieldFlags.IsConnection))
            {
                throw new ArgumentException("Scalar field can't be used as connected objects");
            }

            return new ApiField(name, flags)
                       {
                           ScalarType = type,
                           Description = description,
                           Arguments =
                               arguments != null
                                   ? new List<ApiField>(arguments)
                                   : new List<ApiField>()
                       };
        }

        /// <summary>
        /// Fills the <see cref="AuthorizationRules"/>, <see cref="LogAccessRules"/>, 
        /// <see cref="RequireAuthenticatedSession"/> and <see cref="RequireAuthenticatedUserSession"/> 
        /// from attributes of related API member
        /// </summary>
        /// <param name="memberInfo">The related API member</param>
        public void FillAuthorizationProperties(MemberInfo memberInfo)
        {
            this.RequireAuthenticatedSession = memberInfo.GetCustomAttribute<RequireSessionAttribute>() != null;
            this.RequireAuthenticatedUserSession = memberInfo.GetCustomAttribute<RequireUserAttribute>() != null;
            this.AuthorizationRules =
                memberInfo.GetCustomAttributes<RequirePrivilegeAttribute>().Select(a => a.CreateRule()).ToList();
            this.LogAccessRules =
                memberInfo.GetCustomAttributes<LogAccessAttribute>().Select(a => a.CreateRule(memberInfo)).ToList();
        }

        /// <summary>
        /// Checks if provided context is authorized to access this field
        /// </summary>
        /// <param name="context">
        /// The request context
        /// </param>
        /// <param name="action">
        /// The performed action.
        /// </param>
        /// <returns>
        /// Whether context is authorized to access this field
        /// </returns>
        public bool CheckAuthorization(RequestContext context, EnConnectionAction action)
        {
            var accessTicket = context?.Authentication;
            if (accessTicket == null)
            {
                return !this.RequireAuthenticatedSession;
            }

            if (accessTicket.User == null && this.RequireAuthenticatedUserSession)
            {
                return false;
            }

            if (this.AuthorizationRules == null)
            {
                return true;
            }

            foreach (var rule in this.AuthorizationRules)
            {
                if ((accessTicket.User != null && rule.IgnoreOnUserPresent)
                    || (accessTicket.User == null && rule.IgnoreOnUserNotPresent))
                {
                    continue;
                }

                if (this.Flags.HasFlag(EnFieldFlags.IsConnection))
                {
                    if (!rule.ConnectionActions.HasFlag(action))
                    {
                        continue;
                    }
                }

                var rulePrivilege = rule.Privilege;
                if (rule.AddActionNameToRequiredPrivilege)
                {
                    rulePrivilege = $"{rulePrivilege}.{action}";
                }

                if (rule.Scope != EnPrivilegeScope.User)
                {
                    var clientHasPrivilege = accessTicket.ClientScope.Contains(rulePrivilege);
                    if (!clientHasPrivilege
                        && (rule.Scope == EnPrivilegeScope.Both || rule.Scope == EnPrivilegeScope.Client))
                    {
                        return false;
                    }

                    if (clientHasPrivilege
                        && (rule.Scope == EnPrivilegeScope.Any || rule.Scope == EnPrivilegeScope.Client))
                    {
                        continue;
                    }
                }

                var userHasPrivilege = accessTicket.UserScope.Contains(rulePrivilege);
                if (!userHasPrivilege)
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.Name}: {this.TypeName}";
        }

        /// <summary>
        /// Creates a clone of the current object
        /// </summary>
        /// <returns>The cloned instance</returns>
        public ApiField Clone()
        {
            return new ApiField(this.Name, this.Flags)
                       {
                           Arguments = new List<ApiField>(this.Arguments),
                           Description = this.Description,
                           ScalarType = this.ScalarType,
                           TypeName = this.TypeName,
                           AuthorizationRules =
                               this.AuthorizationRules != null
                                   ? new List<AuthorizationRule>(this.AuthorizationRules)
                                   : null,
                           RequireAuthenticatedSession =
                               this.RequireAuthenticatedSession,
                           RequireAuthenticatedUserSession =
                               this.RequireAuthenticatedUserSession,
                           LogAccessRules = this.LogAccessRules
                       };
        }
    }
}