# SpotMapApi Commands and Conventions

## Build and Run Commands
- Build: `dotnet build`
- Run: `dotnet run`
- Watch mode: `dotnet watch run`
- Test all: `dotnet test`
- Test specific: `dotnet test --filter "FullyQualifiedName~TestNamespace.TestClass.TestMethod"`
- Create migration: `dotnet ef migrations add <MigrationName>`
- Update database: `dotnet ef database update`

## Code Style Guidelines
- C# 12 with nullable reference types enabled
- Use record types for DTOs and immutable data models
- Minimal API approach for endpoint definitions
- Async/await for all IO and database operations
- Error handling with appropriate HTTP status codes
- LINQ for database queries with strong typing
- JWT authentication with Google Auth integration

## Project Structure
- Features folder for domain-specific endpoints
- Data folder for DbContext and migrations
- Models folder for entity definitions
- Services folder for business logic
- SQL Server with EF Core
- Swagger/OpenAPI for API documentation