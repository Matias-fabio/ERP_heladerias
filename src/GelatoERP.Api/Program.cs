                                                                                                                           
    using GelatoERP.Api.Middlewares;                                                                                       
    using GelatoERP.Application;                                                                                           
    using GelatoERP.Infrastructure;                                                                                        
    using Microsoft.OpenApi.Models;                                                                                        
                                                                                                                           
    var builder = WebApplication.CreateBuilder(args);                                                                      
                                                                                                                           
    // 1. Agregar servicios de las capas de Aplicación e Infraestructura                                                   
    builder.Services.AddApplicationServices();                                                                             
    builder.Services.AddInfrastructureServices(builder.Configuration);                                                     
                                                                                                                           
    // 2. Agregar controladores y manejador de excepciones estandarizado (.NET 8 ProblemDetails)                           
    builder.Services.AddControllers();                                                                                     
    builder.Services.AddExceptionHandler<CustomExceptionHandler>();                                                        
    builder.Services.AddProblemDetails();                                                                                  
                                                                                                                           
    // 3. Configurar Swagger/OpenAPI con soporte para el Header X-Tenant-Id                                                
    builder.Services.AddEndpointsApiExplorer();                                                                            
    builder.Services.AddSwaggerGen(options =>                                                                              
    {                                                                                                                      
        options.SwaggerDoc("v1", new OpenApiInfo                                                                           
        {                                                                                                                  
            Title = "GelatoERP API",                                                                                       
            Version = "v1",                                                                                                
            Description = "API del ERP Multi-Tenant para Heladerías y Fábricas de Helado"                                  
        });                                                                                                                
                                                                                                                           
        // Agregar Header X-Tenant-Id a la interfaz visual de Swagger                                                      
        options.AddSecurityDefinition("TenantId", new OpenApiSecurityScheme                                                
        {                                                                                                                  
            Name = "X-Tenant-Id",                                                                                          
            Type = SecuritySchemeType.ApiKey,                                                                              
            In = ParameterLocation.Header,                                                                                 
            Description = "ID del Tenant (Guid) para la heladería/sucursal actual"                                         
        });                                                                                                                
                                                                                                                           
        options.AddSecurityRequirement(new OpenApiSecurityRequirement                                                      
        {                                                                                                                  
            {                                                                                                              
                new OpenApiSecurityScheme                                                                                  
                {                                                                                                          
                    Reference = new OpenApiReference                                                                       
                    {                                                                                                      
                        Type = ReferenceType.SecurityScheme,                                                               
                        Id = "TenantId"                                                                                    
                    }                                                                                                      
                },                                                                                                         
                Array.Empty<string>()                                                                                      
            }                                                                                                              
        });                                                                                                                
    });                                                                                                                    
                                                                                                                           
    var app = builder.Build();                                                                                             
                                                                                                                           
    // 4. Configurar el pipeline HTTP                                                                                      
    app.UseExceptionHandler();                                                                                             
                                                                                                                           
    if (app.Environment.IsDevelopment())                                                                                   
    {                                                                                                                      
        app.UseSwagger();                                                                                                  
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GelatoERP API v1"));                          
    }                                                                                                                      
                                                                                                                           
    app.UseHttpsRedirection();                                                                                             
    app.UseAuthorization();                                                                                                
    app.MapControllers();                                                                                                  
                                                                                                                           
    app.Run();