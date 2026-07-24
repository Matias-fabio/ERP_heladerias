using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GelatoERP.Domain.Common;

namespace GelatoERP.Domain.Entities
{
    public class User : BaseEntity, ITenantEntity
    {
       public Guid TenantId { get; set; } 
       public string FirstName { get; private set; } = string.Empty;
       public string LastName { get; private set; } = string.Empty;
       public string Email {get; private set;} = string.Empty;
       public string PasswordHash {get; private set;} = string.Empty;
       public bool IsActive {get; private set;} = true;
                                                                                                                                                    
        // Planta/Sucursal por defecto asignada al usuario (Opcional)                                                                              
        public Guid? AssignedPlantId { get; private set; }                                                                                         
        public Plant? AssignedPlant { get; private set; }                                                                                          
                                                                                                                                                   
        // Propiedad de Navegación hacia la Heladería                                                                                              
        public Tenant Tenant { get; private set; } = null!;                                                                                        
                                                                                                                                                   
        // Colección de Roles del Usuario                                                                                                          
        private readonly List<UserRole> _userRoles = new();                                                                                        
        public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();                                                                 
                                                                                                                                                   
        private User() { }                                                                                                                         
                                                                                                                                                   
        public User(Guid tenantId, string firstName, string lastName, string email, string passwordHash, Guid? assignedPlantId = null)             
        {                                                                                                                                          
            if (string.IsNullOrWhiteSpace(email))                                                                                                  
                throw new ArgumentException("El email es obligatorio.", nameof(email));                                                            
                                                                                                                                                   
            TenantId = tenantId;                                                                                                                   
            FirstName = firstName.Trim();                                                                                                          
            LastName = lastName.Trim();                                                                                                            
            Email = email.Trim().ToLowerInvariant(); // Normalizamos siempre a minúsculas                                                          
            PasswordHash = passwordHash;                                                                                                           
            AssignedPlantId = assignedPlantId;                                                                                                     
            IsActive = true;                                                                                                                       
        }                                                                                                                                          
                                                                                                                                                   
        public string FullName => $"{FirstName} {LastName}";                                                                                       
                                                                                                                                                   
        public void UpdateProfile(string firstName, string lastName, Guid? assignedPlantId)                                                        
        {                                                                                                                                          
            FirstName = firstName.Trim();                                                                                                          
            LastName = lastName.Trim();                                                                                                            
            AssignedPlantId = assignedPlantId;                                                                                                     
            UpdateAuditInfo();                                                                                                                     
        }                                                                                                                                          
                                                                                                                                                   
        public void SetPasswordHash(string newPasswordHash)                                                                                        
        {                                                                                                                                          
            if (string.IsNullOrWhiteSpace(newPasswordHash))                                                                                        
                throw new ArgumentException("El hash de la contraseña no puede estar vacío.", nameof(newPasswordHash));                            
                                                                                                                                                   
            PasswordHash = newPasswordHash;                                                                                                        
            UpdateAuditInfo();                                                                                                                     
        }                                                                                                                                          
                                                                                                                                                   
        public void Deactivate()                                                                                                                   
        {                                                                                                                                          
            IsActive = false;                                                                                                                      
            UpdateAuditInfo();                                                                                                                     
        }                                                                                                                                          
                                                                                                                                                   
        public void Activate()                                                                                                                     
        {                                                                                                                                          
            IsActive = true;                                                                                                                       
            UpdateAuditInfo();                                                                                                                     
        }                                  
    }
}