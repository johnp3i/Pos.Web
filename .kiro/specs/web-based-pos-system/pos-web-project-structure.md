# Pos.Web Solution - Complete Project Structure Documentation

## Table of Contents

1. [Solution Overview](#solution-overview)
2. [Project 1: Pos.Web.Shared](#1-poswebshared-class-library)
3. [Project 2: Pos.Web.Infrastructure](#2-poswebinfrastructure-class-library)
4. [Project 3: Pos.Web.API](#3-poswebapi-aspnet-core-web-api)
5. [Project 4: Pos.Web.Client](#4-poswebclient-blazor-webassembly)
6. [Project 5: Pos.Web.Tests](#5-poswebtests-xunit-test-project)
7. [Project Dependencies](#project-dependencies-diagram)
8. [Namespace Conventions](#namespace-conventions)
9. [Quick Reference](#quick-reference)

---

## Solution Overview

```
Pos.Web.sln
├── src/
│   ├── Pos.Web.Shared/           (Class Library - .NET 8)
│   ├── Pos.Web.Infrastructure/   (Class Library - .NET 8)
│   ├── Pos.Web.API/              (ASP.NET Core Web API - .NET 8)
│   ├── Pos.Web.Client/           (Blazor WebAssembly - .NET 8)
│   └── Pos.Web.Tests/            (xUnit Test Project - .NET 8)
```

**Architecture Pattern**: Clean Architecture with clear separation of concerns

**Technology Stack**:
- .NET 8
- Blazor WebAssembly
- ASP.NET Core Web API
- Entity Framework Core 8
- SignalR
- MudBlazor
- Fluxor (state management)
- xUnit (testing)

---

## 1. Pos.Web.Shared (Class Library)

**Project Type**: Class Library (.NET 8)  
**Purpose**: Shared DTOs, models, enums, and constants used by both client and server  
**Dependencies**: None (pure .NET 8)  
**Target Framework**: net8.0

### Complete Structure

```
Pos.Web.Shared/
├── Pos.Web.Shared.csproj
│
├── DTOs/                                    # Data Transfer Objects
│   ├── Orders/
│   │   ├── OrderDto.cs
│   │   ├── OrderItemDto.cs
│   │   ├── CreateOrderRequest.cs
│   │   ├── UpdateOrderRequest.cs
│   │   └── OrderResponse.cs
│   ├── Customers/
│   │   ├── CustomerDto.cs
│   │   ├── CustomerAddressDto.cs
│   │   ├── CreateCustomerRequest.cs
│   │   └── SearchCustomerRequest.cs
│   ├── Products/
│   │   ├── ProductDto.cs
│   │   ├── CategoryDto.cs
│   │   └── ProductSearchRequest.cs
│   ├── Payments/
│   │   ├── PaymentDto.cs
│   │   ├── ProcessPaymentRequest.cs
│   │   ├── ApplyDiscountRequest.cs
│   │   └── SplitPaymentRequest.cs
│   └── Common/
│       ├── ApiResponse.cs
│       ├── PagedResult.cs
│       └── ValidationError.cs
│
├── Models/                                  # Domain models
│   ├── SignalR/
│   │   ├── OrderStatusChangedMessage.cs
│   │   ├── OrderLockedMessage.cs
│   │   ├── OrderUnlockedMessage.cs
│   │   ├── KitchenOrderMessage.cs
│   │   └── ServerCommandMessage.cs
│   └── Configuration/
│       ├── FeatureFlagDto.cs
│       └── UserSessionDto.cs
│
├── Enums/                                   # Enumerations
│   ├── OrderStatus.cs
│   ├── PaymentMethod.cs
│   ├── ServiceType.cs
│   ├── OrderLockStatus.cs
│   ├── ServerCommandType.cs
│   ├── DeviceType.cs
│   └── UserRole.cs
│
├── Constants/                               # Application constants
│   ├── ApiRoutes.cs
│   ├── SignalRHubNames.cs
│   ├── SignalRMethodNames.cs
│   ├── CacheKeys.cs
│   └── ValidationMessages.cs
│
└── Validators/                              # FluentValidation validators
    ├── CreateOrderRequestValidator.cs
    ├── CreateCustomerRequestValidator.cs
    ├── ProcessPaymentRequestValidator.cs
    └── ApplyDiscountRequestValidator.cs
```

### Key Files

**ApiRoutes.cs**
```csharp
namespace Pos.Web.Shared.Constants;

public static class ApiRoutes
{
    public const string BaseUrl = "/api";
    
    public static class Orders
    {
        public const string Base = $"{BaseUrl}/orders";
        public const string GetById = $"{Base}/{{id}}";
        public const string Pending = $"{Base}/pending";
        public const string Split = $"{Base}/{{id}}/split";
    }
    
    public static class Customers
    {
        public const string Base = $"{BaseUrl}/customers";
        public const string Search = $"{Base}/search";
        public const string History = $"{Base}/{{id}}/history";
    }
    
    public static class Products
    {
        public const string Base = $"{BaseUrl}/products";
        public const string Search = $"{Base}/search";
        public const string Categories = $"{Base}/categories";
    }
}
```

**OrderDto.cs**
```csharp
namespace Pos.Web.Shared.DTOs.Orders;

public class OrderDto
{
    public int Id { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public byte ServiceTypeId { get; set; }
    public byte? TableNumber { get; set; }
    public decimal TotalCost { get; set; }
    public decimal? CustomerPaid { get; set; }
    public DateTime Timestamp { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public string? Notes { get; set; }
    public bool IsPending { get; set; }
}
```

### NuGet Packages
- FluentValidation (11.9.0+)

---

## 2. Pos.Web.Infrastructure (Class Library)

**Project Type**: Class Library (.NET 8)  
**Purpose**: Data access layer with EF Core, repositories, and infrastructure services  
**Dependencies**: Pos.Web.Shared, Entity Framework Core 8, SQL Server provider  
**Target Framework**: net8.0

### Complete Structure

```
Pos.Web.Infrastructure/
├── Pos.Web.Infrastructure.csproj
│
├── Data/                                    # EF Core DbContext
│   ├── PosDbContext.cs
│   ├── Configurations/                      # Entity configurations
│   │   ├── OrderConfiguration.cs
│   │   ├── CustomerConfiguration.cs
│   │   ├── ProductConfiguration.cs
│   │   ├── OrderLockConfiguration.cs
│   │   ├── ApiAuditLogConfiguration.cs
│   │   ├── UserSessionConfiguration.cs
│   │   ├── FeatureFlagConfiguration.cs
│   │   └── SyncQueueConfiguration.cs
│   └── Migrations/                          # EF Core migrations
│
├── Entities/                                # EF Core entities
│   ├── Dbo/                                 # Legacy dbo schema entities
│   │   ├── Invoice.cs
│   │   ├── InvoiceItem.cs
│   │   ├── PendingInvoice.cs
│   │   ├── PendingInvoiceItem.cs
│   │   ├── Customer.cs
│   │   ├── CustomerAddress.cs
│   │   ├── CategoryItem.cs
│   │   ├── Category.cs
│   │   ├── User.cs
│   │   ├── ServingType.cs
│   │   └── PaymentMethod.cs
│   └── Web/                                 # New web schema entities
│       ├── OrderLock.cs
│       ├── ApiAuditLog.cs
│       ├── UserSession.cs
│       ├── FeatureFlag.cs
│       └── SyncQueue.cs
│
├── Repositories/                            # Repository pattern
│   ├── Interfaces/
│   │   ├── IRepository.cs                   # Generic repository interface
│   │   ├── IOrderRepository.cs
│   │   ├── ICustomerRepository.cs
│   │   ├── IProductRepository.cs
│   │   ├── IOrderLockRepository.cs
│   │   ├── IAuditLogRepository.cs
│   │   ├── IUserSessionRepository.cs
│   │   ├── IFeatureFlagRepository.cs
│   │   └── ISyncQueueRepository.cs
│   └── Implementations/
│       ├── GenericRepository.cs             # Base repository
│       ├── OrderRepository.cs
│       ├── CustomerRepository.cs
│       ├── ProductRepository.cs
│       ├── OrderLockRepository.cs
│       ├── AuditLogRepository.cs
│       ├── UserSessionRepository.cs
│       ├── FeatureFlagRepository.cs
│       └── SyncQueueRepository.cs
│
├── UnitOfWork/                              # Unit of Work pattern
│   ├── IUnitOfWork.cs
│   └── UnitOfWork.cs
│
├── Services/                                # Infrastructure services
│   ├── Interfaces/
│   │   ├── ICacheService.cs
│   │   ├── IFeatureFlagService.cs
│   │   └── IAuditLogService.cs
│   └── Implementations/
│       ├── RedisCacheService.cs
│       ├── FeatureFlagService.cs
│       └── AuditLogService.cs
│
└── Extensions/                              # Extension methods
    ├── ServiceCollectionExtensions.cs       # DI registration
    └── QueryableExtensions.cs               # LINQ extensions
```

### Key Files

**PosDbContext.cs**
```csharp
namespace Pos.Web.Infrastructure.Data;

public class PosDbContext : DbContext
{
    public PosDbContext(DbContextOptions<PosDbContext> options) : base(options) { }
    
    // Legacy dbo schema (existing tables)
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }
    public DbSet<PendingInvoice> PendingInvoices { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerAddress> CustomerAddresses { get; set; }
    public DbSet<CategoryItem> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<User> Users { get; set; }
    
    // New web schema (web POS tables)
    public DbSet<OrderLock> OrderLocks { get; set; }
    public DbSet<ApiAuditLog> ApiAuditLogs { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<FeatureFlag> FeatureFlags { get; set; }
    public DbSet<SyncQueue> SyncQueue { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PosDbContext).Assembly);
    }
}
```

**IUnitOfWork.cs**
```csharp
namespace Pos.Web.Infrastructure.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IOrderRepository Orders { get; }
    ICustomerRepository Customers { get; }
    IProductRepository Products { get; }
    IOrderLockRepository OrderLocks { get; }
    IAuditLogRepository AuditLogs { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

### NuGet Packages
- Microsoft.EntityFrameworkCore (8.0.0+)
- Microsoft.EntityFrameworkCore.SqlServer (8.0.0+)
- Microsoft.EntityFrameworkCore.Tools (8.0.0+)
- StackExchange.Redis (2.7.0+)

---

## 3. Pos.Web.API (ASP.NET Core Web API)

**Project Type**: ASP.NET Core Web API (.NET 8)  
**Purpose**: RESTful API with SignalR hubs for real-time communication  
**Dependencies**: Pos.Web.Shared, Pos.Web.Infrastructure, ASP.NET Core 8, SignalR  
**Target Framework**: net8.0

### Complete Structure

```
Pos.Web.API/
├── Pos.Web.API.csproj
├── Program.cs                               # Application entry point
├── appsettings.json
├── appsettings.Development.json
│
├── Controllers/                             # API controllers
│   ├── OrdersController.cs
│   ├── PaymentsController.cs
│   ├── CustomersController.cs
│   ├── ProductsController.cs
│   ├── KitchenController.cs
│   ├── ReportsController.cs
│   └── AuthController.cs
│
├── Hubs/                                    # SignalR hubs
│   ├── KitchenHub.cs
│   ├── OrderLockHub.cs
│   └── ServerCommandHub.cs
│
├── Services/                                # Business logic services
│   ├── Interfaces/
│   │   ├── IOrderService.cs
│   │   ├── IOrderLockService.cs
│   │   ├── IPaymentService.cs
│   │   ├── ICustomerService.cs
│   │   ├── IProductService.cs
│   │   └── IKitchenService.cs
│   └── Implementations/
│       ├── OrderService.cs
│       ├── OrderLockService.cs
│       ├── PaymentService.cs
│       ├── CustomerService.cs
│       ├── ProductService.cs
│       └── KitchenService.cs
│
├── Middleware/                              # Custom middleware
│   ├── ExceptionHandlingMiddleware.cs
│   ├── RequestLoggingMiddleware.cs
│   └── AuditLoggingMiddleware.cs
│
├── Filters/                                 # Action filters
│   ├── ValidationFilter.cs
│   └── FeatureFlagFilter.cs
│
├── Extensions/                              # Extension methods
│   ├── ServiceCollectionExtensions.cs
│   └── ApplicationBuilderExtensions.cs
│
├── Mapping/                                 # AutoMapper profiles
│   ├── OrderMappingProfile.cs
│   ├── CustomerMappingProfile.cs
│   └── ProductMappingProfile.cs
│
└── Properties/
    └── launchSettings.json
```

### Key Files

**Program.cs**
```csharp
using Pos.Web.Infrastructure.Extensions;
using Pos.Web.API.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, config) => 
    config.ReadFrom.Configuration(context.Configuration));

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR
builder.Services.AddSignalR();

// Add Infrastructure services (EF Core, Repositories, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Add Application services (Business logic)
builder.Services.AddApplicationServices();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();

// Add Authentication & Authorization
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalNetwork", policy =>
    {
        policy.WithOrigins("https://localhost:5001", "https://pos.local")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowLocalNetwork");

app.UseAuthentication();
app.UseAuthorization();

// Custom middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<AuditLoggingMiddleware>();

app.MapControllers();

// Map SignalR hubs
app.MapHub<KitchenHub>("/hubs/kitchen");
app.MapHub<OrderLockHub>("/hubs/orderlock");
app.MapHub<ServerCommandHub>("/hubs/servercommand");

app.Run();
```

**OrdersController.cs**
```csharp
namespace Pos.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var result = await _orderService.CreateOrderAsync(request);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        return order != null ? Ok(order) : NotFound();
    }

    [HttpGet("pending")]
    public async Task<ActionResult<List<OrderDto>>> GetPendingOrders()
    {
        var orders = await _orderService.GetPendingOrdersAsync();
        return Ok(orders);
    }
}
```

**KitchenHub.cs**
```csharp
namespace Pos.Web.API.Hubs;

public class KitchenHub : Hub
{
    private readonly ILogger<KitchenHub> _logger;

    public KitchenHub(ILogger<KitchenHub> logger)
    {
        _logger = logger;
    }

    public async Task SendOrderToKitchen(KitchenOrderMessage message)
    {
        await Clients.All.SendAsync("ReceiveOrder", message);
    }

    public async Task UpdateOrderStatus(int orderId, string status)
    {
        await Clients.All.SendAsync("OrderStatusChanged", orderId, status);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Kitchen display connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }
}
```

### NuGet Packages
- Microsoft.AspNetCore.SignalR (8.0.0+)
- AutoMapper.Extensions.Microsoft.DependencyInjection (12.0.0+)
- Serilog.AspNetCore (8.0.0+)
- Swashbuckle.AspNetCore (6.5.0+)

---

## 4. Pos.Web.Client (Blazor WebAssembly)

**Project Type**: Blazor WebAssembly (.NET 8)  
**Purpose**: Progressive Web App frontend with offline support  
**Dependencies**: Pos.Web.Shared, MudBlazor, Fluxor, Blazored.LocalStorage, SignalR Client  
**Target Framework**: net8.0

### Complete Structure

```
Pos.Web.Client/
├── Pos.Web.Client.csproj
├── Program.cs                               # Client entry point
├── _Imports.razor                           # Global using statements
│
├── wwwroot/                                 # Static assets
│   ├── index.html
│   ├── manifest.json                        # PWA manifest
│   ├── service-worker.js                    # Service worker
│   ├── service-worker.published.js
│   ├── css/
│   │   ├── app.css
│   │   └── mudblazor-overrides.css
│   ├── images/
│   │   ├── logo.png
│   │   ├── icons/                           # PWA icons (192x192, 512x512)
│   │   │   ├── icon-192.png
│   │   │   └── icon-512.png
│   │   └── products/
│   └── js/
│       └── interop.js                       # JS interop
│
├── Pages/                                   # Routable pages
│   ├── Index.razor
│   ├── Cashier.razor
│   ├── Waiter.razor
│   ├── Kitchen.razor
│   ├── Checkout.razor
│   ├── PendingOrders.razor
│   ├── Reports.razor
│   ├── Login.razor
│   └── NotFound.razor
│
├── Components/                              # Reusable components
│   ├── Products/
│   │   ├── ProductGrid.razor
│   │   ├── ProductCard.razor
│   │   ├── ProductSearch.razor
│   │   └── CategoryFilter.razor
│   ├── Cart/
│   │   ├── ShoppingCart.razor
│   │   ├── CartItem.razor
│   │   ├── CartSummary.razor
│   │   └── CartActions.razor
│   ├── Customers/
│   │   ├── CustomerSearch.razor
│   │   ├── CustomerCard.razor
│   │   ├── CustomerForm.razor
│   │   └── CustomerHistory.razor
│   ├── Kitchen/
│   │   ├── OrderCard.razor
│   │   ├── OrderStatusButton.razor
│   │   └── OrderTimer.razor
│   └── Common/
│       ├── LoadingSpinner.razor
│       ├── ErrorBoundary.razor
│       └── ConfirmDialog.razor
│
├── Layouts/                                 # Layout components
│   ├── MainLayout.razor
│   ├── CashierLayout.razor
│   ├── TabletLayout.razor
│   └── KitchenLayout.razor
│
├── Store/                                   # Fluxor state management
│   ├── OrderState/
│   │   ├── OrderState.cs
│   │   ├── OrderActions.cs
│   │   ├── OrderReducers.cs
│   │   └── OrderEffects.cs
│   ├── CustomerState/
│   │   ├── CustomerState.cs
│   │   ├── CustomerActions.cs
│   │   ├── CustomerReducers.cs
│   │   └── CustomerEffects.cs
│   ├── ProductState/
│   │   ├── ProductState.cs
│   │   ├── ProductActions.cs
│   │   ├── ProductReducers.cs
│   │   └── ProductEffects.cs
│   ├── KitchenState/
│   │   ├── KitchenState.cs
│   │   ├── KitchenActions.cs
│   │   ├── KitchenReducers.cs
│   │   └── KitchenEffects.cs
│   └── UIState/
│       ├── UIState.cs
│       ├── UIActions.cs
│       └── UIReducers.cs
│
├── Services/                                # Client services
│   ├── API/
│   │   ├── OrderApiClient.cs
│   │   ├── CustomerApiClient.cs
│   │   ├── ProductApiClient.cs
│   │   └── PaymentApiClient.cs
│   ├── SignalR/
│   │   ├── KitchenHubClient.cs
│   │   ├── OrderLockHubClient.cs
│   │   └── ServerCommandHubClient.cs
│   ├── Offline/
│   │   ├── IOfflineStorageService.cs
│   │   ├── OfflineStorageService.cs
│   │   └── SyncService.cs
│   ├── Print/
│   │   ├── IPrintService.cs
│   │   └── PrintService.cs
│   ├── Notification/
│   │   ├── INotificationService.cs
│   │   └── NotificationService.cs
│   └── Auth/
│       ├── CustomAuthenticationStateProvider.cs
│       └── AuthService.cs
│
└── Extensions/                              # Extension methods
    └── ServiceCollectionExtensions.cs
```

### Key Files

**Program.cs**
```csharp
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Pos.Web.Client;
using MudBlazor.Services;
using Fluxor;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
});

// Add MudBlazor
builder.Services.AddMudServices();

// Add Fluxor (state management)
builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(typeof(Program).Assembly);
    options.UseReduxDevTools();
});

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Add application services
builder.Services.AddScoped<IOrderApiClient, OrderApiClient>();
builder.Services.AddScoped<ICustomerApiClient, CustomerApiClient>();
builder.Services.AddScoped<IProductApiClient, ProductApiClient>();
builder.Services.AddScoped<IOfflineStorageService, OfflineStorageService>();
builder.Services.AddScoped<IPrintService, PrintService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Add SignalR clients
builder.Services.AddScoped<KitchenHubClient>();
builder.Services.AddScoped<OrderLockHubClient>();

await builder.Build().RunAsync();
```

**Cashier.razor**
```razor
@page "/cashier"
@layout CashierLayout
@using Pos.Web.Client.Store.OrderState
@using Pos.Web.Client.Store.ProductState
@inherits Fluxor.Blazor.Web.Components.FluxorComponent

<MudGrid>
    <MudItem xs="4">
        <ProductGrid />
    </MudItem>
    <MudItem xs="2">
        <QuickActions />
    </MudItem>
    <MudItem xs="6">
        <ShoppingCart />
        <CartSummary />
        <CartActions />
    </MudItem>
</MudGrid>

@code {
    [Inject] private IState<OrderState> OrderState { get; set; } = default!;
    [Inject] private IState<ProductState> ProductState { get; set; } = default!;
}
```

**OrderState.cs (Fluxor)**
```csharp
namespace Pos.Web.Client.Store.OrderState;

public record OrderState
{
    public OrderDto? CurrentOrder { get; init; }
    public List<OrderDto> PendingOrders { get; init; } = new();
    public bool IsLoading { get; init; }
    public string? ErrorMessage { get; init; }
}

public class OrderFeature : Feature<OrderState>
{
    public override string GetName() => "Order";
    protected override OrderState GetInitialState() => new();
}
```

### NuGet Packages
- MudBlazor (6.11.0+)
- Fluxor.Blazor.Web (5.9.0+)
- Blazored.LocalStorage (4.4.0+)
- Microsoft.AspNetCore.SignalR.Client (8.0.0+)
- Microsoft.AspNetCore.Components.WebAssembly (8.0.0+)

---

## 5. Pos.Web.Tests (xUnit Test Project)

**Project Type**: xUnit Test Project (.NET 8)  
**Purpose**: Unit, integration, and property-based tests  
**Dependencies**: All other projects, xUnit, Moq, FluentAssertions, FsCheck  
**Target Framework**: net8.0

### Complete Structure

```
Pos.Web.Tests/
├── Pos.Web.Tests.csproj
│
├── Unit/                                    # Unit tests
│   ├── Services/
│   │   ├── OrderServiceTests.cs
│   │   ├── PaymentServiceTests.cs
│   │   ├── CustomerServiceTests.cs
│   │   ├── OrderLockServiceTests.cs
│   │   └── ProductServiceTests.cs
│   ├── Repositories/
│   │   ├── OrderRepositoryTests.cs
│   │   ├── CustomerRepositoryTests.cs
│   │   └── ProductRepositoryTests.cs
│   └── Validators/
│       ├── CreateOrderRequestValidatorTests.cs
│       ├── CreateCustomerRequestValidatorTests.cs
│       └── ProcessPaymentRequestValidatorTests.cs
│
├── Integration/                             # Integration tests
│   ├── API/
│   │   ├── OrdersControllerTests.cs
│   │   ├── CustomersControllerTests.cs
│   │   ├── PaymentsControllerTests.cs
│   │   └── ProductsControllerTests.cs
│   ├── Database/
│   │   ├── OrderRepositoryIntegrationTests.cs
│   │   ├── CustomerRepositoryIntegrationTests.cs
│   │   └── TransactionTests.cs
│   └── SignalR/
│       ├── KitchenHubTests.cs
│       ├── OrderLockHubTests.cs
│       └── ServerCommandHubTests.cs
│
├── PropertyBased/                           # Property-based tests
│   ├── OrderCalculationTests.cs
│   ├── OrderLockingTests.cs
│   └── SyncQueueTests.cs
│
├── Fixtures/                                # Test fixtures
│   ├── DatabaseFixture.cs
│   ├── WebApplicationFixture.cs
│   └── SignalRFixture.cs
│
└── Helpers/                                 # Test helpers
    ├── TestDataBuilder.cs
    ├── MockFactory.cs
    └── AssertionExtensions.cs
```

### Key Files

**OrderServiceTests.cs (Unit Test)**
```csharp
namespace Pos.Web.Tests.Unit.Services;

public class OrderServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _unitOfWorkMock.Setup(x => x.Orders).Returns(_orderRepositoryMock.Object);
        _sut = new OrderService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateOrderAsync_ValidRequest_ReturnsOrderResponse()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerId = 1,
            ServiceTypeId = 1,
            Items = new List<OrderItemDto>
            {
                new() { ProductId = 1, Quantity = 2, Price = 10.00m }
            }
        };

        // Act
        var result = await _sut.CreateOrderAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.TotalCost.Should().Be(20.00m);
        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Invoice>()), Times.Once);
    }
}
```

**OrdersControllerTests.cs (Integration Test)**
```csharp
namespace Pos.Web.Tests.Integration.API;

