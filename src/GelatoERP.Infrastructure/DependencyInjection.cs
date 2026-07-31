    using GelatoERP.Application.Common.Interfaces;                                                                         
    using GelatoERP.Infrastructure.Persistence;                                                                            
    using GelatoERP.Infrastructure.Services;                                                                               
    using Microsoft.EntityFrameworkCore;                                                                                   
    using Microsoft.Extensions.Configuration;                                                                              
    using Microsoft.Extensions.DependencyInjection;                                                                        
                                                                                                                           
    namespace GelatoERP.Infrastructure;                                                                                    
                                                                                                                           
    public static class DependencyInjection                                                                                
    {                                                                                                                      
        public static IServiceCollection AddInfrastructureServices(                                                        
            this IServiceCollection services,                                                                              
            IConfiguration configuration)                                                                                  
        {                                                                                                                  
            // 1. Registrar servicio para resolver el Tenant y Usuario de la petición actual                               
            services.AddHttpContextAccessor();                                                                             
            services.AddScoped<ICurrentTenantService, CurrentTenantService>();                                             
                                                                                                                           
            // 2. Registrar DbContext con PostgreSQL                                                                       
            var connectionString = configuration.GetConnectionString("DefaultConnection");                                 
            services.AddDbContext<ApplicationDbContext>(options =>                                                         
                options.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.       
  FullName)));                                                                                                             
                                                                                                                           
            // 3. Registrar IApplicationDbContext para la Inversión de Dependencias                                        
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());    
                                                                                                                           
            return services;                                                                                               
        }                                                                                                                  
    } 