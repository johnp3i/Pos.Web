# Project Structure & Organization

## Solution Architecture

MyChair POS follows a layered, multi-project architecture with clear separation of concerns:

### Application Layer Projects

**POS** - Main point-of-sale application
```
POS/
├── Actions/              # Business action handlers
├── CheckoutHelpers/      # Payment and checkout logic
├── Controls/             # Custom WPF controls
├── Convertres/           # Value converters for data binding
├── CustomExtensions/     # Extension methods
├── Enums/                # Application-specific enumerations
├── ExceptionsHandlers/   # Custom exceptions and error handling
├── Helpers/              # Utility classes (DB, Print, Format, etc.)
├── Keyboard/             # Touch keyboard implementation
├── Models/               # View models and data transfer objects
├── Omas/                 # Online order integration helpers
├── RecoveryMechanism/    # Order snapshot and recovery system
├── Resources/            # Images and static assets
├── StaticData/           # Static data repositories
├── Themes/               # WPF styling and themes
├── ViewHelpers/          # UI helper classes
├── Windows/              # WPF windows and dialogs
├── App.xaml              # Application entry point
└── MainWindow.xaml       # Main application window
```

**OrdersMonitor** - Kitchen/service order tracking
```
OrdersMonitor/
├── Comparables/          # Custom comparers
├── Converters/           # WPF value converters
├── Enums/                # Application enums
├── Helper/               # Utility classes (DB, Device, Text, etc.)
├── Images/               # UI resources
├── Models/               # Data models
├── StaticData/           # Static repositories
├── Windows/              # Dialog windows
└── MainWindow.xaml       # Main monitoring interface
```

### Data Access Layer Projects

**PosDbForAll** - POS database entities
- Entity Framework DbContext for POS database
- Auto-generated entity classes from .edmx
- T4 templates for code generation

**OmasDbForAll** - OMAS database entities
- Entity Framework DbContext for online orders
- Auto-generated entity classes from .edmx
- Enums generated from database lookup tables

**PosDbCentralForAll** - Central database entities
- Shared/central database access

### Infrastructure Projects

**ApplicationsSecurity** - Security and licensing
```
ApplicationsSecurity/
├── Models/               # License and validation models
├── Security/             # Encryption (RSA, AES), key generation
└── Application.cs        # Main application security interface
```

**Security** - Additional security utilities

**ShipData** - Data synchronization utility

**PosServerCommands** - Server command processor

## Code Organization Patterns

### Helpers Pattern
Helper classes provide static utility methods organized by domain:
- `DbHelper` - Database operations and transactions
- `PrintHelper` - Receipt and label printing
- `DeviceHelper` - Hardware device interactions
- `TextHelper` - String formatting and manipulation
- `CalculationsHelper` - Business calculations

### Models Pattern
Models are organized by feature in subdirectories:
```
Models/
├── DeliveryCharge/       # Delivery-related models
├── DocumensHistory/      # Document history models
├── Recipes/              # Recipe and ingredient models
├── SplitPayments/        # Payment splitting models
├── Stock/                # Inventory models
└── VoucherRedeem/        # Voucher models
```

### Windows Pattern
Each WPF window has paired files:
- `.xaml` - UI markup
- `.xaml.cs` - Code-behind with event handlers

### Repository Pattern
Following the JDS repository design guidelines (see repository-standards.md):

**Table Repositories**: Direct table access
- Naming: `[EntityName]Repository.cs`
- Methods: CRUD operations (Insert, Update, Delete, Get)
- Execution: `ExecuteSqlRawAsync()`

**Stored Procedure Repositories**: Execute stored procedures
- Naming: `Get[ProcedureName]Repository.cs`
- Methods: `Get[ProcedureName]Collection()`
- Return: `IEnumerable<T>` or single entity
- Execution: `EXEC [dbo].[ProcName]`

**Base Repository**: `GenericStoredProcedureRepository<T>`
- Provides `ExecuteStoredProcedure()` and `ExecuteSingleRecordStoredProcedure()`

## Naming Conventions

### Files & Classes
- PascalCase for all class names and file names
- Suffix pattern: `Helper`, `Window`, `Model`, `Repository`, `Converter`
- Enum suffix: `Enum` (e.g., `MessageTypesEnum`)

### Database Entities
- Generated from database schema via T4 templates
- Match database table/column names exactly
- Partial classes allow custom extensions

### XAML Resources
- Images stored in `Images/` or `Resources/` folders
- Lowercase with hyphens for image files (e.g., `coffee-bag.png`)
- Embedded as resources in project files

## Configuration

### App.config Structure
```xml
<configuration>
  <connectionStrings>
    <!-- Database connection strings -->
  </connectionStrings>
  <appSettings>
    <!-- Application settings -->
  </appSettings>
  <entityFramework>
    <!-- EF configuration -->
  </entityFramework>
</configuration>
```

### Database-First Workflow
1. Design database schema in SQL Server
2. Update .edmx model from database
3. T4 templates regenerate entity classes
4. Extend entities with partial classes if needed

## Exception Handling

Custom exception types in `ExceptionsHandlers/`:
- `PaymentProcessingException` - Payment-specific errors
- `AppCancellation` - User cancellation handling
- `IOperationScope` / `OperationScope` - Scoped operation tracking

All repository methods use try/catch with rethrow pattern:
```csharp
try
{
    // Database operation
}
catch (Exception)
{
    throw; // Preserve stack trace
}
```

## Static Data & Repositories

Static repositories provide in-memory caching and data access:
- Located in `StaticData/Repository.cs`
- Cache frequently accessed lookup data
- Reduce database round-trips

## Reactive Programming

System.Reactive (Rx) used for:
- Event-driven UI updates
- Asynchronous data streams
- Observable collections
- Throttling and debouncing user input
