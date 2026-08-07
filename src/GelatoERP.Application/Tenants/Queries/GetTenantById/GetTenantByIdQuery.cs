using GelatoERP.Application.Common.Interfaces;                                                                         
    using GelatoERP.Application.Tenants.Commands.CreateTenant;                                                             
    using MediatR;                                                                                                         
    using Microsoft.EntityFrameworkCore;                                                                                   
                                                                                                                           
    namespace GelatoERP.Application.Tenants.Queries.GetTenantById;                                                         
                                                                                                                           
    /// <summary>                                                                                                          
    /// Consulta CQRS para obtener un Tenant específico por su ID.                                                         
    /// </summary>                                                                                                         
    public record GetTenantByIdQuery(Guid Id) : IRequest<TenantDto?>;                                                      
                                                                                                                           
    public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, TenantDto?>                               
    {                                                                                                                      
        private readonly IApplicationDbContext _context;                                                                   
                                                                                                                           
        public GetTenantByIdQueryHandler(IApplicationDbContext context)                                                    
        {                                                                                                                  
            _context = context;                                                                                            
        }                                                                                                                  
                                                                                                                           
        public async Task<TenantDto?> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)              
        {                                                                                                                  
            var tenant = await _context.Tenants                                                                            
                .AsNoTracking()                                                                                            
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);                                          
                                                                                                                           
            if (tenant == null)                                                                                            
                return null;                                                                                               
                                                                                                                           
            return new TenantDto(                                                                                          
                tenant.Id,                                                                                                 
                tenant.Name,                                                                                               
                tenant.TaxId,                                                                                              
                tenant.DomainOrSlug,                                                                                       
                tenant.Status,                                                                                             
                tenant.CreatedAtUtc);                                                                                      
        }                                                                                                                  
    }                                   