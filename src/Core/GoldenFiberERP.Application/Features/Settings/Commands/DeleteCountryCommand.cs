using MediatR;
using Microsoft.Extensions.Logging;
using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.Application.Common.Models;
using GoldenFiberERP.Application.Common.Exceptions;
using GoldenFiberERP.Domain.Entities.Settings;
using GoldenFiberERP.Domain.Interfaces.Repositories.Settings;
using GoldenFiberERP.Domain.Events.Settings;

namespace GoldenFiberERP.Application.Features.Settings.Commands;

/// <summary>
/// Command to delete a country
/// </summary>
public record DeleteCountryCommand(int Id) : IRequest<Result>;

/// <summary>
/// Handler for DeleteCountryCommand
/// </summary>
public class DeleteCountryCommandHandler : IRequestHandler<DeleteCountryCommand, Result>
{
    private readonly ICountryRepository _countryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteCountryCommandHandler> _logger;

    public DeleteCountryCommandHandler(
        ICountryRepository countryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<DeleteCountryCommandHandler> logger)
    {
        _countryRepository = countryRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteCountryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting country with ID: {CountryId}", request.Id);

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

                // Store country info for event
                var countryName = country.Name;
                var countryCode = country.Code;

                // Delete the country
                await _countryRepository.DeleteAsync(request.Id, cancellationToken);

                _logger.LogInformation("Successfully deleted country with ID: {CountryId}, Code: {Code}",
                    request.Id, countryCode);

                return Result.Success();

            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting country with ID: {CountryId}", request.Id);
            return Result.Failure(new[] { "An error occurred while deleting the country" });
        }
    }
}
