# Entity Framework Migrations Guide

## Overview

This guide explains how to create and apply Entity Framework migrations for the WebPosMembership database. Since the database schema was initially created via SQL script, migrations are optional but recommended for future schema changes.

## Prerequisites

- .NET 8.0 SDK installed
- Entity Framework Core tools installed globally:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

## Creating Initial Migration

To create an initial migration that matches the existing database schema:

```bash
# Navigate to solution root
cd Pos.Web

# Create migration
dotnet ef migrations add InitialCreate \
  --context WebPosMembershipDbContext \
  --project Pos.Web.Infrastructure \
  --startup-project Pos.Web.API \
  --output-dir Data/Migrations
```

## Applying Migrations

### Option 1: Apply via Command Line

```bash
# Apply all pending migrations
dotnet ef database update \
  --context WebPosMembershipDbContext \
  --project Pos.Web.Infrastructure \
  --startup-project Pos.Web.API
```

### Option 2: Apply on Application Startup (Already Configured)

The `DbInitializer.InitializeAsync()` method in Program.cs automatically ensures the database is created on startup using:

```csharp
await context.Database.EnsureCreatedAsync();
```

**Note**: `EnsureCreatedAsync()` does NOT apply migrations. It only creates the database if it doesn't exist. For production, consider using migrations explicitly.

## Future Schema Changes

When you need to modify the database schema:

1. **Update Entity Models**: Modify the entity classes in `Pos.Web.Infrastructure/Entities/`

2. **Create Migration**:
   ```bash
   dotnet ef migrations add <MigrationName> \
     --context WebPosMembershipDbContext \
     --project Pos.Web.Infrastructure \
     --startup-project Pos.Web.API
   ```

3. **Review Migration**: Check the generated migration file in `Data/Migrations/`

4. **Apply Migration**:
   ```bash
   dotnet ef database update \
     --context WebPosMembershipDbContext \
     --project Pos.Web.Infrastructure \
     --startup-project Pos.Web.API
   ```

## Common Migration Commands

### List All Migrations
```bash
dotnet ef migrations list \
  --context WebPosMembershipDbContext \
  --project Pos.Web.Infrastructure \
  --startup-project Pos.Web.API
```

### Remove Last Migration (if not applied)
```bash
dotnet ef migrations remove \
  --context WebPosMembershipDbContext \
  --project Pos.Web.Infrastructure \
  --startup-project Pos.Web.API
```

### Rollback to Specific Migration
```bash
dotnet ef database update <MigrationName> \
  --context WebPosMembershipDbContext \
  --project Pos.Web.Infrastructure \
  --startup-project Pos.Web.API
```

### Generate SQL Script from Migrations
```bash
dotnet ef migrations script \
  --context WebPosMembershipDbContext \
  --project Pos.Web.Infrastructure \
  --startup-project Pos.Web.API \
  --output migration.sql
```

## Production Deployment

For production deployments, it's recommended to:

1. **Generate SQL Script**: Create a SQL script from migrations
2. **Review Script**: Have DBA review the script
3. **Apply Manually**: Execute the script during deployment window
4. **Backup First**: Always backup the database before applying migrations

```bash
# Generate production migration script
dotnet ef migrations script \
  --context WebPosMembershipDbContext \
  --project Pos.Web.Infrastructure \
  --startup-project Pos.Web.API \
  --idempotent \
  --output production-migration.sql
```

The `--idempotent` flag ensures the script can be run multiple times safely.

## Troubleshooting

### Error: "No DbContext was found"
- Ensure you're running the command from the solution root
- Verify the project paths are correct
- Check that WebPosMembershipDbContext is properly registered in Program.cs

### Error: "Connection string not found"
- Verify `appsettings.json` contains the `WebPosMembership` connection string
- Ensure the connection string is valid and the database server is accessible

### Error: "Build failed"
- Run `dotnet build` first to ensure the solution compiles
- Check for any compilation errors in the entity models or DbContext

## Current Status

✅ Database schema created via SQL script (`.kiro/specs/web-pos-membership-database/database-schema.sql`)
✅ DbContext configured with all entities and relationships
✅ ASP.NET Core Identity configured in Program.cs
✅ DbInitializer configured to seed roles on startup

**Migrations are optional** since the database already exists. However, creating an initial migration is recommended for tracking future schema changes.
