namespace QuickBite.API.DTOs
{
    /// <summary>
    /// Data Transfer Object for creating a new menu item
    /// Validation is handled by FluentValidation
    /// </summary>
    public class CreateMenuItemDto
    {
        /// <summary>
        /// Name of the menu item (1-100 characters)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the menu item (1-500 characters)
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Price of the menu item (positive value)
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Category of the menu item (from predefined list)
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Array of dietary tags (optional)
        /// </summary>
        public List<string>? DietaryTags { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for updating an existing menu item
    /// Validation is handled by FluentValidation
    /// </summary>
    public class UpdateMenuItemDto
    {
        /// <summary>
        /// Name of the menu item (1-100 characters)
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Description of the menu item (1-500 characters)
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Price of the menu item (positive value)
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// Category of the menu item (from predefined list)
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Array of dietary tags (optional)
        /// </summary>
        public List<string>? DietaryTags { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for menu item response
    /// </summary>
    public class MenuItemResponseDto
    {
        /// <summary>
        /// Unique identifier for the menu item
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the menu item
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the menu item
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Price of the menu item
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Category of the menu item
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Array of dietary tags
        /// </summary>
        public List<string> DietaryTags { get; set; } = new();

        /// <summary>
        /// Timestamp when the item was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the item was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for paginated menu item responses
    /// </summary>
    public class PaginatedMenuItemsResponseDto
    {
        /// <summary>
        /// Array of menu items
        /// </summary>
        public List<MenuItemResponseDto> Data { get; set; } = new();

        /// <summary>
        /// Pagination metadata
        /// </summary>
        public PaginationMetadata Pagination { get; set; } = new();
    }

    /// <summary>
    /// Pagination metadata
    /// </summary>
    public class PaginationMetadata
    {
        /// <summary>
        /// Current page number
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Total number of items
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages { get; set; }
    }
}