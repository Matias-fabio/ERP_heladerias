using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GelatoERP.Domain.Common
{
    public interface ITenantEntity
    {
        public Guid TenantId { get; set; }
    }
}