# Legacy Code Reference Map

This document maps legacy WPF POS code to new Web POS implementation locations.

## Purpose

- Identify where legacy business logic lives
- Track porting progress
- Ensure feature parity
- Document business rules discovered in legacy code

---

## Business Logic Files

### 💰 Payment Processing

**Legacy Location**: `MyChairPos/POS/Helpers/DbHelper.cs`

**Key Methods**:
- `ProcessPayment()` (line ~400, 400+ lines)
  - Creates invoice
  - Records payment
  - Updates stock
  - Applies discounts
  - Calculates VAT
  - Logs history

**New Location**: `Pos.Web.API/Services/Implementations/PaymentService.cs`

**Status**: ⏳ Not started

**Notes**:
- Legacy uses multiple `SaveChanges()` without transaction
- Need to add transaction management
- Split into smaller methods
- Add proper error handling

---

### 🧮 Calculations

**Legacy Location**: `MyChairPos/POS/Helpers/CalculationsHelper.cs`

**Key Methods**:
- Discount calculations (percentage, fixed amount)
- VAT calculations
- Total calculations
- Rounding logic

**New Location**: `Pos.Web.API/Services/Implementations/CalculationService.cs`

**Status**: ⏳ Not started

**Notes**:
- Static methods need to be converted to instance methods
- Add unit tests for all calculation scenarios
- Document rounding rules

---

### 🛒 Checkout Flow

**Legacy Location**: `MyChairPos/POS/CheckoutHelpers/CheckoutHelper.cs`

**Key Logic**:
- Checkout validation
- Payment method handling
- Receipt generation
- Order finalization

**New Location**: `Pos.Web.API/Services/Implementations/CheckoutService.cs`

**Status**: ⏳ Not started

**Notes**:
- Separate validation from business logic
- Add async/await
- Improve error messages

---

### 👥 Customer Management

**Legacy Location**: `MyChairPos/POS/Helpers/DbHelper.cs`

**Key Methods**:
- `SaveNewCustomer()` - Creates customer record
- `CreateNewCustomerAddress()` - Creates address
- `CreateAddressToCustomerConnection()` - Links customer to address

**New Location**: `Pos.Web.API/Services/Implementations/CustomerService.cs`

**Status**: ⏳ Not started

**Notes**:
- Legacy uses multiple `SaveChanges()` without transaction
- Add transaction management
- Add validation for duplicate customers

---

### 📦 Stock Management

**Legacy Location**: `MyChairPos/POS/Helpers/StockManagementHelper.cs`

**Key Logic**:
- Stock validation before order
- Stock updates after payment
- Inventory tracking
- Low stock alerts

**New Location**: `Pos.Web.API/Services/Implementations/StockService.cs`

**Status**: ⏳ Not started

**Notes**:
- Add concurrency handling (optimistic locking)
- Prevent negative stock
- Add stock reservation for pending orders

---

### 🖨️ Printing

**Legacy Location**: `MyChairPos/POS/Helpers/PrintHelper.cs`

**Key Logic**:
- Receipt formatting
- Label printing
- Kitchen ticket printing
- Printer selection

**New Location**: `Pos.Web.Client/Services/Print/PrintService.cs`

**Status**: ⏳ Not started

**Notes**:
- Convert to browser-based printing (window.print())
- Use CSS for print formatting
- Add print preview
- Consider thermal printer support via browser extensions

---

## Database Entities

### 📊 Entity Framework Models

**Legacy Location**: `MyChairPos/PosDbForAll/`

**Key Files**:
- `Invoice.cs` - Invoice entity
- `InvoiceItem.cs` - Invoice line items
- `PendingInvoice.cs` - Pending/saved orders
- `PendingInvoiceItem.cs` - Pending order items
- `Customer.cs` - Customer entity
- `CustomerAddress.cs` - Customer addresses
- `CategoryItem.cs` - Products
- `Category.cs` - Product categories
- `User.cs` - System users
- `ServingType.cs` - Service types (dine-in, takeout, delivery)
- `PaymentMethod.cs` - Payment methods

**New Location**: `Pos.Web.Infrastructure/Entities/Dbo/`

**Status**: ⏳ Not started

**Notes**:
- Port entities to EF Core 8
- Keep same table/column names for compatibility
- Add navigation properties
- Use Fluent API for configuration

---

## UI Logic

### 💵 Cashier Interface

**Legacy Location**: `MyChairPos/POS/MainWindow.xaml.cs`

**Key Logic** (4500+ lines):
- Product catalog display
- Product search (by name, by alphabet)
- Category navigation
- Shopping cart management
- Customer selection
- Service type selection
- Discount application
- Checkout flow
- Pending order management

**New Location**: 
- `Pos.Web.Client/Pages/Cashier.razor` (UI)
- `Pos.Web.Client/Store/OrderState/` (State management)
- `Pos.Web.Client/Components/Products/` (Product components)
- `Pos.Web.Client/Components/Cart/` (Cart components)

**Status**: ⏳ Not started

**Notes**:
- Split God object into smaller components
- Use Fluxor for state management
- Separate UI from business logic
- Add responsive design for tablets

---

### 🍳 Kitchen Display

**Legacy Location**: `MyChairPos/OrdersMonitor/MainWindow.xaml.cs`

**Key Logic**:
- Order display (pending, in-progress, completed)
- Order status updates
- Order timers
- Order filtering
- Real-time updates

**New Location**: 
- `Pos.Web.Client/Pages/Kitchen.razor` (UI)
- `Pos.Web.Client/Store/KitchenState/` (State management)
- `Pos.Web.Client/Components/Kitchen/` (Kitchen components)
- `Pos.Web.Client/Services/SignalR/KitchenHubClient.cs` (Real-time)

