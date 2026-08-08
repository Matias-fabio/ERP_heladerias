using GelatoERP.Application.Common.Interfaces;
using GelatoERP.Application.Plants.Commands.CreatePlant;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GelatoERP.Application.Plants.Queries.GetPlants;

/// <summary>
    /// Consulta CQRS para obtener el listado de Plantas / Sucursales.                      
    /// Permite filtrar opcionalmente por TenantId.
/// </summary>

public record GetPlantsQuery(Guid? TenantId = null) : IRequest<List<PlantDto>>;

public class GetPlantsQueryHandler : IRequestHandler<GetPlantsQuery, List<PlantDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPlantsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PlantDto>> Handle(GetPlantsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Plants.AsNoTracking();
        if (request.TenantId.HasValue)
        {
            query = query.Where(p => p.TenantId == request.TenantId.Value);
        }

        return await query
            .Select(p => new PlantDto(
                p.Id,
                p.TenantId,
                p.Name,
                p.Code,
                p.Address,
                p.IsProductionPlant,
                p.IsActive,
                p.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
    
}

