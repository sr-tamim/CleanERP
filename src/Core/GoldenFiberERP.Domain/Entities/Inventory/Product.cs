using System.ComponentModel.DataAnnotations;
using GoldenFiberERP.Domain.Entities.Common;
using GoldenFiberERP.Domain.Events.Inventory;
using GoldenFiberERP.Domain.Exceptions;

namespace GoldenFiberERP.Domain.Entities.Inventory
{
    public class Product : AuditableEntity
    {
        // Private constructor to enforce use of factory method
        private Product() { }

        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Description { get; set; } = null;

        public decimal Price { get; set; }
        
        public decimal Cost { get; set; }
        
        public int StockQuantity { get; set; }
        
        public int MinimumStockLevel { get; set; } = 10;
        
        public int ReorderLevel { get; set; } = 20;
        
        [StringLength(100)]
        public string? SKU { get; set; } = null;
        
        [StringLength(50)]
        public string? Category { get; set; } = null;
        
        [StringLength(20)]
        public string? Unit { get; set; } = "PCS";
        
        public bool IsActive { get; set; } = true;

        // Business methods for stock management
        public void AdjustStock(int quantity, string reason = "Manual adjustment", int? changedBy = null)
        {
            if (StockQuantity + quantity < 0)
            {
                throw new InsufficientStockException(
                    $"Cannot adjust stock for product {Name}. Insufficient stock. Current: {StockQuantity}, Adjustment: {quantity}");
            }

            var previousQuantity = StockQuantity;
            StockQuantity += quantity;
            
            // Raise domain event for stock change
            AddDomainEvent(new ProductStockChangedEvent(
                Id, 
                SKU, 
                Name, 
                previousQuantity, 
                StockQuantity, 
                quantity, 
                reason, 
                changedBy)
            {
                IsLowStock = IsLowStock(),
                MinimumStockLevel = MinimumStockLevel
            });
        }

        public void ReserveStock(int quantity, string reason = "Stock reservation", int? changedBy = null)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            if (StockQuantity < quantity)
            {
                throw new InsufficientStockException(
                    $"Cannot reserve {quantity} units of {Name}. Only {StockQuantity} available.");
            }

            var previousQuantity = StockQuantity;
            StockQuantity -= quantity;
            
            // Raise domain event for stock change
            AddDomainEvent(new ProductStockChangedEvent(
                Id, 
                SKU, 
                Name, 
                previousQuantity, 
                StockQuantity, 
                -quantity, 
                reason, 
                changedBy)
            {
                IsLowStock = IsLowStock(),
                MinimumStockLevel = MinimumStockLevel
            });
        }

        public void ReleaseStock(int quantity, string reason = "Stock release", int? changedBy = null)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            var previousQuantity = StockQuantity;
            StockQuantity += quantity;
            
            // Raise domain event for stock change
            AddDomainEvent(new ProductStockChangedEvent(
                Id, 
                SKU, 
                Name, 
                previousQuantity, 
                StockQuantity, 
                quantity, 
                reason, 
                changedBy)
            {
                IsLowStock = IsLowStock(),
                MinimumStockLevel = MinimumStockLevel
            });
        }

        public bool IsLowStock()
        {
            return StockQuantity <= MinimumStockLevel;
        }

        public bool NeedsReorder()
        {
            return StockQuantity <= ReorderLevel;
        }

        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice < 0)
                throw new ArgumentException("Price cannot be negative", nameof(newPrice));

            Price = newPrice;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        /// <summary>
        /// Creates a new product with the specified properties and raises a ProductCreatedEvent
        /// </summary>
        public static Product Create(
            string name, 
            string description, 
            string sku, 
            string category, 
            decimal price, 
            decimal cost, 
            int initialStock, 
            int minimumStockLevel, 
            int reorderLevel, 
            string unit,
            int createdBy)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name cannot be empty", nameof(name));
            
            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("Product SKU cannot be empty", nameof(sku));
                
            if (price < 0)
                throw new ArgumentException("Price cannot be negative", nameof(price));
                
            if (cost < 0)
                throw new ArgumentException("Cost cannot be negative", nameof(cost));

            var product = new Product
            {
                Name = name,
                Description = description,
                SKU = sku,
                Category = category,
                Price = price,
                Cost = cost,
                StockQuantity = initialStock,
                MinimumStockLevel = minimumStockLevel,
                ReorderLevel = reorderLevel,
                Unit = unit,
                IsActive = true
            };

            // Raise domain event for product creation
            product.AddDomainEvent(new ProductCreatedEvent(
                product.Id,
                product.SKU,
                product.Name,
                product.Category,
                product.Price,
                product.StockQuantity,
                createdBy));

            return product;
        }
    }
}
