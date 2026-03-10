# Blazor WebAssembly Project Structure for MyChair POS

## Solution Structure

```
MyChairPOS.sln
│
├── MyChairPOS.Shared/              # Shared DTOs, Models, Validators
├── MyChairPOS.API/                 # ASP.NET Core Web API
├── MyChairPOS.Client/              # Blazor WebAssembly App
├── MyChairPOS.Infrastructure/      # EF Core, Repositories
└── MyChairPOS.Tests/               # Unit & Integration Tests
```

---

## 1. MyChairPOS.Shared Project

**Purpose**: Code shared between API and Client (DTOs, validation, constants)

```
MyChairPOS.Shared/
├── DTOs/
│   ├── Orders/
│   │   ├── OrderDto.cs
│   │   ├── CreateOrderDto.cs
│   │   ├── UpdateOrderDto.cs
│   │   └── OrderItemDto.cs
│   ├── Products/
│   │   ├── ProductDto.cs
│   │   └── CategoryDto.cs
│   ├── Customers/
│   │   ├── CustomerDto.cs
│   │   └── AddressDto.cs
│   └── Payments/
│       ├── PaymentDto.cs
│       └── ProcessPaymentDto.cs
│
├── Validators/
│   ├── OrderValidator.cs
│   ├── CustomerValidator.cs
│   └── PaymentValidator.cs
│
├── Enums/
│   ├── OrderStatus.cs
│   ├── PaymentMethod.cs
│   └── ServiceType.cs
│
├── Constants/
│   ├── ApiRoutes.cs
│   └── AppConstants.cs
│
└── Extensions/
    └── DateTimeExtensions.cs
```

### Example Files

**DTOs/Orders/OrderDto.cs**
```csharp
namespace MyChairPOS.Shared.DTOs.Orders;

public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; }
    public int? CustomerId { get; set; }
    public string CustomerName { get; set; }
    public DateTime OrderTime { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public int Version { get; set; }
}

public class OrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Notes { get; set; }
    public List<ModifierDto> Modifiers { get; set; } = new();
}
```

**Validators/OrderValidator.cs**
```csharp
using FluentValidation;

namespace MyChairPOS.Shared.Validators;

public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Order must have at least one item");
            
        RuleFor(x => x.ServiceTypeId)
            .GreaterThan((byte)0)
            .WithMessage("Service type is required");
            
        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemValidator());
    }
}
```

---

## 2. MyChairPOS.API Project

**Purpose**: ASP.NET Core Web API backend

```
MyChairPOS.API/
├── Controllers/
│   ├── OrdersController.cs
│   ├── ProductsController.cs
│   ├── CustomersController.cs
│   ├── PaymentsController.cs
│   └── AuthController.cs
│
├── Hubs/
│   ├── OrderHub.cs
│   ├── KitchenHub.cs
│   └── LockHub.cs
│
├── Services/
│   ├── Interfaces/
│   │   ├── IOrderService.cs
│   │   ├── IProductService.cs
│   │   └── ICustomerService.cs
│   └── Implementation/
│       ├── OrderService.cs
│       ├── ProductService.cs
│       └── CustomerService.cs
│
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs
│   └── RequestLoggingMiddleware.cs
│
├── Extensions/
│   ├── ServiceCollectionExtensions.cs
│   └── ApplicationBuilderExtensions.cs
│
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

### Example Files

**Controllers/OrdersController.cs**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IHubContext<OrderHub> _orderHub;
    
    public OrdersController(
        IOrderService orderService,
        IHubContext<OrderHub> orderHub)
    {
        _orderService = orderService;
        _orderHub = orderHub;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<OrderDto>>> GetOrders(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var orders = await _orderService.GetOrdersAsync(from, to);
        return Ok(orders);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound();
        return Ok(order);
    }
    
    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto dto)
    {
        var order = await _orderService.CreateOrderAsync(dto);
        
        // Broadcast to all clients
        await _orderHub.Clients.All.SendAsync("OrderCreated", order);
        
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult<OrderDto>> UpdateOrder(int id, UpdateOrderDto dto)
    {
        try
        {
            var order = await _orderService.UpdateOrderAsync(id, dto);
            await _orderHub.Clients.All.SendAsync("OrderUpdated", order);
            return Ok(order);
        }
        catch (ConcurrencyException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
```

