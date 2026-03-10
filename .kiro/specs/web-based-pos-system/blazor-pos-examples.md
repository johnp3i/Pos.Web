# Blazor POS - Practical Code Examples

## Overview

This document provides comprehensive Blazor WebAssembly code examples for the MyChair POS system. Each example demonstrates a complete feature with real-world implementation patterns.

## Table of Contents

1. [Product Catalog with Search](#1-product-catalog-with-search)
2. [Shopping Cart Management](#2-shopping-cart-management)
3. [Pending Orders Management](#3-pending-orders-management)
4. [Kitchen Display Real-Time Updates](#4-kitchen-display-real-time-updates)
5. [Payment Processing](#5-payment-processing)
6. [Offline Sync Handling](#6-offline-sync-handling)
7. [Print Integration](#7-print-integration)
8. [Customer Search and Management](#8-customer-search-and-management)
9. [Order Locking with SignalR](#9-order-locking-with-signalr)
10. [Server Commands System](#10-server-commands-system)

---

## 1. Product Catalog with Search

### ProductCatalog.razor

```razor
@page "/products"
@inject IProductService ProductService
@inject ICartState CartState
@inject ISnackbar Snackbar

<MudContainer MaxWidth="MaxWidth.ExtraLarge" Class="mt-4">
    <MudGrid>
        <!-- Search and Filter Section -->
        <MudItem xs="12">
            <MudPaper Class="pa-4">
                <MudGrid>
                    <MudItem xs="12" md="6">
                        <MudTextField @bind-Value="_searchQuery" 
                                      Label="Search Products" 
                                      Variant="Variant.Outlined"
                                      Adornment="Adornment.End"
                                      AdornmentIcon="@Icons.Material.Filled.Search"
                                      OnKeyUp="HandleSearchKeyUp"
                                      Immediate="true" />
                    </MudItem>
                    <MudItem xs="12" md="6">
                        <MudSelect @bind-Value="_selectedCategoryId" 
                                   Label="Category" 
                                   Variant="Variant.Outlined"
                                   T="int?"
                                   Clearable="true"
                                   OnClearButtonClick="() => LoadProductsAsync()">
                            @foreach (var category in _categories)
                            {
                                <MudSelectItem Value="@category.Id">@category.Name</MudSelectItem>
                            }
                        </MudSelect>
                    </MudItem>
                </MudGrid>
            </MudPaper>
        </MudItem>

        <!-- Product Grid -->
        <MudItem xs="12">
            @if (_isLoading)
            {
                <MudProgressLinear Indeterminate="true" Color="Color.Primary" />
            }
            else if (_products == null || !_products.Any())
            {
                <MudAlert Severity="Severity.Info">No products found</MudAlert>
            }
            else
            {
                <MudGrid>
                    @foreach (var product in _products)
                    {
                        <MudItem xs="12" sm="6" md="4" lg="3">
                            <MudCard Class="product-card">
                                <MudCardMedia Image="@product.ImageUrl" Height="200" />
                                <MudCardContent>
                                    <MudText Typo="Typo.h6">@product.Name</MudText>
                                    <MudText Typo="Typo.body2" Color="Color.Secondary">
                                        @product.CategoryName
                                    </MudText>
                                    <MudText Typo="Typo.h5" Color="Color.Primary" Class="mt-2">
                                        $@product.Price.ToString("F2")
                                    </MudText>
                                    @if (product.StockQuantity <= 5)
                                    {
                                        <MudChip Size="Size.Small" Color="Color.Warning">
                                            Low Stock: @product.StockQuantity
                                        </MudChip>
                                    }
                                </MudCardContent>
                                <MudCardActions>
                                    <MudButton Variant="Variant.Filled" 
                                               Color="Color.Primary" 
                                               FullWidth="true"
                                               OnClick="() => AddToCart(product)"
                                               Disabled="@(product.StockQuantity == 0)">
                                        @(product.StockQuantity == 0 ? "Out of Stock" : "Add to Cart")
                                    </MudButton>
                                </MudCardActions>
                            </MudCard>
                        </MudItem>
                    }
                </MudGrid>
            }
        </MudItem>
    </MudGrid>
</MudContainer>

@code {
    private List<ProductDto> _products = new();
    private List<CategoryDto> _categories = new();
    private string _searchQuery = string.Empty;
    private int? _selectedCategoryId;
    private bool _isLoading;
    private System.Timers.Timer _searchDebounceTimer;

    protected override async Task OnInitializedAsync()
    {
        await LoadCategoriesAsync();
        await LoadProductsAsync();
        
        // Setup debounce timer for search
        _searchDebounceTimer = new System.Timers.Timer(300);
        _searchDebounceTimer.Elapsed += async (sender, e) => await PerformSearchAsync();
        _searchDebounceTimer.AutoReset = false;
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            _categories = await ProductService.GetCategoriesAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to load categories: {ex.Message}", Severity.Error);
        }
    }

    private async Task LoadProductsAsync()
    {
        _isLoading = true;
        try
        {
            _products = await ProductService.GetProductsAsync(
                categoryId: _selectedCategoryId,
                searchQuery: _searchQuery
            );
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to load products: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void HandleSearchKeyUp(KeyboardEventArgs e)
    {
        _searchDebounceTimer.Stop();
        _searchDebounceTimer.Start();
    }

    private async Task PerformSearchAsync()
    {
        await InvokeAsync(async () =>
        {
            await LoadProductsAsync();
            StateHasChanged();
        });
    }

    private async Task AddToCart(ProductDto product)
    {
        try
        {
            await CartState.AddItemAsync(new CartItemDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = 1,
                ImageUrl = product.ImageUrl
            });
            
            Snackbar.Add($"{product.Name} added to cart", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to add to cart: {ex.Message}", Severity.Error);
        }
    }

    public void Dispose()
    {
        _searchDebounceTimer?.Dispose();
    }
}
```

### IProductService.cs (Client/Services)

```csharp
public interface IProductService
{
    Task<List<ProductDto>> GetProductsAsync(int? categoryId = null, string searchQuery = null);
    Task<List<CategoryDto>> GetCategoriesAsync();
    Task<ProductDto> GetProductByIdAsync(int id);
}

public class ProductService : IProductService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductService> _logger;

    public ProductService(HttpClient httpClient, ILogger<ProductService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<ProductDto>> GetProductsAsync(int? categoryId = null, string searchQuery = null)
    {
        var queryParams = new List<string>();
        if (categoryId.HasValue)
            queryParams.Add($"categoryId={categoryId}");
        if (!string.IsNullOrWhiteSpace(searchQuery))
            queryParams.Add($"search={Uri.EscapeDataString(searchQuery)}");

        var query = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
        var response = await _httpClient.GetAsync($"api/v1/products{query}");
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<List<ProductDto>>();
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        var response = await _httpClient.GetAsync("api/v1/categories");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
    }

    public async Task<ProductDto> GetProductByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"api/v1/products/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductDto>();
    }
}
```

---

## 2. Shopping Cart Management

### ShoppingCart.razor

```razor
@inject ICartState CartState
@inject ISnackbar Snackbar
@implements IDisposable

<MudPaper Class="pa-4" Elevation="2">
    <MudText Typo="Typo.h5" Class="mb-4">
        <MudIcon Icon="@Icons.Material.Filled.ShoppingCart" Class="mr-2" />
        Shopping Cart (@CartState.ItemCount)
    </MudText>

    @if (!CartState.Items.Any())
    {
        <MudAlert Severity="Severity.Info">Cart is empty</MudAlert>
    }
    else
    {
        <MudList>
            @foreach (var item in CartState.Items)
            {
                <MudListItem>
                    <MudGrid>
                        <MudItem xs="6">
                            <MudText Typo="Typo.body1">@item.ProductName</MudText>
                            @if (!string.IsNullOrEmpty(item.Notes))
                            {
                                <MudText Typo="Typo.caption" Color="Color.Secondary">
                                    Note: @item.Notes
                                </MudText>
                            }
                            @if (item.Extras?.Any() == true)
                            {
                                <MudText Typo="Typo.caption" Color="Color.Secondary">
                                    Extras: @string.Join(", ", item.Extras.Select(e => e.Name))
                                </MudText>
                            }
                        </MudItem>
                        <MudItem xs="3">
                            <MudNumericField @bind-Value="item.Quantity"
                                             Min="1"
                                             Max="99"
                                             Variant="Variant.Outlined"
                                             OnValueChanged="() => UpdateQuantity(item)" />
                        </MudItem>
                        <MudItem xs="2">
                            <MudText Typo="Typo.body1" Align="Align.Right">
                                $@((item.Price * item.Quantity).ToString("F2"))
                            </MudText>
                        </MudItem>
                        <MudItem xs="1">
                            <MudIconButton Icon="@Icons.Material.Filled.Delete"
                                           Color="Color.Error"
                                           Size="Size.Small"
                                           OnClick="() => RemoveItem(item)" />
                        </MudItem>
                    </MudGrid>
                </MudListItem>
                <MudDivider />
            }
        </MudList>

        <MudDivider Class="my-4" />

        <!-- Totals Section -->
        <MudGrid>
            <MudItem xs="8">
                <MudText Typo="Typo.body1">Subtotal:</MudText>
            </MudItem>
            <MudItem xs="4">
                <MudText Typo="Typo.body1" Align="Align.Right">
                    $@CartState.Subtotal.ToString("F2")
                </MudText>
            </MudItem>

            @if (CartState.DiscountAmount > 0)
            {
                <MudItem xs="8">
                    <MudText Typo="Typo.body1" Color="Color.Success">
                        Discount (@CartState.DiscountPercentage%):
                    </MudText>
                </MudItem>
                <MudItem xs="4">
                    <MudText Typo="Typo.body1" Align="Align.Right" Color="Color.Success">
                        -$@CartState.DiscountAmount.ToString("F2")
                    </MudText>
                </MudItem>
            }

            <MudItem xs="8">
                <MudText Typo="Typo.body1">Tax:</MudText>
            </MudItem>
            <MudItem xs="4">
                <MudText Typo="Typo.body1" Align="Align.Right">
                    $@CartState.TaxAmount.ToString("F2")
                </MudText>
            </MudItem>

            <MudItem xs="8">
                <MudText Typo="Typo.h6">Total:</MudText>
            </MudItem>
            <MudItem xs="4">
                <MudText Typo="Typo.h6" Align="Align.Right" Color="Color.Primary">
                    $@CartState.Total.ToString("F2")
                </MudText>
            </MudItem>
        </MudGrid>

        <!-- Action Buttons -->
        <MudGrid Class="mt-4">
            <MudItem xs="6">
                <MudButton Variant="Variant.Outlined"
                           Color="Color.Secondary"
                           FullWidth="true"
                           OnClick="ClearCart">
                    Clear Cart
                </MudButton>
            </MudItem>
            <MudItem xs="6">
                <MudButton Variant="Variant.Filled"
                           Color="Color.Primary"
                           FullWidth="true"
                           OnClick="Checkout"
                           Disabled="@(!CartState.Items.Any())">
                    Checkout
                </MudButton>
            </MudItem>
        </MudGrid>
    }
</MudPaper>

@code {
    [Parameter]
    public EventCallback OnCheckout { get; set; }

    protected override void OnInitialized()
    {
        CartState.OnChange += StateHasChanged;
    }

    private async Task UpdateQuantity(CartItemDto item)
    {
        await CartState.UpdateQuantityAsync(item.ProductId, item.Quantity);
    }

    private async Task RemoveItem(CartItemDto item)
    {
        var confirmed = await Snackbar.Add(
            $"Remove {item.ProductName}?",
            Severity.Warning,
            config => config.Action = "Remove"
        ).Result;

        if (confirmed)
        {
            await CartState.RemoveItemAsync(item.ProductId);
            Snackbar.Add("Item removed", Severity.Success);
        }
    }

    private async Task ClearCart()
    {
        await CartState.ClearAsync();
        Snackbar.Add("Cart cleared", Severity.Info);
    }

    private async Task Checkout()
    {
        await OnCheckout.InvokeAsync();
    }

    public void Dispose()
    {
        CartState.OnChange -= StateHasChanged;
    }
}
```

### CartState.cs (Fluxor State Management)

```csharp
public class CartState
{
    public List<CartItemDto> Items { get; init; } = new();
    public decimal Subtotal { get; init; }
    public decimal DiscountPercentage { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal Total { get; init; }
    public int ItemCount => Items.Sum(i => i.Quantity);
}

public class CartFeature : Feature<CartState>
{
    public override string GetName() => "Cart";
    protected override CartState GetInitialState() => new CartState();
}

// Actions
public record AddItemToCartAction(CartItemDto Item);
public record RemoveItemFromCartAction(int ProductId);
public record UpdateItemQuantityAction(int ProductId, int Quantity);
public record ClearCartAction();
public record ApplyDiscountAction(decimal DiscountPercentage);

// Reducers
public static class CartReducers
{
    [ReducerMethod]
    public static CartState OnAddItem(CartState state, AddItemToCartAction action)
    {
        var items = state.Items.ToList();
        var existingItem = items.FirstOrDefault(i => 
            i.ProductId == action.Item.ProductId &&
            i.Notes == action.Item.Notes &&
            AreExtrasEqual(i.Extras, action.Item.Extras));

        if (existingItem != null)
        {
            existingItem.Quantity += action.Item.Quantity;
        }
        else
        {
            items.Add(action.Item);
        }

        return RecalculateTotals(state with { Items = items });
    }

    [ReducerMethod]
    public static CartState OnRemoveItem(CartState state, RemoveItemFromCartAction action)
    {
        var items = state.Items.Where(i => i.ProductId != action.ProductId).ToList();
        return RecalculateTotals(state with { Items = items });
    }

    [ReducerMethod]
    public static CartState OnUpdateQuantity(CartState state, UpdateItemQuantityAction action)
    {
        var items = state.Items.ToList();
        var item = items.FirstOrDefault(i => i.ProductId == action.ProductId);
        if (item != null)
        {
            item.Quantity = action.Quantity;
        }
        return RecalculateTotals(state with { Items = items });
    }

    [ReducerMethod]
    public static CartState OnClearCart(CartState state, ClearCartAction action)
    {
        return new CartState();
    }

    [ReducerMethod]
    public static CartState OnApplyDiscount(CartState state, ApplyDiscountAction action)
    {
        return RecalculateTotals(state with { DiscountPercentage = action.DiscountPercentage });
    }

    private static CartState RecalculateTotals(CartState state)
    {
        var subtotal = state.Items.Sum(i => i.Price * i.Quantity);
        var discountAmount = subtotal * (state.DiscountPercentage / 100);
        var taxableAmount = subtotal - discountAmount;
        var taxAmount = taxableAmount * 0.10m; // 10% tax
        var total = taxableAmount + taxAmount;

        return state with
        {
            Subtotal = subtotal,
            DiscountAmount = discountAmount,
            TaxAmount = taxAmount,
            Total = total
        };
    }

    private static bool AreExtrasEqual(List<ExtraDto> extras1, List<ExtraDto> extras2)
    {
        if (extras1 == null && extras2 == null) return true;
        if (extras1 == null || extras2 == null) return false;
        if (extras1.Count != extras2.Count) return false;
        
        return extras1.OrderBy(e => e.Id).SequenceEqual(extras2.OrderBy(e => e.Id));
    }
}
```

---

## 3. Pending Orders Management

### PendingOrders.razor

```razor
@page "/pending-orders"
@inject IPendingOrderService PendingOrderService
@inject IDialogService DialogService
@inject ISnackbar Snackbar
@inject NavigationManager Navigation

<MudContainer MaxWidth="MaxWidth.ExtraLarge" Class="mt-4">
    <MudText Typo="Typo.h4" Class="mb-4">Pending Orders</MudText>

    @if (_isLoading)
    {
        <MudProgressLinear Indeterminate="true" Color="Color.Primary" />
    }
    else if (_pendingOrders == null || !_pendingOrders.Any())
    {
        <MudAlert Severity="Severity.Info">No pending orders</MudAlert>
    }
    else
    {
        <MudGrid>
            @foreach (var order in _pendingOrders)
            {
                <MudItem xs="12" md="6" lg="4">
                    <MudCard>
                        <MudCardHeader>
                            <CardHeaderContent>
                                <MudText Typo="Typo.h6">Order #@order.Id</MudText>
                                <MudText Typo="Typo.body2" Color="Color.Secondary">
                                    @order.CreatedAt.ToString("MMM dd, yyyy HH:mm")
                                </MudText>
                            </CardHeaderContent>
                            <CardHeaderActions>
                                <MudChip Size="Size.Small" Color="GetServiceTypeColor(order.ServiceTypeId)">
                                    @order.ServiceTypeName
                                </MudChip>
                            </CardHeaderActions>
                        </MudCardHeader>
                        <MudCardContent>
                            @if (order.Customer != null)
                            {
                                <MudText Typo="Typo.body2">
                                    <MudIcon Icon="@Icons.Material.Filled.Person" Size="Size.Small" />
                                    @order.Customer.Name
                                </MudText>
                            }
                            @if (order.TableNumber.HasValue)
                            {
                                <MudText Typo="Typo.body2">
                                    <MudIcon Icon="@Icons.Material.Filled.TableRestaurant" Size="Size.Small" />
                                    Table @order.TableNumber
                                </MudText>
                            }
                            <MudDivider Class="my-2" />
                            <MudText Typo="Typo.body2">
                                @order.ItemCount item(s)
                            </MudText>
                            <MudText Typo="Typo.h6" Color="Color.Primary">
                                Total: $@order.Total.ToString("F2")
                            </MudText>
                            @if (!string.IsNullOrEmpty(order.Notes))
                            {
                                <MudText Typo="Typo.caption" Color="Color.Secondary" Class="mt-2">
                                    Note: @order.Notes
                                </MudText>
                            }
                        </MudCardContent>
                        <MudCardActions>
                            <MudButton Variant="Variant.Text" 
                                       Color="Color.Primary"
                                       OnClick="() => EditOrder(order)">
                                Edit
                            </MudButton>
                            <MudButton Variant="Variant.Text" 
                                       Color="Color.Success"
                                       OnClick="() => ConvertToInvoice(order)">
                                Convert
                            </MudButton>
                            <MudButton Variant="Variant.Text" 
                                       Color="Color.Error"
                                       OnClick="() => DeleteOrder(order)">
                                Delete
                            </MudButton>
                        </MudCardActions>
                    </MudCard>
                </MudItem>
            }
        </MudGrid>
    }
</MudContainer>

@code {
    private List<PendingOrderDto> _pendingOrders = new();
    private bool _isLoading;

    protected override async Task OnInitializedAsync()
    {
        await LoadPendingOrdersAsync();
    }

    private async Task LoadPendingOrdersAsync()
    {
        _isLoading = true;
        try
        {
            _pendingOrders = await PendingOrderService.GetPendingOrdersAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to load pending orders: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void EditOrder(PendingOrderDto order)
    {
        Navigation.NavigateTo($"/orders/edit/{order.Id}");
    }

    private async Task ConvertToInvoice(PendingOrderDto order)
    {
        var parameters = new DialogParameters
        {
            ["Order"] = order
        };

        var dialog = await DialogService.ShowAsync<ConvertPendingOrderDialog>(
            "Convert to Invoice", 
            parameters
        );
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            await LoadPendingOrdersAsync();
            Snackbar.Add("Order converted to invoice", Severity.Success);
        }
    }

    private async Task DeleteOrder(PendingOrderDto order)
    {
        bool? confirmed = await DialogService.ShowMessageBox(
            "Delete Order",
            $"Are you sure you want to delete order #{order.Id}?",
            yesText: "Delete",
            cancelText: "Cancel"
        );

        if (confirmed == true)
        {
            try
            {
                await PendingOrderService.DeletePendingOrderAsync(order.Id);
                await LoadPendingOrdersAsync();
                Snackbar.Add("Order deleted", Severity.Success);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Failed to delete order: {ex.Message}", Severity.Error);
            }
        }
    }

    private Color GetServiceTypeColor(int serviceTypeId)
    {
        return serviceTypeId switch
        {
            1 => Color.Primary,   // Dine-in
            2 => Color.Success,   // Takeout
            3 => Color.Warning,   // Delivery
            _ => Color.Default
        };
    }
}
```

---

## 4. Kitchen Display Real-Time Updates

### KitchenDisplay.razor

```razor
@page "/kitchen"
@inject IOrderService OrderService
@inject IKitchenHubService KitchenHub
@inject ISnackbar Snackbar
@implements IAsyncDisposable

<MudContainer MaxWidth="MaxWidth.ExtraLarge" Class="mt-4">
    <MudText Typo="Typo.h4" Class="mb-4">
        Kitchen Display
        <MudChip Size="Size.Small" Color="Color.Info">
            @_activeOrders.Count Active Orders
        </MudChip>
    </MudText>

    <MudGrid>
        <!-- New Orders Column -->
        <MudItem xs="12" md="4">
            <MudPaper Class="pa-4" Elevation="2">
                <MudText Typo="Typo.h6" Class="mb-3">
                    <MudIcon Icon="@Icons.Material.Filled.FiberNew" Color="Color.Warning" />
                    New (@_newOrders.Count)
                </MudText>
                @foreach (var order in _newOrders)
                {
                    <KitchenOrderCard Order="@order" 
                                      OnStatusChange="HandleStatusChange" />
                }
            </MudPaper>
        </MudItem>

        <!-- In Progress Column -->
        <MudItem xs="12" md="4">
            <MudPaper Class="pa-4" Elevation="2">
                <MudText Typo="Typo.h6" Class="mb-3">
                    <MudIcon Icon="@Icons.Material.Filled.Restaurant" Color="Color.Info" />
                    In Progress (@_inProgressOrders.Count)
                </MudText>
                @foreach (var order in _inProgressOrders)
                {
                    <KitchenOrderCard Order="@order" 
                                      OnStatusChange="HandleStatusChange" />
                }
            </MudPaper>
        </MudItem>

        <!-- Ready Column -->
        <MudItem xs="12" md="4">
            <MudPaper Class="pa-4" Elevation="2">
                <MudText Typo="Typo.h6" Class="mb-3">
                    <MudIcon Icon="@Icons.Material.Filled.CheckCircle" Color="Color.Success" />
                    Ready (@_readyOrders.Count)
                </MudText>
                @foreach (var order in _readyOrders)
                {
                    <KitchenOrderCard Order="@order" 
                                      OnStatusChange="HandleStatusChange" />
                }
            </MudPaper>
        </MudItem>
    </MudGrid>
</MudContainer>

@code {
    private List<KitchenOrderDto> _activeOrders = new();
    private List<KitchenOrderDto> _newOrders = new();
    private List<KitchenOrderDto> _inProgressOrders = new();
    private List<KitchenOrderDto> _readyOrders = new();

    protected override async Task OnInitializedAsync()
    {
        // Load initial orders
        await LoadOrdersAsync();

        // Subscribe to SignalR events
        await KitchenHub.StartAsync();
        KitchenHub.OnNewOrder += HandleNewOrder;
        KitchenHub.OnOrderStatusChanged += HandleOrderStatusChanged;
        KitchenHub.OnOrderCancelled += HandleOrderCancelled;
    }

    private async Task LoadOrdersAsync()
    {
        try
        {
            _activeOrders = await OrderService.GetActiveKitchenOrdersAsync();
            OrganizeOrders();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to load orders: {ex.Message}", Severity.Error);
        }
    }

    private void OrganizeOrders()
    {
        _newOrders = _activeOrders.Where(o => o.Status == "New").ToList();
        _inProgressOrders = _activeOrders.Where(o => o.Status == "InProgress").ToList();
        _readyOrders = _activeOrders.Where(o => o.Status == "Ready").ToList();
    }

    private async Task HandleNewOrder(KitchenOrderDto order)
    {
        await InvokeAsync(() =>
        {
            _activeOrders.Add(order);
            OrganizeOrders();
            StateHasChanged();
            Snackbar.Add($"New order #{order.OrderNumber}", Severity.Info);
        });
    }

    private async Task HandleOrderStatusChanged(int orderId, string newStatus)
    {
        await InvokeAsync(() =>
        {
            var order = _activeOrders.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                order.Status = newStatus;
                OrganizeOrders();
                StateHasChanged();
            }
        });
    }

    private async Task HandleOrderCancelled(int orderId)
    {
        await InvokeAsync(() =>
        {
            _activeOrders.RemoveAll(o => o.Id == orderId);
            OrganizeOrders();
            StateHasChanged();
            Snackbar.Add($"Order #{orderId} cancelled", Severity.Warning);
        });
    }

    private async Task HandleStatusChange(KitchenOrderDto order, string newStatus)
    {
        try
        {
            await OrderService.UpdateOrderStatusAsync(order.Id, newStatus);
            // SignalR will notify all clients including this one
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to update status: {ex.Message}", Severity.Error);
        }
    }

    public async ValueTask DisposeAsync()
    {
        KitchenHub.OnNewOrder -= HandleNewOrder;
        KitchenHub.OnOrderStatusChanged -= HandleOrderStatusChanged;
        KitchenHub.OnOrderCancelled -= HandleOrderCancelled;
        await KitchenHub.StopAsync();
    }
}
```

### KitchenOrderCard.razor (Component)

```razor
<MudCard Class="mb-3 kitchen-order-card">
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">Order #@Order.OrderNumber</MudText>
            <MudText Typo="Typo.caption">
                @GetElapsedTime()
            </MudText>
        </CardHeaderContent>
        <CardHeaderActions>
            @if (Order.TableNumber.HasValue)
            {
                <MudChip Size="Size.Small">Table @Order.TableNumber</MudChip>
            }
        </CardHeaderActions>
    </MudCardHeader>
    <MudCardContent>
        @foreach (var item in Order.Items)
        {
            <MudText Typo="Typo.body2">
                <strong>@item.Quantity x</strong> @item.ProductName
            </MudText>
            @if (!string.IsNullOrEmpty(item.Notes))
            {
                <MudText Typo="Typo.caption" Color="Color.Warning" Class="ml-4">
                    ⚠ @item.Notes
                </MudText>
            }
            @if (item.Extras?.Any() == true)
            {
                <MudText Typo="Typo.caption" Color="Color.Secondary" Class="ml-4">
                    + @string.Join(", ", item.Extras.Select(e => e.Name))
                </MudText>
            }
        }
    </MudCardContent>
    <MudCardActions>
        @if (Order.Status == "New")
        {
            <MudButton Variant="Variant.Filled" 
                       Color="Color.Primary"
                       Size="Size.Small"
                       FullWidth="true"
                       OnClick="() => ChangeStatus(\"InProgress\")">
                Start Preparing
            </MudButton>
        }
        else if (Order.Status == "InProgress")
        {
            <MudButton Variant="Variant.Filled" 
                       Color="Color.Success"
                       Size="Size.Small"
                       FullWidth="true"
                       OnClick="() => ChangeStatus(\"Ready\")">
                Mark Ready
            </MudButton>
        }
        else if (Order.Status == "Ready")
        {
            <MudButton Variant="Variant.Filled" 
                       Color="Color.Default"
                       Size="Size.Small"
                       FullWidth="true"
                       OnClick="() => ChangeStatus(\"Delivered\")">
                Delivered
            </MudButton>
        }
    </MudCardActions>
</MudCard>

@code {
    [Parameter]
    public KitchenOrderDto Order { get; set; }

    [Parameter]
    public EventCallback<(KitchenOrderDto Order, string NewStatus)> OnStatusChange { get; set; }

    private string GetElapsedTime()
    {
        var elapsed = DateTime.Now - Order.CreatedAt;
        if (elapsed.TotalMinutes < 60)
            return $"{(int)elapsed.TotalMinutes} min ago";
        return $"{(int)elapsed.TotalHours}h {elapsed.Minutes}m ago";
    }

    private async Task ChangeStatus(string newStatus)
    {
        await OnStatusChange.InvokeAsync((Order, newStatus));
    }
}
```

### IKitchenHubService.cs

```csharp
public interface IKitchenHubService
{
    event Func<KitchenOrderDto, Task> OnNewOrder;
    event Func<int, string, Task> OnOrderStatusChanged;
    event Func<int, Task> OnOrderCancelled;

    Task StartAsync();
    Task StopAsync();
    Task MarkOrderInProgressAsync(int orderId);
    Task MarkOrderReadyAsync(int orderId);
    Task MarkOrderDeliveredAsync(int orderId);
}

public class KitchenHubService : IKitchenHubService, IAsyncDisposable
{
    private readonly HubConnection _hubConnection;
    private readonly ILogger<KitchenHubService> _logger;

    public event Func<KitchenOrderDto, Task> OnNewOrder;
    public event Func<int, string, Task> OnOrderStatusChanged;
    public event Func<int, Task> OnOrderCancelled;

    public KitchenHubService(NavigationManager navigationManager, ILogger<KitchenHubService> logger)
    {
        _logger = logger;
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(navigationManager.ToAbsoluteUri("/hubs/kitchen"))
            .WithAutomaticReconnect()
            .Build();

        // Register server-to-client handlers
        _hubConnection.On<KitchenOrderDto>("NewOrderReceived", async (order) =>
        {
            _logger.LogInformation("New order received: {OrderId}", order.Id);
            if (OnNewOrder != null)
                await OnNewOrder.Invoke(order);
        });

        _hubConnection.On<int, string>("OrderStatusChanged", async (orderId, status) =>
        {
            _logger.LogInformation("Order {OrderId} status changed to {Status}", orderId, status);
            if (OnOrderStatusChanged != null)
                await OnOrderStatusChanged.Invoke(orderId, status);
        });

        _hubConnection.On<int>("OrderCancelled", async (orderId) =>
        {
            _logger.LogInformation("Order {OrderId} cancelled", orderId);
            if (OnOrderCancelled != null)
                await OnOrderCancelled.Invoke(orderId);
        });
    }

    public async Task StartAsync()
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
        {
            await _hubConnection.StartAsync();
            _logger.LogInformation("Kitchen Hub connected");
        }
    }

    public async Task StopAsync()
    {
        if (_hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.StopAsync();
            _logger.LogInformation("Kitchen Hub disconnected");
        }
    }

    public async Task MarkOrderInProgressAsync(int orderId)
    {
        await _hubConnection.InvokeAsync("MarkOrderInProgress", orderId);
    }

    public async Task MarkOrderReadyAsync(int orderId)
    {
        await _hubConnection.InvokeAsync("MarkOrderReady", orderId);
    }

    public async Task MarkOrderDeliveredAsync(int orderId)
    {
        await _hubConnection.InvokeAsync("MarkOrderDelivered", orderId);
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

---

## 5. Payment Processing

### PaymentDialog.razor

```razor
@inject IPaymentService PaymentService
@inject ISnackbar Snackbar

<MudDialog>
    <DialogContent>
        <MudText Typo="Typo.h6" Class="mb-4">Process Payment</MudText>
        
        <MudGrid>
            <MudItem xs="12">
                <MudText Typo="Typo.body1">Order Total: <strong>$@OrderTotal.ToString("F2")</strong></MudText>
            </MudItem>

            <MudItem xs="12">
                <MudSelect @bind-Value="_selectedPaymentMethodId" 
                           Label="Payment Method" 
                           Variant="Variant.Outlined"
                           Required="true">
                    @foreach (var method in _paymentMethods)
                    {
                        <MudSelectItem Value="@method.Id">
                            <MudIcon Icon="@GetPaymentIcon(method.Type)" Class="mr-2" />
                            @method.Name
                        </MudSelectItem>
                    }
                </MudSelect>
            </MudItem>

            <MudItem xs="12">
                <MudNumericField @bind-Value="_amountPaid"
                                 Label="Amount Paid"
                                 Variant="Variant.Outlined"
                                 Min="0"
                                 Format="F2"
                                 Adornment="Adornment.Start"
                                 AdornmentText="$"
                                 Required="true" />
            </MudItem>

            @if (_amountPaid > OrderTotal)
            {
                <MudItem xs="12">
                    <MudAlert Severity="Severity.Success">
                        Change: $@((_amountPaid - OrderTotal).ToString("F2"))
                    </MudAlert>
                </MudItem>
            }
            else if (_amountPaid < OrderTotal && _amountPaid > 0)
            {
                <MudAlert Severity="Severity.Warning">
                    Remaining: $@((OrderTotal - _amountPaid).ToString("F2"))
                </MudAlert>
            }

            @if (_selectedPaymentMethodId == 2) // Credit Card
            {
                <MudItem xs="12">
                    <MudTextField @bind-Value="_cardNumber"
                                  Label="Card Number"
                                  Variant="Variant.Outlined"
                                  Mask="@(new PatternMask("0000 0000 0000 0000"))"
                                  Required="true" />
                </MudItem>
                <MudItem xs="6">
                    <MudTextField @bind-Value="_expiryDate"
                                  Label="Expiry (MM/YY)"
                                  Variant="Variant.Outlined"
                                  Mask="@(new PatternMask("00/00"))"
                                  Required="true" />
                </MudItem>
                <MudItem xs="6">
                    <MudTextField @bind-Value="_cvv"
                                  Label="CVV"
                                  Variant="Variant.Outlined"
                                  Mask="@(new PatternMask("000"))"
                                  InputType="InputType.Password"
                                  Required="true" />
                </MudItem>
            }

            <MudItem xs="12">
                <MudTextField @bind-Value="_notes"
                              Label="Payment Notes (Optional)"
                              Variant="Variant.Outlined"
                              Lines="3" />
            </MudItem>
        </MudGrid>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">Cancel</MudButton>
        <MudButton Color="Color.Primary" 
                   Variant="Variant.Filled"
                   OnClick="ProcessPayment"
                   Disabled="@(!IsValid() || _isProcessing)">
            @if (_isProcessing)
            {
                <MudProgressCircular Size="Size.Small" Indeterminate="true" />
                <span class="ml-2">Processing...</span>
            }
            else
            {
                <span>Process Payment</span>
            }
        </MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; }

    [Parameter]
    public decimal OrderTotal { get; set; }

    [Parameter]
    public int OrderId { get; set; }

    private List<PaymentMethodDto> _paymentMethods = new();
    private int _selectedPaymentMethodId;
    private decimal _amountPaid;
    private string _cardNumber;
    private string _expiryDate;
    private string _cvv;
    private string _notes;
    private bool _isProcessing;

    protected override async Task OnInitializedAsync()
    {
        _paymentMethods = await PaymentService.GetPaymentMethodsAsync();
        _selectedPaymentMethodId = _paymentMethods.FirstOrDefault()?.Id ?? 0;
        _amountPaid = OrderTotal;
    }

    private bool IsValid()
    {
        if (_selectedPaymentMethodId == 0 || _amountPaid <= 0)
            return false;

        if (_selectedPaymentMethodId == 2) // Credit Card
        {
            return !string.IsNullOrWhiteSpace(_cardNumber) &&
                   !string.IsNullOrWhiteSpace(_expiryDate) &&
                   !string.IsNullOrWhiteSpace(_cvv);
        }

        return true;
    }

    private async Task ProcessPayment()
    {
        _isProcessing = true;
        try
        {
            var request = new ProcessPaymentRequest
            {
                OrderId = OrderId,
                PaymentMethodId = _selectedPaymentMethodId,
                AmountPaid = _amountPaid,
                CardNumber = _cardNumber,
                ExpiryDate = _expiryDate,
                CVV = _cvv,
                Notes = _notes
            };

            var result = await PaymentService.ProcessPaymentAsync(request);
            
            if (result.Success)
            {
                Snackbar.Add("Payment processed successfully", Severity.Success);
                MudDialog.Close(DialogResult.Ok(result));
            }
            else
            {
                Snackbar.Add($"Payment failed: {result.ErrorMessage}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Payment error: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private string GetPaymentIcon(string paymentType)
    {
        return paymentType switch
        {
            "Cash" => Icons.Material.Filled.Money,
            "CreditCard" => Icons.Material.Filled.CreditCard,
            "DebitCard" => Icons.Material.Filled.CreditCard,
            "MobilePayment" => Icons.Material.Filled.PhoneAndroid,
            _ => Icons.Material.Filled.Payment
        };
    }
}
```

---

## 6. Offline Sync Handling

### OfflineSyncService.cs

```csharp
public interface IOfflineSyncService
{
    Task<bool> IsOnlineAsync();
    Task QueueOrderAsync(CreateOrderRequest order);
    Task<List<PendingSync>> GetPendingSyncsAsync();
    Task SyncPendingOrdersAsync();
    event Action<bool> OnlineStatusChanged;
}

public class OfflineSyncService : IOfflineSyncService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<OfflineSyncService> _logger;
    private readonly ISnackbar _snackbar;
    private bool _isOnline = true;
    private const string PENDING_ORDERS_KEY = "pending_orders";

    public event Action<bool> OnlineStatusChanged;

    public OfflineSyncService(
        HttpClient httpClient,
        ILocalStorageService localStorage,
        ILogger<OfflineSyncService> logger,
        ISnackbar snackbar)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _logger = logger;
        _snackbar = snackbar;

        // Monitor online/offline status
        Task.Run(MonitorConnectionAsync);
    }

    public async Task<bool> IsOnlineAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/health", 
                new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task QueueOrderAsync(CreateOrderRequest order)
    {
        var pendingOrders = await GetPendingOrdersFromStorageAsync();
        
        var pendingSync = new PendingSync
        {
            Id = Guid.NewGuid().ToString(),
            Order = order,
            QueuedAt = DateTime.Now,
            RetryCount = 0
        };

        pendingOrders.Add(pendingSync);
        await _localStorage.SetItemAsync(PENDING_ORDERS_KEY, pendingOrders);
        
        _logger.LogInformation("Order queued for sync: {OrderId}", pendingSync.Id);
        _snackbar.Add("Order saved offline. Will sync when online.", Severity.Info);
    }

    public async Task<List<PendingSync>> GetPendingSyncsAsync()
    {
        return await GetPendingOrdersFromStorageAsync();
    }

    public async Task SyncPendingOrdersAsync()
    {
        if (!await IsOnlineAsync())
        {
            _logger.LogWarning("Cannot sync: offline");
            return;
        }

        var pendingOrders = await GetPendingOrdersFromStorageAsync();
        if (!pendingOrders.Any())
            return;

        _logger.LogInformation("Syncing {Count} pending orders", pendingOrders.Count);

        var successfulSyncs = new List<string>();

        foreach (var pending in pendingOrders)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/v1/orders", pending.Order);
                
                if (response.IsSuccessStatusCode)
                {
                    successfulSyncs.Add(pending.Id);
                    _logger.LogInformation("Successfully synced order: {OrderId}", pending.Id);
                }
                else
                {
                    pending.RetryCount++;
                    pending.LastError = $"HTTP {response.StatusCode}";
                    _logger.LogWarning("Failed to sync order {OrderId}: {Error}", 
                        pending.Id, pending.LastError);
                }
            }
            catch (Exception ex)
            {
                pending.RetryCount++;
                pending.LastError = ex.Message;
                _logger.LogError(ex, "Error syncing order {OrderId}", pending.Id);
            }
        }

        // Remove successfully synced orders
        pendingOrders.RemoveAll(p => successfulSyncs.Contains(p.Id));
        await _localStorage.SetItemAsync(PENDING_ORDERS_KEY, pendingOrders);

        if (successfulSyncs.Any())
        {
            _snackbar.Add($"{successfulSyncs.Count} order(s) synced successfully", Severity.Success);
        }
    }

    private async Task<List<PendingSync>> GetPendingOrdersFromStorageAsync()
    {
        try
        {
            return await _localStorage.GetItemAsync<List<PendingSync>>(PENDING_ORDERS_KEY) 
                   ?? new List<PendingSync>();
        }
        catch
        {
            return new List<PendingSync>();
        }
    }

    private async Task MonitorConnectionAsync()
    {
        while (true)
        {
            var wasOnline = _isOnline;
            _isOnline = await IsOnlineAsync();

            if (wasOnline != _isOnline)
            {
                _logger.LogInformation("Connection status changed: {Status}", 
                    _isOnline ? "Online" : "Offline");
                OnlineStatusChanged?.Invoke(_isOnline);

                if (_isOnline)
                {
                    // Back online, sync pending orders
                    await SyncPendingOrdersAsync();
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    }
}

public class PendingSync
{
    public string Id { get; set; }
    public CreateOrderRequest Order { get; set; }
    public DateTime QueuedAt { get; set; }
    public int RetryCount { get; set; }
    public string LastError { get; set; }
}
```

### OfflineIndicator.razor (Component)

```razor
@inject IOfflineSyncService OfflineSyncService
@implements IDisposable

@if (!_isOnline)
{
    <MudAlert Severity="Severity.Warning" Class="offline-banner">
        <MudIcon Icon="@Icons.Material.Filled.CloudOff" Class="mr-2" />
        You are offline. Orders will be synced when connection is restored.
        @if (_pendingSyncCount > 0)
        {
            <MudChip Size="Size.Small" Color="Color.Warning" Class="ml-2">
                @_pendingSyncCount pending
            </MudChip>
        }
    </MudAlert>
}

@code {
    private bool _isOnline = true;
    private int _pendingSyncCount;

    protected override async Task OnInitializedAsync()
    {
        _isOnline = await OfflineSyncService.IsOnlineAsync();
        await UpdatePendingCountAsync();
        
        OfflineSyncService.OnlineStatusChanged += HandleOnlineStatusChanged;
    }

    private async void HandleOnlineStatusChanged(bool isOnline)
    {
        await InvokeAsync(async () =>
        {
            _isOnline = isOnline;
            await UpdatePendingCountAsync();
            StateHasChanged();
        });
    }

    private async Task UpdatePendingCountAsync()
    {
        var pending = await OfflineSyncService.GetPendingSyncsAsync();
        _pendingSyncCount = pending.Count;
    }

    public void Dispose()
    {
        OfflineSyncService.OnlineStatusChanged -= HandleOnlineStatusChanged;
    }
}
```

---

## 7. Print Integration

### PrintService.cs

```csharp
public interface IPrintService
{
    Task<bool> PrintReceiptAsync(int orderId);
    Task<bool> PrintKitchenTicketAsync(int orderId);
    Task<bool> PrintLabelAsync(int productId, int quantity);
    Task<List<PrinterDto>> GetAvailablePrintersAsync();
}

public class PrintService : IPrintService
{
    private readonly IServerCommandService _serverCommandService;
    private readonly ILogger<PrintService> _logger;

    public PrintService(
        IServerCommandService serverCommandService,
        ILogger<PrintService> logger)
    {
        _serverCommandService = serverCommandService;
        _logger = logger;
    }

    public async Task<bool> PrintReceiptAsync(int orderId)
    {
        try
        {
            var command = new ServerCommandRequest
            {
                CommandTypeId = 3, // PrintReceipt
                TargetEntityId = orderId,
                TargetEntityType = "Invoice",
                Priority = 1
            };

            var result = await _serverCommandService.SubmitCommandAsync(command);
            _logger.LogInformation("Print receipt command submitted: {CommandId}", result.CommandId);
            
            return await WaitForCommandCompletionAsync(result.CommandId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to print receipt for order {OrderId}", orderId);
            return false;
        }
    }

    public async Task<bool> PrintKitchenTicketAsync(int orderId)
    {
        try
        {
            var command = new ServerCommandRequest
            {
                CommandTypeId = 4, // PrintDrinkAndFoods
                TargetEntityId = orderId,
                TargetEntityType = "Invoice",
                Priority = 2 // Higher priority for kitchen
            };

            var result = await _serverCommandService.SubmitCommandAsync(command);
            return await WaitForCommandCompletionAsync(result.CommandId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to print kitchen ticket for order {OrderId}", orderId);
            return false;
        }
    }

    public async Task<bool> PrintLabelAsync(int productId, int quantity)
    {
        try
        {
            var command = new ServerCommandRequest
            {
                CommandTypeId = 8, // PrintLabelSingle
                TargetEntityId = productId,
                TargetEntityType = "Product",
                Parameters = new Dictionary<string, object>
                {
                    ["Quantity"] = quantity
                },
                Priority = 1
            };

            var result = await _serverCommandService.SubmitCommandAsync(command);
            return await WaitForCommandCompletionAsync(result.CommandId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to print label for product {ProductId}", productId);
            return false;
        }
    }

    public async Task<List<PrinterDto>> GetAvailablePrintersAsync()
    {
        // This would call an API endpoint that queries available printers
        // For now, return mock data
        return new List<PrinterDto>
        {
            new PrinterDto { Id = 1, Name = "Receipt Printer", Type = "Receipt", IsOnline = true },
            new PrinterDto { Id = 2, Name = "Kitchen Printer", Type = "Kitchen", IsOnline = true },
            new PrinterDto { Id = 3, Name = "Label Printer", Type = "Label", IsOnline = false }
        };
    }

    private async Task<bool> WaitForCommandCompletionAsync(int commandId, int timeoutSeconds = 30)
    {
        var startTime = DateTime.Now;
        
        while ((DateTime.Now - startTime).TotalSeconds < timeoutSeconds)
        {
            var status = await _serverCommandService.GetCommandStatusAsync(commandId);
            
            if (status.Status == "Completed")
                return true;
            
            if (status.Status == "Failed")
            {
                _logger.LogWarning("Print command {CommandId} failed: {Error}", 
                    commandId, status.ErrorMessage);
                return false;
            }

            await Task.Delay(1000); // Check every second
        }

        _logger.LogWarning("Print command {CommandId} timed out", commandId);
        return false;
    }
}
```

### PrintDialog.razor

```razor
@inject IPrintService PrintService
@inject ISnackbar Snackbar

<MudDialog>
    <DialogContent>
        <MudText Typo="Typo.h6" Class="mb-4">Print Options</MudText>
        
        <MudGrid>
            <MudItem xs="12">
                <MudText Typo="Typo.body1">Order #@OrderId</MudText>
            </MudItem>

            <MudItem xs="12">
                <MudCheckBox @bind-Checked="_printReceipt" 
                             Label="Customer Receipt" 
                             Color="Color.Primary" />
            </MudItem>

            <MudItem xs="12">
                <MudCheckBox @bind-Checked="_printKitchenTicket" 
                             Label="Kitchen Ticket" 
                             Color="Color.Primary" />
            </MudItem>

            @if (_printReceipt)
            {
                <MudItem xs="12">
                    <MudNumericField @bind-Value="_receiptCopies"
                                     Label="Receipt Copies"
                                     Min="1"
                                     Max="5"
                                     Variant="Variant.Outlined" />
                </MudItem>
            }

            <MudItem xs="12">
                <MudText Typo="Typo.subtitle2" Class="mt-2">Available Printers:</MudText>
                @foreach (var printer in _printers)
                {
                    <MudChip Size="Size.Small" 
                             Color="@(printer.IsOnline ? Color.Success : Color.Error)"
                             Icon="@Icons.Material.Filled.Print">
                        @printer.Name (@printer.Type)
                    </MudChip>
                }
            </MudItem>
        </MudGrid>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">Cancel</MudButton>
        <MudButton Color="Color.Primary" 
                   Variant="Variant.Filled"
                   OnClick="Print"
                   Disabled="@_isPrinting">
            @if (_isPrinting)
            {
                <MudProgressCircular Size="Size.Small" Indeterminate="true" />
                <span class="ml-2">Printing...</span>
            }
            else
            {
                <span>Print</span>
            }
        </MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; }

    [Parameter]
    public int OrderId { get; set; }

    private List<PrinterDto> _printers = new();
    private bool _printReceipt = true;
    private bool _printKitchenTicket = true;
    private int _receiptCopies = 1;
    private bool _isPrinting;

    protected override async Task OnInitializedAsync()
    {
        _printers = await PrintService.GetAvailablePrintersAsync();
    }

    private async Task Print()
    {
        _isPrinting = true;
        var allSuccess = true;

        try
        {
            if (_printReceipt)
            {
                for (int i = 0; i < _receiptCopies; i++)
                {
                    var success = await PrintService.PrintReceiptAsync(OrderId);
                    if (!success)
                    {
                        allSuccess = false;
                        Snackbar.Add($"Failed to print receipt copy {i + 1}", Severity.Error);
                    }
                }
            }

            if (_printKitchenTicket)
            {
                var success = await PrintService.PrintKitchenTicketAsync(OrderId);
                if (!success)
                {
                    allSuccess = false;
                    Snackbar.Add("Failed to print kitchen ticket", Severity.Error);
                }
            }

            if (allSuccess)
            {
                Snackbar.Add("Print jobs sent successfully", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
            }
        }
        finally
        {
            _isPrinting = false;
        }
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
```

---

## 8. Customer Search and Management

### CustomerSearch.razor

```razor
@inject ICustomerService CustomerService
@inject IDialogService DialogService
@inject ISnackbar Snackbar

<MudPaper Class="pa-4" Elevation="2">
    <MudText Typo="Typo.h6" Class="mb-3">Customer</MudText>

    @if (_selectedCustomer != null)
    {
        <MudCard Outlined="true" Class="mb-3">
            <MudCardContent>
                <MudGrid>
                    <MudItem xs="10">
                        <MudText Typo="Typo.body1">
                            <MudIcon Icon="@Icons.Material.Filled.Person" Size="Size.Small" />
                            <strong>@_selectedCustomer.Name</strong>
                        </MudText>
                        <MudText Typo="Typo.body2" Color="Color.Secondary">
                            <MudIcon Icon="@Icons.Material.Filled.Phone" Size="Size.Small" />
                            @_selectedCustomer.Telephone
                        </MudText>
                        @if (!string.IsNullOrEmpty(_selectedCustomer.Email))
                        {
                            <MudText Typo="Typo.body2" Color="Color.Secondary">
                                <MudIcon Icon="@Icons.Material.Filled.Email" Size="Size.Small" />
                                @_selectedCustomer.Email
                            </MudText>
                        }
                        @if (_selectedCustomer.Addresses?.Any() == true)
                        {
                            <MudText Typo="Typo.body2" Color="Color.Secondary">
                                <MudIcon Icon="@Icons.Material.Filled.LocationOn" Size="Size.Small" />
                                @_selectedCustomer.Addresses.First().FullAddress
                            </MudText>
                        }
                    </MudItem>
                    <MudItem xs="2">
                        <MudIconButton Icon="@Icons.Material.Filled.Close"
                                       Size="Size.Small"
                                       OnClick="ClearCustomer" />
                    </MudItem>
                </MudGrid>
            </MudCardContent>
        </MudCard>
    }
    else
    {
        <MudAutocomplete T="CustomerDto"
                         @bind-Value="_selectedCustomer"
                         SearchFunc="SearchCustomersAsync"
                         Label="Search Customer"
                         Variant="Variant.Outlined"
                         ToStringFunc="@(c => c?.Name)"
                         Adornment="Adornment.Start"
                         AdornmentIcon="@Icons.Material.Filled.Search"
                         Clearable="true"
                         DebounceInterval="300">
            <ItemTemplate Context="customer">
                <MudGrid>
                    <MudItem xs="12">
                        <MudText Typo="Typo.body1">@customer.Name</MudText>
                    </MudItem>
                    <MudItem xs="12">
                        <MudText Typo="Typo.caption" Color="Color.Secondary">
                            @customer.Telephone
                            @if (customer.OrderCount > 0)
                            {
                                <MudChip Size="Size.Small" Class="ml-2">
                                    @customer.OrderCount orders
                                </MudChip>
                            }
                        </MudText>
                    </MudItem>
                </MudGrid>
            </ItemTemplate>
        </MudAutocomplete>

        <MudButton Variant="Variant.Text"
                   Color="Color.Primary"
                   StartIcon="@Icons.Material.Filled.PersonAdd"
                   OnClick="CreateNewCustomer"
                   Class="mt-2">
            New Customer
        </MudButton>
    }
</MudPaper>

@code {
    [Parameter]
    public EventCallback<CustomerDto> OnCustomerSelected { get; set; }

    private CustomerDto _selectedCustomer;

    private async Task<IEnumerable<CustomerDto>> SearchCustomersAsync(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText) || searchText.Length < 2)
            return Enumerable.Empty<CustomerDto>();

        try
        {
            return await CustomerService.SearchCustomersAsync(searchText);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Search failed: {ex.Message}", Severity.Error);
            return Enumerable.Empty<CustomerDto>();
        }
    }

    private async Task ClearCustomer()
    {
        _selectedCustomer = null;
        await OnCustomerSelected.InvokeAsync(null);
    }

    private async Task CreateNewCustomer()
    {
        var dialog = await DialogService.ShowAsync<CreateCustomerDialog>("New Customer");
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is CustomerDto newCustomer)
        {
            _selectedCustomer = newCustomer;
            await OnCustomerSelected.InvokeAsync(newCustomer);
            Snackbar.Add("Customer created successfully", Severity.Success);
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_selectedCustomer != null)
        {
            await OnCustomerSelected.InvokeAsync(_selectedCustomer);
        }
    }
}
```

### CreateCustomerDialog.razor

```razor
@inject ICustomerService CustomerService
@inject ISnackbar Snackbar

<MudDialog>
    <DialogContent>
        <MudText Typo="Typo.h6" Class="mb-4">Create New Customer</MudText>
        
        <MudForm @ref="_form" @bind-IsValid="_isValid">
            <MudGrid>
                <MudItem xs="12">
                    <MudTextField @bind-Value="_customer.Name"
                                  Label="Full Name"
                                  Variant="Variant.Outlined"
                                  Required="true"
                                  RequiredError="Name is required" />
                </MudItem>

                <MudItem xs="12" md="6">
                    <MudTextField @bind-Value="_customer.Telephone"
                                  Label="Phone Number"
                                  Variant="Variant.Outlined"
                                  Required="true"
                                  RequiredError="Phone is required"
                                  Mask="@(new PatternMask("0000-000-000"))" />
                </MudItem>

                <MudItem xs="12" md="6">
                    <MudTextField @bind-Value="_customer.Email"
                                  Label="Email (Optional)"
                                  Variant="Variant.Outlined"
                                  InputType="InputType.Email" />
                </MudItem>

                <MudItem xs="12">
                    <MudDivider Class="my-2" />
                    <MudText Typo="Typo.subtitle1">Address (Optional)</MudText>
                </MudItem>

                <MudItem xs="12">
                    <MudTextField @bind-Value="_address.Street"
                                  Label="Street Address"
                                  Variant="Variant.Outlined" />
                </MudItem>

                <MudItem xs="12" md="6">
                    <MudTextField @bind-Value="_address.City"
                                  Label="City"
                                  Variant="Variant.Outlined" />
                </MudItem>

                <MudItem xs="12" md="6">
                    <MudTextField @bind-Value="_address.PostalCode"
                                  Label="Postal Code"
                                  Variant="Variant.Outlined" />
                </MudItem>

                <MudItem xs="12">
                    <MudTextField @bind-Value="_address.Notes"
                                  Label="Delivery Notes"
                                  Variant="Variant.Outlined"
                                  Lines="3" />
                </MudItem>
            </MudGrid>
        </MudForm>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">Cancel</MudButton>
        <MudButton Color="Color.Primary" 
                   Variant="Variant.Filled"
                   OnClick="Save"
                   Disabled="@(!_isValid || _isSaving)">
            @if (_isSaving)
            {
                <MudProgressCircular Size="Size.Small" Indeterminate="true" />
                <span class="ml-2">Saving...</span>
            }
            else
            {
                <span>Create Customer</span>
            }
        </MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; }

    private MudForm _form;
    private bool _isValid;
    private bool _isSaving;
    private CreateCustomerRequest _customer = new();
    private CustomerAddressDto _address = new();

    private async Task Save()
    {
        await _form.Validate();
        if (!_isValid)
            return;

        _isSaving = true;
        try
        {
            // Add address if any field is filled
            if (!string.IsNullOrWhiteSpace(_address.Street) || 
                !string.IsNullOrWhiteSpace(_address.City))
            {
                _customer.Addresses = new List<CustomerAddressDto> { _address };
            }

            var result = await CustomerService.CreateCustomerAsync(_customer);
            MudDialog.Close(DialogResult.Ok(result));
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to create customer: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isSaving = false;
        }
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
```

### ICustomerService.cs

```csharp
public interface ICustomerService
{
    Task<List<CustomerDto>> SearchCustomersAsync(string query);
    Task<CustomerDto> GetCustomerByIdAsync(int id);
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request);
    Task<CustomerDto> UpdateCustomerAsync(int id, UpdateCustomerRequest request);
    Task<List<OrderDto>> GetCustomerOrderHistoryAsync(int customerId);
}

public class CustomerService : ICustomerService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(HttpClient httpClient, ILogger<CustomerService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<CustomerDto>> SearchCustomersAsync(string query)
    {
        var response = await _httpClient.GetAsync(
            $"api/v1/customers/search?q={Uri.EscapeDataString(query)}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<CustomerDto>>();
    }

    public async Task<CustomerDto> GetCustomerByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"api/v1/customers/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CustomerDto>();
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/customers", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CustomerDto>();
    }

    public async Task<CustomerDto> UpdateCustomerAsync(int id, UpdateCustomerRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/v1/customers/{id}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CustomerDto>();
    }

    public async Task<List<OrderDto>> GetCustomerOrderHistoryAsync(int customerId)
    {
        var response = await _httpClient.GetAsync($"api/v1/customers/{customerId}/orders");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<OrderDto>>();
    }
}
```

---

## 9. Order Locking with SignalR

### OrderLockService.cs

```csharp
public interface IOrderLockService
{
    Task<LockResult> AcquireLockAsync(int orderId, string orderType);
    Task ReleaseLockAsync(int orderId, string orderType);
    Task<bool> IsLockedAsync(int orderId, string orderType);
    Task<LockInfo> GetLockInfoAsync(int orderId, string orderType);
    event Func<int, LockInfo, Task> OnLockAcquired;
    event Func<int, Task> OnLockReleased;
    event Func<int, Task> OnLockExpired;
}

public class OrderLockService : IOrderLockService, IAsyncDisposable
{
    private readonly HubConnection _hubConnection;
    private readonly ILogger<OrderLockService> _logger;
    private readonly Dictionary<int, Timer> _keepAliveTimers = new();
    private readonly string _sessionId;

    public event Func<int, LockInfo, Task> OnLockAcquired;
    public event Func<int, Task> OnLockReleased;
    public event Func<int, Task> OnLockExpired;

    public OrderLockService(
        NavigationManager navigationManager,
        ILogger<OrderLockService> logger)
    {
        _logger = logger;
        _sessionId = Guid.NewGuid().ToString();

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(navigationManager.ToAbsoluteUri("/hubs/orderlock"))
            .WithAutomaticReconnect()
            .Build();

        RegisterHandlers();
    }

    private void RegisterHandlers()
    {
        _hubConnection.On<int, LockInfo>("LockAcquired", async (orderId, lockInfo) =>
        {
            _logger.LogInformation("Lock acquired for order {OrderId}", orderId);
            StartKeepAlive(orderId, lockInfo.LockId);
            if (OnLockAcquired != null)
                await OnLockAcquired.Invoke(orderId, lockInfo);
        });

        _hubConnection.On<int>("LockReleased", async (orderId) =>
        {
            _logger.LogInformation("Lock released for order {OrderId}", orderId);
            StopKeepAlive(orderId);
            if (OnLockReleased != null)
                await OnLockReleased.Invoke(orderId);
        });

        _hubConnection.On<int>("LockExpired", async (orderId) =>
        {
            _logger.LogWarning("Lock expired for order {OrderId}", orderId);
            StopKeepAlive(orderId);
            if (OnLockExpired != null)
                await OnLockExpired.Invoke(orderId);
        });

        _hubConnection.On<int, UserInfo>("LockStolen", async (orderId, newOwner) =>
        {
            _logger.LogWarning("Lock stolen for order {OrderId} by {User}", 
                orderId, newOwner.UserName);
            StopKeepAlive(orderId);
            if (OnLockExpired != null)
                await OnLockExpired.Invoke(orderId);
        });
    }

    public async Task<LockResult> AcquireLockAsync(int orderId, string orderType)
    {
        await EnsureConnectedAsync();

        try
        {
            var result = await _hubConnection.InvokeAsync<LockResult>(
                "RequestLock", orderId, orderType, _sessionId);
            
            if (result.Success)
            {
                _logger.LogInformation("Successfully acquired lock for order {OrderId}", orderId);
            }
            else
            {
                _logger.LogWarning("Failed to acquire lock for order {OrderId}: {Reason}", 
                    orderId, result.ErrorMessage);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring lock for order {OrderId}", orderId);
            return new LockResult 
            { 
                Success = false, 
                ErrorMessage = ex.Message 
            };
        }
    }

    public async Task ReleaseLockAsync(int orderId, string orderType)
    {
        await EnsureConnectedAsync();

        try
        {
            await _hubConnection.InvokeAsync("ReleaseLock", orderId, orderType, _sessionId);
            StopKeepAlive(orderId);
            _logger.LogInformation("Released lock for order {OrderId}", orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing lock for order {OrderId}", orderId);
        }
    }

    public async Task<bool> IsLockedAsync(int orderId, string orderType)
    {
        await EnsureConnectedAsync();

        try
        {
            var lockInfo = await _hubConnection.InvokeAsync<LockInfo>(
                "GetLockInfo", orderId, orderType);
            return lockInfo != null && lockInfo.IsLocked;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking lock status for order {OrderId}", orderId);
            return false;
        }
    }

    public async Task<LockInfo> GetLockInfoAsync(int orderId, string orderType)
    {
        await EnsureConnectedAsync();

        try
        {
            return await _hubConnection.InvokeAsync<LockInfo>(
                "GetLockInfo", orderId, orderType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lock info for order {OrderId}", orderId);
            return null;
        }
    }

    private void StartKeepAlive(int orderId, int lockId)
    {
        StopKeepAlive(orderId); // Stop existing timer if any

        var timer = new Timer(async _ =>
        {
            try
            {
                await _hubConnection.InvokeAsync("KeepAlive", lockId);
                _logger.LogDebug("Keep-alive sent for lock {LockId}", lockId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending keep-alive for lock {LockId}", lockId);
            }
        }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));

        _keepAliveTimers[orderId] = timer;
    }

    private void StopKeepAlive(int orderId)
    {
        if (_keepAliveTimers.TryGetValue(orderId, out var timer))
        {
            timer.Dispose();
            _keepAliveTimers.Remove(orderId);
        }
    }

    private async Task EnsureConnectedAsync()
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
        {
            await _hubConnection.StartAsync();
            _logger.LogInformation("OrderLock Hub connected");
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var timer in _keepAliveTimers.Values)
        {
            timer.Dispose();
        }
        _keepAliveTimers.Clear();

        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}

public class LockResult
{
    public bool Success { get; set; }
    public int LockId { get; set; }
    public string ErrorMessage { get; set; }
    public LockInfo ExistingLock { get; set; }
}

public class LockInfo
{
    public int LockId { get; set; }
    public bool IsLocked { get; set; }
    public int LockedByUserId { get; set; }
    public string LockedByUserName { get; set; }
    public DateTime LockedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
```

### EditOrderWithLock.razor

```razor
@page "/orders/edit/{OrderId:int}"
@inject IOrderService OrderService
@inject IOrderLockService LockService
@inject ISnackbar Snackbar
@inject NavigationManager Navigation
@implements IAsyncDisposable

@if (_isLoading)
{
    <MudProgressLinear Indeterminate="true" />
}
else if (_lockConflict)
{
    <MudAlert Severity="Severity.Warning" Class="ma-4">
        <MudText Typo="Typo.h6">Order is Locked</MudText>
        <MudText Typo="Typo.body1">
            This order is currently being edited by <strong>@_existingLock.LockedByUserName</strong>
        </MudText>
        <MudText Typo="Typo.body2" Color="Color.Secondary">
            Locked at: @_existingLock.LockedAt.ToString("HH:mm:ss")
        </MudText>
        <MudButton Variant="Variant.Filled" 
                   Color="Color.Primary"
                   OnClick="RetryAcquireLock"
                   Class="mt-2">
            Retry
        </MudButton>
        <MudButton Variant="Variant.Text"
                   OnClick="GoBack"
                   Class="mt-2">
            Go Back
        </MudButton>
    </MudAlert>
}
else if (_order != null)
{
    <MudContainer MaxWidth="MaxWidth.Large" Class="mt-4">
        <MudPaper Class="pa-4">
            <MudText Typo="Typo.h5" Class="mb-4">
                Edit Order #@_order.OrderNumber
                <MudChip Size="Size.Small" Color="Color.Success" Icon="@Icons.Material.Filled.Lock">
                    Locked by you
                </MudChip>
            </MudText>

            <!-- Order editing form here -->
            <MudGrid>
                <MudItem xs="12">
                    <MudTextField @bind-Value="_order.Notes"
                                  Label="Order Notes"
                                  Variant="Variant.Outlined"
                                  Lines="3" />
                </MudItem>

                <!-- Add more order fields as needed -->
            </MudGrid>

            <MudDivider Class="my-4" />

            <MudButton Variant="Variant.Filled"
                       Color="Color.Primary"
                       OnClick="SaveChanges"
                       Disabled="@_isSaving">
                @if (_isSaving)
                {
                    <MudProgressCircular Size="Size.Small" Indeterminate="true" />
                    <span class="ml-2">Saving...</span>
                }
                else
                {
                    <span>Save Changes</span>
                }
            </MudButton>
            <MudButton Variant="Variant.Text"
                       OnClick="Cancel"
                       Class="ml-2">
                Cancel
            </MudButton>
        </MudPaper>
    </MudContainer>
}

@code {
    [Parameter]
    public int OrderId { get; set; }

    private OrderDto _order;
    private bool _isLoading = true;
    private bool _lockConflict;
    private LockInfo _existingLock;
    private bool _isSaving;
    private bool _hasLock;

    protected override async Task OnInitializedAsync()
    {
        await AcquireLockAndLoadOrder();
        
        // Subscribe to lock events
        LockService.OnLockExpired += HandleLockExpired;
    }

    private async Task AcquireLockAndLoadOrder()
    {
        _isLoading = true;
        try
        {
            // Try to acquire lock
            var lockResult = await LockService.AcquireLockAsync(OrderId, "Invoice");
            
            if (lockResult.Success)
            {
                _hasLock = true;
                _lockConflict = false;
                
                // Load order data
                _order = await OrderService.GetOrderByIdAsync(OrderId);
            }
            else
            {
                _lockConflict = true;
                _existingLock = lockResult.ExistingLock;
                Snackbar.Add($"Order is locked by {_existingLock.LockedByUserName}", Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task RetryAcquireLock()
    {
        await AcquireLockAndLoadOrder();
    }

    private async Task HandleLockExpired(int orderId)
    {
        if (orderId == OrderId)
        {
            await InvokeAsync(() =>
            {
                _hasLock = false;
                Snackbar.Add("Your lock has expired. Changes cannot be saved.", Severity.Error);
                StateHasChanged();
            });
        }
    }

    private async Task SaveChanges()
    {
        if (!_hasLock)
        {
            Snackbar.Add("Cannot save: lock expired", Severity.Error);
            return;
        }

        _isSaving = true;
        try
        {
            await OrderService.UpdateOrderAsync(OrderId, _order);
            Snackbar.Add("Order updated successfully", Severity.Success);
            await ReleaseLockAndNavigateBack();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to save: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task Cancel()
    {
        await ReleaseLockAndNavigateBack();
    }

    private async Task ReleaseLockAndNavigateBack()
    {
        if (_hasLock)
        {
            await LockService.ReleaseLockAsync(OrderId, "Invoice");
        }
        Navigation.NavigateTo("/orders");
    }

    private void GoBack()
    {
        Navigation.NavigateTo("/orders");
    }

    public async ValueTask DisposeAsync()
    {
        LockService.OnLockExpired -= HandleLockExpired;
        
        if (_hasLock)
        {
            await LockService.ReleaseLockAsync(OrderId, "Invoice");
        }
    }
}
```

---

## 10. Server Commands System

### IServerCommandService.cs

```csharp
public interface IServerCommandService
{
    Task<ServerCommandResult> SubmitCommandAsync(ServerCommandRequest request);
    Task<CommandStatus> GetCommandStatusAsync(int commandId);
    Task<List<ServerCommandDto>> GetPendingCommandsAsync();
    Task UpdateCommandStatusAsync(int commandId, string status, string result = null);
    event Func<ServerCommandDto, Task> OnCommandReceived;
    event Func<int, string, Task> OnCommandStatusChanged;
}

public class ServerCommandService : IServerCommandService, IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private readonly HubConnection _hubConnection;
    private readonly ILogger<ServerCommandService> _logger;

    public event Func<ServerCommandDto, Task> OnCommandReceived;
    public event Func<int, string, Task> OnCommandStatusChanged;

    public ServerCommandService(
        HttpClient httpClient,
        NavigationManager navigationManager,
        ILogger<ServerCommandService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(navigationManager.ToAbsoluteUri("/hubs/servercommand"))
            .WithAutomaticReconnect()
            .Build();

        RegisterHandlers();
    }

    private void RegisterHandlers()
    {
        _hubConnection.On<ServerCommandDto>("CommandReceived", async (command) =>
        {
            _logger.LogInformation("Command received: {CommandId} - {Type}", 
                command.Id, command.CommandTypeName);
            if (OnCommandReceived != null)
                await OnCommandReceived.Invoke(command);
        });

        _hubConnection.On<int, string>("CommandStatusChanged", async (commandId, status) =>
        {
            _logger.LogInformation("Command {CommandId} status changed to {Status}", 
                commandId, status);
            if (OnCommandStatusChanged != null)
                await OnCommandStatusChanged.Invoke(commandId, status);
        });
    }

    public async Task<ServerCommandResult> SubmitCommandAsync(ServerCommandRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/server-commands", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ServerCommandResult>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit server command");
            throw;
        }
    }

    public async Task<CommandStatus> GetCommandStatusAsync(int commandId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/server-commands/{commandId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CommandStatus>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get command status for {CommandId}", commandId);
            throw;
        }
    }

    public async Task<List<ServerCommandDto>> GetPendingCommandsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/server-commands?status=pending");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ServerCommandDto>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get pending commands");
            throw;
        }
    }

    public async Task UpdateCommandStatusAsync(int commandId, string status, string result = null)
    {
        try
        {
            var request = new UpdateCommandStatusRequest
            {
                Status = status,
                Result = result,
                CompletedAt = DateTime.Now
            };

            var response = await _httpClient.PutAsJsonAsync(
                $"api/v1/server-commands/{commandId}/status", request);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update command status for {CommandId}", commandId);
            throw;
        }
    }

    public async Task StartAsync()
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
        {
            await _hubConnection.StartAsync();
            _logger.LogInformation("ServerCommand Hub connected");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}

public class ServerCommandRequest
{
    public int CommandTypeId { get; set; }
    public int? TargetEntityId { get; set; }
    public string TargetEntityType { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
    public int Priority { get; set; } = 1;
}

public class ServerCommandResult
{
    public int CommandId { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CommandStatus
{
    public int CommandId { get; set; }
    public string Status { get; set; } // Pending, Processing, Completed, Failed
    public string Result { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
```

### ServerCommandMonitor.razor (Master POS Station)

```razor
@page "/server-commands"
@inject IServerCommandService CommandService
@inject ISnackbar Snackbar
@implements IAsyncDisposable

<MudContainer MaxWidth="MaxWidth.ExtraLarge" Class="mt-4">
    <MudText Typo="Typo.h4" Class="mb-4">
        Server Command Monitor
        <MudChip Size="Size.Small" Color="Color.Info">
            @_pendingCommands.Count Pending
        </MudChip>
    </MudText>

    @if (_isLoading)
    {
        <MudProgressLinear Indeterminate="true" Color="Color.Primary" />
    }
    else if (!_pendingCommands.Any())
    {
        <MudAlert Severity="Severity.Info">No pending commands</MudAlert>
    }
    else
    {
        <MudTable Items="_pendingCommands" Hover="true" Striped="true">
            <HeaderContent>
                <MudTh>ID</MudTh>
                <MudTh>Type</MudTh>
                <MudTh>Target</MudTh>
                <MudTh>Priority</MudTh>
                <MudTh>Created</MudTh>
                <MudTh>Status</MudTh>
                <MudTh>Actions</MudTh>
            </HeaderContent>
            <RowTemplate>
                <MudTd DataLabel="ID">@context.Id</MudTd>
                <MudTd DataLabel="Type">
                    <MudIcon Icon="@GetCommandIcon(context.CommandTypeId)" Size="Size.Small" Class="mr-2" />
                    @context.CommandTypeName
                </MudTd>
                <MudTd DataLabel="Target">
                    @if (context.TargetEntityId.HasValue)
                    {
                        <span>@context.TargetEntityType #@context.TargetEntityId</span>
                    }
                </MudTd>
                <MudTd DataLabel="Priority">
                    <MudChip Size="Size.Small" Color="@GetPriorityColor(context.Priority)">
                        @context.Priority
                    </MudChip>
                </MudTd>
                <MudTd DataLabel="Created">
                    @context.CreatedAt.ToString("HH:mm:ss")
                </MudTd>
                <MudTd DataLabel="Status">
                    <MudChip Size="Size.Small" Color="@GetStatusColor(context.Status)">
                        @context.Status
                    </MudChip>
                </MudTd>
                <MudTd DataLabel="Actions">
                    @if (context.Status == "Pending")
                    {
                        <MudButton Size="Size.Small"
                                   Variant="Variant.Filled"
                                   Color="Color.Primary"
                                   OnClick="() => ProcessCommand(context)">
                            Process
                        </MudButton>
                    }
                    else if (context.Status == "Processing")
                    {
                        <MudProgressCircular Size="Size.Small" Indeterminate="true" />
                    }
                </MudTd>
            </RowTemplate>
        </MudTable>
    }
</MudContainer>

@code {
    private List<ServerCommandDto> _pendingCommands = new();
    private bool _isLoading;

    protected override async Task OnInitializedAsync()
    {
        await LoadPendingCommandsAsync();
        
        // Start SignalR connection
        await CommandService.StartAsync();
        
        // Subscribe to events
        CommandService.OnCommandReceived += HandleNewCommand;
        CommandService.OnCommandStatusChanged += HandleStatusChanged;
    }

    private async Task LoadPendingCommandsAsync()
    {
        _isLoading = true;
        try
        {
            _pendingCommands = await CommandService.GetPendingCommandsAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to load commands: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task HandleNewCommand(ServerCommandDto command)
    {
        await InvokeAsync(() =>
        {
            _pendingCommands.Add(command);
            _pendingCommands = _pendingCommands.OrderByDescending(c => c.Priority)
                                               .ThenBy(c => c.CreatedAt)
                                               .ToList();
            StateHasChanged();
            Snackbar.Add($"New command: {command.CommandTypeName}", Severity.Info);
        });
    }

    private async Task HandleStatusChanged(int commandId, string status)
    {
        await InvokeAsync(() =>
        {
            var command = _pendingCommands.FirstOrDefault(c => c.Id == commandId);
            if (command != null)
            {
                command.Status = status;
                
                if (status == "Completed" || status == "Failed")
                {
                    _pendingCommands.Remove(command);
                }
                
                StateHasChanged();
            }
        });
    }

    private async Task ProcessCommand(ServerCommandDto command)
    {
        try
        {
            // Update status to Processing
            await CommandService.UpdateCommandStatusAsync(command.Id, "Processing");
            command.Status = "Processing";
            StateHasChanged();

            // Execute the command based on type
            var result = await ExecuteCommandAsync(command);

            // Update status to Completed
            await CommandService.UpdateCommandStatusAsync(command.Id, "Completed", result);
            
            Snackbar.Add($"Command {command.Id} completed", Severity.Success);
        }
        catch (Exception ex)
        {
            await CommandService.UpdateCommandStatusAsync(command.Id, "Failed", ex.Message);
            Snackbar.Add($"Command {command.Id} failed: {ex.Message}", Severity.Error);
        }
    }

    private async Task<string> ExecuteCommandAsync(ServerCommandDto command)
    {
        // This is where you'd implement the actual command execution logic
        // For example, calling print services, updating database, etc.
        
        await Task.Delay(2000); // Simulate processing
        
        return command.CommandTypeId switch
        {
            1 => "Invoice printed successfully",
            2 => "Invoice deleted successfully",
            3 => "Receipt printed successfully",
            4 => "Kitchen ticket printed successfully",
            7 => "Labels printed successfully",
            9 => "Voucher printed successfully",
            _ => "Command executed successfully"
        };
    }

    private string GetCommandIcon(int commandTypeId)
    {
        return commandTypeId switch
        {
            1 or 2 => Icons.Material.Filled.Receipt,
            3 => Icons.Material.Filled.Print,
            4 or 5 or 6 => Icons.Material.Filled.Restaurant,
            7 or 8 => Icons.Material.Filled.Label,
            9 => Icons.Material.Filled.CardGiftcard,
            10 => Icons.Material.Filled.Announcement,
            _ => Icons.Material.Filled.Settings
        };
    }

    private Color GetPriorityColor(int priority)
    {
        return priority switch
        {
            >= 3 => Color.Error,
            2 => Color.Warning,
            _ => Color.Default
        };
    }

    private Color GetStatusColor(string status)
    {
        return status switch
        {
            "Pending" => Color.Default,
            "Processing" => Color.Info,
            "Completed" => Color.Success,
            "Failed" => Color.Error,
            _ => Color.Default
        };
    }

    public async ValueTask DisposeAsync()
    {
        CommandService.OnCommandReceived -= HandleNewCommand;
        CommandService.OnCommandStatusChanged -= HandleStatusChanged;
        await CommandService.DisposeAsync();
    }
}
```

### ServerCommandTypes.cs (Shared DTOs)

```csharp
public class ServerCommandDto
{
    public int Id { get; set; }
    public int CommandTypeId { get; set; }
    public string CommandTypeName { get; set; }
    public int? TargetEntityId { get; set; }
    public string TargetEntityType { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
    public int Priority { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Result { get; set; }
}

public enum ServerCommandType
{
    PrintInvoice = 1,
    DeleteInvoice = 2,
    PrintReceipt = 3,
    PrintDrinkAndFoods = 4,
    PrintDrinks = 5,
    PrintFood = 6,
    PrintAllLabels = 7,
    PrintLabelSingle = 8,
    PrintVoucher = 9,
    PrintAnnouncement = 10
}
```

---

## Summary

This document provides 10 complete, production-ready Blazor WebAssembly examples for the MyChair POS system:

1. **Product Catalog with Search** - Full product browsing with debounced search and category filtering
2. **Shopping Cart Management** - Fluxor state management with proper item grouping logic
3. **Pending Orders Management** - CRUD operations for pending orders
4. **Kitchen Display Real-Time Updates** - SignalR-powered kanban board for kitchen staff
5. **Payment Processing** - Multi-method payment dialog with validation
6. **Offline Sync Handling** - Queue orders offline and sync when connection restored
7. **Print Integration** - Server command-based printing system
8. **Customer Search and Management** - Autocomplete search with customer creation
9. **Order Locking with SignalR** - Prevent concurrent editing conflicts
10. **Server Commands System** - Command queue for device-to-master communication

### Key Patterns Demonstrated:

- **MudBlazor Components**: Material Design UI library
- **Fluxor State Management**: Redux pattern for Blazor
- **SignalR Integration**: Real-time bidirectional communication
- **Service Layer**: Clean separation with HttpClient services
- **Offline-First**: Local storage and sync queue
- **Error Handling**: Comprehensive try-catch with user feedback
- **Loading States**: Progress indicators and disabled states
- **Event-Driven Architecture**: Component communication via events
- **Dependency Injection**: Constructor injection throughout
- **Async/Await**: Proper async patterns everywhere

All examples follow the project structure defined in `blazor-project-structure.md` and use patterns familiar to ASP.NET Core MVC developers.
