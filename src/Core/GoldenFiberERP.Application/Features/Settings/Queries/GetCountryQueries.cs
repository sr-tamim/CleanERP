using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using GoldenFiberERP.Application.Common.Models;
using GoldenFiberERP.Application.Features.Settings.DTOs;
using GoldenFiberERP.Domain.Interfaces.Repositories.Settings;

namespace GoldenFiberERP.Application.Features.Settings.Queries;

/// <summary>
/// Query to get a country by ID
/// </summary>
public record GetCountryByIdQuery(int Id) : IRequest<Result<CountryDto>>;

/// <summary>
/// Handler for GetCountryByIdQuery
/// </summary>
public class GetCountryByIdQueryHandler : IRequestHandler<GetCountryByIdQuery, Result<CountryDto>>
{
    private readonly ICountryRepository _countryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCountryByIdQueryHandler> _logger;

    public GetCountryByIdQueryHandler(
        ICountryRepository countryRepository,
        IMapper mapper,
        ILogger<GetCountryByIdQueryHandler> logger)
    {
        _countryRepository = countryRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CountryDto>> Handle(GetCountryByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting country by ID: {CountryId}", request.Id);

            var country = await _countryRepository.GetByIdAsync(request.Id, cancellationToken);

            if (country == null)
            {
                return Result<CountryDto>.Failure(new[] { $"Country with ID {request.Id} not found" });
            }

            var countryDto = _mapper.Map<CountryDto>(country);
            return Result<CountryDto>.Success(countryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting country by ID: {CountryId}", request.Id);
            return Result<CountryDto>.Failure(new[] { "An error occurred while retrieving the country" });
        }
    }
}

/// <summary>
/// Query to get a country by code
/// </summary>
public record GetCountryByCodeQuery(string Code) : IRequest<Result<CountryDto>>;

/// <summary>
/// Handler for GetCountryByCodeQuery
/// </summary>
public class GetCountryByCodeQueryHandler : IRequestHandler<GetCountryByCodeQuery, Result<CountryDto>>
{
    private readonly ICountryRepository _countryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCountryByCodeQueryHandler> _logger;

    public GetCountryByCodeQueryHandler(
        ICountryRepository countryRepository,
        IMapper mapper,
        ILogger<GetCountryByCodeQueryHandler> logger)
    {
        _countryRepository = countryRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CountryDto>> Handle(GetCountryByCodeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting country by code: {Code}", request.Code);

            var country = await _countryRepository.GetByCodeAsync(request.Code, cancellationToken);

            if (country == null)
            {
                return Result<CountryDto>.Failure(new[] { $"Country with code '{request.Code}' not found" });
            }

            var countryDto = _mapper.Map<CountryDto>(country);
            return Result<CountryDto>.Success(countryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting country by code: {Code}", request.Code);
            return Result<CountryDto>.Failure(new[] { "An error occurred while retrieving the country" });
        }
    }
}

/// <summary>
/// Query to get active countries for lookup/dropdown purposes
/// </summary>
public record GetActiveCountriesQuery : IRequest<Result<IEnumerable<CountryLookupDto>>>;

/// <summary>
/// Handler for GetActiveCountriesQuery
/// </summary>
public class GetActiveCountriesQueryHandler : IRequestHandler<GetActiveCountriesQuery, Result<IEnumerable<CountryLookupDto>>>
{
    private readonly ICountryRepository _countryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetActiveCountriesQueryHandler> _logger;

    public GetActiveCountriesQueryHandler(
        ICountryRepository countryRepository,
        IMapper mapper,
        ILogger<GetActiveCountriesQueryHandler> logger)
    {
        _countryRepository = countryRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<CountryLookupDto>>> Handle(GetActiveCountriesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting active countries for lookup");

            var countries = await _countryRepository.GetActiveCountriesAsync(cancellationToken);
            var countryLookups = _mapper.Map<IEnumerable<CountryLookupDto>>(countries);

            return Result<IEnumerable<CountryLookupDto>>.Success(countryLookups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active countries");
            return Result<IEnumerable<CountryLookupDto>>.Failure(new[] { "An error occurred while retrieving active countries" });
        }
    }
}
