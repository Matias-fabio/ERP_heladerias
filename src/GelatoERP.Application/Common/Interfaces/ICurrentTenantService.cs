using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GelatoERP.Application.Common.Interfaces
{
    public interface ICurrentTenantService
    {
        public Guid? TenantId { get; }
        public string? UserId { get; }
        public bool IsSuperAdmin { get; }
    }
}