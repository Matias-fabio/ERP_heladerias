using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GelatoERP.Application.Tenants.Commands.CreateTenant;
using GelatoERP.Application.Tenants.Queries.GetTenantById;
using GelatoERP.Application.Tenants.Queries.GetTenants;
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
        
                                                                                                                           
        /// <summary>                                                                                                      
        /// Obtener el listado de todos los Tenants registrados.                                                           
        /// </summary>                                                                                                     
        [HttpGet]                                                                                                          
        [ProducesResponseType(typeof(List<TenantDto>), StatusCodes.Status200OK)]                                           
        public async Task<ActionResult<List<TenantDto>>> GetAll()                                                          
        {                                                                                                                  
            var result = await Mediator.Send(new GetTenantsQuery());                                                       
            return Ok(result);                                                                                             
        }                                                                                                                  
                                                                                                                           
        /// <summary>                                                                                                      
        /// Obtener un Tenant específico por su ID.                                                                        
        /// </summary>                                                                                                     
        [HttpGet("{id:guid}")]                                                                                             
        [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]                                                 
        [ProducesResponseType(StatusCodes.Status404NotFound)]                                                              
        public async Task<ActionResult<TenantDto>> GetById(Guid id)                                                        
        {                                                                                                                  
            var result = await Mediator.Send(new GetTenantByIdQuery(id));                                                  
                                                                                                                           
            if (result == null)                                                                                            
                return NotFound(new { message = $"No se encontró ningún Tenant con el ID {id}." });                        
                                                                                                                           
            return Ok(result);                                                                                             
        }          
    }
}