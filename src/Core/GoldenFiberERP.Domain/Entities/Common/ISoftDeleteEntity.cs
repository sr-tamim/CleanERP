namespace GoldenFiberERP.Domain.Entities.Common
{
    public interface ISoftDeleteEntity
    {
        /// <summary>
        /// Soft delete flag - true if the entity is deleted
        /// </summary>
        public bool IsDeleted { get; set; }

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
