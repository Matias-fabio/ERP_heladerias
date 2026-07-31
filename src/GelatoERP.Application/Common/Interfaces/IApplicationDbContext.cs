using GelatoERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GelatoERP.Application.Common.Interfaces;

/// <summary>
/// Abstracción del DbContext para ser utilizada en los Handlers/Casos de Uso de Application.
/// Cumple con el Principio de Inversión de Dependencias (DIP).
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<Plant> Plants { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}