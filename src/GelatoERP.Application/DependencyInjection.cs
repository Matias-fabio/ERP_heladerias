using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentValidation;
using GelatoERP.Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GelatoERP.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            //Registrar MediaTR para manajar Commands y Queries (CQRS)
            services.AddMediatR(cfg => {                                                                                   
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));                                         
            });

            // 2. Registrar automáticamente todos los validadores de FluentValidation del ensamblado                       
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());  
            return services;
        }
    }
}