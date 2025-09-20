using QuickBite.API.Tests.Helpers;
using FluentAssertions;

namespace QuickBite.API.Tests
{
    /// <summary>
    /// Sample test class to demonstrate test setup and verify configuration
    /// </summary>
    public class SampleTests
    {
        [Fact]
        public void TestDataFactory_CreateMenuItem_ShouldCreateValidMenuItem()
        {
            // Act
            var menuItem = TestDataFactory.CreateMenuItem(
                name: "Test Item",
                category: "Main Course",
                price: 15.99m);

            // Assert
            menuItem.Should().NotBeNull();
            menuItem.Name.Should().Be("Test Item");
            menuItem.Category.Should().Be("Main Course");
            menuItem.Price.Should().Be(15.99m);
            menuItem.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        }

        [Fact]
        public void TestDataFactory_CreateCreateMenuItemDto_ShouldCreateValidDto()
        {
            // Act
            var dto = TestDataFactory.CreateCreateMenuItemDto(
                name: "Test DTO",
                category: "Appetizers",
                price: 8.50m);

            // Assert
            dto.Should().NotBeNull();
            dto.Name.Should().Be("Test DTO");
            dto.Category.Should().Be("Appetizers");
            dto.Price.Should().Be(8.50m);
        }

        [Theory]
        [InlineData("Appetizers")]
        [InlineData("Main Course")]
        [InlineData("Desserts")]
        [InlineData("Beverages")]
        public void TestDataFactory_CreateMenuItem_ShouldAcceptValidCategories(string category)
        {
            // Act
            var menuItem = TestDataFactory.CreateMenuItem(category: category);

            // Assert
            menuItem.Category.Should().Be(category);
        }

        [Fact]
        public void ApiResponse_SuccessResponse_ShouldCreateValidResponse()
        {
            // Arrange
            var data = TestDataFactory.CreateMenuItemResponseDto();

            // Act
            var response = TestDataFactory.CreateSuccessResponse(data, "Test message");

            // Assert
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Message.Should().Be("Test message");
            response.Data.Should().Be(data);
        }

        [Fact]
        public void ErrorResponse_ShouldCreateValidErrorResponse()
        {
            // Act
            var errorResponse = TestDataFactory.CreateErrorResponse(
                code: "TEST_ERROR",
                message: "Test error message",
                details: "Additional details");

            // Assert
            errorResponse.Should().NotBeNull();
            errorResponse.Error.Should().NotBeNull();
            errorResponse.Error.Code.Should().Be("TEST_ERROR");
            errorResponse.Error.Message.Should().Be("Test error message");
            errorResponse.Error.Details.Should().Be("Additional details");
            errorResponse.Error.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        }
    }
}