**Status**: ⏳ Not started

**Notes**:
- Use SignalR for real-time updates
- Add order timers (elapsed time)
- Add sound notifications
- Add color coding for order urgency

---

## Configuration & Initialization

### ⚙️ Feature Flags & Configuration

**Legacy Location**: `MyChairPos/POS/SplashWindow.xaml.cs`

**Key Method**: `LoadStaticData()` (line ~329, 250+ lines)

**Logic**:
- Loads 50+ configuration properties from database
- Populates static collections (users, categories, products, etc.)
- Initializes feature flags
- Sets up external listeners

**New Location**: 
- `Pos.Web.Infrastructure/Services/Implementations/FeatureFlagService.cs`
- `Pos.Web.Infrastructure/Services/Implementations/ConfigurationService.cs`

**Status**: ⏳ Not started

**Notes**:
- Replace static collections with caching (Redis)
- Add cache expiration
- Load configuration on-demand (not all at startup)
- Use dependency injection

---

## Quick Reference Table

| Legacy File | Key Logic | New Location | Status |
|-------------|-----------|--------------|--------|
| `POS/Helpers/DbHelper.cs` | Payment, Customer CRUD | `API/Services/PaymentService.cs` | ⏳ Not started |
| `POS/Helpers/CalculationsHelper.cs` | Calculations | `API/Services/CalculationService.cs` | ⏳ Not started |
| `POS/CheckoutHelpers/CheckoutHelper.cs` | Checkout flow | `API/Services/CheckoutService.cs` | ⏳ Not started |
| `POS/Helpers/StockManagementHelper.cs` | Stock management | `API/Services/StockService.cs` | ⏳ Not started |
| `POS/Helpers/PrintHelper.cs` | Printing | `Client/Services/Print/PrintService.cs` | ⏳ Not started |
| `POS/MainWindow.xaml.cs` | Cashier UI logic | `Client/Pages/Cashier.razor` | ⏳ Not started |
| `OrdersMonitor/MainWindow.xaml.cs` | Kitchen display | `Client/Pages/Kitchen.razor` | ⏳ Not started |
| `PosDbForAll/*.cs` | EF entities | `Infrastructure/Entities/Dbo/` | ⏳ Not started |
| `POS/SplashWindow.xaml.cs` | Configuration loading | `Infrastructure/Services/ConfigurationService.cs` | ⏳ Not started |

---

## Business Rules Discovered

### Discount Rules
- Discount percentage cannot exceed 100%
- Discount amount cannot exceed total cost
- Only one discount type per order (percentage OR fixed amount)
- Source: `DbHelper.cs` line ~450

### Payment Rules
- Customer paid amount must be >= total cost
- Change is calculated as: customerPaid - totalCost
- Multiple payment methods not supported in legacy (single payment only)
- Source: `DbHelper.cs` line ~500

### Stock Rules
- Stock must be checked before order creation
- Stock is updated after payment (not before)
- Negative stock is prevented
- Source: `StockManagementHelper.cs` line ~100

### VAT Rules
- VAT is calculated based on service type
- Different VAT rates for dine-in vs takeout
- VAT is included in total (not added on top)
- Source: `CalculationsHelper.cs` line ~200

### Order Rules
- Pending orders can be saved without payment
- Pending orders can be recalled and completed later
- Order items can have modifiers (extras, flavors)
- Order items can have notes
- Source: `MainWindow.xaml.cs` line ~2000

---

## Porting Progress Tracker

### Phase 1: Infrastructure (Database & Entities)
- [ ] Port EF entities to EF Core 8
- [ ] Create entity configurations
- [ ] Set up DbContext
- [ ] Create migrations
- [ ] Test database connectivity

### Phase 2: Core Services (Business Logic)
- [ ] Port CalculationService
- [ ] Port CustomerService
- [ ] Port StockService
- [ ] Port PaymentService
- [ ] Port CheckoutService

### Phase 3: API Layer
- [ ] Create API controllers
- [ ] Add validation
- [ ] Add error handling
- [ ] Add authentication/authorization
- [ ] Test API endpoints

### Phase 4: Client (UI)
- [ ] Create Cashier page
- [ ] Create Kitchen page
- [ ] Create product components
- [ ] Create cart components
- [ ] Set up Fluxor state management
- [ ] Add SignalR integration

### Phase 5: Testing
- [ ] Unit tests for services
- [ ] Integration tests for API
- [ ] Property-based tests for calculations
- [ ] End-to-end tests for critical flows

---

## Notes & Observations

### Code Quality Issues in Legacy
- Static methods everywhere (not testable)
- God objects (MainWindow.xaml.cs is 4500+ lines)
- No transaction management
- Multiple SaveChanges() calls
- Deep nesting (5-7 levels)
- No separation of concerns
- Minimal error handling

### Improvements in New Implementation
- ✅ Dependency injection
- ✅ Async/await throughout
- ✅ Transaction management
- ✅ Separation of concerns (services, repositories)
- ✅ Proper error handling
- ✅ Unit tests
- ✅ Clean Architecture
- ✅ Modern patterns (CQRS, Fluxor)

---

## How to Use This Document

1. **Before implementing a feature**: Check this document to find relevant legacy code
2. **While porting**: Update status and add notes
3. **After porting**: Mark as complete and document any deviations
4. **When finding business rules**: Add them to "Business Rules Discovered" section

---

**Last Updated**: 2026-02-26  
**Maintained By**: Development Team
