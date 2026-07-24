using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GelatoERP.Domain.Common;

namespace GelatoERP.Domain.Entities
{
    public class Plant : BaseEntity, ITenantEntity
    {
        public Guid TenantId { get; set; }
        public string Name {get; private set; } = string.Empty;
        public string Code {get; private set; } = string.Empty;
        public string Address {get; private set; } = string.Empty;
        public bool IsProductionPlant {get; private set; } = true;
        public bool IsActive {get; private set; } = true;
        public Tenant Tenant {get; private set;} = null!;

        private Plant() { } // Constructor privado para EF Core

        public Plant(Guid tenantId, string name, string code, string address, bool isProductionPlant)
        {
           if(string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre de la planta es obligatorio.", nameof(name));

            TenantId = tenantId;                                                                                                                   
            Name = name.Trim();                                                                                                                    
            Code = code.Trim().ToUpperInvariant();                                                                                                 
            Address = address.Trim();                                                                                                              
            IsProductionPlant = isProductionPlant;                                                                                                 
            IsActive = true;           
        }

        public void Update(string name, string code, string address, bool isProductionPlant)
        {
            Name = name.Trim();
            Code = code.Trim().ToUpperInvariant();
            Address = address.Trim();
            IsProductionPlant = isProductionPlant;
            UpdateAuditInfo();
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdateAuditInfo();
        }
    }
        
}
