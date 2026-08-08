                                                                                                                           
    using FluentValidation;                                                                                                
    using Microsoft.AspNetCore.Diagnostics;                                                                                
    using Microsoft.AspNetCore.Mvc;                                                                                        
                                                                                                                           
    namespace GelatoERP.Api.Middlewares;                                                                                   
                                                                                                                           
    /// <summary>                                                                                                          
    /// Manejador global de excepciones en .NET 8 (IExceptionHandler).                                                     
    /// Transforma errores en respuestas HTTP estandarizadas RFC 7807 (ProblemDetails).                                    
    /// </summary>                                                                                                         
    public class CustomExceptionHandler : IExceptionHandler                                                                
    {                                                                                                                      
        private readonly ILogger<CustomExceptionHandler> _logger;                                                          
                                                                                                                           
        public CustomExceptionHandler(ILogger<CustomExceptionHandler> logger)                                              
        {                                                                                                                  
            _logger = logger;                                                                                              
        }                                                                                                                  
                                                                                                                           
        public async ValueTask<bool> TryHandleAsync(                                                                       
            HttpContext httpContext,                                                                                       
            Exception exception,                                                                                           
            CancellationToken cancellationToken)                                                                           
        {                                                                                                                  
            _logger.LogError(exception, "Ocurrió una excepción no controlada: {Message}", exception.Message);              
                                                                                                                           
            var (statusCode, title, detail, errors) = exception switch                                                     
            {                                                                                                              
                ValidationException validationException => (                                                               
                    StatusCodes.Status400BadRequest,                                                                       
                    "Error de Validación",                                                                                 
                    "Uno o más errores de validación ocurrieron.",                                                         
                    validationException.Errors                                                                             
                        .GroupBy(e => e.PropertyName)                                                                      
                        .ToDictionary(                                                                                     
                            g => g.Key,                                                                                    
                            g => g.Select(e => e.ErrorMessage).ToArray()                                                   
                        ) as IDictionary<string, object?>                                                                  
                ),                                                                                                         
                KeyNotFoundException notFoundException => (                                                                
                    StatusCodes.Status404NotFound,                                                                         
                    "Recurso no encontrado",                                                                               
                    notFoundException.Message,                                                                             
                    null                                                                                                   
                ),                                                                                                         
                _ => (                                                                                                     
                    StatusCodes.Status500InternalServerError,                                                              
                    "Error interno del servidor",                                                                          
                    "Ha ocurrido un error inesperado en el servidor.",                                                     
                    null                                                                                                   
                )                                                                                                          
            };                                                                                                             
                                                                                                                           
            httpContext.Response.StatusCode = statusCode;                                                                  
                                                                                                                           
            var problemDetails = new ProblemDetails                                                                        
            {                                                                                                              
                Status = statusCode,                                                                                       
                Title = title,                                                                                             
                Detail = detail,                                                                                           
                Instance = httpContext.Request.Path                                                                        
            };                                                                                                             
                                                                                                                           
            if (errors != null)                                                                                            
            {                                                                                                              
                problemDetails.Extensions["errors"] = errors;                                                              
            }                                                                                                              
                                                                                                                           
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);                                
                                                                                                                           
            return true;                                                                                                   
        }                                                                                                                  
    }  