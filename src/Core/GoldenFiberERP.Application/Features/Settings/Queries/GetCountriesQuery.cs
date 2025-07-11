using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using GoldenFiberERP.Application.Common.Models;
using GoldenFiberERP.Application.Features.Settings.DTOs;
using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.Domain.Specifications.Settings;
using GoldenFiberERP.Application.Common.Extensions;

namespace GoldenFiberERP.Application.Features.Settings.Queries;

/// <summary>
/// Query to get all countries with pagination and filtering
/// </summary>
public record GetCountriesQuery : IRequest<Result<PagedResult<CountryDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public string? Region { get; init; }
    public bool? IsActive { get; init; }
    public string? SortBy { get; init; } = "Name";
    public bool SortDescending { get; init; } = false;
}

/// <summary>
/// Handler for GetCountriesQuery - Updated to use specifications pattern
/// </summary>
public class GetCountriesQueryHandler : IRequestHandler<GetCountriesQuery, Result<PagedResult<CountryDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCountriesQueryHandler> _logger;

    public GetCountriesQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetCountriesQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PagedResult<CountryDto>>> Handle(GetCountriesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting countries with specifications - Page: {Page}, Size: {Size}, Search: {Search}",
                request.PageNumber, request.PageSize, request.SearchTerm);

            var query = _context.Countries.AsQueryable();

            // Apply specifications based on request parameters
            if (request.IsActive.HasValue)
            {
                if (request.IsActive.Value)
                {
                    var activeSpec = new ActiveCountriesSpecification();
                    query = query.ApplySpecification(activeSpec);
                }
                else
                {
                    // For inactive countries, apply the opposite filter
                    query = query.Where(c => !c.IsActive);
                }
            }

            // Apply region filter
            if (!string.IsNullOrWhiteSpace(request.Region))
            {
                var regionSpec = new CountriesByRegionSpecification(request.Region);
                query = query.ApplySpecification(regionSpec);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchSpec = new CountriesWithSearchSpecification(request.SearchTerm);
                query = query.ApplySpecification(searchSpec);
            }

            // Get total count before applying pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            if (request.PageNumber > 0 && request.PageSize > 0)
            {
                query = query.Skip((request.PageNumber - 1) * request.PageSize)
                            .Take(request.PageSize);
            }

            // Execute query and get countries
            var countries = await query.ToListAsync(cancellationToken);

            var countryDtos = _mapper.Map<IEnumerable<CountryDto>>(countries);

            // Apply sorting if specified (done in-memory after DB query for simplicity)
            if (!string.IsNullOrEmpty(request.SortBy))
            {
                countryDtos = request.SortBy.ToLowerInvariant() switch
                {
                    "name" => request.SortDescending ? countryDtos.OrderByDescending(x => x.Name) : countryDtos.OrderBy(x => x.Name),
                    "code" => request.SortDescending ? countryDtos.OrderByDescending(x => x.Code) : countryDtos.OrderBy(x => x.Code),
                    "region" => request.SortDescending ? countryDtos.OrderByDescending(x => x.Region) : countryDtos.OrderBy(x => x.Region),
                    "displayorder" => request.SortDescending ? countryDtos.OrderByDescending(x => x.DisplayOrder) : countryDtos.OrderBy(x => x.DisplayOrder),
                    "createdat" => request.SortDescending ? countryDtos.OrderByDescending(x => x.CreatedAt) : countryDtos.OrderBy(x => x.CreatedAt),
                    _ => countryDtos.OrderBy(x => x.Name)
                };
            }

            var pagedResult = new PagedResult<CountryDto>
            {
                Items = countryDtos.ToList(),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };

            return Result<PagedResult<CountryDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting countries with specifications");
            return Result<PagedResult<CountryDto>>.Failure(new[] { "An error occurred while retrieving countries" });
        }
    }
}
