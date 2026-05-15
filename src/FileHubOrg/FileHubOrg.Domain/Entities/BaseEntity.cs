// ==================================================================================
// ENTITY: BaseEntity
// ==================================================================================
// Purpose: Base entity class that provides common properties for all entities
// This class encapsulates common audit fields and identifiers used across all entities
// ==================================================================================

using System;
using System.ComponentModel.DataAnnotations;

namespace FileHubOrg.Domain.Entities
{
    public abstract class BaseEntity
    {
        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            RowVersion = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
        }

        /// <summary>
        /// Gets or sets the unique identifier for the entity
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the entity was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the entity was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the user who created the entity
        /// </summary>
        public string CreatedByIP { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the user who last updated the entity
        /// </summary>
        public string? UpdatedByIP { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who created the entity
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who last updated the entity
        /// </summary>
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets the row version for optimistic concurrency control
        /// </summary>
        public byte[] RowVersion { get; set; }

    }
}
