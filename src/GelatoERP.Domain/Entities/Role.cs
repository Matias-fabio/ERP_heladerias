using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GelatoERP.Domain.Common;
using GelatoERP.Domain.Enums;

namespace GelatoERP.Domain.Entities
{
    public class Role : BaseEntity
    {
        public string Name {get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public UserRoleType RoleType { get; private set; } 

        private readonly List<UserRole> _userRoles = new();
        public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

        private Role() { } // Constructor privado para EF Core
        public Role(string name, string description, UserRoleType roleType)
        {
            Name = name.Trim();
            Description = description.Trim();
            RoleType = roleType;
        }
    }
}