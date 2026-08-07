using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GelatoERP.Application.Common.Interfaces;
using GelatoERP.Application.Tenants.Commands.CreateTenant;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GelatoERP.Application.Tenants.Queries.GetTenants;
 /// <summary>                                                                                                          
    /// Consulta CQRS para obtener el listado completo de Tenants.                                                         
/// </summary> 

    public record GetTenantsQuery : IRequest<List<TenantDto>>;

    public class GetTenantsQueryHandler : IRequestHandler<GetTenantsQuery, List<TenantDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTenantsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TenantDto>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Tenants
            .AsNoTracking()
            .Select(t => new TenantDto(
                t.Id,
                t.Name,
                t.TaxId,
                t.DomainOrSlug,
                t.Status,
                t.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
     