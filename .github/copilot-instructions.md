# Copilot Instructions for Gibbon Git Server

## Project Overview

Gibbon Git Server is a self-hosted Git server built with **ASP.NET Core 8.0**. This is a modernized fork of the legacy Bonobo Git Server, migrated from .NET Framework to .NET 8.

## Technology Stack

- **Framework**: ASP.NET Core 8.0 (.NET 8)
- **Language**: C#
- **UI**: Razor Views (MVC pattern)
- **ORM**: Entity Framework Core
- **Database**: SQLite (default), SQL Server (supported)
- **Git Operations**: LibGit2Sharp
- **Testing**: MSTest with NSubstitute for mocking
- **Styling**: SCSS (compiled via AspNetCore.SassCompiler)

## Project Structure

- `Gibbon.Git.Server/` - Main web application
  - `Controllers/` - MVC controllers
  - `Views/` - Razor views
  - `Models/` - Data models and view models
  - `Data/` - Database context and entities
  - `Services/` - Business logic services
  - `Repositories/` - Data access layer
  - `Security/` - Authentication and authorization
  - `Git/` - Git operations and LibGit2Sharp integration
  - `Middleware/` - Custom ASP.NET Core middleware
  - `Migrations/` - Entity Framework migrations
- `Gibbon.Git.Server.Tests/` - Unit tests
- `Gibbon.Git.Server.Integration.Tests/` - Integration tests
- `Gibbon.Git.Server.Benchmarks/` - Performance benchmarks

## Coding Conventions

### Follow Microsoft C# Style Guide

All code must adhere to the [Microsoft C# Style Guide](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).

### Naming Conventions

- **Interfaces**: Must begin with `I` (e.g., `IRepository`, `IUserService`)
- **Classes, Methods, Properties**: Use PascalCase
- **Private fields**: Use camelCase with underscore prefix (e.g., `_fieldName`)
- **Constants**: Use PascalCase

### Code Style Rules

- **No trailing whitespaces** - Ensure clean line endings
- **Consistent line endings** - Use the project's configured line ending style
- **Don't reformat code** unnecessarily - Only format code you're actively modifying
- **Tab width**: 4 spaces (configured in .editorconfig)
- **Nullable reference types**: Enabled for test projects

## Build and Test Commands

### Build
```bash
dotnet restore
dotnet build --configuration Release
```

### Run Tests
```bash
dotnet test --configuration Release --logger "console;verbosity=normal"
```

### Publish
```bash
dotnet publish -c Release -o ./publish
```

### Database Migrations
```bash
dotnet ef database update
```

## Contributing Guidelines

- **Open an issue first** before starting work on new features
- **Link pull requests** to corresponding issues
- **Write meaningful commit messages** that reference issues (e.g., "Add feature X #123")
- **Maintain consistency** with existing design patterns
- **No unnecessary formatting changes** - Keep diffs focused on actual changes

## Testing Practices

- **Framework**: MSTest
- **Mocking**: NSubstitute
- **Test file naming**: `[ClassName]Tests.cs`
- **Test method naming**: Descriptive names explaining what is being tested
- All test projects have nullable reference types enabled
- Tests should be in `Gibbon.Git.Server.Tests/` for unit tests
- Integration tests go in `Gibbon.Git.Server.Integration.Tests/`

## Key Architectural Patterns

- **MVC Pattern**: Controllers handle HTTP requests, Views render UI
- **Repository Pattern**: Data access abstraction via repository classes
- **Service Layer**: Business logic separated into service classes
- **Dependency Injection**: ASP.NET Core's built-in DI container
- **Entity Framework**: Code-first approach with migrations

## Authentication & Security

- **Cookie Authentication**: Primary authentication mechanism
- **Basic Authentication**: For Git operations
- **Role-based Authorization**: Admin role for administrative features
- **HTTPS recommended**: For production deployments

## Configuration

- `appsettings.json` - Main configuration file (not `web.config`)
- Connection strings and authentication settings configured here
- Environment-specific settings via `appsettings.{Environment}.json`

## Important Notes

- This is a **cross-platform** application - works on Windows, Linux, and macOS
- **SQLite is the default database** but SQL Server is also supported
- **LibGit2Sharp** handles all Git operations
- The project is under active development
- Follow the existing patterns and conventions in the codebase

## Links

- [Contributing Guidelines](../CONTRIBUTING.md)
- [Website & Documentation](https://code-ix.github.io/Gibbon-Git-Server/)
- [GitHub Repository](https://github.com/Code-iX/Gibbon-Git-Server)
