using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GelatoERP.Domain.Common;
using GelatoERP.Domain.Enums;

namespace GelatoERP.Domain.Entities
{
    public class Tenant : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string TaxId { get; private set; } = string.Empty;
        public string DomainOrSlug { get; private set; } = string.Empty;
        public TenantStatus Status { get; private set; } = TenantStatus.Trialing;

         // Colecciones de Navegación de EF Core (Encapsuladas como ReadOnly)      
        private readonly List<Plant> _plants = new();
        public IReadOnlyCollection<Plant> Plants => _plants.AsReadOnly();
        private readonly List<User> _users = new();
        public IReadOnlyCollection<User> Users => _users.AsReadOnly();
        // Constructor privado para Entity Framework Core  
        private Tenant() { }
         // Factory Method (Patrón de Creación en DDD)  
         public Tenant(string name, string taxId, string domainOrSlug)
        {
            if(string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre de la empresa es obligatorio.", nameof(name));
            
            if(string.IsNullOrWhiteSpace(taxId))
                throw new ArgumentException("El identificador fiscal es obligatorio.", nameof(taxId));

            Name = name.Trim();
            TaxId = taxId.Trim();
            DomainOrSlug = domainOrSlug.Trim().ToLowerInvariant();
            Status = TenantStatus.Trialing; // Estado inicial por defecto
        }

        // Métodos de Dominio para modificar el estado (DDD en lugar de setters públicos)
        public void UpdateInfo(string name, string taxId)
        {
            Name = name.Trim();
            TaxId = taxId.Trim();
            UpdateAuditInfo();
        }

        public void ChangeStatus(TenantStatus newStatus)
        {
            Status = newStatus;
            UpdateAuditInfo();
        }

        

    }
}