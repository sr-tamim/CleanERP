using FluentValidation;
using CleanERP.Application.Features.Inventory.Commands;

namespace CleanERP.Application.Features.Inventory.Validators;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");

        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(v => v.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");

        RuleFor(v => v.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be greater than or equal to 0.");

        RuleFor(v => v.SKU)
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters.")
            .When(v => !string.IsNullOrEmpty(v.SKU));

        RuleFor(v => v.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(v => !string.IsNullOrEmpty(v.Description));
    }
}
