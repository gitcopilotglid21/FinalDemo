using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickBite.API.Models
{
    /// <summary>
    /// Represents a menu item in the restaurant's menu
    /// Based on Business Requirements Document specifications
    /// </summary>
    public class MenuItem
    {
        /// <summary>
        /// Unique identifier for the menu item (auto-generated)
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Name of the menu item (1-100 characters, unique within category)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the menu item (1-500 characters)
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Price of the menu item (positive value, max 2 decimal places)
        /// </summary>
        [Required]
        [Range(0.01, 999.99)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        /// <summary>
        /// Category of the menu item (from predefined list)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// JSON array of dietary tags (optional)
        /// </summary>
        public string? DietaryTags { get; set; }

        /// <summary>
        /// Timestamp when the item was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the item was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Timestamp when the item was deleted (null if active) - for soft delete
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Indicates if the menu item is active (not soft deleted)
        /// </summary>
        [NotMapped]
        public bool IsActive => DeletedAt == null;
    }
}