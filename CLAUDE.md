# SpotMapApi Commands and Conventions

## Build and Run Commands
- Build: `dotnet build`
- Run: `dotnet run`
- Watch mode: `dotnet watch run`
- Test: `dotnet test`
- Migrations: `dotnet ef migrations add <MigrationName>`, `dotnet ef database update`

## Code Style Guidelines
- Use C# record types for DTOs and immutable data models
- Nullable reference types enabled (`<Nullable>enable</Nullable>`)
- Use minimal API approach for endpoint definitions
- Method naming: PascalCase for methods, camelCase for parameters
- Use async/await for all database operations
- Error handling: Return appropriate HTTP status codes (404, 400, etc.)
- Prefer LINQ for database queries

## Project Structure
- Entity models and DbContext in separate files
- Use of DotNetEnv for environment variable management
- SQL Server for production, SQLite for development
- Entity Framework Core for data access
- Swagger/OpenAPI for API documentation