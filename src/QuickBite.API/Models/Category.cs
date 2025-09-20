using System.ComponentModel.DataAnnotations;

namespace QuickBite.API.Models
{
    /// <summary>
    /// Represents a menu category reference
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Unique identifier for the category
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Name of the category (unique)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Display order for the category
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Indicates if the category is active
        /// </summary>
        public bool Active { get; set; } = true;

        /// <summary>
        /// Timestamp when the category was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}