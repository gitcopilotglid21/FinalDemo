using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QuickBite.API.Controllers;
using QuickBite.API.DTOs;
using QuickBite.API.Exceptions;
using QuickBite.API.Services;
using FluentAssertions;
using AutoFixture;

namespace QuickBite.API.Tests.Controllers
{
    public class MenuItemsControllerTests
    {
        private readonly Mock<IMenuItemService> _mockMenuItemService;
        private readonly Mock<ILogger<MenuItemsController>> _mockLogger;
        private readonly MenuItemsController _controller;
        private readonly Fixture _fixture;

        public MenuItemsControllerTests()
        {
            _mockMenuItemService = new Mock<IMenuItemService>();
            _mockLogger = new Mock<ILogger<MenuItemsController>>();
            _controller = new MenuItemsController(_mockMenuItemService.Object, _mockLogger.Object);
            _fixture = new Fixture();
        }

        #region GetMenuItems Tests

        [Fact]
        public async Task GetMenuItems_WithValidParameters_ReturnsOkResult()
        {
            // Arrange
            var expectedResponse = _fixture.Create<PaginatedMenuItemsResponseDto>();
            _mockMenuItemService
                .Setup(s => s.GetMenuItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetMenuItems();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(expectedResponse);
        }

        [Fact]
        public async Task GetMenuItems_WithCustomParameters_CallsServiceWithCorrectParameters()
        {
            // Arrange
            var page = 2;
            var limit = 10;
            var category = "Appetizers";
            var dietaryTags = "Vegetarian,Gluten-Free";
            var search = "salad";
            var expectedResponse = _fixture.Create<PaginatedMenuItemsResponseDto>();

            _mockMenuItemService
                .Setup(s => s.GetMenuItemsAsync(page, limit, category, dietaryTags, search))
                .ReturnsAsync(expectedResponse);

            // Act
            await _controller.GetMenuItems(page, limit, category, dietaryTags, search);

            // Assert
            _mockMenuItemService.Verify(
                s => s.GetMenuItemsAsync(page, limit, category, dietaryTags, search),
                Times.Once);
        }

        [Fact]
        public async Task GetMenuItems_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _mockMenuItemService
                .Setup(s => s.GetMenuItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetMenuItems();

            // Assert
            var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(500);
        }

        #endregion

        #region GetMenuItem Tests

        [Fact]
        public async Task GetMenuItem_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var menuItemId = 1;
            var menuItem = _fixture.Create<MenuItemResponseDto>();
            _mockMenuItemService
                .Setup(s => s.GetMenuItemByIdAsync(menuItemId))
                .ReturnsAsync(menuItem);

            // Act
            var result = await _controller.GetMenuItem(menuItemId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<ApiResponse<MenuItemResponseDto>>().Subject;
            response.Data.Should().Be(menuItem);
            response.Success.Should().BeTrue();
        }

        [Fact]
        public async Task GetMenuItem_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var menuItemId = 999;
            _mockMenuItemService
                .Setup(s => s.GetMenuItemByIdAsync(menuItemId))
                .ReturnsAsync((MenuItemResponseDto?)null);

            // Act
            var result = await _controller.GetMenuItem(menuItemId);

            // Assert
            var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var errorResponse = notFoundResult.Value.Should().BeOfType<ErrorResponse>().Subject;
            errorResponse.Error.Code.Should().Be("NOT_FOUND");
        }

