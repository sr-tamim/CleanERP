using FluentValidation;
using CleanERP.Application.Features.Settings.Commands;
using CleanERP.Domain.Interfaces.Repositories.Settings;

namespace CleanERP.Application.Features.Settings.Validators;

/// <summary>
/// Validator for CreateCountryCommand
/// </summary>
public class CreateCountryCommandValidator : AbstractValidator<CreateCountryCommand>
{
    private readonly ICountryRepository _countryRepository;

    public CreateCountryCommandValidator(ICountryRepository countryRepository)
    {
        _countryRepository = countryRepository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Country name is required")
            .MaximumLength(100).WithMessage("Country name cannot exceed 100 characters")
            .MinimumLength(2).WithMessage("Country name must be at least 2 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Country code is required")
            .Length(2).WithMessage("Country code must be exactly 2 characters")
            .Matches("^[A-Z]{2}$").WithMessage("Country code must be 2 uppercase letters")
            .MustAsync(BeUniqueCode).WithMessage("Country code already exists");

        RuleFor(x => x.Code3)
            .NotEmpty().WithMessage("Country code3 is required")
            .Length(3).WithMessage("Country code3 must be exactly 3 characters")
            .Matches("^[A-Z]{3}$").WithMessage("Country code3 must be 3 uppercase letters")
            .MustAsync(BeUniqueCode3).WithMessage("Country code3 already exists");

        RuleFor(x => x.NumericCode)
            .MaximumLength(3).WithMessage("Numeric code must be at most 3 digits")
            .Matches("^[0-9]{3}$").When(x => !string.IsNullOrEmpty(x.NumericCode))
            .WithMessage("Numeric code must be exactly 3 digits when provided");

        RuleFor(x => x.PhoneCode)
            .MaximumLength(5).WithMessage("Phone code cannot exceed 5 characters");

        RuleFor(x => x.Capital)
            .MaximumLength(100).WithMessage("Capital name cannot exceed 100 characters");

        RuleFor(x => x.CurrencyCode)
            .MaximumLength(3).WithMessage("Currency code cannot exceed 3 characters")
            .Matches("^[A-Z]{0,3}$").When(x => !string.IsNullOrEmpty(x.CurrencyCode))
            .WithMessage("Currency code must be uppercase letters");

        RuleFor(x => x.CurrencySymbol)
            .MaximumLength(5).WithMessage("Currency symbol cannot exceed 5 characters");

        RuleFor(x => x.TimeZone)
            .MaximumLength(50).WithMessage("Time zone cannot exceed 50 characters");

        RuleFor(x => x.Region)
            .MaximumLength(50).WithMessage("Region cannot exceed 50 characters");

        RuleFor(x => x.SubRegion)
            .MaximumLength(50).WithMessage("Sub-region cannot exceed 50 characters");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative");
    }

    private async Task<bool> BeUniqueCode(string code, CancellationToken cancellationToken)
    {
        return !await _countryRepository.CodeExistsAsync(code, cancellationToken: cancellationToken);
    }

    private async Task<bool> BeUniqueCode3(string code3, CancellationToken cancellationToken)
    {
        return !await _countryRepository.Code3ExistsAsync(code3, cancellationToken: cancellationToken);
    }
}

/// <summary>
/// Validator for UpdateCountryCommand
/// </summary>
public class UpdateCountryCommandValidator : AbstractValidator<UpdateCountryCommand>
{
    public UpdateCountryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid country ID");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Country name is required")
            .MaximumLength(100).WithMessage("Country name cannot exceed 100 characters")
            .MinimumLength(2).WithMessage("Country name must be at least 2 characters");

        RuleFor(x => x.Capital)
            .MaximumLength(100).WithMessage("Capital name cannot exceed 100 characters");

        RuleFor(x => x.CurrencyCode)
            .MaximumLength(3).WithMessage("Currency code cannot exceed 3 characters")
            .Matches("^[A-Z]{0,3}$").When(x => !string.IsNullOrEmpty(x.CurrencyCode))
            .WithMessage("Currency code must be uppercase letters");

        RuleFor(x => x.CurrencySymbol)
            .MaximumLength(5).WithMessage("Currency symbol cannot exceed 5 characters");

        RuleFor(x => x.TimeZone)
            .MaximumLength(50).WithMessage("Time zone cannot exceed 50 characters");

        RuleFor(x => x.Region)
            .MaximumLength(50).WithMessage("Region cannot exceed 50 characters");

        RuleFor(x => x.SubRegion)
            .MaximumLength(50).WithMessage("Sub-region cannot exceed 50 characters");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative");
    }
}

/// <summary>
/// Validator for DeleteCountryCommand
/// </summary>
public class DeleteCountryCommandValidator : AbstractValidator<DeleteCountryCommand>
{
    public DeleteCountryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid country ID");
    }
}
