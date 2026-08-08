using GelatoERP.Application.Common.Interfaces;
using GelatoERP.Application.Plants.Commands.CreatePlant;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GelatoERP.Application.Plants.Queries.GetPlantsById;

 /// <summary>                                                                           
    /// Consulta CQRS para obtener una Planta específica por su ID.                         
/// </summary>                                                                          
public record GetPlantByIdQuery(Guid Id) : IRequest<PlantDto?>;

public class GetPlantByIdQueryHandler : IRequestHandler<GetPlantByIdQuery, PlantDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPlantByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PlantDto?> Handle(GetPlantByIdQuery request, CancellationToken cancellationToken)
    {
        var plant = await _context.Plants
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (plant == null)
            return null;


        return new PlantDto(
            plant.Id,
            plant.TenantId,
            plant.Name,
            plant.Code,
            plant.Address,
            plant.IsProductionPlant,
            plant.IsActive,
            plant.CreatedAtUtc);
    }
}