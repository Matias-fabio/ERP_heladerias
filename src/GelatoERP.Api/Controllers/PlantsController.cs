using GelatoERP.Application.Plants.Commands.CreatePlant;
using GelatoERP.Application.Plants.Queries.GetPlants;
using GelatoERP.Application.Plants.Queries.GetPlantsById;
using Microsoft.AspNetCore.Mvc;                                                                                        
                                                                                                                           
    namespace GelatoERP.Api.Controllers;                                                                                   
                                                                                                                           
    public class PlantsController : ApiControllerBase                                                                      
    {                                                                                                                      
        /// <summary>                                                                                                      
        /// Registrar una nueva Planta de Producción o Sucursal.                                                           
        /// </summary>                                                                                                     
        [HttpPost]                                                                                                         
        [ProducesResponseType(typeof(PlantDto), StatusCodes.Status201Created)]                                             
        [ProducesResponseType(StatusCodes.Status400BadRequest)]                                                            
        public async Task<ActionResult<PlantDto>> Create(CreatePlantCommand command)                                       
        {                                                                                                                  
            var result = await Mediator.Send(command);                                                                     
            return CreatedAtAction(nameof(Create), new { id = result.Id }, result);                                        
        }   

        /// <summary>                                                                       
        /// Obtener el listado de Plantas / Sucursales (filtrado opcional por tenantId).    
        /// </summary>      
        [HttpGet]
        [ProducesResponseType(typeof(List<PlantDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PlantDto>>> GetAll([FromQuery] Guid? tenantId)
        {
            var result = await Mediator.Send(new GetPlantsQuery(tenantId));
            return Ok(result);
        }


        /// <summary>
        /// Obtener una Planta / Sucursal específica por su ID. 
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PlantDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PlantDto>> GetById(Guid id)
        {
            var result = await Mediator.Send(new GetPlantByIdQuery(id));
            if (result == null)
                return NotFound(new { message = $"No se encontró ninguna Planta con el ID {id}." });

            return Ok(result);
        }
    }  