        [Fact]
        public async Task GetMenuItem_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var menuItemId = 1;
            _mockMenuItemService
                .Setup(s => s.GetMenuItemByIdAsync(menuItemId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetMenuItem(menuItemId);

            // Assert
            var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(500);
        }

        #endregion

        #region CreateMenuItem Tests

        [Fact]
        public async Task CreateMenuItem_WithValidData_ReturnsCreatedResult()
        {
            // Arrange
            var createDto = _fixture.Create<CreateMenuItemDto>();
            var createdMenuItem = _fixture.Create<MenuItemResponseDto>();
            _mockMenuItemService
                .Setup(s => s.CreateMenuItemAsync(createDto))
                .ReturnsAsync(createdMenuItem);

            // Act
            var result = await _controller.CreateMenuItem(createDto);

            // Assert
            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var response = createdResult.Value.Should().BeOfType<ApiResponse<MenuItemResponseDto>>().Subject;
            response.Data.Should().Be(createdMenuItem);
            response.Success.Should().BeTrue();
        }

        [Fact]
        public async Task CreateMenuItem_WithDuplicateItem_ReturnsConflict()
        {
            // Arrange
            var createDto = _fixture.Create<CreateMenuItemDto>();
            _mockMenuItemService
                .Setup(s => s.CreateMenuItemAsync(createDto))
                .ThrowsAsync(new InvalidOperationException("Duplicate menu item"));

            // Act
            var result = await _controller.CreateMenuItem(createDto);

            // Assert
            var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
            var errorResponse = conflictResult.Value.Should().BeOfType<ErrorResponse>().Subject;
            errorResponse.Error.Code.Should().Be("DUPLICATE_ITEM");
        }

        [Fact]
        public async Task CreateMenuItem_WithBusinessLogicException_ReturnsBadRequest()
        {
            // Arrange
            var createDto = _fixture.Create<CreateMenuItemDto>();
            var businessException = new BusinessLogicException("INVALID_PRICE", "Price must be positive");
            _mockMenuItemService
                .Setup(s => s.CreateMenuItemAsync(createDto))
                .ThrowsAsync(businessException);

            // Act
            var result = await _controller.CreateMenuItem(createDto);

            // Assert
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var errorResponse = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
            errorResponse.Error.Code.Should().Be("INVALID_PRICE");
        }

        #endregion

        #region UpdateMenuItem Tests

        [Fact]
        public async Task UpdateMenuItem_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var menuItemId = 1;
            var updateDto = _fixture.Create<UpdateMenuItemDto>();
            var updatedMenuItem = _fixture.Create<MenuItemResponseDto>();
            _mockMenuItemService
                .Setup(s => s.UpdateMenuItemAsync(menuItemId, updateDto))
                .ReturnsAsync(updatedMenuItem);

            // Act
            var result = await _controller.UpdateMenuItem(menuItemId, updateDto);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<ApiResponse<MenuItemResponseDto>>().Subject;
            response.Data.Should().Be(updatedMenuItem);
            response.Success.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateMenuItem_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var menuItemId = 999;
            var updateDto = _fixture.Create<UpdateMenuItemDto>();
            _mockMenuItemService
                .Setup(s => s.UpdateMenuItemAsync(menuItemId, updateDto))
                .ReturnsAsync((MenuItemResponseDto?)null);

            // Act
            var result = await _controller.UpdateMenuItem(menuItemId, updateDto);

            // Assert
            var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var errorResponse = notFoundResult.Value.Should().BeOfType<ErrorResponse>().Subject;
            errorResponse.Error.Code.Should().Be("NOT_FOUND");
        }

        [Fact]
        public async Task UpdateMenuItem_WithDuplicateItem_ReturnsConflict()
        {
            // Arrange
            var menuItemId = 1;
            var updateDto = _fixture.Create<UpdateMenuItemDto>();
            _mockMenuItemService
                .Setup(s => s.UpdateMenuItemAsync(menuItemId, updateDto))
                .ThrowsAsync(new InvalidOperationException("Duplicate menu item"));

            // Act
            var result = await _controller.UpdateMenuItem(menuItemId, updateDto);

            // Assert
            var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
            var errorResponse = conflictResult.Value.Should().BeOfType<ErrorResponse>().Subject;
            errorResponse.Error.Code.Should().Be("DUPLICATE_ITEM");
        }

        #endregion

        #region DeleteMenuItem Tests

        [Fact]
        public async Task DeleteMenuItem_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var menuItemId = 1;
            _mockMenuItemService
                .Setup(s => s.DeleteMenuItemAsync(menuItemId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteMenuItem(menuItemId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
            response.Success.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteMenuItem_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var menuItemId = 999;
            _mockMenuItemService
                .Setup(s => s.DeleteMenuItemAsync(menuItemId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteMenuItem(menuItemId);

            // Assert
            var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var errorResponse = notFoundResult.Value.Should().BeOfType<ErrorResponse>().Subject;
            errorResponse.Error.Code.Should().Be("NOT_FOUND");
        }

        [Fact]
        public async Task DeleteMenuItem_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var menuItemId = 1;
            _mockMenuItemService
                .Setup(s => s.DeleteMenuItemAsync(menuItemId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.DeleteMenuItem(menuItemId);

            // Assert
            var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(500);
        }

        #endregion
    }
}