public class OrdersControllerTests : IClassFixture<WebApplicationFixture>
{
    private readonly HttpClient _client;

    public OrdersControllerTests(WebApplicationFixture factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_ReturnsCreatedOrder()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerId = 1,
            ServiceTypeId = 1,
            Items = new List<OrderItemDto>
            {
                new() { ProductId = 1, Quantity = 2, Price = 10.00m }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        order.Should().NotBeNull();
        order!.TotalCost.Should().Be(20.00m);
    }
}
```

**OrderCalculationTests.cs (Property-Based Test)**
```csharp
namespace Pos.Web.Tests.PropertyBased;

public class OrderCalculationTests
{
    [Property]
    public Property OrderTotal_ShouldEqualSumOfItems()
    {
        return Prop.ForAll(
            Arb.Generate<List<OrderItemDto>>().ToArbitrary(),
            items =>
            {
                // Arrange
                var order = new OrderDto { Items = items };

                // Act
                var calculatedTotal = items.Sum(i => i.Quantity * i.Price);
                var orderTotal = order.TotalCost;

                // Assert
                return (orderTotal == calculatedTotal).Label($"Expected {calculatedTotal}, got {orderTotal}");
            });
    }

    [Property]
    public Property OrderWithDiscount_ShouldBeCorrect()
    {
        return Prop.ForAll(
            Arb.Generate<decimal>().Where(x => x > 0 && x < 1000).ToArbitrary(),
            Arb.Generate<decimal>().Where(x => x >= 0 && x <= 100).ToArbitrary(),
            (subtotal, discountPercent) =>
            {
                // Arrange
                var expectedTotal = subtotal * (1 - discountPercent / 100);

                // Act
                var actualTotal = CalculateOrderTotal(subtotal, discountPercent);

                // Assert
                return Math.Abs(actualTotal - expectedTotal) < 0.01m;
            });
    }

