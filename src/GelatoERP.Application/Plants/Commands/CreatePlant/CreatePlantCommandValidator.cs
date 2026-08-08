    using FluentValidation;                                                                                                
                                                                                                                           
    namespace GelatoERP.Application.Plants.Commands.CreatePlant;                                                           
                                                                                                                           
    public class CreatePlantCommandValidator : AbstractValidator<CreatePlantCommand>                                       
    {                                                                                                                      
        public CreatePlantCommandValidator()                                                                               
        {                                                                                                                  
            RuleFor(v => v.TenantId)                                                                                       
                .NotEmpty().WithMessage("El ID del Tenant es obligatorio.");                                               
                                                                                                                           
            RuleFor(v => v.Name)                                                                                           
                .NotEmpty().WithMessage("El nombre de la planta/sucursal es obligatorio.")                                 
                .MaximumLength(100).WithMessage("El nombre no debe superar los 100 caracteres.");                          
                                                                                                                           
            RuleFor(v => v.Code)                                                                                           
                .NotEmpty().WithMessage("El código de la planta es obligatorio.")                                          
                .MaximumLength(10).WithMessage("El código no debe superar los 10 caracteres.");                            
                                                                                                                           
            RuleFor(v => v.Address)                                                                                        
                .NotEmpty().WithMessage("La dirección de la planta es obligatoria.")                                       
                .MaximumLength(200).WithMessage("La dirección no debe superar los 200 caracteres.");                       
        }                                                                                                                  
    } 