using MediatR;
using Microsoft.Extensions.Logging;
using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.Application.Common.Models;
using GoldenFiberERP.Application.Common.Exceptions;
using GoldenFiberERP.Domain.Entities.Settings;
using GoldenFiberERP.Domain.Interfaces.Repositories.Settings;

namespace GoldenFiberERP.Application.Features.Settings.Commands;

/// <summary>
/// Command to update an existing country
/// </summary>
public record UpdateCountryCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Capital { get; init; } = null;
    public string? CurrencyCode { get; init; } = null;
    public string? CurrencySymbol { get; init; } = null;
    public string? TimeZone { get; init; } = null;
    public string? Region { get; init; } = null;
    public string? SubRegion { get; init; } = null;
    public int DisplayOrder { get; init; } = 0;
    public bool IsActive { get; init; } = true;
}

/// <summary>
/// Handler for UpdateCountryCommand
/// </summary>
public class UpdateCountryCommandHandler : IRequestHandler<UpdateCountryCommand, Result>
{
    private readonly ICountryRepository _countryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateCountryCommandHandler> _logger;

    public UpdateCountryCommandHandler(
        ICountryRepository countryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<UpdateCountryCommandHandler> logger)
    {
        _countryRepository = countryRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateCountryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating country with ID: {CountryId}", request.Id);

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // Get existing country
                var country = await _countryRepository.GetByIdAsync(request.Id, cancellationToken);
                if (country == null)
                {
                    return Result.Failure(new[] { $"Country with ID {request.Id} not found" });
                }

                // Get current user ID
                var userId = int.TryParse(_currentUserService.UserId, out var parsedUserId) ? parsedUserId : 0;

                // Update country information
                country.Update(
                    name: request.Name,
                    capital: request.Capital,
                    currencyCode: request.CurrencyCode,
                    currencySymbol: request.CurrencySymbol,
                    timeZone: request.TimeZone,
                    region: request.Region,
                    subRegion: request.SubRegion,
                    displayOrder: request.DisplayOrder,
                    updatedBy: userId);

                // Update active status if changed
                if (request.IsActive != country.IsActive)
                {
                    if (request.IsActive)
                        country.Activate(userId);
                    else
                        country.Deactivate(userId);
                }

                // Save changes
                await _countryRepository.UpdateAsync(country, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully updated country with ID: {CountryId}", country.Id);

                return Result.Success();

            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating country with ID: {CountryId}", request.Id);
            return Result.Failure(new[] { "An error occurred while updating the country" });
        }
    }
}
