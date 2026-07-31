using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GelatoERP.Application.Common.Interfaces;
using GelatoERP.Domain.Common;
using GelatoERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GelatoERP.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly ICurrentTenantService _currentTenantService;
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ICurrentTenantService currentTenantService) 
            : base(options)
        {
            _currentTenantService = currentTenantService;
        }
 
        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<Plant> Plants => Set<Plant>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // 1. Configurar clave primaria compuesta para la tabla intermedia UserRole
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });
            // 2. Aplicar Global Query Filters a las entidades
            foreach(var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                    var body = System.Linq.Expressions.Expression.Equal(
                        System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted)),
                        System.Linq.Expressions.Expression.Constant(false));
                    
                    var lambda = System.Linq.Expressions.Expression.Lambda(body, parameter);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);

                }

                //filtro automatico por tenantId
                if(typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                    
                    var tenantIdProperty = System.Linq.Expressions.Expression.Property(parameter, nameof(ITenantEntity.TenantId));

                    var currentTenantIdValue = System.Linq.Expressions.Expression.Property(  
                    System.Linq.Expressions.Expression.Constant(_currentTenantService),nameof(ICurrentTenantService.TenantId));  

                    var body = System.Linq.Expressions.Expression.Equal(
                        tenantIdProperty,
                        System.Linq.Expressions.Expression.Convert(currentTenantIdValue, typeof(Guid)));

                    var lambda = System.Linq.Expressions.Expression.Lambda(body, parameter);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }


        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Property(e => e.CreateBy).CurrentValue = _currentTenantService.UserId;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdateAuditInfo(_currentTenantService.UserId);
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

    }
}