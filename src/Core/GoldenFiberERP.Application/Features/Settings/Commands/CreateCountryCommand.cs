using MediatR;
using Microsoft.Extensions.Logging;
using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.Application.Common.Models;
using GoldenFiberERP.Application.Features.Settings.DTOs;
using GoldenFiberERP.Domain.Entities.Settings;
using GoldenFiberERP.Domain.Interfaces.Repositories.Settings;

namespace GoldenFiberERP.Application.Features.Settings.Commands;

/// <summary>
/// Command to create a new country
/// </summary>
public record CreateCountryCommand : IRequest<Result<int>>
{
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Code3 { get; init; } = string.Empty;
    public string NumericCode { get; init; } = string.Empty;
    public string PhoneCode { get; init; } = string.Empty;
    public string Capital { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = string.Empty;
    public string CurrencySymbol { get; init; } = string.Empty;
    public string TimeZone { get; init; } = string.Empty;
    public string Region { get; init; } = string.Empty;
    public string SubRegion { get; init; } = string.Empty;
    public int DisplayOrder { get; init; } = 0;
}

/// <summary>
/// Handler for CreateCountryCommand
/// </summary>
public class CreateCountryCommandHandler : IRequestHandler<CreateCountryCommand, Result<int>>
{
    private readonly ICountryRepository _countryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateCountryCommandHandler> _logger;

    public CreateCountryCommandHandler(
        ICountryRepository countryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<CreateCountryCommandHandler> logger)
    {
        _countryRepository = countryRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreateCountryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating country with code: {Code}", request.Code);

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // Check if country code already exists
                if (await _countryRepository.CodeExistsAsync(request.Code, cancellationToken: cancellationToken))
                {
                    return Result<int>.Failure(new[] { $"Country with code '{request.Code}' already exists" });
                }

                // Check if country code3 already exists
                if (await _countryRepository.Code3ExistsAsync(request.Code3, cancellationToken: cancellationToken))
                {
                    return Result<int>.Failure(new[] { $"Country with code3 '{request.Code3}' already exists" });
                }

                // Get current user ID
                var userId = int.TryParse(_currentUserService.UserId, out var parsedUserId) ? parsedUserId : 0;

                // Create country using factory method
                var country = Country.Create(
                    name: request.Name,
                    code: request.Code,
                    code3: request.Code3,
                    numericCode: request.NumericCode,
                    phoneCode: request.PhoneCode,
                    capital: request.Capital,
                    currencyCode: request.CurrencyCode,
                    currencySymbol: request.CurrencySymbol,
                    timeZone: request.TimeZone,
                    region: request.Region,
                    subRegion: request.SubRegion,
                    displayOrder: request.DisplayOrder,
                    createdBy: userId);

                // Add to repository
                var createdCountry = await _countryRepository.AddAsync(country, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully created country with ID: {CountryId}, Code: {Code}",
                    createdCountry.Id, createdCountry.Code);

                return Result<int>.Success(createdCountry.Id);

            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating country with code: {Code}", request.Code);
            return Result<int>.Failure(new[] { "An error occurred while creating the country" });
        }
    }
}
