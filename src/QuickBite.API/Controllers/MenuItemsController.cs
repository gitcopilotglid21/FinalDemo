using Microsoft.AspNetCore.Mvc;
using QuickBite.API.DTOs;
using QuickBite.API.Exceptions;
using QuickBite.API.Services;

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
        private readonly IMenuItemService _menuItemService;
        private readonly ILogger<MenuItemsController> _logger;

        public MenuItemsController(IMenuItemService menuItemService, ILogger<MenuItemsController> logger)
        {
            _menuItemService = menuItemService;
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
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<ActionResult<PaginatedMenuItemsResponseDto>> GetMenuItems(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20,
            [FromQuery] string? category = null,
            [FromQuery] string? dietaryTags = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var result = await _menuItemService.GetMenuItemsAsync(page, limit, category, dietaryTags, search);
                return Ok(result);
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
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<ActionResult<ApiResponse<MenuItemResponseDto>>> GetMenuItem(int id)
        {
            try
            {
                var menuItem = await _menuItemService.GetMenuItemByIdAsync(id);

                if (menuItem == null)
                {
                    return NotFound(CreateErrorResponse("NOT_FOUND", $"Menu item with ID {id} not found"));
                }

                var response = ApiResponse<MenuItemResponseDto>.SuccessResponse(
                    menuItem,
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
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<ActionResult<ApiResponse<MenuItemResponseDto>>> CreateMenuItem([FromBody] CreateMenuItemDto createDto)
        {
            try
            {
                var menuItem = await _menuItemService.CreateMenuItemAsync(createDto);

                var response = ApiResponse<MenuItemResponseDto>.SuccessResponse(
                    menuItem,
                    "Menu item created successfully"
                );

                return CreatedAtAction(nameof(GetMenuItem), new { id = menuItem.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(CreateErrorResponse("DUPLICATE_ITEM", ex.Message));
            }
            catch (BusinessLogicException ex)
            {
                return BadRequest(CreateErrorResponse(ex.ErrorCode, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating menu item");
                return StatusCode(500, CreateErrorResponse("INTERNAL_ERROR", "An error occurred while creating the menu item"));
            }
        }

        /// <summary>
        /// Update an existing menu item
        /// </summary>
        /// <param name="id">Menu item ID</param>
        /// <param name="updateDto">Menu item update data</param>
        /// <returns>Updated menu item</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<MenuItemResponseDto>), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 409)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<ActionResult<ApiResponse<MenuItemResponseDto>>> UpdateMenuItem(int id, [FromBody] UpdateMenuItemDto updateDto)
        {
            try
            {
                var menuItem = await _menuItemService.UpdateMenuItemAsync(id, updateDto);

                if (menuItem == null)
                {
                    return NotFound(CreateErrorResponse("NOT_FOUND", $"Menu item with ID {id} not found"));
                }

                var response = ApiResponse<MenuItemResponseDto>.SuccessResponse(
                    menuItem,
                    "Menu item updated successfully"
                );

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(CreateErrorResponse("DUPLICATE_ITEM", ex.Message));
            }
            catch (BusinessLogicException ex)
            {
                return BadRequest(CreateErrorResponse(ex.ErrorCode, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating menu item {Id}", id);
                return StatusCode(500, CreateErrorResponse("INTERNAL_ERROR", "An error occurred while updating the menu item"));
            }
        }

        /// <summary>
        /// Delete a menu item (soft delete)
        /// </summary>
        /// <param name="id">Menu item ID</param>
        /// <returns>Success confirmation</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteMenuItem(int id)
        {
            try
            {
                var deleted = await _menuItemService.DeleteMenuItemAsync(id);

                if (!deleted)
                {
                    return NotFound(CreateErrorResponse("NOT_FOUND", $"Menu item with ID {id} not found"));
                }

                var response = ApiResponse<object>.SuccessResponse(
                    new { },
                    "Menu item deleted successfully"
                );

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting menu item {Id}", id);
                return StatusCode(500, CreateErrorResponse("INTERNAL_ERROR", "An error occurred while deleting the menu item"));
            }
        }

        #region Private Helper Methods

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

        #endregion
    }
}