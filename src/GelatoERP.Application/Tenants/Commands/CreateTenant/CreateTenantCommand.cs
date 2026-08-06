using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GelatoERP.Application.Common.Interfaces;
using GelatoERP.Domain.Entities;
using GelatoERP.Domain.Enums;
using MediatR;

namespace GelatoERP.Application.Tenants.Commands.CreateTenant;
 /// <summary>                                                                                                          
    /// DTO de respuesta devuelto tras crear un Tenant.                                                                    
/// </summary>
public record TenantDto(

    Guid Id,
    string Name,
    string TaxId,
    string DomainOrSlug,
    TenantStatus Status,
    DateTime CreatedAtUtc);

 /// <summary>                                                                                                          
    /// Comando MediatR para solicitar la creación de un nuevo Tenant.                                                     
/// </summary> 

public record CreateTenantCommand(
    string Name,
    string TaxId,
    string DomainOrSlug
) : IRequest<TenantDto>;

/// <summary>                                                                                                          
    /// Manejador de la lógica de negocio para procesar CreateTenantCommand.                                               
/// </summary>

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, TenantDto>
{
    private readonly IApplicationDbContext _context;
    
    public CreateTenantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TenantDto> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        // Lógica para crear un nuevo Tenant en la base de datos
        var tenant = new Tenant(
            request.Name,                                                                                              
            request.TaxId,                                                                                             
            request.DomainOrSlug);

        _context.Tenants.Add(tenant);
        
        await _context.SaveChangesAsync(cancellationToken);

        return new TenantDto(
            tenant.Id,
            tenant.Name,
            tenant.TaxId,
            tenant.DomainOrSlug,
            tenant.Status,
            tenant.CreatedAtUtc);
    }
}