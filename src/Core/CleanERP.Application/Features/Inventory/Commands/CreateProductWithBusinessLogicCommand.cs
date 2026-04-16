using MediatR;
using Microsoft.Extensions.Logging;
using CleanERP.Application.Common.Interfaces;
using CleanERP.Application.Common.Models;
using CleanERP.Domain.Entities.Inventory;
using CleanERP.Domain.Events.Inventory;
using CleanERP.Domain.Services.Inventory;
using CleanERP.Domain.Services.Pricing;
using CleanERP.Domain.ValueObjects;
using CleanERP.Domain.Interfaces.Repositories.Inventory;

namespace CleanERP.Application.Features.Inventory.Commands;

/// <summary>
/// Command to create a new product with full business logic
/// </summary>
public record CreateProductWithBusinessLogicCommand : IRequest<Result<int>>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal Cost { get; init; }
    public int InitialStock { get; init; }
    public int MinimumStockLevel { get; init; } = 10;
    public int ReorderLevel { get; init; } = 20;
    public string Unit { get; init; } = "PCS";
}

/// <summary>
/// Handler for CreateProductWithBusinessLogicCommand demonstrating the full Clean Architecture stack
/// </summary>
public class CreateProductWithBusinessLogicCommandHandler : IRequestHandler<CreateProductWithBusinessLogicCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStockManagementService _stockManagementService;
    private readonly IPricingService _pricingService;
    private readonly ILogger<CreateProductWithBusinessLogicCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IProductRepository _productRepository;

    public CreateProductWithBusinessLogicCommandHandler(
        IUnitOfWork unitOfWork,
        IStockManagementService stockManagementService,
        IPricingService pricingService,
        ILogger<CreateProductWithBusinessLogicCommandHandler> logger,
        ICurrentUserService currentUserService,
        IProductRepository productRepository)
    {
        _unitOfWork = unitOfWork;
        _stockManagementService = stockManagementService;
        _pricingService = pricingService;
        _logger = logger;
        _currentUserService = currentUserService;
        _productRepository = productRepository;
    }

    public async Task<Result<int>> Handle(CreateProductWithBusinessLogicCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating product with SKU: {SKU}", request.SKU);

            // Use transaction for data consistency
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // Validate business rules using domain services
                var costMoney = Money.Create(request.Cost);
                var priceMoney = Money.Create(request.Price);

                if (!_pricingService.ValidatePrice(priceMoney, costMoney))
                {
                    return Result<int>.Failure(new[] { "Price must be higher than cost with minimum markup" });
                }

                // Create value objects
                var sku = ProductSku.Create(request.SKU);

                // Check if SKU already exists
                var existingProduct = await _productRepository.GetBySkuAsync(sku.Value, cancellationToken);

                if (existingProduct != null)
                {
                    return Result<int>.Failure(new[] { $"Product with SKU '{request.SKU}' already exists" });
                }

                // Get current user ID
                var userId = int.TryParse(_currentUserService.UserId, out var parsedUserId) ? parsedUserId : 0;

                // Create the product using the factory method
                var product = Product.Create(
                    name: request.Name.Trim(),
                    description: request.Description?.Trim() ?? string.Empty,
                    sku: sku.Value,
                    category: request.Category.Trim(),
                    price: request.Price,
                    cost: request.Cost,
                    initialStock: request.InitialStock,
                    minimumStockLevel: request.MinimumStockLevel,
                    reorderLevel: request.ReorderLevel,
                    unit: request.Unit,
                    createdBy: userId);

                // Add to context
                await _productRepository.AddAsync(product, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Check if initial stock triggers low stock condition
                if (_stockManagementService.IsLowStock(product))
                {
                    _logger.LogWarning("New product {SKU} created with low stock level: {Stock} (minimum: {MinStock})",
                        product.SKU, product.StockQuantity, product.MinimumStockLevel);

                    // This would typically trigger a domain event or notification
                    // For demonstration, we'll just log it
                }

                _logger.LogInformation("Successfully created product with ID: {ProductId}, SKU: {SKU}",
                    product.Id, product.SKU);

                return Result<int>.Success(product.Id);

            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product with SKU: {SKU}", request.SKU);
            return Result<int>.Failure(new[] { "An error occurred while creating the product" });
        }
    }
}