    private decimal CalculateOrderTotal(decimal subtotal, decimal discountPercent)
    {
        return subtotal * (1 - discountPercent / 100);
    }
}
```

### NuGet Packages
- xunit (2.6.0+)
- xunit.runner.visualstudio (2.5.0+)
- Moq (4.20.0+)
- FluentAssertions (6.12.0+)
- FsCheck (2.16.0+)
- FsCheck.Xunit (2.16.0+)
- Microsoft.AspNetCore.Mvc.Testing (8.0.0+)
- Microsoft.EntityFrameworkCore.InMemory (8.0.0+)

---

## Project Dependencies Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    Pos.Web.Client                           │
│                 (Blazor WebAssembly)                        │
│  - Pages, Components, Layouts                               │
│  - Fluxor State Management                                  │
│  - SignalR Client                                           │
└────────────────────────┬────────────────────────────────────┘
                         │ references
                         ↓
┌─────────────────────────────────────────────────────────────┐
│                    Pos.Web.Shared                           │
│                   (Class Library)                           │
│  - DTOs, Models, Enums                                      │
│  - Constants, Validators                                    │
│  - NO dependencies                                          │
└────────────────────────↑────────────────────────────────────┘
                         │ references
┌────────────────────────┴────────────────────────────────────┐
│                    Pos.Web.API                              │
│              (ASP.NET Core Web API)                         │
│  - Controllers, Hubs                                        │
│  - Business Services                                        │
│  - Middleware, Filters                                      │
└────────────────────────┬────────────────────────────────────┘
                         │ references
                         ↓
┌─────────────────────────────────────────────────────────────┐
│                Pos.Web.Infrastructure                       │
│                   (Class Library)                           │
│  - EF Core DbContext                                        │
│  - Repositories, Unit of Work                               │
│  - Infrastructure Services                                  │
└────────────────────────┬────────────────────────────────────┘
                         │ references
                         ↓
┌─────────────────────────────────────────────────────────────┐
│                    Pos.Web.Shared                           │
│                   (Class Library)                           │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                    Pos.Web.Tests                            │
│                  (xUnit Test Project)                       │
│  - Unit Tests                                               │
│  - Integration Tests                                        │
│  - Property-Based Tests                                     │
└────────────────────────┬────────────────────────────────────┘
                         │ references ALL projects
                         ↓
              (All Projects Above)
```

### Dependency Flow

**Clean Architecture Layers**:
1. **Presentation Layer**: Pos.Web.Client, Pos.Web.API
2. **Application Layer**: Pos.Web.API (Services)
3. **Domain Layer**: Pos.Web.Shared
4. **Infrastructure Layer**: Pos.Web.Infrastructure
5. **Test Layer**: Pos.Web.Tests

**Dependency Rules**:
- Inner layers (Shared) have NO dependencies on outer layers
- Outer layers depend on inner layers
- Infrastructure depends on Shared (for DTOs/interfaces)
- API depends on Infrastructure and Shared
- Client depends ONLY on Shared (not on API or Infrastructure)
- Tests depend on all projects

---

## Namespace Conventions

### Pos.Web.Shared
```csharp
namespace Pos.Web.Shared.DTOs.Orders;
namespace Pos.Web.Shared.DTOs.Customers;
namespace Pos.Web.Shared.DTOs.Products;
namespace Pos.Web.Shared.Models.SignalR;
namespace Pos.Web.Shared.Enums;
namespace Pos.Web.Shared.Constants;
namespace Pos.Web.Shared.Validators;
```

### Pos.Web.Infrastructure
```csharp
namespace Pos.Web.Infrastructure.Data;
namespace Pos.Web.Infrastructure.Data.Configurations;
namespace Pos.Web.Infrastructure.Entities.Dbo;
namespace Pos.Web.Infrastructure.Entities.Web;
namespace Pos.Web.Infrastructure.Repositories.Interfaces;
namespace Pos.Web.Infrastructure.Repositories.Implementations;
namespace Pos.Web.Infrastructure.UnitOfWork;
namespace Pos.Web.Infrastructure.Services.Interfaces;
namespace Pos.Web.Infrastructure.Services.Implementations;
```

### Pos.Web.API
```csharp
namespace Pos.Web.API.Controllers;
namespace Pos.Web.API.Hubs;
namespace Pos.Web.API.Services.Interfaces;
namespace Pos.Web.API.Services.Implementations;
namespace Pos.Web.API.Middleware;
namespace Pos.Web.API.Filters;
namespace Pos.Web.API.Extensions;
namespace Pos.Web.API.Mapping;
```

### Pos.Web.Client
```csharp
namespace Pos.Web.Client.Pages;
namespace Pos.Web.Client.Components.Products;
namespace Pos.Web.Client.Components.Cart;
namespace Pos.Web.Client.Components.Customers;
namespace Pos.Web.Client.Layouts;
namespace Pos.Web.Client.Store.OrderState;
namespace Pos.Web.Client.Store.CustomerState;
namespace Pos.Web.Client.Services.API;
namespace Pos.Web.Client.Services.SignalR;
namespace Pos.Web.Client.Services.Offline;
```

### Pos.Web.Tests
```csharp
namespace Pos.Web.Tests.Unit.Services;
namespace Pos.Web.Tests.Unit.Repositories;
namespace Pos.Web.Tests.Integration.API;
namespace Pos.Web.Tests.Integration.Database;
namespace Pos.Web.Tests.PropertyBased;
namespace Pos.Web.Tests.Fixtures;
namespace Pos.Web.Tests.Helpers;
```

---

## Quick Reference

### Project Types Summary

| Project | Type | Framework | Purpose |
|---------|------|-----------|---------|
| Pos.Web.Shared | Class Library | net8.0 | Shared DTOs, models, enums |
| Pos.Web.Infrastructure | Class Library | net8.0 | Data access, repositories |
| Pos.Web.API | Web API | net8.0 | RESTful API, SignalR hubs |
| Pos.Web.Client | Blazor WASM | net8.0 | Frontend PWA |
| Pos.Web.Tests | Test Project | net8.0 | Unit, integration, PBT tests |

### Key Technologies by Project

**Pos.Web.Shared**:
- FluentValidation

**Pos.Web.Infrastructure**:
- Entity Framework Core 8
- SQL Server Provider
- StackExchange.Redis

**Pos.Web.API**:
- ASP.NET Core 8
- SignalR
- AutoMapper
- Serilog
- Swagger/OpenAPI

**Pos.Web.Client**:
- Blazor WebAssembly
- MudBlazor (UI components)
- Fluxor (state management)
- Blazored.LocalStorage
- SignalR Client

**Pos.Web.Tests**:
- xUnit
- Moq
- FluentAssertions
- FsCheck (property-based testing)
- Microsoft.AspNetCore.Mvc.Testing

### Folder Count Summary

- **Total Projects**: 5
- **Total Folders**: ~80+
- **Estimated Files**: ~150+

### Development Commands

```bash
# Create solution
dotnet new sln -n Pos.Web

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

# Build solution
dotnet build

# Run API
dotnet run --project src/Pos.Web.API

# Run Client
dotnet run --project src/Pos.Web.Client

# Run Tests
dotnet test
```

---

## Next Steps

1. **Create the directory structure** as outlined above
2. **Move spec files** from legacy solution to new solution's `.kiro/specs/` folder
3. **Create steering files** for the new solution in `.kiro/steering/`
4. **Generate project files** using the commands in Quick Reference
5. **Install NuGet packages** as listed in each project section
6. **Begin implementation** following the tasks.md file

---

**Document Version**: 1.0  
**Last Updated**: 2026-02-26  
**Maintained By**: Development Team

