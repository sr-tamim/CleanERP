using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using CleanERP.Application.Features.Inventory.Commands;
using CleanERP.Application.Features.Inventory.Queries;
using CleanERP.Application.Features.Inventory.DTOs;
using CleanERP.API.Authorization;

namespace CleanERP.API.Controllers;

/// <summary>
/// Products management endpoints for inventory operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    [HttpGet]
    [RequirePermission(Permissions.ProductRead)]
    public async Task<IActionResult> GetProducts()
    {
        var query = new GetProductsQuery();
        var result = await _mediator.Send(query);

        if (result.Succeeded)
            return Ok(result.Data);

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var query = new GetProductByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result.Succeeded)
            return Ok(result.Data);

        return NotFound(result.Errors);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        var command = new CreateProductCommand
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            SKU = dto.SKU
        };

        var result = await _mediator.Send(command);

        if (result.Succeeded)
            return CreatedAtAction(nameof(GetProduct), new { id = result.Data }, result.Data);

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
    {
        var command = new UpdateProductCommand
        {
            Id = id,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            SKU = dto.SKU
        };

        var result = await _mediator.Send(command);

        if (result.Succeeded)
            return NoContent();

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var command = new DeleteProductCommand(id);
        var result = await _mediator.Send(command);

        if (result.Succeeded)
            return NoContent();

        return BadRequest(result.Errors);
    }
}
