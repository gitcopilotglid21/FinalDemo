# QuickBite.API.Tests

This project contains comprehensive tests for the QuickBite API, including unit tests, integration tests, and test utilities.

## Test Structure

### Unit Tests

#### Controllers (`Controllers/`)
- **MenuItemsControllerTests.cs**: Tests for the MenuItemsController
  - Tests all HTTP actions (GET, POST, PUT, DELETE)
  - Validates proper status codes and response formats
  - Tests error handling and exception scenarios
  - Uses mocking for service dependencies

#### Services (`Services/`)
- **MenuItemServiceTests.cs**: Tests for the MenuItemService
  - Tests business logic operations
  - Uses in-memory database for data operations
  - Tests validation rules and error conditions
  - Covers all CRUD operations and filtering

### Integration Tests (`Integration/`)
- **MenuItemsIntegrationTests.cs**: End-to-end API tests
  - Tests complete HTTP request/response cycle
  - Uses TestServer for realistic testing environment
  - Validates actual JSON serialization/deserialization
  - Tests API contract compliance

### Test Helpers (`Helpers/`)
- **TestDataFactory.cs**: Factory for creating test data objects
- **TestDbContextFactory.cs**: Helper for creating test database contexts
- **MockLoggerHelper.cs**: Utilities for mocking and verifying loggers

## Dependencies

The test project includes the following key dependencies:

- **xUnit**: Primary testing framework
- **Moq**: Mocking framework for unit tests
- **FluentAssertions**: Fluent assertion library for better test readability
- **AutoFixture**: Automatic test data generation
- **Microsoft.AspNetCore.Mvc.Testing**: Integration testing support
- **Microsoft.EntityFrameworkCore.InMemory**: In-memory database for testing

## Running Tests

### All Tests
```bash
dotnet test
```

### Specific Test Class
```bash
dotnet test --filter "ClassName=MenuItemsControllerTests"
```

### Specific Test Method
```bash
dotnet test --filter "MethodName=GetMenuItems_WithValidParameters_ReturnsOkResult"
```

### With Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Categories

### Unit Tests
- Fast-running tests that test individual components in isolation
- Use mocking to isolate dependencies
- Focus on business logic and controller behavior

### Integration Tests
- Test complete request/response cycles
- Use TestServer with in-memory database
- Validate API contracts and serialization

## Test Data

The `TestDataFactory` class provides methods to create various test objects:

```csharp
// Create a test MenuItem
var menuItem = TestDataFactory.CreateMenuItem(
    name: "Test Item",
    category: "Main Course",
    price: 15.99m);

// Create a test DTO
var createDto = TestDataFactory.CreateCreateMenuItemDto(
    name: "New Item",
    price: 12.50m);

// Create API responses
var successResponse = TestDataFactory.CreateSuccessResponse(data, "Success");
var errorResponse = TestDataFactory.CreateErrorResponse("ERROR_CODE", "Error message");
```

## Database Testing

### In-Memory Database
Tests use Entity Framework's in-memory database provider for fast, isolated tests:

```csharp
// Create test context
using var context = TestDbContextFactory.CreateInMemoryContext();

// Create seeded context
using var context = await TestDbContextFactory.CreateContextWithMenuItemsAsync(10);
```

### Test Data Isolation
Each test class uses a unique database name to ensure isolation:
- Unit tests: Each test gets a fresh context
- Integration tests: Uses WebApplicationFactory with in-memory database

## Mocking

### Service Mocking
```csharp
var mockService = new Mock<IMenuItemService>();
mockService.Setup(s => s.GetMenuItemByIdAsync(1))
          .ReturnsAsync(expectedMenuItem);
```

### Logger Verification
```csharp
mockLogger.VerifyInformation(Times.Once);
mockLogger.VerifyErrorWithException(Times.Once);
```

## Best Practices

1. **Arrange-Act-Assert**: All tests follow the AAA pattern
2. **Descriptive Names**: Test method names clearly describe what is being tested
3. **Single Responsibility**: Each test focuses on one specific behavior
4. **Test Data**: Use TestDataFactory for consistent test data creation
5. **Isolation**: Tests don't depend on each other or external state
6. **Assertions**: Use FluentAssertions for readable and maintainable assertions

## Example Test Structure

```csharp
[Fact]
public async Task MethodUnderTest_Scenario_ExpectedResult()
{
    // Arrange
    var testData = TestDataFactory.CreateTestObject();
    mockService.Setup(/* ... */);

    // Act
    var result = await systemUnderTest.MethodUnderTest(testData);

    // Assert
    result.Should().NotBeNull();
    result.Property.Should().Be(expectedValue);
    mockService.Verify(/* ... */, Times.Once);
}
```

## Test Coverage

The test suite aims for high code coverage across:
- Controllers: All actions and error paths
- Services: All business logic methods
- DTOs: Validation rules
- Exception handling
- API contracts

Run with coverage to see detailed reports:
```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory:./coverage
```