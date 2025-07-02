using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoldenFiberERP.Domain.Entities.Common
{
    public abstract class AuditableEntity : BaseEntity, ISoftDeleteEntity
    {
        /// <summary>
        /// User ID who created the entity
        /// </summary>
        public int? CreatedBy { get; set; }

        /// <summary>
        /// User ID who last updated the entity
        /// </summary>
        public int? UpdatedBy { get; set; }

        /// <summary>
        /// Soft delete flag - true if the entity is deleted
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Timestamp when the entity was deleted (if soft deleted)
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// User ID who deleted the entity
        /// </summary>
        public int? DeletedBy { get; set; }
    }
}
