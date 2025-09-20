using QuickBite.API.DTOs;

namespace QuickBite.API.Services
{
    /// <summary>
    /// Interface for menu item business logic operations
    /// </summary>
    public interface IMenuItemService
    {
        /// <summary>
        /// Get paginated menu items with optional filtering
        /// </summary>
        Task<PaginatedMenuItemsResponseDto> GetMenuItemsAsync(
            int page, 
            int limit, 
            string? category = null, 
            string? dietaryTags = null, 
            string? search = null);

        /// <summary>
        /// Get a specific menu item by ID
        /// </summary>
        Task<MenuItemResponseDto?> GetMenuItemByIdAsync(int id);

        /// <summary>
        /// Create a new menu item
        /// </summary>
        Task<MenuItemResponseDto> CreateMenuItemAsync(CreateMenuItemDto createDto);

        /// <summary>
        /// Update an existing menu item
        /// </summary>
        Task<MenuItemResponseDto?> UpdateMenuItemAsync(int id, UpdateMenuItemDto updateDto);

        /// <summary>
        /// Soft delete a menu item
        /// </summary>
        Task<bool> DeleteMenuItemAsync(int id);

        /// <summary>
        /// Check if menu item exists with same name in category
        /// </summary>
        Task<bool> MenuItemExistsAsync(string name, string category, int? excludeId = null);
    }
}