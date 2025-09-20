using AutoFixture;
using QuickBite.API.DTOs;
using QuickBite.API.Models;
using System.Text.Json;

namespace QuickBite.API.Tests.Helpers
{
    /// <summary>
    /// Factory class for creating test data objects
    /// </summary>
    public static class TestDataFactory
    {
        private static readonly Fixture _fixture = new();

        static TestDataFactory()
        {
            // Configure AutoFixture to avoid circular references
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        #region MenuItem Factory Methods

        public static MenuItem CreateMenuItem(
            string? name = null,
            string? category = null,
            string? description = null,
            decimal? price = null,
            List<string>? dietaryTags = null,
            DateTime? createdAt = null,
            DateTime? updatedAt = null,
            DateTime? deletedAt = null)
        {
            var now = DateTime.UtcNow;
            
            return new MenuItem
            {
                Name = name ?? _fixture.Create<string>(),
                Category = category ?? GetRandomCategory(),
                Description = description ?? _fixture.Create<string>(),
                Price = price ?? _fixture.Create<decimal>(),
                DietaryTags = dietaryTags != null ? JsonSerializer.Serialize(dietaryTags) : null,
                CreatedAt = createdAt ?? now,
                UpdatedAt = updatedAt ?? now,
                DeletedAt = deletedAt
            };
        }

        public static List<MenuItem> CreateMenuItems(int count, string? category = null)
        {
            var items = new List<MenuItem>();
            for (int i = 0; i < count; i++)
            {
                items.Add(CreateMenuItem(
                    name: $"Test Item {i + 1}",
                    category: category ?? GetRandomCategory()));
            }
            return items;
        }

        #endregion

        #region DTO Factory Methods

        public static CreateMenuItemDto CreateCreateMenuItemDto(
            string? name = null,
            string? category = null,
            string? description = null,
            decimal? price = null,
            List<string>? dietaryTags = null)
        {
            return new CreateMenuItemDto
            {
                Name = name ?? _fixture.Create<string>(),
                Category = category ?? GetRandomCategory(),
                Description = description ?? _fixture.Create<string>(),
                Price = price ?? Math.Round(_fixture.Create<decimal>() % 100, 2),
                DietaryTags = dietaryTags ?? GetRandomDietaryTags().ToList()
            };
        }

        public static UpdateMenuItemDto CreateUpdateMenuItemDto(
            string? name = null,
            string? category = null,
            string? description = null,
            decimal? price = null,
            List<string>? dietaryTags = null)
        {
            return new UpdateMenuItemDto
            {
                Name = name,
                Category = category,
                Description = description,
                Price = price,
                DietaryTags = dietaryTags
            };
        }

        public static MenuItemResponseDto CreateMenuItemResponseDto(
            int? id = null,
            string? name = null,
            string? category = null,
            string? description = null,
            decimal? price = null,
            List<string>? dietaryTags = null,
            DateTime? createdAt = null,
            DateTime? updatedAt = null)
        {
            var now = DateTime.UtcNow;
            
            return new MenuItemResponseDto
            {
                Id = id ?? _fixture.Create<int>(),
                Name = name ?? _fixture.Create<string>(),
                Category = category ?? GetRandomCategory(),
                Description = description ?? _fixture.Create<string>(),
                Price = price ?? Math.Round(_fixture.Create<decimal>() % 100, 2),
                DietaryTags = dietaryTags ?? GetRandomDietaryTags().ToList(),
                CreatedAt = createdAt ?? now,
                UpdatedAt = updatedAt ?? now
            };
        }

        public static PaginatedMenuItemsResponseDto CreatePaginatedMenuItemsResponseDto(
            int itemCount = 5,
            int page = 1,
            int limit = 20,
            int? total = null)
        {
            var items = new List<MenuItemResponseDto>();
            for (int i = 0; i < itemCount; i++)
            {
                items.Add(CreateMenuItemResponseDto(name: $"Item {i + 1}"));
            }

            var totalItems = total ?? itemCount;
            var totalPages = (int)Math.Ceiling((double)totalItems / limit);

            return new PaginatedMenuItemsResponseDto
            {
                Data = items,
                Pagination = new PaginationMetadata
                {
                    Page = page,
                    Limit = limit,
                    Total = totalItems,
                    TotalPages = totalPages
                }
            };
        }

        #endregion

        #region Response Factory Methods

        public static ApiResponse<T> CreateSuccessResponse<T>(T data, string? message = null)
        {
            return ApiResponse<T>.SuccessResponse(data, message ?? "Success");
        }

        public static ErrorResponse CreateErrorResponse(
            string code = "TEST_ERROR",
            string message = "Test error message",
            string? details = null)
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

        #region Helper Methods

        private static string GetRandomCategory()
        {
            var categories = new[] { "Appetizers", "Main Course", "Desserts", "Beverages", "Sides" };
            return categories[Random.Shared.Next(categories.Length)];
        }

        private static string[] GetRandomDietaryTags()
        {
            var allTags = new[] { "Vegetarian", "Vegan", "Gluten-Free", "Dairy-Free", "Spicy", "Organic", "Low-Carb" };
            var tagCount = Random.Shared.Next(0, 4); // 0 to 3 tags
            return allTags.OrderBy(_ => Random.Shared.Next()).Take(tagCount).ToArray();
        }

        #endregion
    }
}