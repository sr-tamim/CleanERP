using Microsoft.AspNetCore.Mvc;
using MediatR;
using GoldenFiberERP.Application.Features.Settings.Commands;
using GoldenFiberERP.Application.Features.Settings.Queries;
using GoldenFiberERP.Application.Features.Settings.DTOs;
using GoldenFiberERP.Application.Common.Models;

namespace GoldenFiberERP.API.Controllers.Settings;

/// <summary>
/// Country management endpoints for geographical and regional settings
/// </summary>
[ApiController]
[Route("api/settings/[controller]")]
[Tags("Settings - Countries")]
public class CountriesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CountriesController> _logger;

    public CountriesController(IMediator mediator, ILogger<CountriesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all countries with pagination and filtering
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="searchTerm">Search term for name, code, or capital</param>
    /// <param name="region">Filter by region</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="sortBy">Sort field (Name, Code, Region, DisplayOrder, CreatedAt)</param>
    /// <param name="sortDescending">Sort direction</param>
    /// <returns>Paginated list of countries</returns>
    [HttpGet]
    public async Task<IActionResult> GetCountries(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? region = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string sortBy = "Name",
        [FromQuery] bool sortDescending = false)
    {
        try
        {
            // Validate pagination parameters
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = new GetCountriesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                Region = region,
                IsActive = isActive,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _mediator.Send(query);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    success = true,
                    data = result.Data,
                    message = "Countries retrieved successfully"
                });
            }

            return BadRequest(new
            {
                success = false,
                errors = result.Errors,
                message = "Failed to retrieve countries"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting countries");
            return StatusCode(500, new
            {
                success = false,
                message = "An internal server error occurred"
            });
        }
    }

    /// <summary>
    /// Get active countries for dropdown/lookup purposes
    /// </summary>
    /// <returns>List of active countries</returns>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveCountries()
    {
        try
        {
            var query = new GetActiveCountriesQuery();
            var result = await _mediator.Send(query);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    success = true,
                    data = result.Data,
                    message = "Active countries retrieved successfully"
                });
            }

            return BadRequest(new
            {
                success = false,
                errors = result.Errors,
                message = "Failed to retrieve active countries"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting active countries");
            return StatusCode(500, new
            {
                success = false,
                message = "An internal server error occurred"
            });
        }
    }

    /// <summary>
    /// Get country by ID
    /// </summary>
    /// <param name="id">Country ID</param>
    /// <returns>Country details</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCountry(int id)
    {
        try
        {
            var query = new GetCountryByIdQuery(id);
            var result = await _mediator.Send(query);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    success = true,
                    data = result.Data,
                    message = "Country retrieved successfully"
                });
            }

            return NotFound(new
            {
                success = false,
                errors = result.Errors,
                message = "Country not found"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting country {CountryId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "An internal server error occurred"
            });
        }
    }

    /// <summary>
    /// Get country by ISO code
    /// </summary>
    /// <param name="code">ISO 2-letter country code (e.g., "US", "CA")</param>
    /// <returns>Country details</returns>
    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetCountryByCode(string code)
    {
        try
        {
            var query = new GetCountryByCodeQuery(code);
            var result = await _mediator.Send(query);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    success = true,
                    data = result.Data,
                    message = "Country retrieved successfully"
                });
            }

            return NotFound(new
            {
                success = false,
                errors = result.Errors,
                message = "Country not found"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting country by code {Code}", code);
            return StatusCode(500, new
            {
                success = false,
                message = "An internal server error occurred"
            });
        }
    }

    /// <summary>
    /// Create a new country
    /// </summary>
    /// <param name="dto">Country creation data</param>
    /// <returns>Created country ID</returns>
    [HttpPost]
    public async Task<IActionResult> CreateCountry([FromBody] CreateCountryDto dto)
    {
        try
        {
            var command = new CreateCountryCommand
            {
                Name = dto.Name,
                Code = dto.Code,
                Code3 = dto.Code3,
                NumericCode = dto.NumericCode,
                PhoneCode = dto.PhoneCode,
                Capital = dto.Capital,
                CurrencyCode = dto.CurrencyCode,
                CurrencySymbol = dto.CurrencySymbol,
                TimeZone = dto.TimeZone,
                Region = dto.Region,
                SubRegion = dto.SubRegion,
                DisplayOrder = dto.DisplayOrder
            };

            var result = await _mediator.Send(command);

            if (result.Succeeded)
            {
                return CreatedAtAction(
                    nameof(GetCountry),
                    new { id = result.Data },
                    new
                    {
                        success = true,
                        data = new { id = result.Data },
                        message = "Country created successfully"
                    });
            }

            return BadRequest(new
            {
                success = false,
                errors = result.Errors,
                message = "Failed to create country"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating country");
            return StatusCode(500, new
            {
                success = false,
                message = "An internal server error occurred"
            });
        }
    }

    /// <summary>
    /// Update an existing country
    /// </summary>
    /// <param name="id">Country ID</param>
    /// <param name="dto">Country update data</param>
    /// <returns>Success status</returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCountry(int id, [FromBody] UpdateCountryDto dto)
    {
        try
        {
            var command = new UpdateCountryCommand
            {
                Id = id,
                Name = dto.Name,
                Capital = dto.Capital,
                CurrencyCode = dto.CurrencyCode,
                CurrencySymbol = dto.CurrencySymbol,
                TimeZone = dto.TimeZone,
                Region = dto.Region,
                SubRegion = dto.SubRegion,
                DisplayOrder = dto.DisplayOrder,
                IsActive = dto.IsActive
            };

            var result = await _mediator.Send(command);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    success = true,
                    message = "Country updated successfully"
                });
            }

            return BadRequest(new
            {
                success = false,
                errors = result.Errors,
                message = "Failed to update country"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating country {CountryId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "An internal server error occurred"
            });
        }
    }

    /// <summary>
    /// Delete a country
    /// </summary>
    /// <param name="id">Country ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        try
        {
            var command = new DeleteCountryCommand(id);
            var result = await _mediator.Send(command);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    success = true,
                    message = "Country deleted successfully"
                });
            }

            return BadRequest(new
            {
                success = false,
                errors = result.Errors,
                message = "Failed to delete country"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting country {CountryId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "An internal server error occurred"
            });
        }
    }
}
