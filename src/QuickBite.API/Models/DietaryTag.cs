using System.ComponentModel.DataAnnotations;

namespace QuickBite.API.Models
{
    /// <summary>
    /// Represents a dietary tag reference
    /// </summary>
    public class DietaryTag
    {
        /// <summary>
        /// Unique identifier for the dietary tag
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Name of the dietary tag (unique)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the dietary tag
        /// </summary>
        [MaxLength(200)]
        public string? Description { get; set; }

        /// <summary>
        /// Indicates if the dietary tag is active
        /// </summary>
        public bool Active { get; set; } = true;

        /// <summary>
        /// Timestamp when the dietary tag was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}