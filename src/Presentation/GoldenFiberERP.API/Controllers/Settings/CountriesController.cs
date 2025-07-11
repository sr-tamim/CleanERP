using Microsoft.AspNetCore.Mvc;
using MediatR;
using GoldenFiberERP.API.Controllers.Common;
using GoldenFiberERP.API.Models;
using GoldenFiberERP.Application.Features.Settings.Commands;
using GoldenFiberERP.Application.Features.Settings.Queries;
using GoldenFiberERP.Application.Features.Settings.DTOs;
using GoldenFiberERP.Application.Common.Models;

namespace GoldenFiberERP.API.Controllers.Settings;

/// <summary>
/// Example controller showing the new logging and error handling approach
/// No try-catch blocks needed!
/// </summary>
[Route("api/settings/[controller]")]
[Tags("Settings - Countries")]
public class CountriesController : BaseController
{
    private readonly IMediator _mediator;

    public CountriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all countries - No try-catch needed!
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<CountryDto>>>> GetCountries(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? region = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string sortBy = "Name",
        [FromQuery] bool sortDescending = false)
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
        
        // Use base controller method to handle result
        return HandleResult(result, "Countries retrieved successfully");
    }

    /// <summary>
    /// Get country by ID - No try-catch needed!
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CountryDto>>> GetCountry(int id)
    {
        var result = await _mediator.Send(new GetCountryByIdQuery(id));
        return HandleResult(result, "Country retrieved successfully");
    }

    /// <summary>
    /// Create a new country - No try-catch needed!
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<int>>> CreateCountry([FromBody] CreateCountryDto dto)
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
            Region = dto.Region,
            DisplayOrder = dto.DisplayOrder
        };

        var result = await _mediator.Send(command);
        
        if (result.Succeeded)
        {
            return Created(result.Data, "Country created successfully");
        }
        
        return HandleResult(result);
    }

    /// <summary>
    /// Update a country - No try-catch needed!
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse>> UpdateCountry(int id, [FromBody] UpdateCountryDto dto)
    {
        var command = new UpdateCountryCommand
        {
            Id = id,
            Name = dto.Name,
            Capital = dto.Capital,
            CurrencyCode = dto.CurrencyCode,
            Region = dto.Region,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive
        };

        var result = await _mediator.Send(command);
        return HandleResult(result, "Country updated successfully");
    }

    /// <summary>
    /// Delete a country - No try-catch needed!
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteCountry(int id)
    {
        var result = await _mediator.Send(new DeleteCountryCommand(id));
        return HandleResult(result, "Country deleted successfully");
    }

    /// <summary>
    /// Example of throwing custom exceptions - they will be handled automatically!
    /// </summary>
    [HttpPost("test-exceptions/{type}")]
    public ActionResult<ApiResponse> TestExceptions(string type)
    {
        // These exceptions will be caught by GlobalExceptionHandler automatically!
        switch (type.ToLower())
        {
            case "notfound":
                throw new Application.Common.Exceptions.NotFoundException("Test", "123");
            
            case "validation":
                throw new Application.Common.Exceptions.ValidationException();
            
            case "business":
                throw new Shared.Exceptions.BusinessException("This is a test business exception", "TEST_ERROR");
            
            case "forbidden":
                throw new Application.Common.Exceptions.ForbiddenException("You don't have permission to access this resource");
            
            case "conflict":
                throw new Application.Common.Exceptions.ConflictException("Test", "123");
            
            default:
                throw new InvalidOperationException("Unknown exception type");
        }
    }
}
