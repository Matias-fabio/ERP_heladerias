using FluentValidation;

namespace GelatoERP.Application.Tenants.Commands.CreateTenant;

/// <summary>                                                                                                          
    /// Reglas de validación para CreateTenantCommand utilizando FluentValidation.
/// </summary>

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("El nombre de la heladería/empresa es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no debe superar los 100 caracteres.");

        RuleFor(v => v.TaxId)
            .NotEmpty().WithMessage("El NIF/CIF es obligatorio.")
            .MaximumLength(20).WithMessage("El NIF/CIF no debe superar los 20 caracteres.");

        RuleFor(v => v.DomainOrSlug)
            .NotEmpty().WithMessage("El dominio o slug es obligatorio.")
            .MaximumLength(50).WithMessage("El dominio o slug no debe superar los 50 caracteres.")
            .Matches("^[a-z0-9-]+$").WithMessage("El subdominio solo puede contener letras minúsculas, números y guiones medios (ej: heladeria-don-luis).");
    }
}