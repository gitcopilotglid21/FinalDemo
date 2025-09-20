using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using QuickBite.API.Data;
using QuickBite.API.DTOs;
using QuickBite.API.Models;
using QuickBite.API.Services;
using FluentAssertions;
using AutoFixture;
using System.Text.Json;

namespace QuickBite.API.Tests.Services
{
    public class MenuItemServiceTests : IDisposable
    {
        private readonly QuickBiteDbContext _context;
        private readonly Mock<ILogger<MenuItemService>> _mockLogger;
        private readonly MenuItemService _service;
        private readonly Fixture _fixture;

        public MenuItemServiceTests()
        {
            // Create in-memory database
            var options = new DbContextOptionsBuilder<QuickBiteDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new QuickBiteDbContext(options);
            _mockLogger = new Mock<ILogger<MenuItemService>>();
            _service = new MenuItemService(_context, _mockLogger.Object);
            _fixture = new Fixture();

            // Configure AutoFixture to avoid circular references
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        #region GetMenuItemsAsync Tests

        [Fact]
        public async Task GetMenuItemsAsync_WithDefaultParameters_ReturnsAllMenuItems()
        {
            // Arrange
            var menuItems = CreateTestMenuItems(5);
            await _context.MenuItems.AddRangeAsync(menuItems);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetMenuItemsAsync(1, 20);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(5);
            result.Pagination.Total.Should().Be(5);
            result.Pagination.Page.Should().Be(1);
            result.Pagination.Limit.Should().Be(20);
        }

        [Fact]
        public async Task GetMenuItemsAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var menuItems = CreateTestMenuItems(25);
            await _context.MenuItems.AddRangeAsync(menuItems);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetMenuItemsAsync(2, 10);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(10);
            result.Pagination.Total.Should().Be(25);
            result.Pagination.Page.Should().Be(2);
            result.Pagination.TotalPages.Should().Be(3);
        }

        [Fact]
        public async Task GetMenuItemsAsync_WithCategoryFilter_ReturnsFilteredResults()
        {
            // Arrange
            var menuItems = new List<MenuItem>
            {
                CreateMenuItem("Item1", "Appetizers"),
                CreateMenuItem("Item2", "Main Course"),
                CreateMenuItem("Item3", "Appetizers"),
                CreateMenuItem("Item4", "Desserts")
            };
            await _context.MenuItems.AddRangeAsync(menuItems);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetMenuItemsAsync(1, 20, "Appetizers");

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.Should().OnlyContain(item => item.Category == "Appetizers");
        }

        [Fact]
        public async Task GetMenuItemsAsync_WithSearchFilter_ReturnsMatchingItems()
        {
            // Arrange
            var menuItems = new List<MenuItem>
            {
                CreateMenuItem("Caesar Salad", "Appetizers", "Fresh lettuce with caesar dressing"),
                CreateMenuItem("Greek Salad", "Appetizers", "Mediterranean style salad"),
                CreateMenuItem("Grilled Chicken", "Main Course", "Tender grilled chicken breast")
            };
            await _context.MenuItems.AddRangeAsync(menuItems);
            await _context.SaveChangesAsync();

            // Act - search for "Salad" (case-sensitive)
            var result = await _service.GetMenuItemsAsync(1, 20, search: "Salad");

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.Should().OnlyContain(item => 
                item.Name.Contains("Salad") ||
                item.Description.Contains("salad"));
        }

        [Fact]
        public async Task GetMenuItemsAsync_WithDietaryTagsFilter_ReturnsMatchingItems()
        {
            // Arrange
            var menuItems = new List<MenuItem>
            {
                CreateMenuItem("Veggie Burger", "Main Course", dietaryTags: new List<string> { "Vegetarian", "Gluten-Free" }),
                CreateMenuItem("Chicken Wings", "Appetizers", dietaryTags: new List<string> { "Spicy" }),
                CreateMenuItem("Quinoa Salad", "Appetizers", dietaryTags: new List<string> { "Vegetarian", "Vegan" })
            };
            await _context.MenuItems.AddRangeAsync(menuItems);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetMenuItemsAsync(1, 20, dietaryTags: "Vegetarian");

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.Should().OnlyContain(item => item.DietaryTags.Contains("Vegetarian"));
        }

