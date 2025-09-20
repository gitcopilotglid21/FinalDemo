# QuickBite API 🍽️

A RESTful API for restaurant menu management built with ASP.NET Core 9.0, Entity Framework Core, and SQLite. This API provides comprehensive CRUD operations for managing restaurant menu items with advanced features like filtering, pagination, validation, and dietary tag management.

## 📋 Table of Contents

- [Features](#features)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Docker Deployment](#docker-deployment)
- [API Documentation](#api-documentation)
- [Testing](#testing)
- [Configuration](#configuration)
- [Development](#development)
- [CI/CD](#cicd)
- [Contributing](#contributing)

## ✨ Features

- **Complete CRUD Operations** - Create, Read, Update, Delete menu items
- **Advanced Filtering** - Filter by category, dietary tags, and search terms
- **Pagination** - Efficient data retrieval with configurable page sizes
- **Data Validation** - Comprehensive input validation using FluentValidation
- **Soft Delete** - Menu items are soft-deleted to maintain data integrity
- **Error Handling** - Standardized error responses with detailed information
- **API Documentation** - Interactive Swagger/OpenAPI documentation
- **Database Management** - SQLite database with Entity Framework Core
- **Comprehensive Testing** - Unit tests, integration tests, and test utilities

## 🛠 Technology Stack

- **Framework**: ASP.NET Core 9.0
- **Database**: SQLite with Entity Framework Core 9.0
- **Validation**: FluentValidation 11.3.1
- **Documentation**: Swagger/OpenAPI (Swashbuckle 9.0.4)
- **Testing**: xUnit, Moq, FluentAssertions, AutoFixture
- **CI/CD**: GitHub Actions
- **Code Quality**: Husky pre-commit hooks, dotnet format

## 📁 Project Structure

```
QuickBite/
├── 📄 Business_Requirements_Document.md    # Project requirements and specifications
├── 📄 QuickBite.sln                       # Solution file
├── 📄 README.md                           # This file
├── 📄 Dockerfile                          # Docker container configuration
├── 📄 docker-compose.yml                  # Docker Compose services
├── 📄 docker-compose.override.yml         # Development overrides
├── 📄 .dockerignore                       # Docker build exclusions
│
├── 🔧 .husky/                             # Git hooks for code quality
│   └── pre-commit                         # Pre-commit validation script
│
├── 🔧 .github/workflows/                  # CI/CD pipelines
│   └── ci.yml                            # GitHub Actions workflow
│
├── 📁 src/QuickBite.API/                 # Main API project
│   ├── 📁 Controllers/                    # API controllers
│   │   └── MenuItemsController.cs         # Menu items CRUD operations
│   ├── 📁 Data/                          # Database context and configuration
│   │   └── QuickBiteDbContext.cs         # Entity Framework DbContext
│   ├── 📁 DTOs/                          # Data Transfer Objects
│   │   ├── ApiResponseDtos.cs            # Standardized API responses
│   │   └── MenuItemDtos.cs               # Menu item request/response DTOs
│   ├── 📁 Exceptions/                     # Custom exception classes
│   │   └── BusinessLogicException.cs     # Business rule violations
│   ├── 📁 Middleware/                     # Custom middleware
│   │   └── ValidationExceptionMiddleware.cs # Global validation handling
│   ├── 📁 Models/                         # Entity models
│   │   ├── Category.cs                   # Category enumeration
│   │   ├── DietaryTag.cs                # Dietary tag enumeration
│   │   └── MenuItem.cs                   # Menu item entity
│   ├── 📁 Services/                       # Business logic layer
│   │   ├── IMenuItemService.cs           # Service interface
│   │   └── MenuItemService.cs            # Service implementation
│   ├── 📁 Validators/                     # Input validation rules
│   │   ├── CreateMenuItemDtoValidator.cs # Create validation
│   │   ├── QueryValidators.cs            # Query parameter validation
│   │   └── UpdateMenuItemDtoValidator.cs # Update validation
│   ├── 📄 Program.cs                     # Application entry point
│   ├── 📄 appsettings.json              # Configuration settings
│   ├── 📄 appsettings.Development.json  # Development settings
│   └── 📄 QuickBite.API.csproj          # Project file
│
└── 📁 tests/QuickBite.API.Tests/         # Test project
    ├── 📁 Controllers/                    # Controller unit tests
    │   └── MenuItemsControllerTests.cs   # Controller test suite
    ├── 📁 Services/                       # Service unit tests
    │   └── MenuItemServiceTests.cs       # Service test suite
    ├── 📁 Helpers/                        # Test utilities
    │   ├── TestDataFactory.cs           # Test data generation
    │   ├── TestDbContextFactory.cs      # Test database setup
    │   └── MockLoggerHelper.cs          # Logger mocking utilities
    ├── 📄 SampleTests.cs                # Sample test demonstrations
    ├── 📄 README.md                     # Test project documentation
    └── 📄 QuickBite.API.Tests.csproj   # Test project file
```

## 📋 Prerequisites

### For Local Development
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Git](https://git-scm.com/) (for version control)
- [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio 2022](https://visualstudio.microsoft.com/) (recommended)

### For Docker Development
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (includes Docker and Docker Compose)
- [Git](https://git-scm.com/) (for version control)

## 🚀 Getting Started

You can run the QuickBite API in two ways: locally with .NET or using Docker containers.

### Option 1: Local Development with .NET

#### 1. Clone the Repository

```bash
git clone https://github.com/gitcopilotglid21/FinalDemo.git
cd FinalDemo
```

#### 2. Restore Dependencies

```bash
dotnet restore
```

#### 3. Build the Solution

```bash
dotnet build
```

#### 4. Run the API

```bash
cd src/QuickBite.API
dotnet run
```

The API will be available at:
- **HTTP**: http://localhost:5280
- **HTTPS**: https://localhost:7180 (if configured)
- **Swagger UI**: http://localhost:5280 (opens automatically in development)

#### 5. Test the API

Open your browser and navigate to http://localhost:5280 to access the interactive Swagger documentation.

### Option 2: Docker Development

#### 1. Clone the Repository

```bash
git clone https://github.com/gitcopilotglid21/FinalDemo.git
cd FinalDemo
```

#### 2. Build and Run with Docker Compose

```bash
# Build and start the services
docker-compose up --build

# Or run in detached mode
docker-compose up --build -d
```

The API will be available at:
- **HTTP**: http://localhost:5280
- **Swagger UI**: http://localhost:5280

## 🐳 Docker Deployment

### Docker Architecture

The project includes comprehensive Docker support with:

- **Multi-stage Dockerfile**: Optimized for both development and production
- **Docker Compose**: Local development environment
- **Development Override**: Hot reload and debugging support
- **Volume Management**: Persistent SQLite database storage
- **Health Checks**: Container health monitoring

### Docker Files

| File | Purpose |
|------|---------|
| `Dockerfile` | Multi-stage build for optimized production images |
| `docker-compose.yml` | Base configuration for all environments |
| `docker-compose.override.yml` | Development-specific settings (auto-loaded) |
| `.dockerignore` | Excludes unnecessary files from build context |

### Environment Variables

Docker containers support these environment variables:

```bash
# ASP.NET Core Configuration
ASPNETCORE_ENVIRONMENT=Development|Production
ASPNETCORE_URLS=http://+:8080

# Database Configuration
ConnectionStrings__DefaultConnection=Data Source=/app/data/quickbite.db
```

### Docker Volumes

- `quickbite-data`: Persistent SQLite database storage
- `nuget-cache`: NuGet package cache for faster builds (development)

### Production Deployment

For production deployment, use the base docker-compose.yml without the override:

```bash
# Production deployment
docker-compose -f docker-compose.yml up --build -d

# Or build and tag for registry
docker build -t your-registry/quickbite-api:v1.0.0 .
docker push your-registry/quickbite-api:v1.0.0
```

### Container Health Monitoring

The API container includes health checks that monitor:
- Application availability on port 8080
- Response time and service health
- Container restart policies for resilience

#### 3. Docker Development Commands

```bash
# View logs
docker-compose logs -f quickbite-api

# Stop services
docker-compose down

# Remove volumes (reset database)
docker-compose down -v

# Rebuild without cache
docker-compose build --no-cache

# Run tests in container
docker-compose exec quickbite-api dotnet test
```

#### 4. Production Docker Build

```bash
# Build production image
docker build -t quickbite-api:latest .

# Run production container
docker run -d \
  --name quickbite-api \
  -p 8080:8080 \
  -v quickbite-data:/app/data \
  quickbite-api:latest
```

## 📚 API Documentation

### Base URL
```
http://localhost:5280/api
```

### Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/menuitems` | Get all menu items with optional filtering |
| GET | `/menuitems/{id}` | Get a specific menu item by ID |
| POST | `/menuitems` | Create a new menu item |
| PUT | `/menuitems/{id}` | Update an existing menu item |
| DELETE | `/menuitems/{id}` | Delete a menu item (soft delete) |

### Sample API Calls

#### Get All Menu Items
```bash
curl -X GET "http://localhost:5280/api/menuitems?page=1&limit=10"
```

#### Create a Menu Item
```bash
curl -X POST "http://localhost:5280/api/menuitems" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Margherita Pizza",
    "description": "Classic Italian pizza with tomato and mozzarella",
    "price": 12.99,
    "category": "Main Course",
    "dietaryTags": ["Vegetarian"]
  }'
```

#### Update a Menu Item
```bash
curl -X PUT "http://localhost:5280/api/menuitems/1" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Margherita Pizza Deluxe",
    "price": 14.99,
    "dietaryTags": ["Vegetarian", "Organic"]
  }'
```

### Query Parameters

- **page**: Page number (default: 1)
- **limit**: Items per page (default: 20, max: 100)
- **category**: Filter by category
- **dietaryTags**: Filter by dietary tags (comma-separated)
- **search**: Search in name or description

### Response Format

All API responses follow a standardized format:

#### Success Response
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { /* response data */ },
  "timestamp": "2025-09-20T12:00:00Z"
}
```

#### Error Response
```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "Error description",
    "details": "Additional error details",
    "timestamp": "2025-09-20T12:00:00Z"
  }
}
```

## 🧪 Testing

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Category
```bash
# Unit tests only
dotnet test --filter "FullyQualifiedName!~Integration"

# Controller tests only
dotnet test --filter "MenuItemsControllerTests"

# Service tests only
dotnet test --filter "MenuItemServiceTests"
```

### Test Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Test Structure

- **Unit Tests**: Fast, isolated tests with mocking
- **Service Tests**: Business logic tests with in-memory database
- **Helper Tests**: Test utility validation

Current test coverage:
- ✅ **45 tests** passing
- ✅ Controller actions and responses
- ✅ Service business logic
- ✅ Data operations
- ✅ Error handling
- ✅ Validation rules

## ⚙️ Configuration

### Database

The API uses SQLite by default. The database file (`quickbite.db`) is created automatically in the API project directory.

### Connection String

Default connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=quickbite.db"
  }
}
```

### Environment Variables

- `ASPNETCORE_ENVIRONMENT`: Set to `Development`, `Staging`, or `Production`
- Custom connection strings can be set via environment variables

### Validation Rules

- **Name**: 1-100 characters, required
- **Description**: 1-500 characters, required
- **Price**: Positive decimal value, required
- **Category**: Must be from predefined list
- **Dietary Tags**: Optional array of predefined tags

## 💻 Development

### Code Formatting
```bash
dotnet format
```

### Database Migrations
```bash
# Add new migration
cd src/QuickBite.API
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update
```

### Pre-commit Hooks

The project uses Husky for pre-commit hooks that automatically:
- Build the project
- Run tests
- Check code formatting
- Validate code quality

### Development Tools

Recommended VS Code extensions:
- C# Dev Kit
- REST Client (for testing APIs)
- SQLite Viewer (for database inspection)

## 🔄 CI/CD

### GitHub Actions

The project includes a comprehensive CI/CD pipeline (`.github/workflows/ci.yml`) that:

- ✅ Builds the application
- ✅ Runs all tests
- ✅ Validates code formatting
- ✅ Checks for compilation errors

### Workflow Triggers

- **Pull Requests** to `master` branch
- **Manual workflow dispatch**

### Pipeline Steps

1. **Setup**: Install .NET 9.0
2. **Restore**: Download dependencies
3. **Build**: Compile the application
4. **Test**: Run all unit tests
5. **Format**: Check code formatting compliance

## 🤝 Contributing

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/AmazingFeature`)
3. **Commit** your changes (`git commit -m 'Add some AmazingFeature'`)
4. **Push** to the branch (`git push origin feature/AmazingFeature`)
5. **Open** a Pull Request

### Development Guidelines

- Follow C# coding conventions
- Write unit tests for new features
- Update documentation as needed
- Ensure all tests pass before submitting PR
- Use meaningful commit messages

### Code Quality

- All code must pass `dotnet format` validation
- Maintain test coverage above 80%
- Follow SOLID principles
- Use dependency injection
- Implement proper error handling

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Support

For questions and support:
- Create an [Issue](https://github.com/gitcopilotglid21/FinalDemo/issues)
- Check the [Business Requirements Document](Business_Requirements_Document.md)
- Review the [API Documentation](#api-documentation)

---

**QuickBite API** - Built with ❤️ using ASP.NET Core