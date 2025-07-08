using MediatR;
using Microsoft.Extensions.Logging;
using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.Application.Common.Models;
using GoldenFiberERP.Application.Common.Exceptions;
using GoldenFiberERP.Domain.Entities.Settings;
using GoldenFiberERP.Domain.Interfaces.Repositories.Settings;

namespace GoldenFiberERP.Application.Features.Settings.Commands;

/// <summary>
/// Command to activate a country
/// </summary>
public record ActivateCountryCommand : IRequest<Result>
{
    public int Id { get; init; }
}

/// <summary>
/// Handler for ActivateCountryCommand
/// </summary>
public class ActivateCountryCommandHandler : IRequestHandler<ActivateCountryCommand, Result>
{
    private readonly ICountryRepository _countryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ActivateCountryCommandHandler> _logger;

    public ActivateCountryCommandHandler(
        ICountryRepository countryRepository,
        ICurrentUserService currentUserService,
        ILogger<ActivateCountryCommandHandler> logger)
    {
        _countryRepository = countryRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(ActivateCountryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Activating country with ID: {CountryId}", request.Id);

            var country = await _countryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (country == null)
            {
                throw new NotFoundException(nameof(Country), request.Id);
            }

            var userId = int.TryParse(_currentUserService.UserId, out var parsedUserId) ? parsedUserId : 0;
            country.Activate(userId);

            await _countryRepository.UpdateAsync(country, cancellationToken);

            _logger.LogInformation("Successfully activated country: {CountryName} ({CountryCode})", 
                country.Name, country.Code);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating country with ID: {CountryId}", request.Id);
            return Result.Failure(new[] { ex.Message });
        }
    }
}

/// <summary>
/// Command to deactivate a country
/// </summary>
public record DeactivateCountryCommand : IRequest<Result>
{
    public int Id { get; init; }
}

/// <summary>
/// Handler for DeactivateCountryCommand
/// </summary>
public class DeactivateCountryCommandHandler : IRequestHandler<DeactivateCountryCommand, Result>
{
    private readonly ICountryRepository _countryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeactivateCountryCommandHandler> _logger;

    public DeactivateCountryCommandHandler(
        ICountryRepository countryRepository,
        ICurrentUserService currentUserService,
        ILogger<DeactivateCountryCommandHandler> logger)
    {
        _countryRepository = countryRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(DeactivateCountryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deactivating country with ID: {CountryId}", request.Id);

            var country = await _countryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (country == null)
            {
                throw new NotFoundException(nameof(Country), request.Id);
            }

            var userId = int.TryParse(_currentUserService.UserId, out var parsedUserId) ? parsedUserId : 0;
            country.Deactivate(userId);

            await _countryRepository.UpdateAsync(country, cancellationToken);

            _logger.LogInformation("Successfully deactivated country: {CountryName} ({CountryCode})", 
                country.Name, country.Code);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating country with ID: {CountryId}", request.Id);
            return Result.Failure(new[] { ex.Message });
        }
    }
}