**Hubs/OrderHub.cs**
```csharp
public class OrderHub : Hub
{
    private readonly IOrderService _orderService;
    
    public OrderHub(IOrderService orderService)
    {
        _orderService = orderService;
    }
    
    public async Task JoinOrderGroup(int orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");
    }
    
    public async Task LeaveOrderGroup(int orderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order-{orderId}");
    }
    
    public async Task<bool> LockOrder(int orderId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var locked = await _orderService.TryLockOrderAsync(orderId, userId);
        
        if (locked)
        {
            await Clients.OthersInGroup($"order-{orderId}")
                .SendAsync("OrderLocked", orderId, userId);
        }
        
        return locked;
    }
}
```

**Program.cs**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<POSDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// SignalR
builder.Services.AddSignalR();

// Application Services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// CORS for Blazor
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:7001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("BlazorClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<OrderHub>("/hubs/orders");
app.MapHub<KitchenHub>("/hubs/kitchen");

app.Run();
```

---

## 3. MyChairPOS.Client Project (Blazor WASM)

**Purpose**: Blazor WebAssembly frontend application

```
MyChairPOS.Client/
├── Pages/
│   ├── Index.razor
│   ├── Orders/
│   │   ├── OrdersList.razor
│   │   ├── CreateOrder.razor
│   │   ├── OrderDetails.razor
│   │   └── PendingOrders.razor
│   ├── Products/
│   │   ├── ProductCatalog.razor
│   │   └── ProductDetails.razor
│   ├── Customers/
│   │   ├── CustomerSearch.razor
│   │   └── CustomerForm.razor
│   ├── Checkout/
│   │   └── CheckoutPage.razor
│   └── Kitchen/
│       └── KitchenDisplay.razor
│
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   ├── NavMenu.razor
│   │   └── TopBar.razor
│   ├── Orders/
│   │   ├── OrderCard.razor
│   │   ├── OrderItemsList.razor
│   │   └── OrderStatusBadge.razor
│   ├── Products/
│   │   ├── ProductCard.razor
│   │   ├── ProductGrid.razor
│   │   └── CategoryFilter.razor
│   ├── Shared/
│   │   ├── LoadingSpinner.razor
│   │   ├── ErrorBoundary.razor
│   │   └── ConfirmDialog.razor
│   └── ShoppingCart/
│       ├── ShoppingCart.razor
│       └── CartItem.razor
│
├── Services/
│   ├── ApiServices/
│   │   ├── OrderApiService.cs
│   │   ├── ProductApiService.cs
│   │   └── CustomerApiService.cs
│   ├── StateManagement/
│   │   ├── AppState.cs
│   │   ├── CartState.cs
│   │   └── OrderState.cs
│   ├── SignalRService.cs
│   ├── OfflineService.cs
│   └── PrintService.cs
│
├── wwwroot/
│   ├── css/
│   │   ├── app.css
│   │   └── pos-theme.css
│   ├── js/
│   │   ├── app.js
│   │   └── print-interop.js
│   ├── icons/
│   ├── manifest.json
│   ├── service-worker.js
│   ├── service-worker.published.js
│   └── index.html
│
├── _Imports.razor
├── App.razor
├── Program.cs
└── appsettings.json
```



### Example Blazor Files

**Pages/Orders/CreateOrder.razor**
```razor
@page "/orders/create"
@inject IOrderApiService OrderApi
@inject IProductApiService ProductApi
@inject CartState CartState
@inject NavigationManager Navigation
@inject ISnackbar Snackbar

<MudContainer MaxWidth="MaxWidth.ExtraLarge" Class="mt-4">
    <MudGrid>
        <!-- Product Catalog -->
        <MudItem xs="12" md="7">
            <MudCard>
                <MudCardHeader>
                    <CardHeaderContent>
                        <MudText Typo="Typo.h6">Products</MudText>
                    </CardHeaderContent>
                    <CardHeaderActions>
                        <MudTextField @bind-Value="searchText" 
                                      Placeholder="Search products..."
                                      Adornment="Adornment.Start"
                                      AdornmentIcon="@Icons.Material.Filled.Search"
                                      Immediate="true" />
                    </CardHeaderActions>
                </MudCardHeader>
                <MudCardContent>
                    <ProductGrid Products="@filteredProducts" 
                                 OnProductSelected="AddToCart" />
                </MudCardContent>
            </MudCard>
        </MudItem>
        
        <!-- Shopping Cart -->
        <MudItem xs="12" md="5">
            <ShoppingCart OnCheckout="HandleCheckout" />
        </MudItem>
    </MudGrid>
</MudContainer>

@code {
    private List<ProductDto> products = new();
    private List<ProductDto> filteredProducts = new();
    private string searchText = "";
    
    protected override async Task OnInitializedAsync()
    {
        products = await ProductApi.GetProductsAsync();
        filteredProducts = products;
    }
    
    protected override void OnParametersSet()
    {
        FilterProducts();
    }
    
    private void FilterProducts()
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            filteredProducts = products;
        }
        else
        {
            filteredProducts = products
                .Where(p => p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
    
    private void AddToCart(ProductDto product)
    {
        CartState.AddItem(product);
        Snackbar.Add($"{product.Name} added to cart", Severity.Success);
    }
    
    private void HandleCheckout()
    {
        Navigation.NavigateTo("/checkout");
    }
}
```

**Components/ShoppingCart/ShoppingCart.razor**
```razor
@inject CartState CartState

<MudCard>
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">Shopping Cart</MudText>
        </CardHeaderContent>
    </MudCardHeader>
    <MudCardContent>
        @if (!CartState.Items.Any())
        {
            <MudText Color="Color.Secondary">Cart is empty</MudText>
        }
        else
        {
            @foreach (var item in CartState.Items)
            {
                <CartItem Item="@item" 
                          OnQuantityChanged="UpdateQuantity"
                          OnRemove="RemoveItem" />
            }
            
            <MudDivider Class="my-4" />
            
            <MudGrid>
                <MudItem xs="6">
                    <MudText Typo="Typo.subtitle1">Subtotal:</MudText>
                </MudItem>
                <MudItem xs="6" Class="text-right">
                    <MudText Typo="Typo.subtitle1">€@CartState.Subtotal.ToString("F2")</MudText>
                </MudItem>
                
                <MudItem xs="6">
                    <MudText Typo="Typo.subtitle1">Tax:</MudText>
                </MudItem>
                <MudItem xs="6" Class="text-right">
                    <MudText Typo="Typo.subtitle1">€@CartState.Tax.ToString("F2")</MudText>
                </MudItem>
                
                <MudItem xs="6">
                    <MudText Typo="Typo.h6">Total:</MudText>
                </MudItem>
                <MudItem xs="6" Class="text-right">
                    <MudText Typo="Typo.h6">€@CartState.Total.ToString("F2")</MudText>
                </MudItem>
            </MudGrid>
        }
    </MudCardContent>
    <MudCardActions>
        <MudButton OnClick="ClearCart" 
                   Variant="Variant.Text" 
                   Color="Color.Error"
                   Disabled="@(!CartState.Items.Any())">
            Clear
        </MudButton>
        <MudSpacer />
        <MudButton OnClick="SavePending"
                   Variant="Variant.Outlined"
                   Color="Color.Primary"
                   Disabled="@(!CartState.Items.Any())">
            Save Pending
        </MudButton>
        <MudButton OnClick="Checkout"
                   Variant="Variant.Filled"
                   Color="Color.Success"
                   Disabled="@(!CartState.Items.Any())">
            Checkout
        </MudButton>
    </MudCardActions>
</MudCard>

@code {
    [Parameter]
    public EventCallback OnCheckout { get; set; }
    
    private void UpdateQuantity(CartItemDto item, decimal newQuantity)
    {
        CartState.UpdateQuantity(item.ProductId, newQuantity);
    }
    
    private void RemoveItem(CartItemDto item)
    {
        CartState.RemoveItem(item.ProductId);
    }
    
    private void ClearCart()
    {
        CartState.Clear();
    }
    
    private async Task SavePending()
    {
        // Save as pending order
        await OnCheckout.InvokeAsync();
    }
    
    private async Task Checkout()
    {
        await OnCheckout.InvokeAsync();
    }
}
```

**Services/StateManagement/CartState.cs**
```csharp
public class CartState
{
    public event Action OnChange;
    
    private List<CartItemDto> _items = new();
    
    public IReadOnlyList<CartItemDto> Items => _items.AsReadOnly();
    
    public decimal Subtotal => _items.Sum(x => x.TotalPrice);
    public decimal Tax => Subtotal * 0.19m; // 19% VAT
    public decimal Total => Subtotal + Tax;
    
    public void AddItem(ProductDto product)
    {
        var existing = _items.FirstOrDefault(x => x.ProductId == product.Id);
        
        if (existing != null)
        {
            existing.Quantity++;
            existing.TotalPrice = existing.Quantity * existing.UnitPrice;
        }
        else
        {
            _items.Add(new CartItemDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = 1,
                UnitPrice = product.Price,
                TotalPrice = product.Price
            });
        }
        
        NotifyStateChanged();
    }
    
    public void UpdateQuantity(int productId, decimal quantity)
    {
        var item = _items.FirstOrDefault(x => x.ProductId == productId);
        if (item != null)
        {
            item.Quantity = quantity;
            item.TotalPrice = item.Quantity * item.UnitPrice;
            NotifyStateChanged();
        }
    }
    
    public void RemoveItem(int productId)
    {
        _items.RemoveAll(x => x.ProductId == productId);
        NotifyStateChanged();
    }
    
    public void Clear()
    {
        _items.Clear();
        NotifyStateChanged();
    }
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}
```

**Services/ApiServices/OrderApiService.cs**
```csharp
public interface IOrderApiService
{
    Task<List<OrderDto>> GetOrdersAsync(DateTime? from = null, DateTime? to = null);
    Task<OrderDto> GetOrderByIdAsync(int id);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
    Task<OrderDto> UpdateOrderAsync(int id, UpdateOrderDto dto);
    Task DeleteOrderAsync(int id);
}

public class OrderApiService : IOrderApiService
{
    private readonly HttpClient _http;
    private readonly ILogger<OrderApiService> _logger;
    
    public OrderApiService(HttpClient http, ILogger<OrderApiService> logger)
    {
        _http = http;
        _logger = logger;
    }
    
    public async Task<List<OrderDto>> GetOrdersAsync(DateTime? from = null, DateTime? to = null)
    {
        try
        {
            var query = $"api/orders?from={from}&to={to}";
            return await _http.GetFromJsonAsync<List<OrderDto>>(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get orders");
            throw;
        }
    }
    
    public async Task<OrderDto> GetOrderByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<OrderDto>($"api/orders/{id}");
    }
    
    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/orders", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OrderDto>();
    }
    
    public async Task<OrderDto> UpdateOrderAsync(int id, UpdateOrderDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/orders/{id}", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OrderDto>();
    }
    
    public async Task DeleteOrderAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/orders/{id}");
        response.EnsureSuccessStatusCode();
    }
}
```

**Services/SignalRService.cs**
```csharp
public class SignalRService : IAsyncDisposable
{
    private readonly HubConnection _hubConnection;
    private readonly ILogger<SignalRService> _logger;
    
