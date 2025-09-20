using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickBite.API.Data;
using QuickBite.API.DTOs;
using QuickBite.API.Models;
using System.Text.Json;

namespace QuickBite.API.Controllers
{
    /// <summary>
    /// Controller for managing menu items
    /// Provides CRUD operations for restaurant menu items as specified in BRD
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MenuItemsController : ControllerBase
    {
        private readonly QuickBiteDbContext _context;
        private readonly ILogger<MenuItemsController> _logger;

        public MenuItemsController(QuickBiteDbContext context, ILogger<MenuItemsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all menu items with optional filtering and pagination
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="limit">Items per page (default: 20, max: 100)</param>
        /// <param name="category">Filter by category</param>
        /// <param name="dietaryTags">Filter by dietary tags (comma-separated)</param>
        /// <param name="search">Search in name or description</param>
        /// <returns>Paginated list of menu items</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedMenuItemsResponseDto), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<ActionResult<PaginatedMenuItemsResponseDto>> GetMenuItems(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20,
            [FromQuery] string? category = null,
            [FromQuery] string? dietaryTags = null,
            [FromQuery] string? search = null)
        {
            try
            {
                // Validate pagination parameters
                if (page < 1) page = 1;
                if (limit < 1) limit = 20;
                if (limit > 100) limit = 100;

                var query = _context.MenuItems.Where(m => m.DeletedAt == null);

                // Apply filters
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
                        query = query.Where(m => m.DietaryTags != null && m.DietaryTags.Contains(tag.Trim()));
                    }
                }

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / limit);

                var items = await query
                    .OrderBy(m => m.Category)
                    .ThenBy(m => m.Name)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var response = new PaginatedMenuItemsResponseDto
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

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving menu items");
                return StatusCode(500, CreateErrorResponse("INTERNAL_ERROR", "An error occurred while retrieving menu items"));
            }
        }

        /// <summary>
        /// Get a specific menu item by ID
        /// </summary>
        /// <param name="id">Menu item ID</param>
        /// <returns>Menu item details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<MenuItemResponseDto>), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public async Task<ActionResult<ApiResponse<MenuItemResponseDto>>> GetMenuItem(int id)
        {
            try
            {
                var menuItem = await _context.MenuItems
                    .Where(m => m.Id == id && m.DeletedAt == null)
                    .FirstOrDefaultAsync();

                if (menuItem == null)
                {
                    return NotFound(CreateErrorResponse("NOT_FOUND", $"Menu item with ID {id} not found"));
                }

                var response = ApiResponse<MenuItemResponseDto>.SuccessResponse(
                    MapToResponseDto(menuItem),
                    "Menu item retrieved successfully"
                );

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving menu item {Id}", id);
                return StatusCode(500, CreateErrorResponse("INTERNAL_ERROR", "An error occurred while retrieving the menu item"));
            }
        }

        /// <summary>
        /// Create a new menu item
        /// </summary>
        /// <param name="createDto">Menu item creation data</param>
        /// <returns>Created menu item</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<MenuItemResponseDto>), 201)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 409)]
        public async Task<ActionResult<ApiResponse<MenuItemResponseDto>>> CreateMenuItem([FromBody] CreateMenuItemDto createDto)
        {
            try
            {
                // Check if menu item with same name exists in the same category
                var existingItem = await _context.MenuItems
                    .Where(m => m.Name == createDto.Name && m.Category == createDto.Category && m.DeletedAt == null)
                    .FirstOrDefaultAsync();

                if (existingItem != null)
                {
                    return Conflict(CreateErrorResponse("DUPLICATE_ITEM", $"Menu item '{createDto.Name}' already exists in category '{createDto.Category}'"));
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

                var response = ApiResponse<MenuItemResponseDto>.SuccessResponse(
                    MapToResponseDto(menuItem),
                    "Menu item created successfully"
                );

                return CreatedAtAction(nameof(GetMenuItem), new { id = menuItem.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating menu item");
                return StatusCode(500, CreateErrorResponse("INTERNAL_ERROR", "An error occurred while creating the menu item"));
            }
        }

        /// <summary>
        /// Maps MenuItem entity to response DTO
        /// </summary>
        private static MenuItemResponseDto MapToResponseDto(MenuItem menuItem)
        {
            var dietaryTags = new List<string>();
            if (!string.IsNullOrEmpty(menuItem.DietaryTags))
            {
                try
                {
                    dietaryTags = JsonSerializer.Deserialize<List<string>>(menuItem.DietaryTags) ?? new List<string>();
                }
                catch
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

        /// <summary>
        /// Creates standardized error response
        /// </summary>
        private static ErrorResponse CreateErrorResponse(string code, string message, string? details = null)
        {
            return new ErrorResponse
            {
                Error = new ErrorDetails
                {
                    Code = code,
                    Message = message,
                    Details = details,
                    Timestamp = DateTime.UtcNow
                }
            };
        }
    }
}