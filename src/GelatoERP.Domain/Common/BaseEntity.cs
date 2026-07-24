using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GelatoERP.Domain.Common
{
    public abstract class BaseEntity
    {   
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;

        public string? CreateBy {get; protected set;}

        public DateTime LastModifiedAtUtc { get; protected set; } = DateTime.UtcNow;
        public string? LastModifiedBy {get; protected set;}
        public bool IsDeleted { get; protected set; } = false;

        public void MarkAsDeleted(string? deletedBy = null)
        {
            IsDeleted = true;
            LastModifiedAtUtc = DateTime.UtcNow;
            LastModifiedBy = deletedBy;
        }
        public void UpdateAuditInfo(string? modifiedBy = null)
        {
            LastModifiedAtUtc = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
        }
    }
}