    public event Action<OrderDto> OnOrderCreated;
    public event Action<OrderDto> OnOrderUpdated;
    public event Action<int, string> OnOrderLocked;
    public event Action<int> OnOrderUnlocked;
    
    public SignalRService(NavigationManager navigation, ILogger<SignalRService> logger)
    {
        _logger = logger;
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(navigation.ToAbsoluteUri("/hubs/orders"))
            .WithAutomaticReconnect()
            .Build();
        
        _hubConnection.On<OrderDto>("OrderCreated", order =>
        {
            OnOrderCreated?.Invoke(order);
        });
        
        _hubConnection.On<OrderDto>("OrderUpdated", order =>
        {
            OnOrderUpdated?.Invoke(order);
        });
        
        _hubConnection.On<int, string>("OrderLocked", (orderId, userId) =>
        {
            OnOrderLocked?.Invoke(orderId, userId);
        });
        
        _hubConnection.On<int>("OrderUnlocked", orderId =>
        {
            OnOrderUnlocked?.Invoke(orderId);
        });
    }
    
    public async Task StartAsync()
    {
        try
        {
            await _hubConnection.StartAsync();
            _logger.LogInformation("SignalR connected");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to SignalR");
        }
    }
    
    public async Task<bool> LockOrderAsync(int orderId)
    {
        return await _hubConnection.InvokeAsync<bool>("LockOrder", orderId);
    }
    
