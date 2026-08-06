using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace GelatoERP.Api.Controllers;

    [ApiController]
    [Route("api/[controller]")]
    public abstract class ApiControllerBase : ControllerBase
    {
        private ISender? _mediator;

        /// <summary>                                                                                                      
        /// Propiedad protegida que resuelve el bus de MediatR mediante Inyección de Dependencias.                         
        /// </summary> 
        protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
    }
