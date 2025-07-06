using System.ComponentModel.DataAnnotations;
using GoldenFiberERP.Domain.Entities.Common;

namespace GoldenFiberERP.Domain.Entities.Inventory
{
    public class Product : AuditableEntity
    {
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        [StringLength(100)]
        public string SKU { get; set; } = string.Empty;

        // Additional properties can be added as needed
    }
}