    public async Task UnlockOrderAsync(int orderId)
    {
        await _hubConnection.InvokeAsync("UnlockOrder", orderId);
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
```

**Program.cs**
```csharp
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyChairPOS.Client;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HTTP Client
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress) 
});

// MudBlazor
builder.Services.AddMudServices();

// API Services
builder.Services.AddScoped<IOrderApiService, OrderApiService>();
builder.Services.AddScoped<IProductApiService, ProductApiService>();
builder.Services.AddScoped<ICustomerApiService, CustomerApiService>();

// State Management
builder.Services.AddScoped<CartState>();
builder.Services.AddScoped<OrderState>();
builder.Services.AddScoped<AppState>();

// SignalR
builder.Services.AddScoped<SignalRService>();

// Offline Support
builder.Services.AddScoped<OfflineService>();

await builder.Build().RunAsync();
```

**wwwroot/manifest.json** (PWA)
```json
{
  "name": "MyChair POS",
  "short_name": "MyChair POS",
  "start_url": "/",
  "display": "standalone",
  "background_color": "#ffffff",
  "theme_color": "#1976d2",
  "orientation": "any",
  "icons": [
    {
      "src": "icon-192.png",
      "type": "image/png",
      "sizes": "192x192"
    },
    {
      "src": "icon-512.png",
      "type": "image/png",
      "sizes": "512x512"
    }
  ]
}
```

---

## 4. MyChairPOS.Infrastructure Project

**Purpose**: Data access, repositories, EF Core

```
MyChairPOS.Infrastructure/
├── Data/
│   ├── POSDbContext.cs
│   ├── Configurations/
│   │   ├── OrderConfiguration.cs
│   │   ├── ProductConfiguration.cs
│   │   └── CustomerConfiguration.cs
│   └── Migrations/
│
├── Repositories/
│   ├── Interfaces/
│   │   ├── IOrderRepository.cs
│   │   ├── IProductRepository.cs
│   │   └── ICustomerRepository.cs
│   └── Implementation/
│       ├── OrderRepository.cs
│       ├── ProductRepository.cs
│       └── CustomerRepository.cs
│
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

