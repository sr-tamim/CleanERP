using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace CleanERP.API.Controllers;

/// <summary>
/// Settings management endpoints for system configuration
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Settings")]
public class SettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Future settings endpoints will be added here
    // Countries are now handled by CountriesController
}
