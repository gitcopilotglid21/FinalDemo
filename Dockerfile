# Use the official .NET 9.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set the working directory
WORKDIR /app

# Copy solution file
COPY QuickBite.sln ./

# Copy project files
COPY src/QuickBite.API/QuickBite.API.csproj ./src/QuickBite.API/
COPY tests/QuickBite.API.Tests/QuickBite.API.Tests.csproj ./tests/QuickBite.API.Tests/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR /app/src/QuickBite.API
RUN dotnet build -c Release -o /app/build

# Publish the application
RUN dotnet publish -c Release -o /app/publish --no-restore

# Use the official .NET 9.0 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# Set the working directory
WORKDIR /app

# Create a non-root user for security
RUN adduser --disabled-password --home /app --gecos '' appuser && chown -R appuser /app
USER appuser

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Create data directory for SQLite database
RUN mkdir -p /app/data

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Expose the port
EXPOSE 8080

# Set the entry point
ENTRYPOINT ["dotnet", "QuickBite.API.dll"]