using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GelatoERP.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace GelatoERP.Infrastructure.Services
{
    public class CurrentTenantService : ICurrentTenantService
    {
        public Guid? TenantId { get; }
        public string? UserId { get; }
        public bool IsSuperAdmin { get; }

        public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
        {
            var user = httpContextAccessor.HttpContext?.User;

            if(user is null) return;

            UserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = user.FindFirstValue(ClaimTypes.Role);
            IsSuperAdmin = role == "SuperAdmin";

            var tenantClaim = user.FindFirstValue("TenantId");
            if (Guid.TryParse(tenantClaim, out var tenantId))
            {
                TenantId = tenantId;
            }
        }
    }
}