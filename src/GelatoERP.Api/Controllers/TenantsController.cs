using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GelatoERP.Application.Tenants.Commands.CreateTenant;
using Microsoft.AspNetCore.Mvc;

namespace GelatoERP.Api.Controllers
{
    public class TenantsController : ApiControllerBase
    {
                                                                                                                           
        /// <summary>                                                                                                      
        /// Registrar un nuevo Tenant (Heladería / Empresa) en la plataforma ERP.                                          
        /// </summary>                                                                                                     
        /// <param name="command">Datos del Tenant a crear</param>                                                         
        /// <returns>Tenant creado con su ID generado</returns>    
        /// 
        [HttpPost]
        [ProducesResponseType(typeof(TenantDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<ActionResult<TenantDto>> Create(CreateTenantCommand command)
        {
            var result = await Mediator.Send(command);
            return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
        }
        
    }
}