        [Fact]
        public async Task GetMenuItemsAsync_ExcludesDeletedItems()
        {
            // Arrange
            var menuItems = new List<MenuItem>
            {
                CreateMenuItem("Active Item", "Main Course"),
                CreateMenuItem("Deleted Item", "Main Course", deletedAt: DateTime.UtcNow)
            };
            await _context.MenuItems.AddRangeAsync(menuItems);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetMenuItemsAsync(1, 20);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
            result.Data.Should().OnlyContain(item => item.Name == "Active Item");
        }

        #endregion

        #region GetMenuItemByIdAsync Tests

        [Fact]
        public async Task GetMenuItemByIdAsync_WithValidId_ReturnsMenuItem()
        {
            // Arrange
            var menuItem = CreateMenuItem("Test Item", "Main Course");
            await _context.MenuItems.AddAsync(menuItem);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetMenuItemByIdAsync(menuItem.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(menuItem.Id);
            result.Name.Should().Be("Test Item");
            result.Category.Should().Be("Main Course");
        }

        [Fact]
        public async Task GetMenuItemByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = await _service.GetMenuItemByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetMenuItemByIdAsync_WithDeletedItem_ReturnsNull()
        {
            // Arrange
            var menuItem = CreateMenuItem("Deleted Item", "Main Course", deletedAt: DateTime.UtcNow);
            await _context.MenuItems.AddAsync(menuItem);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetMenuItemByIdAsync(menuItem.Id);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateMenuItemAsync Tests

        [Fact]
        public async Task CreateMenuItemAsync_WithValidData_CreatesMenuItem()
        {
            // Arrange
            var createDto = new CreateMenuItemDto
            {
                Name = "New Item",
                Description = "A new menu item",
                Price = 12.99m,
                Category = "Main Course",
                DietaryTags = new List<string> { "Vegetarian" }
            };

            // Act
            var result = await _service.CreateMenuItemAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("New Item");
            result.Price.Should().Be(12.99m);
            result.DietaryTags.Should().Contain("Vegetarian");

            var savedItem = await _context.MenuItems.FindAsync(result.Id);
            savedItem.Should().NotBeNull();
            savedItem!.Name.Should().Be("New Item");
        }

        [Fact]
        public async Task CreateMenuItemAsync_WithDuplicateNameInCategory_ThrowsInvalidOperationException()
        {
            // Arrange
            var existingItem = CreateMenuItem("Duplicate Item", "Main Course");
            await _context.MenuItems.AddAsync(existingItem);
            await _context.SaveChangesAsync();

            var createDto = new CreateMenuItemDto
            {
                Name = "Duplicate Item",
                Description = "Another item with same name",
                Price = 15.99m,
                Category = "Main Course"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateMenuItemAsync(createDto));
            
            exception.Message.Should().Contain("already exists");
        }

        [Fact]
        public async Task CreateMenuItemAsync_WithSameNameDifferentCategory_CreatesMenuItem()
        {
            // Arrange
            var existingItem = CreateMenuItem("Same Name", "Main Course");
            await _context.MenuItems.AddAsync(existingItem);
            await _context.SaveChangesAsync();

            var createDto = new CreateMenuItemDto
            {
                Name = "Same Name",
                Description = "Same name but different category",
                Price = 8.99m,
                Category = "Appetizers"
            };

            // Act
            var result = await _service.CreateMenuItemAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Same Name");
            result.Category.Should().Be("Appetizers");
        }

        #endregion

        #region UpdateMenuItemAsync Tests

        [Fact]
        public async Task UpdateMenuItemAsync_WithValidData_UpdatesMenuItem()
        {
            // Arrange
            var menuItem = CreateMenuItem("Original Item", "Main Course");
            await _context.MenuItems.AddAsync(menuItem);
            await _context.SaveChangesAsync();

            var updateDto = new UpdateMenuItemDto
            {
                Name = "Updated Item",
                Price = 19.99m,
                Description = "Updated description"
            };

            // Act
            var result = await _service.UpdateMenuItemAsync(menuItem.Id, updateDto);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Updated Item");
            result.Price.Should().Be(19.99m);
            result.Description.Should().Be("Updated description");

            var savedItem = await _context.MenuItems.FindAsync(menuItem.Id);
            savedItem!.Name.Should().Be("Updated Item");
        }

        [Fact]
        public async Task UpdateMenuItemAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var updateDto = new UpdateMenuItemDto { Name = "Updated Item" };

            // Act
            var result = await _service.UpdateMenuItemAsync(999, updateDto);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateMenuItemAsync_WithDuplicateNameInCategory_ThrowsInvalidOperationException()
        {
            // Arrange
            var item1 = CreateMenuItem("Item 1", "Main Course");
            var item2 = CreateMenuItem("Item 2", "Main Course");
            await _context.MenuItems.AddRangeAsync(item1, item2);
            await _context.SaveChangesAsync();

            var updateDto = new UpdateMenuItemDto { Name = "Item 1" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.UpdateMenuItemAsync(item2.Id, updateDto));
            
            exception.Message.Should().Contain("already exists");
        }

        #endregion

        #region DeleteMenuItemAsync Tests

        [Fact]
        public async Task DeleteMenuItemAsync_WithValidId_SoftDeletesMenuItem()
        {
            // Arrange
            var menuItem = CreateMenuItem("Item to Delete", "Main Course");
            await _context.MenuItems.AddAsync(menuItem);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteMenuItemAsync(menuItem.Id);

            // Assert
            result.Should().BeTrue();

            var deletedItem = await _context.MenuItems.FindAsync(menuItem.Id);
            deletedItem!.DeletedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteMenuItemAsync_WithInvalidId_ReturnsFalse()
        {
            // Act
            var result = await _service.DeleteMenuItemAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteMenuItemAsync_WithAlreadyDeletedItem_ReturnsFalse()
        {
            // Arrange
            var menuItem = CreateMenuItem("Already Deleted", "Main Course", deletedAt: DateTime.UtcNow);
            await _context.MenuItems.AddAsync(menuItem);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteMenuItemAsync(menuItem.Id);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region MenuItemExistsAsync Tests

        [Fact]
        public async Task MenuItemExistsAsync_WithExistingItem_ReturnsTrue()
        {
            // Arrange
            var menuItem = CreateMenuItem("Existing Item", "Main Course");
            await _context.MenuItems.AddAsync(menuItem);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.MenuItemExistsAsync("Existing Item", "Main Course");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task MenuItemExistsAsync_WithNonExistingItem_ReturnsFalse()
        {
            // Act
            var result = await _service.MenuItemExistsAsync("Non-existing Item", "Main Course");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task MenuItemExistsAsync_WithExcludeId_ExcludesSpecifiedItem()
        {
            // Arrange
            var menuItem = CreateMenuItem("Test Item", "Main Course");
            await _context.MenuItems.AddAsync(menuItem);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.MenuItemExistsAsync("Test Item", "Main Course", menuItem.Id);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task MenuItemExistsAsync_WithDeletedItem_ReturnsFalse()
        {
            // Arrange
            var menuItem = CreateMenuItem("Deleted Item", "Main Course", deletedAt: DateTime.UtcNow);
            await _context.MenuItems.AddAsync(menuItem);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.MenuItemExistsAsync("Deleted Item", "Main Course");

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Helper Methods

        private List<MenuItem> CreateTestMenuItems(int count)
        {
            var items = new List<MenuItem>();
            var categories = new[] { "Appetizers", "Main Course", "Desserts", "Beverages" };

            for (int i = 1; i <= count; i++)
            {
                items.Add(CreateMenuItem($"Item {i}", categories[i % categories.Length]));
            }

            return items;
        }

        private MenuItem CreateMenuItem(
            string name, 
            string category, 
            string description = "Test description", 
            decimal price = 10.99m,
            List<string>? dietaryTags = null,
            DateTime? deletedAt = null)
        {
            return new MenuItem
            {
                Name = name,
                Description = description,
                Price = price,
                Category = category,
                DietaryTags = dietaryTags != null ? JsonSerializer.Serialize(dietaryTags) : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DeletedAt = deletedAt
            };
        }

        #endregion
    }
}