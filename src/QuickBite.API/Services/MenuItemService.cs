using Microsoft.EntityFrameworkCore;
using QuickBite.API.Data;
using QuickBite.API.DTOs;
using QuickBite.API.Models;
using System.Text.Json;

namespace QuickBite.API.Services
{
    /// <summary>
    /// Service for menu item business logic operations
    /// </summary>
    public class MenuItemService : IMenuItemService
    {
        private readonly QuickBiteDbContext _context;
        private readonly ILogger<MenuItemService> _logger;

        public MenuItemService(QuickBiteDbContext context, ILogger<MenuItemService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PaginatedMenuItemsResponseDto> GetMenuItemsAsync(
            int page,
            int limit,
            string? category = null,
            string? dietaryTags = null,
            string? search = null)
        {
            try
            {
                // Validate and normalize pagination parameters
                page = Math.Max(1, page);
                limit = Math.Clamp(limit, 1, 100);

                var query = _context.MenuItems.Where(m => m.DeletedAt == null);

                // Apply filters
                query = ApplyFilters(query, category, dietaryTags, search);

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / limit);

                var items = await query
                    .OrderBy(m => m.Category)
                    .ThenBy(m => m.Name)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                return new PaginatedMenuItemsResponseDto
                {
                    Data = items.Select(MapToResponseDto).ToList(),
                    Pagination = new PaginationMetadata
                    {
                        Page = page,
                        Limit = limit,
                        Total = totalItems,
                        TotalPages = totalPages
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving menu items with filters: category={Category}, dietaryTags={DietaryTags}, search={Search}",
                    category, dietaryTags, search);
                throw;
            }
        }

        public async Task<MenuItemResponseDto?> GetMenuItemByIdAsync(int id)
        {
            try
            {
                var menuItem = await _context.MenuItems
                    .Where(m => m.Id == id && m.DeletedAt == null)
                    .FirstOrDefaultAsync();

                return menuItem != null ? MapToResponseDto(menuItem) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving menu item with ID {Id}", id);
                throw;
            }
        }

        public async Task<MenuItemResponseDto> CreateMenuItemAsync(CreateMenuItemDto createDto)
        {
            try
            {
                // Check for duplicates
                if (await MenuItemExistsAsync(createDto.Name, createDto.Category))
                {
                    throw new InvalidOperationException($"Menu item '{createDto.Name}' already exists in category '{createDto.Category}'");
                }

                var menuItem = new MenuItem
                {
                    Name = createDto.Name,
                    Description = createDto.Description,
                    Price = createDto.Price,
                    Category = createDto.Category,
                    DietaryTags = createDto.DietaryTags != null ? JsonSerializer.Serialize(createDto.DietaryTags) : null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.MenuItems.Add(menuItem);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created menu item {Name} in category {Category} with ID {Id}",
                    menuItem.Name, menuItem.Category, menuItem.Id);

                return MapToResponseDto(menuItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating menu item {Name} in category {Category}",
                    createDto.Name, createDto.Category);
                throw;
            }
        }

        public async Task<MenuItemResponseDto?> UpdateMenuItemAsync(int id, UpdateMenuItemDto updateDto)
        {
            try
            {
                var menuItem = await _context.MenuItems
                    .Where(m => m.Id == id && m.DeletedAt == null)
                    .FirstOrDefaultAsync();

                if (menuItem == null)
                {
                    return null;
                }

                // Check for duplicates if name or category is being changed
                if ((updateDto.Name != null || updateDto.Category != null))
                {
                    var newName = updateDto.Name ?? menuItem.Name;
                    var newCategory = updateDto.Category ?? menuItem.Category;

                    if (await MenuItemExistsAsync(newName, newCategory, id))
                    {
                        throw new InvalidOperationException($"Menu item '{newName}' already exists in category '{newCategory}'");
                    }
                }

                // Update fields
                if (updateDto.Name != null) menuItem.Name = updateDto.Name;
                if (updateDto.Description != null) menuItem.Description = updateDto.Description;
                if (updateDto.Price.HasValue) menuItem.Price = updateDto.Price.Value;
                if (updateDto.Category != null) menuItem.Category = updateDto.Category;
                if (updateDto.DietaryTags != null)
                {
                    menuItem.DietaryTags = updateDto.DietaryTags.Any() ? JsonSerializer.Serialize(updateDto.DietaryTags) : null;
                }

                menuItem.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated menu item with ID {Id}", id);

                return MapToResponseDto(menuItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating menu item with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteMenuItemAsync(int id)
        {
            try
            {
                var menuItem = await _context.MenuItems
                    .Where(m => m.Id == id && m.DeletedAt == null)
                    .FirstOrDefaultAsync();

                if (menuItem == null)
                {
                    return false;
                }

                // Soft delete
                menuItem.DeletedAt = DateTime.UtcNow;
                menuItem.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Soft deleted menu item with ID {Id}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting menu item with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> MenuItemExistsAsync(string name, string category, int? excludeId = null)
        {
            try
            {
                var query = _context.MenuItems
                    .Where(m => m.Name == name && m.Category == category && m.DeletedAt == null);

                if (excludeId.HasValue)
                {
                    query = query.Where(m => m.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking menu item existence for {Name} in {Category}", name, category);
                throw;
            }
        }

        #region Private Helper Methods

        private static IQueryable<MenuItem> ApplyFilters(
            IQueryable<MenuItem> query,
            string? category,
            string? dietaryTags,
            string? search)
        {
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(m => m.Category == category);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(m => m.Name.Contains(search) || m.Description.Contains(search));
            }

            if (!string.IsNullOrEmpty(dietaryTags))
            {
                var tags = dietaryTags.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var tag in tags)
                {
                    var trimmedTag = tag.Trim();
                    query = query.Where(m => m.DietaryTags != null && m.DietaryTags.Contains(trimmedTag));
                }
            }

            return query;
        }

        private static MenuItemResponseDto MapToResponseDto(MenuItem menuItem)
        {
            var dietaryTags = new List<string>();
            if (!string.IsNullOrEmpty(menuItem.DietaryTags))
            {
                try
                {
                    dietaryTags = JsonSerializer.Deserialize<List<string>>(menuItem.DietaryTags) ?? new List<string>();
                }
                catch (JsonException)
                {
                    // If JSON deserialization fails, treat as empty list
                    dietaryTags = new List<string>();
                }
            }

            return new MenuItemResponseDto
            {
                Id = menuItem.Id,
                Name = menuItem.Name,
                Description = menuItem.Description,
                Price = menuItem.Price,
                Category = menuItem.Category,
                DietaryTags = dietaryTags,
                CreatedAt = menuItem.CreatedAt,
                UpdatedAt = menuItem.UpdatedAt
            };
        }

        #endregion
    }
}