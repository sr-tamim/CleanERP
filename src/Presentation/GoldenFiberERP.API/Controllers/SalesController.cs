using Microsoft.AspNetCore.Mvc;

namespace GoldenFiberERP.API.Controllers;

/// <summary>
/// Sales and order management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Sales Management")]
public class SalesController : ControllerBase
{
    /// <summary>
    /// Get all sales orders
    /// </summary>
    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders()
    {
        await Task.Delay(1); // Placeholder
        
        var orders = new[]
        {
            new
            {
                Id = 1,
                OrderNumber = "SO-2024-001",
                CustomerName = "ABC Textiles Ltd",
                OrderDate = DateTime.UtcNow.AddDays(-5),
                TotalAmount = 15750.00m,
                Status = "Processing"
            },
            new
            {
                Id = 2,
                OrderNumber = "SO-2024-002",
                CustomerName = "XYZ Fashion House",
                OrderDate = DateTime.UtcNow.AddDays(-2),
                TotalAmount = 8900.50m,
                Status = "Pending"
            }
        };

        return Ok(new
        {
            Success = true,
            Data = orders,
            Total = orders.Length
        });
    }

    /// <summary>
    /// Create a new sales order
    /// </summary>
    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        await Task.Delay(1); // Placeholder
        
        return Ok(new
        {
            Success = true,
            Message = "Sales order created successfully",
            OrderId = 3,
            OrderNumber = "SO-2024-003"
        });
    }

    /// <summary>
    /// Get sales statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetSalesStatistics()
    {
        await Task.Delay(1); // Placeholder
        
        return Ok(new
        {
            TotalSales = 245600.75m,
            OrdersThisMonth = 42,
            TopCustomer = "ABC Textiles Ltd",
            AverageOrderValue = 5847.60m
        });
    }
}

/// <summary>
/// Create order request model
/// </summary>
public class CreateOrderRequest
{
    /// <summary>
    /// Customer ID
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Order items
    /// </summary>
    public List<OrderItem> Items { get; set; } = new();
}

/// <summary>
/// Order item model
/// </summary>
public class OrderItem
{
    /// <summary>
    /// Product ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Quantity ordered
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Unit price
    /// </summary>
    public decimal UnitPrice { get; set; }
}