---

## 5. MyChairPOS.Tests Project

```
MyChairPOS.Tests/
├── Unit/
│   ├── Services/
│   │   ├── OrderServiceTests.cs
│   │   └── ProductServiceTests.cs
│   └── Validators/
│       └── OrderValidatorTests.cs
│
├── Integration/
│   ├── Api/
│   │   ├── OrdersControllerTests.cs
│   │   └── ProductsControllerTests.cs
│   └── Repositories/
│       └── OrderRepositoryTests.cs
│
└── E2E/
    └── Blazor/
        └── OrderFlowTests.cs
```

---

## Project Dependencies

```
MyChairPOS.Client
├── depends on → MyChairPOS.Shared

MyChairPOS.API
├── depends on → MyChairPOS.Shared
├── depends on → MyChairPOS.Infrastructure

MyChairPOS.Infrastructure
├── depends on → MyChairPOS.Shared

MyChairPOS.Tests
├── depends on → MyChairPOS.API
├── depends on → MyChairPOS.Client
├── depends on → MyChairPOS.Shared
└── depends on → MyChairPOS.Infrastructure
```

---

## NuGet Packages

### MyChairPOS.Shared
```xml
<ItemGroup>
  <PackageReference Include="FluentValidation" Version="11.9.0" />
</ItemGroup>
```

### MyChairPOS.API
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.SignalR" Version="1.1.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
  <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
  <PackageReference Include="StackExchangeRedis" Version="2.7.0" />
  <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
</ItemGroup>
```

### MyChairPOS.Client
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="8.0.0" />
  <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="8.0.0" />
  <PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="8.0.0" />
  <PackageReference Include="MudBlazor" Version="6.11.2" />
  <PackageReference Include="Blazored.LocalStorage" Version="4.4.0" />
</ItemGroup>
```

---

## Getting Started Commands

```bash
# Create solution
dotnet new sln -n MyChairPOS

# Create projects
dotnet new classlib -n MyChairPOS.Shared
dotnet new webapi -n MyChairPOS.API
dotnet new blazorwasm -n MyChairPOS.Client
dotnet new classlib -n MyChairPOS.Infrastructure
dotnet new xunit -n MyChairPOS.Tests

# Add projects to solution
dotnet sln add MyChairPOS.Shared
dotnet sln add MyChairPOS.API
dotnet sln add MyChairPOS.Client
dotnet sln add MyChairPOS.Infrastructure
dotnet sln add MyChairPOS.Tests

# Add project references
dotnet add MyChairPOS.Client reference MyChairPOS.Shared
dotnet add MyChairPOS.API reference MyChairPOS.Shared
dotnet add MyChairPOS.API reference MyChairPOS.Infrastructure
dotnet add MyChairPOS.Infrastructure reference MyChairPOS.Shared

# Add NuGet packages
dotnet add MyChairPOS.Client package MudBlazor
dotnet add MyChairPOS.API package Microsoft.EntityFrameworkCore.SqlServer
```

---

## Summary

This structure provides:
- ✅ Clear separation of concerns
- ✅ Code sharing between API and Client
- ✅ Testable architecture
- ✅ Scalable for future growth
- ✅ Follows .NET best practices
