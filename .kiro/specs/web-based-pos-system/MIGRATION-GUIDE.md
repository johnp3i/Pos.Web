# Migration Guide: Organizing .kiro Folders Between Legacy and New Solutions

## Overview

This guide explains how to organize the `.kiro` folders between the legacy MyChairPos solution and the new Pos.Web solution.

## Current State

**MyChairPos/.kiro/** (Legacy Solution)
```
MyChairPos/
├── .kiro/
│   ├── specs/
│   │   └── web-based-pos-system/        # ← Needs to move to Pos.Web
│   │       ├── requirements.md
│   │       ├── design.md
│   │       ├── tasks.md
│   │       ├── blazor-project-structure.md
│   │       ├── blazor-pos-examples.md
│   │       ├── mvc-vs-blazor-comparison.md
│   │       ├── browser-compatibility.md
│   │       ├── database-scripts.sql
│   │       ├── pos-web-project-structure.md
│   │       └── .config.kiro
│   └── steering/
│       ├── product.md                   # ← Legacy WPF POS product overview
│       ├── tech.md                      # ← Legacy .NET Framework 4.8, WPF, EF6
│       ├── structure.md                 # ← Legacy multi-project structure
│       ├── repository-standards.md      # ← Keep (applies to both)
│       ├── character-standards.md       # ← Keep (applies to both)
│       ├── pos-code-review.md           # ← Legacy code review
│       ├── refactoring-strategy.md      # ← Legacy V2 refactoring strategy
│       ├── pos-layout-design-pattern.md # ← Legacy WPF layout patterns
│       └── pos-code-structure.md        # ← Legacy code structure analysis
```


## Target State

### Legacy Solution (MyChairPos/.kiro/)

**Keep legacy-specific steering files:**
```
MyChairPos/
├── .kiro/
│   ├── specs/
│   │   └── (empty - web-based-pos-system moved to Pos.Web)
│   └── steering/
│       ├── product.md                   # Legacy WPF POS product overview
│       ├── tech.md                      # Legacy .NET Framework 4.8, WPF, EF6
│       ├── structure.md                 # Legacy multi-project structure
│       ├── repository-standards.md      # Shared standards (copy to both)
│       ├── character-standards.md       # Shared standards (copy to both)
│       ├── pos-code-review.md           # Legacy code review
│       ├── refactoring-strategy.md      # Legacy V2 refactoring strategy
│       ├── pos-layout-design-pattern.md # Legacy WPF layout patterns
│       └── pos-code-structure.md        # Legacy code structure analysis
```

### New Solution (Pos.Web/.kiro/)

**Create new web-specific steering files:**
```
Pos.Web/
├── .kiro/
│   ├── specs/
│   │   └── web-based-pos-system/        # ← Moved from MyChairPos
│   │       ├── requirements.md
│   │       ├── design.md
│   │       ├── tasks.md
│   │       ├── blazor-project-structure.md
│   │       ├── blazor-pos-examples.md
│   │       ├── mvc-vs-blazor-comparison.md
│   │       ├── browser-compatibility.md
│   │       ├── database-scripts.sql
│   │       ├── pos-web-project-structure.md
│   │       ├── MIGRATION-GUIDE.md       # ← This file
│   │       └── .config.kiro
│   └── steering/
│       ├── product.md                   # NEW: Web POS product overview
│       ├── tech.md                      # NEW: .NET 8, Blazor, ASP.NET Core 8
│       ├── structure.md                 # NEW: 5-project architecture
│       ├── repository-standards.md      # COPY: From legacy (still relevant)
│       ├── character-standards.md       # COPY: From legacy (still relevant)
│       ├── blazor-patterns.md           # NEW: Blazor component patterns
│       └── api-design.md                # NEW: RESTful API conventions
```


## Step-by-Step Migration Instructions

### Step 1: Create Pos.Web Directory Structure

**IMPORTANT**: You must manually create the Pos.Web directory as a sibling to MyChairPos (Kiro cannot create directories outside the current workspace).

```bash
# Navigate to parent directory (where MyChairPos is located)
cd /path/to/parent-directory

# Create new solution directory
mkdir Pos.Web
cd Pos.Web

# Create .kiro folder structure
mkdir -p .kiro/specs
mkdir -p .kiro/steering
```

### Step 2: Move Spec Files

**Manually move** the entire `web-based-pos-system` folder from legacy to new solution:

```bash
# From parent directory
mv MyChairPos/.kiro/specs/web-based-pos-system Pos.Web/.kiro/specs/
```

**Files being moved:**
- requirements.md
- design.md
- tasks.md
- blazor-project-structure.md
- blazor-pos-examples.md
- mvc-vs-blazor-comparison.md
- browser-compatibility.md
- database-scripts.sql
- pos-web-project-structure.md
- MIGRATION-GUIDE.md (this file)
- .config.kiro

### Step 3: Create New Steering Files for Pos.Web

Create the following new steering files in `Pos.Web/.kiro/steering/`:

#### 3.1 product.md (Web POS Product Overview)

```markdown
# Pos.Web - Web-Based POS System

## Overview

Pos.Web is a modern, cloud-ready Point of Sale system built with Blazor WebAssembly and ASP.NET Core 8. It provides a responsive, cross-platform interface for cafes and restaurants with offline-first capabilities.

## Core Applications

- **Pos.Web.Client**: Progressive Web App (PWA) frontend with offline support
- **Pos.Web.API**: RESTful API with SignalR for real-time communication
- **Pos.Web.Infrastructure**: Data access layer with EF Core 8
- **Pos.Web.Shared**: Shared DTOs, models, and business logic
- **Pos.Web.Tests**: Comprehensive test suite (unit, integration, property-based)

## Key Features

- Progressive Web App (PWA) with offline support
- Real-time order updates via SignalR
- Responsive design (desktop, tablet, mobile)
- Role-based interfaces (Cashier, Waiter, Kitchen)
- Optimistic locking for concurrent order editing
- Browser-based printing
- Local storage for offline orders
- Automatic sync when connection restored

## Technology Stack

- Frontend: Blazor WebAssembly, MudBlazor, Fluxor
- Backend: ASP.NET Core 8 Web API, SignalR
- Database: SQL Server (shared with legacy system)
- Caching: Redis
- Testing: xUnit, FsCheck (property-based testing)

## Database Strategy

- **Schema separation**: 
  - `dbo` schema: Read existing legacy data
  - `web` schema: New web-specific tables (OrderLocks, ApiAuditLog, UserSessions, FeatureFlags, SyncQueue)
- **Shared database**: Both legacy WPF POS and new Web POS use same SQL Server database
```


#### 3.2 tech.md (Technology Stack)

```markdown
# Technology Stack

## Framework & Language

- **Language**: C# 12
- **Target Framework**: .NET 8
- **Frontend Framework**: Blazor WebAssembly
- **Backend Framework**: ASP.NET Core 8 Web API
- **Build System**: dotnet CLI, MSBuild

## Data Access

- **ORM**: Entity Framework Core 8
- **Database**: Microsoft SQL Server (shared with legacy system)
- **Schema Strategy**: 
  - `dbo` schema: Read existing legacy data
  - `web` schema: New web-specific tables
- **Connection**: ADO.NET with EF Core DbContext
- **Migrations**: EF Core Code-First migrations

## Key Libraries & Dependencies

### Frontend (Pos.Web.Client)
- **MudBlazor** 6.11.0+ - Material Design component library
- **Fluxor** 5.9.0+ - Flux/Redux state management
- **Blazored.LocalStorage** 4.4.0+ - Browser local storage
- **Microsoft.AspNetCore.SignalR.Client** 8.0.0+ - Real-time communication

### Backend (Pos.Web.API)
- **Microsoft.AspNetCore.SignalR** 8.0.0+ - Real-time hubs
- **AutoMapper** 12.0.0+ - Object mapping
- **Serilog.AspNetCore** 8.0.0+ - Structured logging
- **Swashbuckle.AspNetCore** 6.5.0+ - OpenAPI/Swagger

### Infrastructure (Pos.Web.Infrastructure)
- **Microsoft.EntityFrameworkCore** 8.0.0+ - ORM
- **Microsoft.EntityFrameworkCore.SqlServer** 8.0.0+ - SQL Server provider
- **StackExchange.Redis** 2.7.0+ - Redis caching

### Shared (Pos.Web.Shared)
- **FluentValidation** 11.9.0+ - Validation library

### Testing (Pos.Web.Tests)
- **xUnit** 2.6.0+ - Test framework
- **Moq** 4.20.0+ - Mocking library
- **FluentAssertions** 6.12.0+ - Assertion library
- **FsCheck** 2.16.0+ - Property-based testing
- **Microsoft.AspNetCore.Mvc.Testing** 8.0.0+ - Integration testing

## Project Structure

The solution follows Clean Architecture with 5 projects:

```
Pos.Web.sln
├── src/
│   ├── Pos.Web.Shared/           (Class Library - .NET 8)
│   ├── Pos.Web.Infrastructure/   (Class Library - .NET 8)
│   ├── Pos.Web.API/              (ASP.NET Core Web API - .NET 8)
│   ├── Pos.Web.Client/           (Blazor WebAssembly - .NET 8)
│   └── Pos.Web.Tests/            (xUnit Test Project - .NET 8)
```

## Common Commands

### Building
```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build src/Pos.Web.API

# Clean and rebuild
dotnet clean && dotnet build
```

### Running
```bash
# Run API
dotnet run --project src/Pos.Web.API

# Run Client
dotnet run --project src/Pos.Web.Client

# Watch mode (auto-reload)
dotnet watch --project src/Pos.Web.Client
```

### Testing
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test src/Pos.Web.Tests
```

### Database Operations
```bash
# Add migration
dotnet ef migrations add InitialCreate --project src/Pos.Web.Infrastructure --startup-project src/Pos.Web.API

# Update database
dotnet ef database update --project src/Pos.Web.Infrastructure --startup-project src/Pos.Web.API

# Generate SQL script
dotnet ef migrations script --project src/Pos.Web.Infrastructure --startup-project src/Pos.Web.API
```

## Development Environment

- **IDE**: Visual Studio 2022 or Visual Studio Code with C# Dev Kit
- **Required SDKs**: .NET 8 SDK
- **Database Tools**: SQL Server Management Studio or Azure Data Studio
- **Version Control**: Git
- **Browser**: Chrome, Edge, Firefox, Safari (for testing)

## Deployment

- **API**: Docker container or Azure App Service
- **Client**: Static web hosting (Azure Static Web Apps, Netlify, Vercel)
- **Database**: SQL Server (on-premises or Azure SQL Database)
- **Cache**: Redis (Azure Cache for Redis or self-hosted)
```


#### 3.3 structure.md (Project Structure & Organization)

```markdown
# Project Structure & Organization

## Solution Architecture

Pos.Web follows Clean Architecture with clear separation of concerns across 5 projects:

### Application Layer Projects

**Pos.Web.Client** - Blazor WebAssembly frontend
```
Pos.Web.Client/
├── Pages/                # Routable pages (Index, Cashier, Waiter, Kitchen)
├── Components/           # Reusable components (Products, Cart, Customers, Kitchen)
├── Layouts/              # Layout components (MainLayout, CashierLayout, TabletLayout)
├── Store/                # Fluxor state management (OrderState, CustomerState, ProductState)
├── Services/             # Client services (API clients, SignalR clients, offline storage)
└── wwwroot/              # Static assets (CSS, images, service worker)
```

**Pos.Web.API** - ASP.NET Core Web API
```
Pos.Web.API/
├── Controllers/          # API controllers (Orders, Payments, Customers, Products)
├── Hubs/                 # SignalR hubs (KitchenHub, OrderLockHub, ServerCommandHub)
├── Services/             # Business logic services (OrderService, PaymentService)
├── Middleware/           # Custom middleware (Exception handling, logging, audit)
├── Filters/              # Action filters (Validation, feature flags)
└── Mapping/              # AutoMapper profiles
```

### Data Access Layer Projects

**Pos.Web.Infrastructure** - Data access and infrastructure
```
Pos.Web.Infrastructure/
├── Data/                 # EF Core DbContext and configurations
├── Entities/             # EF Core entities
│   ├── Dbo/              # Legacy dbo schema entities (Invoice, Customer, Product)
│   └── Web/              # New web schema entities (OrderLock, ApiAuditLog, UserSession)
├── Repositories/         # Repository pattern implementations
├── UnitOfWork/           # Unit of Work pattern
└── Services/             # Infrastructure services (Cache, FeatureFlags, AuditLog)
```

### Shared Layer

**Pos.Web.Shared** - Shared DTOs and models
```
Pos.Web.Shared/
├── DTOs/                 # Data Transfer Objects (Orders, Customers, Products, Payments)
├── Models/               # Domain models (SignalR messages, configuration)
├── Enums/                # Enumerations (OrderStatus, PaymentMethod, ServiceType)
├── Constants/            # Application constants (API routes, SignalR names, cache keys)
└── Validators/           # FluentValidation validators
```

### Test Layer

**Pos.Web.Tests** - Comprehensive test suite
```
Pos.Web.Tests/
├── Unit/                 # Unit tests (Services, Repositories, Validators)
├── Integration/          # Integration tests (API, Database, SignalR)
├── PropertyBased/        # Property-based tests (FsCheck)
├── Fixtures/             # Test fixtures (Database, WebApplication, SignalR)
└── Helpers/              # Test helpers (TestDataBuilder, MockFactory)
```

## Code Organization Patterns

### Repository Pattern
Following the JDS repository design guidelines:

**Generic Repository**: Base repository with common CRUD operations
```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}
```

**Specific Repositories**: Domain-specific repositories
```csharp
public interface IOrderRepository : IRepository<Invoice>
{
    Task<List<Invoice>> GetPendingOrdersAsync();
    Task<List<Invoice>> GetOrdersByCustomerAsync(int customerId);
    Task<Invoice?> GetOrderWithItemsAsync(int orderId);
}
```

### Unit of Work Pattern
Coordinates multiple repositories and manages transactions:
```csharp
public interface IUnitOfWork : IDisposable
{
    IOrderRepository Orders { get; }
    ICustomerRepository Customers { get; }
    IProductRepository Products { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

### Service Layer Pattern
Business logic separated from controllers:
```csharp
public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
    Task<OrderDto?> GetOrderByIdAsync(int id);
    Task<List<OrderDto>> GetPendingOrdersAsync();
    Task<bool> UpdateOrderAsync(int id, UpdateOrderRequest request);
}
```

### Fluxor State Management (Client)
Flux/Redux pattern for predictable state management:
```
Action → Reducer → State → UI
   ↑                        ↓
   └────── Effect ──────────┘
```

**State**: Immutable state container
```csharp
public record OrderState
{
    public OrderDto? CurrentOrder { get; init; }
    public List<OrderDto> PendingOrders { get; init; } = new();
    public bool IsLoading { get; init; }
    public string? ErrorMessage { get; init; }
}
```

**Actions**: Events that trigger state changes
```csharp
public record LoadPendingOrdersAction;
public record LoadPendingOrdersSuccessAction(List<OrderDto> Orders);
public record LoadPendingOrdersFailureAction(string ErrorMessage);
```

**Reducers**: Pure functions that update state
```csharp
public static class OrderReducers
{
    [ReducerMethod]
    public static OrderState ReduceLoadPendingOrdersAction(OrderState state, LoadPendingOrdersAction action)
        => state with { IsLoading = true, ErrorMessage = null };
    
    [ReducerMethod]
    public static OrderState ReduceLoadPendingOrdersSuccessAction(OrderState state, LoadPendingOrdersSuccessAction action)
        => state with { PendingOrders = action.Orders, IsLoading = false };
}
```

**Effects**: Side effects (API calls, SignalR)
```csharp
public class OrderEffects
{
    [EffectMethod]
    public async Task HandleLoadPendingOrdersAction(LoadPendingOrdersAction action, IDispatcher dispatcher)
    {
        try
        {
            var orders = await _orderApiClient.GetPendingOrdersAsync();
            dispatcher.Dispatch(new LoadPendingOrdersSuccessAction(orders));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoadPendingOrdersFailureAction(ex.Message));
        }
    }
}
```

## Naming Conventions

### Files & Classes
- PascalCase for all class names and file names
- Suffix patterns: `Service`, `Repository`, `Controller`, `Hub`, `Dto`, `Request`, `Response`
- Razor components: PascalCase with `.razor` extension

### Database Entities
- Match database table/column names exactly
- Separate by schema: `Dbo` namespace for legacy, `Web` namespace for new tables

### API Routes
- RESTful conventions: `/api/orders`, `/api/customers`, `/api/products`
- SignalR hubs: `/hubs/kitchen`, `/hubs/orderlock`, `/hubs/servercommand`

## Configuration

### appsettings.json Structure
```json
{
  "ConnectionStrings": {
    "PosDatabase": "Server=...;Database=POS;..."
  },
  "Redis": {
    "Configuration": "localhost:6379"
  },
  "Serilog": {
    "MinimumLevel": "Information"
  },
  "Cors": {
    "AllowedOrigins": ["https://localhost:5001", "https://pos.local"]
  }
}
```

### Database-First Workflow (Legacy Data)
1. Legacy tables already exist in `dbo` schema
2. Create EF Core entities to match existing tables
3. Use Fluent API configurations for mapping
4. Read-only access to legacy data (no modifications)

### Code-First Workflow (New Data)
1. Create entities in `Web` namespace
2. Create entity configurations
3. Generate migrations: `dotnet ef migrations add`
4. Apply migrations: `dotnet ef database update`

## Exception Handling

Custom exception types:
- `OrderLockedException` - Order is locked by another user
- `PaymentProcessingException` - Payment processing failed
- `ValidationException` - Request validation failed
- `NotFoundException` - Resource not found

Global exception handling middleware:
```csharp
public class ExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { errors = ex.Errors });
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new { message = ex.Message });
        }
        // ... other exception types
    }
}
```

## Dependency Injection

All services registered in `Program.cs`:
```csharp
// Infrastructure services
builder.Services.AddDbContext<PosDbContext>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Application services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Infrastructure services
builder.Services.AddSingleton<ICacheService, RedisCacheService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
```

## Testing Strategy

### Unit Tests
- Test services in isolation using mocks
- Test repositories with in-memory database
- Test validators with FluentValidation test extensions

### Integration Tests
- Test API endpoints with WebApplicationFactory
- Test database operations with test database
- Test SignalR hubs with test clients

### Property-Based Tests
- Test business logic invariants with FsCheck
- Test calculation correctness across random inputs
- Test concurrency scenarios (order locking)
```


#### 3.4 blazor-patterns.md (Blazor Component Patterns)

```markdown
# Blazor Component Patterns

## Component Architecture

Blazor components follow a hierarchical structure with clear separation of concerns.

### Component Types

1. **Page Components** (Routable)
   - Located in `Pages/` folder
   - Have `@page` directive
   - Handle routing and layout selection
   - Example: `Cashier.razor`, `Kitchen.razor`

2. **Layout Components**
   - Located in `Layouts/` folder
   - Inherit from `LayoutComponentBase`
   - Define page structure and navigation
   - Example: `MainLayout.razor`, `CashierLayout.razor`

3. **Reusable Components**
   - Located in `Components/` folder
   - Organized by feature (Products, Cart, Customers)
   - Accept parameters and emit events
   - Example: `ProductGrid.razor`, `ShoppingCart.razor`

4. **Common Components**
   - Located in `Components/Common/` folder
   - Generic, reusable across features
   - Example: `LoadingSpinner.razor`, `ConfirmDialog.razor`

## Component Communication Patterns

### 1. Parent-to-Child (Parameters)

**Parent Component**:
```razor
<ProductCard Product="@selectedProduct" OnClick="HandleProductClick" />
```

**Child Component**:
```razor
@code {
    [Parameter]
    public ProductDto Product { get; set; } = default!;
    
    [Parameter]
    public EventCallback<ProductDto> OnClick { get; set; }
    
    private async Task HandleClick()
    {
        await OnClick.InvokeAsync(Product);
    }
}
```

### 2. Child-to-Parent (EventCallback)

**Child Component**:
```razor
<MudButton OnClick="@(() => OnQuantityChanged.InvokeAsync(newQuantity))">
    Update
</MudButton>

@code {
    [Parameter]
    public EventCallback<int> OnQuantityChanged { get; set; }
}
```

**Parent Component**:
```razor
<CartItem OnQuantityChanged="HandleQuantityChanged" />

@code {
    private void HandleQuantityChanged(int newQuantity)
    {
        // Update cart
    }
}
```

### 3. State Management (Fluxor)

**Component**:
```razor
@inherits Fluxor.Blazor.Web.Components.FluxorComponent

@code {
    [Inject] private IState<OrderState> OrderState { get; set; } = default!;
    [Inject] private IDispatcher Dispatcher { get; set; } = default!;
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        Dispatcher.Dispatch(new LoadPendingOrdersAction());
    }
}
```

### 4. Cascading Parameters

**Parent Component**:
```razor
<CascadingValue Value="@currentUser">
    <ChildComponents />
</CascadingValue>

@code {
    private UserDto currentUser = new();
}
```

**Child Component**:
```razor
@code {
    [CascadingParameter]
    public UserDto CurrentUser { get; set; } = default!;
}
```

## MudBlazor Integration

### Form Handling

```razor
<MudForm @ref="form" @bind-IsValid="@isValid">
    <MudTextField @bind-Value="model.Name" 
                  Label="Customer Name" 
                  Required="true" 
                  RequiredError="Name is required" />
    
    <MudTextField @bind-Value="model.Phone" 
                  Label="Phone Number" 
                  Validation="@(new Func<string, string>(ValidatePhone))" />
    
    <MudButton OnClick="Submit" Disabled="@(!isValid)">Submit</MudButton>
</MudForm>

@code {
    private MudForm form = default!;
    private bool isValid;
    private CustomerDto model = new();
    
    private string ValidatePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return "Phone is required";
        if (!Regex.IsMatch(phone, @"^\d{8,15}$"))
            return "Invalid phone format";
        return null;
    }
    
    private async Task Submit()
    {
        await form.Validate();
        if (isValid)
        {
            // Submit form
        }
    }
}
```

### Dialog Pattern

```razor
@inject IDialogService DialogService

<MudButton OnClick="OpenDialog">Open Dialog</MudButton>

@code {
    private async Task OpenDialog()
    {
        var parameters = new DialogParameters
        {
            ["Customer"] = selectedCustomer
        };
        
        var options = new DialogOptions 
        { 
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };
        
        var dialog = await DialogService.ShowAsync<CustomerDialog>("Edit Customer", parameters, options);
        var result = await dialog.Result;
        
        if (!result.Canceled)
        {
            var updatedCustomer = (CustomerDto)result.Data;
            // Handle result
        }
    }
}
```

### Snackbar Notifications

```razor
@inject ISnackbar Snackbar

@code {
    private void ShowSuccess(string message)
    {
        Snackbar.Add(message, Severity.Success);
    }
    
    private void ShowError(string message)
    {
        Snackbar.Add(message, Severity.Error);
    }
    
    private void ShowWarning(string message)
    {
        Snackbar.Add(message, Severity.Warning);
    }
}
```

## Lifecycle Methods

```razor
@code {
    protected override void OnInitialized()
    {
        // Called when component is first initialized
        // Use for one-time setup
    }
    
    protected override async Task OnInitializedAsync()
    {
        // Async version of OnInitialized
        // Use for async initialization (API calls)
        await LoadDataAsync();
    }
    
    protected override void OnParametersSet()
    {
        // Called when parameters change
        // Use to react to parameter changes
    }
    
    protected override async Task OnParametersSetAsync()
    {
        // Async version of OnParametersSet
        await RefreshDataAsync();
    }
    
    protected override void OnAfterRender(bool firstRender)
    {
        // Called after component renders
        // Use for JS interop
        if (firstRender)
        {
            // First render only
        }
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Async version of OnAfterRender
        if (firstRender)
        {
            await JSRuntime.InvokeVoidAsync("initializeComponent");
        }
    }
    
    public void Dispose()
    {
        // Called when component is disposed
        // Use for cleanup (unsubscribe events, dispose resources)
    }
}
```

## JavaScript Interop

### Calling JavaScript from Blazor

```razor
@inject IJSRuntime JSRuntime

@code {
    private async Task PrintReceipt(int invoiceId)
    {
        await JSRuntime.InvokeVoidAsync("printReceipt", invoiceId);
    }
    
    private async Task<string> GetDeviceId()
    {
        return await JSRuntime.InvokeAsync<string>("getDeviceId");
    }
}
```

**JavaScript (wwwroot/js/interop.js)**:
```javascript
window.printReceipt = (invoiceId) => {
    // Print logic
    console.log(`Printing invoice ${invoiceId}`);
};

window.getDeviceId = () => {
    return localStorage.getItem('deviceId') || 'unknown';
};
```

### Calling Blazor from JavaScript

```csharp
[JSInvokable]
public static Task<string> GetCurrentUser()
{
    return Task.FromResult("John Doe");
}
```

```javascript
DotNet.invokeMethodAsync('Pos.Web.Client', 'GetCurrentUser')
    .then(user => console.log(user));
```

## Error Handling

### Error Boundary

```razor
<ErrorBoundary>
    <ChildContent>
        <ProductGrid />
    </ChildContent>
    <ErrorContent Context="exception">
        <MudAlert Severity="Severity.Error">
            An error occurred: @exception.Message
        </MudAlert>
    </ErrorContent>
</ErrorBoundary>
```

### Try-Catch in Components

```razor
@code {
    private string? errorMessage;
    
    private async Task LoadData()
    {
        try
        {
            errorMessage = null;
            var data = await ApiClient.GetDataAsync();
            // Process data
        }
        catch (HttpRequestException ex)
        {
            errorMessage = "Network error. Please check your connection.";
            Logger.LogError(ex, "Failed to load data");
        }
        catch (Exception ex)
        {
            errorMessage = "An unexpected error occurred.";
            Logger.LogError(ex, "Unexpected error loading data");
        }
    }
}

@if (!string.IsNullOrEmpty(errorMessage))
{
    <MudAlert Severity="Severity.Error">@errorMessage</MudAlert>
}
```

## Performance Optimization

### Virtualization

```razor
<Virtualize Items="@products" Context="product">
    <ProductCard Product="@product" />
</Virtualize>
```

### Lazy Loading

```razor
@code {
    private List<ProductDto>? products;
    
    protected override async Task OnInitializedAsync()
    {
        // Load only visible products
        products = await ApiClient.GetProductsAsync(page: 1, pageSize: 20);
    }
    
    private async Task LoadMore()
    {
        var nextPage = await ApiClient.GetProductsAsync(page: 2, pageSize: 20);
        products.AddRange(nextPage);
    }
}
```

### ShouldRender Optimization

```razor
@code {
    protected override bool ShouldRender()
    {
        // Only re-render if specific conditions are met
        return hasDataChanged;
    }
}
```

## Offline Support

### Service Worker Registration

**wwwroot/index.html**:
```html
<script>
    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.register('/service-worker.js');
    }
</script>
```

### Local Storage

```razor
@inject Blazored.LocalStorage.ILocalStorageService LocalStorage

@code {
    private async Task SaveOffline(OrderDto order)
    {
        var offlineOrders = await LocalStorage.GetItemAsync<List<OrderDto>>("offlineOrders") ?? new();
        offlineOrders.Add(order);
        await LocalStorage.SetItemAsync("offlineOrders", offlineOrders);
    }
    
    private async Task<List<OrderDto>> GetOfflineOrders()
    {
        return await LocalStorage.GetItemAsync<List<OrderDto>>("offlineOrders") ?? new();
    }
}
```

## SignalR Integration

### Hub Connection

```csharp
public class KitchenHubClient : IAsyncDisposable
{
    private readonly HubConnection _hubConnection;
    
    public KitchenHubClient(NavigationManager navigationManager)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(navigationManager.ToAbsoluteUri("/hubs/kitchen"))
            .WithAutomaticReconnect()
            .Build();
        
        _hubConnection.On<KitchenOrderMessage>("ReceiveOrder", HandleOrderReceived);
    }
    
    public async Task StartAsync()
    {
        await _hubConnection.StartAsync();
    }
    
    public async Task SendOrderToKitchen(KitchenOrderMessage message)
    {
        await _hubConnection.SendAsync("SendOrderToKitchen", message);
    }
    
    private void HandleOrderReceived(KitchenOrderMessage message)
    {
        // Handle received order
    }
    
    public async ValueTask DisposeAsync()
    {
        await _hubConnection.DisposeAsync();
    }
}
```

### Component Integration

```razor
@inject KitchenHubClient KitchenHub
@implements IAsyncDisposable

@code {
    protected override async Task OnInitializedAsync()
    {
        await KitchenHub.StartAsync();
    }
    
    private async Task SendOrder(OrderDto order)
    {
        var message = new KitchenOrderMessage
        {
            OrderId = order.Id,
            Items = order.Items
        };
        await KitchenHub.SendOrderToKitchen(message);
    }
    
    public async ValueTask DisposeAsync()
    {
        // Cleanup if needed
    }
}
```
```


#### 3.5 api-design.md (RESTful API Conventions)

```markdown
# RESTful API Design Conventions

## API Structure

### Base URL
```
https://api.pos.local/api
```

### Versioning
- URL-based versioning: `/api/v1/orders`
- Current version: v1 (implicit, no version in URL for now)
- Future versions: `/api/v2/orders` when breaking changes occur

## Resource Naming

### Conventions
- Use plural nouns for collections: `/api/orders`, `/api/customers`
- Use lowercase with hyphens for multi-word resources: `/api/order-items`
- Avoid verbs in URLs (use HTTP methods instead)
- Use nested resources for relationships: `/api/customers/{id}/orders`

### Examples
```
✅ GET /api/orders
✅ GET /api/customers/{id}/orders
✅ POST /api/orders
✅ PUT /api/orders/{id}
✅ DELETE /api/orders/{id}

❌ GET /api/getOrders
❌ POST /api/createOrder
❌ GET /api/order (singular)
```

## HTTP Methods

### GET - Retrieve Resources
```csharp
// Get all orders
[HttpGet]
public async Task<ActionResult<List<OrderDto>>> GetOrders()

// Get single order
[HttpGet("{id}")]
public async Task<ActionResult<OrderDto>> GetOrder(int id)

// Get with query parameters
[HttpGet]
public async Task<ActionResult<PagedResult<OrderDto>>> GetOrders(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? status = null)
```

### POST - Create Resources
```csharp
[HttpPost]
public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
{
    var result = await _orderService.CreateOrderAsync(request);
    return CreatedAtAction(nameof(GetOrder), new { id = result.OrderId }, result);
}
```

### PUT - Update Resources (Full Update)
```csharp
[HttpPut("{id}")]
public async Task<ActionResult> UpdateOrder(int id, [FromBody] UpdateOrderRequest request)
{
    await _orderService.UpdateOrderAsync(id, request);
    return NoContent();
}
```

### PATCH - Partial Update
```csharp
[HttpPatch("{id}")]
public async Task<ActionResult> PatchOrder(int id, [FromBody] JsonPatchDocument<OrderDto> patchDoc)
{
    await _orderService.PatchOrderAsync(id, patchDoc);
    return NoContent();
}
```

### DELETE - Remove Resources
```csharp
[HttpDelete("{id}")]
public async Task<ActionResult> DeleteOrder(int id)
{
    await _orderService.DeleteOrderAsync(id);
    return NoContent();
}
```

## Status Codes

### Success Codes
- **200 OK**: Successful GET, PUT, PATCH, or DELETE
- **201 Created**: Successful POST (include Location header)
- **204 No Content**: Successful PUT, PATCH, or DELETE with no response body

### Client Error Codes
- **400 Bad Request**: Invalid request data (validation errors)
- **401 Unauthorized**: Missing or invalid authentication
- **403 Forbidden**: Authenticated but not authorized
- **404 Not Found**: Resource doesn't exist
- **409 Conflict**: Resource conflict (e.g., order already locked)
- **422 Unprocessable Entity**: Semantic validation errors

### Server Error Codes
- **500 Internal Server Error**: Unexpected server error
- **503 Service Unavailable**: Service temporarily unavailable

## Response Format

### Success Response
```json
{
  "data": {
    "id": 123,
    "customerId": 456,
    "totalCost": 25.50,
    "items": [...]
  }
}
```

### Error Response
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "One or more validation errors occurred",
    "details": [
      {
        "field": "customerId",
        "message": "Customer ID is required"
      },
      {
        "field": "items",
        "message": "At least one item is required"
      }
    ]
  }
}
```

### Paginated Response
```json
{
  "data": [...],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalPages": 5,
    "totalCount": 100
  }
}
```

## Request/Response DTOs

### Naming Conventions
- **Request DTOs**: `Create{Resource}Request`, `Update{Resource}Request`
- **Response DTOs**: `{Resource}Response`, `{Resource}Dto`
- **Query DTOs**: `{Resource}SearchRequest`, `{Resource}FilterRequest`

### Example DTOs

**CreateOrderRequest.cs**:
```csharp
public class CreateOrderRequest
{
    public int? CustomerId { get; set; }
    public byte ServiceTypeId { get; set; }
    public byte? TableNumber { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public string? Notes { get; set; }
}
```

**OrderResponse.cs**:
```csharp
public class OrderResponse
{
    public int OrderId { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
}
```

**OrderSearchRequest.cs**:
```csharp
public class OrderSearchRequest
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? CustomerId { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
```

## Validation

### FluentValidation
```csharp
public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.ServiceTypeId)
            .NotEmpty()
            .WithMessage("Service type is required");
        
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("At least one item is required");
        
        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemDtoValidator());
        
        RuleFor(x => x.TableNumber)
            .InclusiveBetween((byte)1, (byte)100)
            .When(x => x.TableNumber.HasValue)
            .WithMessage("Table number must be between 1 and 100");
    }
}
```

### Validation Filter
```csharp
public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value.Errors.Any())
                .Select(x => new ValidationError
                {
                    Field = x.Key,
                    Message = x.Value.Errors.First().ErrorMessage
                })
                .ToList();
            
            context.Result = new BadRequestObjectResult(new
            {
                error = new
                {
                    code = "VALIDATION_ERROR",
                    message = "One or more validation errors occurred",
                    details = errors
                }
            });
        }
    }
    
    public void OnActionExecuted(ActionExecutedContext context) { }
}
```

## Authentication & Authorization

### JWT Bearer Authentication
```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Cashier,Waiter,Admin")]
    public async Task<ActionResult<List<OrderDto>>> GetOrders()
    
    [HttpPost]
    [Authorize(Roles = "Cashier,Waiter")]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteOrder(int id)
}
```

### Custom Authorization Policy
```csharp
// In Program.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanProcessPayments", policy =>
        policy.RequireRole("Cashier", "Admin"));
    
    options.AddPolicy("CanViewReports", policy =>
        policy.RequireRole("Admin", "Manager"));
});

// In controller
[Authorize(Policy = "CanProcessPayments")]
[HttpPost("process-payment")]
public async Task<ActionResult<PaymentResponse>> ProcessPayment([FromBody] ProcessPaymentRequest request)
```

## Error Handling

### Global Exception Handler
```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationException(context, ex);
        }
        catch (NotFoundException ex)
        {
            await HandleNotFoundException(context, ex);
        }
        catch (OrderLockedException ex)
        {
            await HandleOrderLockedException(context, ex);
        }
        catch (Exception ex)
        {
            await HandleUnexpectedException(context, ex);
        }
    }
    
    private async Task HandleValidationException(HttpContext context, ValidationException ex)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new
        {
            error = new
            {
                code = "VALIDATION_ERROR",
                message = ex.Message,
                details = ex.Errors
            }
        });
    }
    
    private async Task HandleNotFoundException(HttpContext context, NotFoundException ex)
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsJsonAsync(new
        {
            error = new
            {
                code = "NOT_FOUND",
                message = ex.Message
            }
        });
    }
    
    private async Task HandleOrderLockedException(HttpContext context, OrderLockedException ex)
    {
        context.Response.StatusCode = 409;
        await context.Response.WriteAsJsonAsync(new
        {
            error = new
            {
                code = "ORDER_LOCKED",
                message = ex.Message,
                lockedBy = ex.LockedByUser
            }
        });
    }
    
    private async Task HandleUnexpectedException(HttpContext context, Exception ex)
    {
        _logger.LogError(ex, "Unexpected error occurred");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new
        {
            error = new
            {
                code = "INTERNAL_ERROR",
                message = "An unexpected error occurred"
            }
        });
    }
}
```

## Logging & Auditing

### Request Logging
```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Request: {Method} {Path}",
            context.Request.Method,
            context.Request.Path);
        
        await _next(context);
        
        stopwatch.Stop();
        
        _logger.LogInformation("Response: {StatusCode} in {ElapsedMs}ms",
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
}
```

### Audit Logging
```csharp
public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAuditLogService _auditLogService;
    
    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);
        
        // Log only mutating operations
        if (context.Request.Method != "GET")
        {
            await _auditLogService.LogAsync(new ApiAuditLog
            {
                UserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Method = context.Request.Method,
                Path = context.Request.Path,
                StatusCode = context.Response.StatusCode,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
```

## API Documentation (Swagger)

### Configuration
```csharp
// In Program.cs
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Pos.Web API",
        Version = "v1",
        Description = "RESTful API for Web-Based POS System"
    });
    
    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
    
    // Add JWT authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
});
```

### XML Documentation
```csharp
/// <summary>
/// Creates a new order
/// </summary>
/// <param name="request">Order creation request</param>
/// <returns>Created order response</returns>
/// <response code="201">Order created successfully</response>
/// <response code="400">Invalid request data</response>
/// <response code="401">Unauthorized</response>
[HttpPost]
[ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
```

## Rate Limiting

### Configuration
```csharp
// In Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// In middleware pipeline
app.UseRateLimiter();
```

## CORS Configuration

```csharp
// In Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalNetwork", policy =>
    {
        policy.WithOrigins(
                "https://localhost:5001",
                "https://pos.local",
                "https://pos.company.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for SignalR
    });
});

// In middleware pipeline
app.UseCors("AllowLocalNetwork");
```
```


### Step 4: Copy Shared Steering Files

Copy `repository-standards.md` and `character-standards.md` from legacy to new solution (these apply to both):

```bash
# From Pos.Web directory
cp ../MyChairPos/.kiro/steering/repository-standards.md .kiro/steering/
cp ../MyChairPos/.kiro/steering/character-standards.md .kiro/steering/
```

### Step 5: Create Solution and Projects

Use the commands from `pos-web-project-structure.md` to create the solution:

```bash
# Navigate to Pos.Web directory
cd Pos.Web

# Create solution
dotnet new sln -n Pos.Web

# Create src directory
mkdir src

# Create projects
dotnet new classlib -n Pos.Web.Shared -o src/Pos.Web.Shared
dotnet new classlib -n Pos.Web.Infrastructure -o src/Pos.Web.Infrastructure
dotnet new webapi -n Pos.Web.API -o src/Pos.Web.API
dotnet new blazorwasm -n Pos.Web.Client -o src/Pos.Web.Client
dotnet new xunit -n Pos.Web.Tests -o src/Pos.Web.Tests

# Add projects to solution
dotnet sln add src/Pos.Web.Shared/Pos.Web.Shared.csproj
dotnet sln add src/Pos.Web.Infrastructure/Pos.Web.Infrastructure.csproj
dotnet sln add src/Pos.Web.API/Pos.Web.API.csproj
dotnet sln add src/Pos.Web.Client/Pos.Web.Client.csproj
dotnet sln add src/Pos.Web.Tests/Pos.Web.Tests.csproj

# Add project references
dotnet add src/Pos.Web.Infrastructure reference src/Pos.Web.Shared
dotnet add src/Pos.Web.API reference src/Pos.Web.Infrastructure
dotnet add src/Pos.Web.API reference src/Pos.Web.Shared
dotnet add src/Pos.Web.Client reference src/Pos.Web.Shared
dotnet add src/Pos.Web.Tests reference src/Pos.Web.Shared
dotnet add src/Pos.Web.Tests reference src/Pos.Web.Infrastructure
dotnet add src/Pos.Web.Tests reference src/Pos.Web.API
dotnet add src/Pos.Web.Tests reference src/Pos.Web.Client

# Build solution to verify
dotnet build
```

### Step 6: Open Pos.Web in Kiro

Once the directory structure is created:

1. Open Kiro
2. File → Open Folder
3. Navigate to `Pos.Web` directory
4. Open the folder

Now Kiro will recognize the `.kiro` folder structure and you can start implementing tasks from `tasks.md`.

## Final Directory Structure

After completing all steps, you should have:

```
/parent-directory/
├── MyChairPos/                              # Legacy WPF POS solution
│   ├── .kiro/
│   │   ├── specs/                           # (empty - moved to Pos.Web)
│   │   └── steering/
│   │       ├── product.md                   # Legacy WPF POS
│   │       ├── tech.md                      # .NET Framework 4.8, WPF
│   │       ├── structure.md                 # Legacy structure
│   │       ├── repository-standards.md      # Shared
│   │       ├── character-standards.md       # Shared
│   │       ├── pos-code-review.md           # Legacy review
│   │       ├── refactoring-strategy.md      # Legacy V2 strategy
│   │       ├── pos-layout-design-pattern.md # Legacy patterns
│   │       └── pos-code-structure.md        # Legacy structure
│   ├── POS/
│   ├── POSAdmin/
│   ├── OrdersMonitor/
│   └── ... (other legacy projects)
│
└── Pos.Web/                                 # New Web POS solution
    ├── .kiro/
    │   ├── specs/
    │   │   └── web-based-pos-system/        # Moved from MyChairPos
    │   │       ├── requirements.md
    │   │       ├── design.md
    │   │       ├── tasks.md
    │   │       ├── blazor-project-structure.md
    │   │       ├── blazor-pos-examples.md
    │   │       ├── mvc-vs-blazor-comparison.md
    │   │       ├── browser-compatibility.md
    │   │       ├── database-scripts.sql
    │   │       ├── pos-web-project-structure.md
    │   │       ├── MIGRATION-GUIDE.md       # This file
    │   │       └── .config.kiro
    │   └── steering/
    │       ├── product.md                   # NEW: Web POS overview
    │       ├── tech.md                      # NEW: .NET 8, Blazor
    │       ├── structure.md                 # NEW: 5-project architecture
    │       ├── repository-standards.md      # COPIED: From legacy
    │       ├── character-standards.md       # COPIED: From legacy
    │       ├── blazor-patterns.md           # NEW: Blazor patterns
    │       └── api-design.md                # NEW: API conventions
    ├── src/
    │   ├── Pos.Web.Shared/
    │   ├── Pos.Web.Infrastructure/
    │   ├── Pos.Web.API/
    │   ├── Pos.Web.Client/
    │   └── Pos.Web.Tests/
    └── Pos.Web.sln
```

## Summary of Changes

### Files Moved
- `.kiro/specs/web-based-pos-system/` → Moved from MyChairPos to Pos.Web

### Files Copied
- `repository-standards.md` → Copied from MyChairPos to Pos.Web (applies to both)
- `character-standards.md` → Copied from MyChairPos to Pos.Web (applies to both)

### Files Created (New in Pos.Web)
- `.kiro/steering/product.md` → Web POS product overview
- `.kiro/steering/tech.md` → .NET 8, Blazor, ASP.NET Core 8 stack
- `.kiro/steering/structure.md` → 5-project Clean Architecture
- `.kiro/steering/blazor-patterns.md` → Blazor component patterns
- `.kiro/steering/api-design.md` → RESTful API conventions
- `.kiro/specs/web-based-pos-system/MIGRATION-GUIDE.md` → This file

### Files Kept in MyChairPos (Legacy-Specific)
- `.kiro/steering/product.md` → Legacy WPF POS overview
- `.kiro/steering/tech.md` → .NET Framework 4.8, WPF, EF6
- `.kiro/steering/structure.md` → Legacy multi-project structure
- `.kiro/steering/pos-code-review.md` → Legacy code review
- `.kiro/steering/refactoring-strategy.md` → Legacy V2 refactoring
- `.kiro/steering/pos-layout-design-pattern.md` → Legacy WPF patterns
- `.kiro/steering/pos-code-structure.md` → Legacy code structure

## Next Steps

1. ✅ Create Pos.Web directory structure (manual step)
2. ✅ Move spec files from MyChairPos to Pos.Web
3. ✅ Create new steering files in Pos.Web
4. ✅ Copy shared steering files
5. ✅ Create solution and projects using dotnet CLI
6. ✅ Open Pos.Web in Kiro
7. 🚀 Start implementing tasks from `tasks.md`

## Notes

- Both solutions share the same SQL Server database
- Legacy system uses `dbo` schema (existing tables)
- New system uses `dbo` schema (read-only) + `web` schema (new tables)
- Both solutions can run simultaneously without conflicts
- Steering files are context-specific to each solution
- Shared standards (repository-standards.md, character-standards.md) are copied to both

---

**Document Version**: 1.0  
**Last Updated**: 2026-02-26  
**Author**: Development Team
