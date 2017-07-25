// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirePrivilegeAttribute.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Marks property / method published for API that access to it requires special granted privilege
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Attributes.Authorization
{
    using System;

    /// <summary>
    /// Marks property / method published for API that access to it requires special granted privilege
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePrivilegeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequirePrivilegeAttribute"/> class.
        /// </summary>
        /// <param name="privilege">
        /// The privilege.
        /// </param>
        public RequirePrivilegeAttribute(string privilege)
        {
            this.Privilege = privilege;
        }

        /// <summary>
        /// Gets the required privilege
        /// </summary>
        public string Privilege { get; }

        /// <summary>
        /// Gets or sets the scope to look for privilege
        /// </summary>
        public EnPrivilegeScope Scope { get; set; } = EnPrivilegeScope.User;

        /// <summary>
        /// Gets or sets the list of connection actions to apply attribute to
        /// </summary>
        public EnConnectionAction ConnectionActions { get; set; } = EnConnectionAction.All;

        /// <summary>
        /// Gets or sets a value indicating whether the connection action type name should be added to required privilege on authorization check
        /// </summary>
        public bool AddActionNameToRequiredPrivilege { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether that this rule will be ignored in case of valid user authentication session
        /// </summary>
        public bool IgnoreOnUserPresent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether that this rule will be ignored in case of request without user authentication session 
        /// Client application makes calls on it's on behalf
        /// </summary>
        public bool IgnoreOnUserNotPresent { get; set; }

        /// <summary>
        /// Creates a rule from attribute
        /// </summary>
        /// <returns>The authorization rule</returns>
        public AuthorizationRule CreateRule()
        {
            return new AuthorizationRule
                       {
                           ConnectionActions = this.ConnectionActions,
                           IgnoreOnUserNotPresent = this.IgnoreOnUserNotPresent,
                           IgnoreOnUserPresent = this.IgnoreOnUserPresent,
                           Privilege = this.Privilege,
                           Scope = this.Scope,
                           AddActionNameToRequiredPrivilege = this.AddActionNameToRequiredPrivilege
                       };
        }
    }
}