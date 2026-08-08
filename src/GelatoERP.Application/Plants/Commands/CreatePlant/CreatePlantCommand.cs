

using GelatoERP.Application.Common.Interfaces;
using GelatoERP.Domain.Entities;
using MediatR;

namespace GelatoERP.Application.Plants.Commands.CreatePlant;

/// <summary>                                                                                                          
    /// DTO de respuesta para Plantas / Sucursales.                                                                        
/// </summary> 

public record PlantDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Code,
    string Address,
    bool IsProductionPlant,
    bool IsActive,
    DateTime CreatedAtUtc);
/// <summary>                                                                                                          
    /// Comando CQRS para crear una nueva Planta o Sucursal.                                                               
/// </summary> 
/// 

public record CreatePlantCommand(
    Guid TenantId,
    string Name,
    string Code,
    string Address,
    bool IsProductionPlant) : IRequest<PlantDto>;
                                                                                                                               
/// <summary>                                                                                                          
    /// Manejador para procesar la creación de una Planta.                                                                 
/// </summary> 
/// 
public class CreatePlantCommandHandler : IRequestHandler<CreatePlantCommand, PlantDto>
{
    private readonly IApplicationDbContext _context;

    public CreatePlantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PlantDto> Handle(CreatePlantCommand request, CancellationToken cancellationToken)
    {
        var plant = new Plant(
            request.TenantId,
            request.Name,
            request.Code,
            request.Address,
            request.IsProductionPlant
        );

        _context.Plants.Add(plant);
        await _context.SaveChangesAsync(cancellationToken);

        return new PlantDto(
            plant.Id,
            plant.TenantId,
            plant.Name,
            plant.Code,
            plant.Address,
            plant.IsProductionPlant,
            plant.IsActive,
            plant.CreatedAtUtc
        );
        
    }
}