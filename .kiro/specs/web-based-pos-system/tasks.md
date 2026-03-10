# Implementation Plan: Web-Based POS System

## Overview

This implementation plan breaks down the web-based POS system into actionable tasks organized by project layer. The system uses Blazor WebAssembly for the frontend and ASP.NET Core 8 for the backend, sharing the existing SQL Server database with the legacy WPF POS.

### Architecture Summary
- **Frontend**: Blazor WebAssembly with MudBlazor UI components
- **State Management**: Fluxor (Flux/Redux pattern)
- **Backend**: ASP.NET Core 8 Web API with SignalR
- **Database**: SQL Server (existing `dbo` schema + new `web` schema)
- **Caching**: Redis
- **Real-time**: SignalR for live updates
- **Testing**: xUnit with property-based testing

### Implementation Strategy
1. Start with infrastructure and shared components
2. Build core API endpoints and repositories
3. Implement frontend state management and UI components
4. Add real-time features with SignalR
5. Implement offline support and PWA features
6. Add hardware integration and advanced features

---

## Tasks

- [ ] 1. Project Setup and Infrastructure
  - Create solution structure with 5 projects (Shared, API, Client, Infrastructure, Tests)
  - Configure project references and dependencies
  - Set up development environment configuration
  - _Requirements: TC-1, TC-2, TC-3_

- [ ] 2. Database Schema Setup
  - [x] 2.1 Create web schema and tables
    - Execute database-scripts.sql to create `web` schema
    - Create web.OrderLocks table with indexes
    - Create web.ApiAuditLog table with indexes
    - Create web.UserSessions table with indexes
    - Create web.FeatureFlags table with indexes
    - Create web.SyncQueue table with indexes
    - _Requirements: TC-1, MR-1_

  - [x] 2.2 Create database maintenance stored procedures
    - Implement web.CleanupExpiredLocks procedure
    - Implement web.CleanupOldAuditLogs procedure
    - Implement web.CleanupExpiredSessions procedure
    - _Requirements: TC-1, NFR-4_

  - [x] 2.3 Configure database permissions
    - Create WebPosAppUser database user
    - Grant appropriate permissions on dbo schema
    - Grant full permissions on web schema
    - _Requirements: NFR-4, TC-1_


- [ ] 3. Shared Project - DTOs and Models
  - [x] 3.1 Create core domain models
    - Implement OrderDto, OrderItemDto, PendingOrderDto
    - Implement CustomerDto, CustomerAddressDto
    - Implement ProductDto, CategoryDto
    - Implement PaymentDto, DiscountDto
    - Add data annotations for validation
    - _Requirements: US-1.1, US-1.3, US-2.1, US-4.1_

  - [x] 3.2 Create API request/response models
    - Implement CreateOrderRequest, UpdateOrderRequest
    - Implement CreateCustomerRequest, SearchCustomerRequest
    - Implement ApplyDiscountRequest, ProcessPaymentRequest
    - Implement ApiResponse<T> wrapper with success/error handling
    - _Requirements: FR-5, NFR-3_

  - [x] 3.3 Create SignalR message models
    - Implement OrderStatusChangedMessage
    - Implement OrderLockedMessage, OrderUnlockedMessage
    - Implement KitchenOrderMessage
    - Implement ServerCommandMessage
    - _Requirements: US-3.1, US-6.1, US-6.2_

  - [x] 3.4 Create enums and constants
    - Define OrderStatus, PaymentMethod, ServiceType enums
    - Define OrderLockStatus, ServerCommandType enums
    - Define API route constants
    - Define SignalR hub method names
    - _Requirements: FR-1, NFR-4_

- [ ] 4. Infrastructure Project - Data Access Layer
  - [x] 4.1 Configure Entity Framework Core
    - Create PosDbContext inheriting from existing POSEntities
    - Configure entity mappings for web schema tables
    - Configure connection string management
    - Implement database migration strategy
    - _Requirements: TC-1, MR-1_

  - [x] 4.2 Implement repository pattern
    - Create IRepository<T> generic interface
    - Implement GenericRepository<T> base class
    - Create IOrderRepository with order-specific methods
    - Create ICustomerRepository with customer-specific methods
    - Create IProductRepository with product-specific methods
    - _Requirements: NFR-4, FR-3_

  - [x] 4.3 Implement Unit of Work pattern
    - Create IUnitOfWork interface
    - Implement UnitOfWork class with transaction management
    - Add repository property accessors
    - Implement SaveChangesAsync with error handling
    - _Requirements: FR-3, NFR-4_

  - [x] 4.4 Create specialized repositories
    - Implement OrderLockRepository for web.OrderLocks
    - Implement AuditLogRepository for web.ApiAuditLog
    - Implement UserSessionRepository for web.UserSessions
    - Implement FeatureFlagRepository for web.FeatureFlags
    - Implement SyncQueueRepository for web.SyncQueue
    - _Requirements: US-6.2, FR-3, NFR-4_


- [x] 5. Infrastructure Project - Caching and Services
  - [x] 5.1 Implement Redis caching service
    - Create ICacheService interface
    - Implement RedisCacheService with Get/Set/Remove methods
    - Add cache key prefix management
    - Implement cache expiration policies
    - _Requirements: FR-2, NFR-2_

  - [x] 5.2 Implement feature flag service
    - Create IFeatureFlagService interface
    - Implement FeatureFlagService with database-backed flags
    - Add in-memory caching for feature flags
    - Implement user-specific and role-specific flag evaluation
    - _Requirements: US-9.2, NFR-4_

  - [x] 5.3 Implement audit logging service
    - Create IAuditLogService interface
    - Implement AuditLogService for web.ApiAuditLog
    - Add automatic change tracking for entities
    - Implement JSON serialization for change history
    - _Requirements: FR-3, NFR-4_

- [x] 6. API Project - Core Configuration
  - [x] 6.1 Configure ASP.NET Core services
    - Set up dependency injection container
    - Configure Entity Framework Core with connection string
    - Configure AutoMapper profiles
    - Configure FluentValidation
    - Configure Serilog structured logging
    - _Requirements: NFR-4, FR-4_

  - [x] 6.2 Configure authentication and authorization
    - Implement JWT token authentication
    - Configure role-based authorization policies
    - Implement session management with web.UserSessions
    - Add refresh token support
    - _Requirements: FR-4, NFR-1_

  - [x] 6.3 Configure middleware pipeline
    - Add exception handling middleware
    - Add request logging middleware
    - Add audit logging middleware
    - Add CORS policy for local network
    - Add response compression
    - _Requirements: NFR-3, NFR-4_

  - [x] 6.4 Configure SignalR hubs
    - Register SignalR services
    - Configure connection timeout and keep-alive
    - Set up authentication for SignalR connections
    - Configure message size limits
    - _Requirements: US-3.1, US-6.1, FR-2_


- [ ] 7. API Project - Business Services
  - [x] 7.1 Implement order service
    - Create IOrderService interface with CRUD operations
    - Implement CreateOrderAsync with validation
    - Implement UpdateOrderAsync with order locking
    - Implement GetPendingOrdersAsync with filtering
    - Implement SplitOrderAsync for split payments
    - Add business rule validation (stock, pricing)
    - _Requirements: US-1.1, US-1.3, US-1.4, US-1.5_

  - [x] 7.2 Implement order locking service
    - Create IOrderLockService interface
    - Implement AcquireLockAsync with timeout
    - Implement ReleaseLockAsync with validation
    - Implement GetLockStatusAsync for UI display
    - Add automatic lock expiration cleanup
    - _Requirements: US-1.4, US-6.2_

  - [x] 7.3 Implement payment service
    - Create IPaymentService interface
    - Implement ProcessPaymentAsync with transaction management
    - Implement ApplyDiscountAsync with validation
    - Implement SplitPaymentAsync for multiple payment methods
    - Add payment method validation
    - _Requirements: US-2.1, US-2.2, FR-3_

  - [x] 7.4 Implement customer service
    - Create ICustomerService interface
    - Implement SearchCustomersAsync with fuzzy matching
    - Implement CreateCustomerAsync with duplicate detection
    - Implement GetCustomerHistoryAsync
    - Add loyalty points calculation
    - _Requirements: US-4.1, US-4.2_

  - [x] 7.5 Implement product service
    - Create IProductService interface
    - Implement GetProductCatalogAsync with caching
    - Implement SearchProductsAsync with filters
    - Implement GetProductByCategoryAsync
    - Add stock availability checking
    - _Requirements: US-1.1, FR-2_

  - [x] 7.6 Implement kitchen service
    - Create IKitchenService interface
    - Implement GetActiveOrdersAsync for kitchen display
    - Implement UpdateOrderStatusAsync (Preparing, Ready, Delivered)
    - Implement GetOrdersByStatusAsync with filtering
    - Add order priority calculation
    - _Requirements: US-3.1, US-3.2_


- [ ] 8. API Project - Controllers
  - [x] 8.1 Implement OrdersController
    - Create POST /api/orders endpoint for order creation
    - Create GET /api/orders/{id} endpoint for order retrieval
    - Create PUT /api/orders/{id} endpoint for order updates
    - Create GET /api/orders/pending endpoint for pending orders
    - Create POST /api/orders/{id}/split endpoint for split orders
    - Add request validation and error handling
    - _Requirements: US-1.1, US-1.3, US-1.4, US-1.5_

  - [x] 8.2 Implement PaymentsController
    - Create POST /api/payments endpoint for payment processing
    - Create POST /api/payments/discount endpoint for discount application
    - Create POST /api/payments/split endpoint for split payments
    - Add payment validation and transaction management
    - _Requirements: US-2.1, US-2.2_

  - [x] 8.3 Implement CustomersController
    - Create GET /api/customers/search endpoint with query parameters
    - Create POST /api/customers endpoint for customer creation
    - Create GET /api/customers/{id} endpoint for customer details
    - Create GET /api/customers/{id}/history endpoint for order history
    - _Requirements: US-4.1, US-4.2_

  - [x] 8.4 Implement ProductsController
    - Create GET /api/products endpoint with pagination
    - Create GET /api/products/search endpoint with filters
    - Create GET /api/products/categories endpoint for category list
    - Create GET /api/products/category/{id} endpoint for products by category
    - _Requirements: US-1.1, FR-2_

  - [x] 8.5 Implement KitchenController
    - Create GET /api/kitchen/orders endpoint for active orders
    - Create PUT /api/kitchen/orders/{id}/status endpoint for status updates
    - Create GET /api/kitchen/orders/history endpoint for completed orders
    - _Requirements: US-3.1, US-3.2_

  - [x] 8.6 Implement ReportsController
    - Create GET /api/reports/daily-sales endpoint with date parameter
    - Create GET /api/reports/inventory endpoint for stock levels
    - Create GET /api/reports/export endpoint for PDF/Excel export
    - _Requirements: US-5.1, US-5.2_

  - [x] 8.7 Implement AuthController
    - Create POST /api/auth/login endpoint with JWT token generation
    - Create POST /api/auth/refresh endpoint for token refresh
    - Create POST /api/auth/logout endpoint with session cleanup
    - _Requirements: FR-4, NFR-1_


- [ ] 9. API Project - SignalR Hubs
  - [x] 9.1 Implement KitchenHub
    - Create KitchenHub with connection management
    - Implement SendOrderToKitchen method
    - Implement UpdateOrderStatus method
    - Add group management for kitchen displays
    - _Requirements: US-3.1, US-3.2_

  - [x] 9.2 Implement OrderLockHub
    - Create OrderLockHub for real-time lock notifications
    - Implement NotifyOrderLocked method
    - Implement NotifyOrderUnlocked method
    - Add automatic lock expiration notifications
    - _Requirements: US-1.4, US-6.2_

  - [x] 9.3 Implement ServerCommandHub
    - Create ServerCommandHub for device-to-master communication
    - Implement SendPrintCommand method
    - Implement SendCashDrawerCommand method
    - Implement GetCommandStatus method
    - _Requirements: US-6.1, US-8.1, US-8.2_

- [ ] 10. Checkpoint - Backend Core Complete
  - Ensure all API endpoints return correct responses
  - Verify database transactions work correctly
  - Test SignalR hubs with manual clients
  - Ensure all tests pass, ask the user if questions arise

- [ ] 11. Client Project - Blazor Setup
  - [x] 11.1 Configure Blazor WebAssembly project
    - Set up Program.cs with dependency injection
    - Configure HttpClient with base address
    - Register MudBlazor services
    - Configure Fluxor state management
    - Add Blazored.LocalStorage for offline storage
    - _Requirements: FR-1, US-7.1_

  - [x] 11.2 Configure authentication
    - Implement AuthenticationStateProvider
    - Add JWT token storage in local storage
    - Implement automatic token refresh
    - Add authorization message handler for HttpClient
    - _Requirements: FR-4, NFR-1_

  - [x] 11.3 Configure SignalR client
    - Set up HubConnection with automatic reconnection
    - Implement connection state management
    - Add authentication for SignalR connections
    - Configure message handlers
    - _Requirements: US-3.1, US-6.1_

  - [x] 11.4 Set up routing and navigation
    - Configure Blazor router with route templates
    - Implement navigation menu component
    - Add role-based route guards
    - Configure 404 not found page
    - _Requirements: FR-1, NFR-3_


- [x] 12. Client Project - Fluxor State Management
  - [x] 12.1 Implement order state
    - Create OrderState with current order and pending orders
    - Implement AddItemToOrderAction and reducer
    - Implement RemoveItemFromOrderAction and reducer
    - Implement UpdateItemQuantityAction and reducer
    - Implement LoadPendingOrdersAction with effect
    - _Requirements: US-1.1, US-1.3, US-1.4_

  - [x] 12.2 Implement customer state
    - Create CustomerState with selected customer and search results
    - Implement SearchCustomersAction with effect
    - Implement SelectCustomerAction and reducer
    - Implement CreateCustomerAction with effect
    - _Requirements: US-4.1, US-4.2_

  - [x] 12.3 Implement product catalog state
    - Create ProductCatalogState with products and categories
    - Implement LoadProductsAction with effect and caching
    - Implement SearchProductsAction with effect
    - Implement FilterByCategoryAction and reducer
    - _Requirements: US-1.1, FR-2_

  - [x] 12.4 Implement kitchen state
    - Create KitchenState with active orders
    - Implement LoadKitchenOrdersAction with effect
    - Implement UpdateOrderStatusAction with effect
    - Add SignalR integration for real-time updates
    - _Requirements: US-3.1, US-3.2_

  - [x] 12.5 Implement UI state
    - Create UIState for loading indicators and notifications
    - Implement ShowLoadingAction and reducer
    - Implement ShowNotificationAction and reducer
    - Implement ShowErrorAction and reducer
    - _Requirements: NFR-3_

- [x] 13. Client Project - Shared Components
  - [x] 13.1 Create layout components
    - Implement MainLayout with navigation and header
    - Implement CashierLayout optimized for desktop
    - Implement TabletLayout optimized for touch
    - Implement KitchenLayout for kitchen display
    - _Requirements: FR-1, US-1.2, US-3.1_

  - [x] 13.2 Create product catalog components
    - Implement ProductGrid with category filtering
    - Implement ProductCard with image and price
    - Implement ProductSearch with autocomplete
    - Implement CategoryFilter with icons
    - _Requirements: US-1.1, FR-2_

  - [x] 13.3 Create shopping cart components
    - Implement ShoppingCart with item list
    - Implement CartItem with quantity controls
    - Implement CartSummary with totals and tax
    - Implement CartActions (clear, save, checkout)
    - _Requirements: US-1.1, US-1.3_

  - [x] 13.4 Create customer components
    - Implement CustomerSearch with recent customers
    - Implement CustomerCard with contact info
    - Implement CustomerForm for new customer creation
    - Implement CustomerHistory with order list
    - _Requirements: US-4.1, US-4.2_


- [-] 14. Client Project - Page Components
  - [x] 14.1 Implement Cashier page
    - Create CashierPage.razor with three-column layout
    - Integrate ProductGrid in left column
    - Integrate ShoppingCart in right column
    - Add quick actions in center column
    - Implement keyboard shortcuts for common actions
    - _Requirements: US-1.1, FR-1, NFR-3_

  - [x] 14.2 Implement Waiter page
    - Create WaiterPage.razor optimized for tablets
    - Add table number selection
    - Integrate touch-optimized product selection
    - Add order notes and special requests
    - Implement offline mode indicator
    - _Requirements: US-1.2, US-7.1, FR-1_

  - [x] 14.3 Implement Kitchen Display page
    - Create KitchenPage.razor with order cards
    - Implement color-coded order urgency
    - Add order status buttons (Preparing, Ready, Delivered)
    - Implement audio/visual alerts for new orders
    - Add auto-refresh with SignalR
    - _Requirements: US-3.1, US-3.2_

  - [x] 14.4 Implement Checkout page
    - Create CheckoutPage.razor with payment options
    - Add payment method selection (cash, card, voucher)
    - Implement discount application UI
    - Add split payment interface
    - Implement receipt preview
    - _Requirements: US-2.1, US-2.2_

  - [x] 14.5 Implement Pending Orders page
    - Create PendingOrdersPage.razor with order list
    - Add search and filter controls
    - Implement order locking indicator
    - Add load and edit actions
    - _Requirements: US-1.3, US-1.4, US-6.2_

  - [ ] 14.6 Implement Reports page
    - Create ReportsPage.razor with report selection
    - Implement daily sales report with charts
    - Add inventory report with low stock alerts
    - Implement export to PDF/Excel
    - _Requirements: US-5.1, US-5.2_

  - [x] 14.7 Implement Login page
    - Create LoginPage.razor with username/password
    - Add remember me checkbox
    - Implement error handling for failed login
    - Add password reset link
    - _Requirements: FR-4, NFR-3_


- [ ] 15. Client Project - Services
  - [x] 15.1 Implement API client services
    - Create OrderApiClient with typed HTTP methods
    - Create CustomerApiClient with typed HTTP methods
    - Create ProductApiClient with typed HTTP methods
    - Create PaymentApiClient with typed HTTP methods
    - Add automatic retry with Polly
    - _Requirements: FR-5, NFR-1_

  - [ ] 15.2 Implement offline storage service
    - Create IOfflineStorageService interface
    - Implement IndexedDB wrapper for offline data
    - Add sync queue management
    - Implement conflict resolution strategy
    - _Requirements: US-7.1, US-7.2_

  - [ ] 15.3 Implement notification service
    - Create INotificationService interface
    - Implement toast notifications with MudBlazor
    - Add browser notification support
    - Implement sound alerts for kitchen
    - _Requirements: US-3.1, NFR-3_

  - [ ] 15.4 Implement print service
    - Create IPrintService interface
    - Implement WebUSB printer support for Chrome/Edge
    - Add fallback to print server API
    - Implement receipt formatting
    - _Requirements: US-8.1_

- [ ] 16. Checkpoint - Frontend Core Complete
  - Verify all pages render correctly on desktop and tablet
  - Test state management with Fluxor DevTools
  - Verify API integration works end-to-end
  - Ensure all tests pass, ask the user if questions arise

- [x] 17. Real-Time Features with SignalR
  - [x] 17.1 Implement kitchen order notifications
    - Subscribe to KitchenHub in KitchenPage
    - Handle SendOrderToKitchen messages
    - Update kitchen state on order status changes
    - Add visual and audio alerts
    - _Requirements: US-3.1, US-3.2_

  - [x] 17.2 Implement order locking notifications
    - Subscribe to OrderLockHub in order editing pages
    - Display lock status in real-time
    - Show "locked by user" indicator
    - Handle lock expiration notifications
    - _Requirements: US-1.4, US-6.2_

  - [x] 17.3 Implement server command system
    - Subscribe to ServerCommandHub in device stations
    - Send print commands to master station
    - Display command status (queued, processing, completed)
    - Handle command failures with retry
    - _Requirements: US-6.1_


- [ ] 18. Offline Support and PWA
  - [ ] 18.1 Configure service worker
    - Create service-worker.js with cache strategies
    - Implement cache-first for static assets
    - Implement network-first for API calls
    - Add offline page fallback
    - _Requirements: US-7.1, US-7.2, NFR-1_

  - [ ] 18.2 Implement offline order creation
    - Store orders in IndexedDB when offline
    - Display offline mode indicator
    - Queue orders for sync when online
    - Implement automatic sync on reconnection
    - _Requirements: US-7.1_

  - [ ] 18.3 Implement offline product catalog
    - Cache product catalog in IndexedDB
    - Update cache when online
    - Implement cache size limits
    - Add cache invalidation strategy
    - _Requirements: US-7.2, FR-2_

  - [ ] 18.4 Configure PWA manifest
    - Create manifest.json with app metadata
    - Add app icons for all sizes
    - Configure display mode and theme colors
    - Set up install prompts
    - _Requirements: FR-1, US-1.2_

- [ ] 19. Hardware Integration
  - [ ] 19.1 Implement receipt printing
    - Create ESC/POS command generator
    - Implement WebUSB printer communication
    - Add print server fallback for non-Chrome browsers
    - Implement print queue with retry
    - _Requirements: US-8.1, TC-3_

  - [ ] 19.2 Implement cash drawer integration
    - Add cash drawer open command via printer kick-out
    - Implement USB cash drawer support
    - Add manual open option for managers
    - Log all drawer open events
    - _Requirements: US-8.2_

  - [ ]* 19.3 Implement barcode scanner support
    - Add keyboard wedge barcode scanner support
    - Implement camera-based barcode scanning
    - Add product lookup by barcode
    - _Requirements: FR-1_

- [ ] 20. Advanced Features
  - [ ] 20.1 Implement quantity column grouping
    - Group order items with identical modifiers and notes
    - Display grouped items in cart
    - Maintain separate items for different modifiers
    - Update kitchen display with grouped items
    - _Requirements: US-1.1, US-1.4_

  - [ ] 20.2 Implement split order functionality
    - Create split order UI with item distribution
    - Generate multiple invoices from single order
    - Link split invoices to original order
    - Update reporting to handle split orders
    - _Requirements: US-1.5_

  - [ ] 20.3 Implement discount management
    - Add percentage and fixed amount discounts
    - Implement item-level and order-level discounts
    - Add manager approval for large discounts
    - Track discount reasons for reporting
    - _Requirements: US-2.2_

  - [ ] 20.4 Implement loyalty points
    - Calculate loyalty points on order completion
    - Display customer loyalty balance
    - Implement points redemption
    - Add loyalty history tracking
    - _Requirements: US-4.1_


- [ ] 21. Testing and Quality Assurance
  - [ ] 21.1 Write unit tests for business services
    - Test OrderService with mock repositories
    - Test PaymentService with transaction scenarios
    - Test CustomerService with duplicate detection
    - Test OrderLockService with concurrency scenarios
    - _Requirements: NFR-4_

  - [ ]* 21.2 Write property-based tests
    - **Property 1: Order total calculation correctness**
    - **Validates: Requirements US-1.1, US-2.1**
    - Test that order total equals sum of item prices plus tax minus discounts
    - Generate random orders with various items, quantities, and discounts
    - Verify calculation invariants hold for all generated cases

  - [ ]* 21.3 Write property-based tests for order locking
    - **Property 2: Lock exclusivity**
    - **Validates: Requirements US-6.2**
    - Test that only one user can hold a lock on an order at a time
    - Generate concurrent lock acquisition attempts
    - Verify that all but one fail with appropriate error

  - [ ]* 21.4 Write property-based tests for offline sync
    - **Property 3: Sync queue ordering**
    - **Validates: Requirements US-7.1**
    - Test that offline operations sync in correct order
    - Generate random sequences of create/update/delete operations
    - Verify final state matches expected result after sync

  - [ ]* 21.5 Write integration tests
    - Test complete order creation flow end-to-end
    - Test payment processing with database transactions
    - Test SignalR message delivery
    - Test offline sync with conflict resolution
    - _Requirements: NFR-4, FR-3_

  - [ ]* 21.6 Write UI component tests
    - Test ProductGrid component rendering
    - Test ShoppingCart component interactions
    - Test CustomerSearch component with mock data
    - Test KitchenPage component with SignalR mocks
    - _Requirements: NFR-4_

- [ ] 22. Performance Optimization
  - [ ] 22.1 Implement caching strategy
    - Cache product catalog in Redis with 1-hour expiration
    - Cache customer search results with 5-minute expiration
    - Implement cache invalidation on data changes
    - Add cache warming for frequently accessed data
    - _Requirements: FR-2, NFR-2_

  - [ ] 22.2 Optimize database queries
    - Add indexes for frequently queried columns
    - Implement eager loading with Include() for related entities
    - Use compiled queries for hot paths
    - Add query result pagination
    - _Requirements: FR-2, NFR-2_

  - [ ] 22.3 Optimize Blazor bundle size
    - Enable Blazor WebAssembly AOT compilation
    - Implement lazy loading for large components
    - Optimize image sizes and formats
    - Remove unused dependencies
    - _Requirements: FR-2_


- [ ] 23. Security Hardening
  - [ ] 23.1 Implement input validation
    - Add FluentValidation rules for all DTOs
    - Implement server-side validation for all endpoints
    - Add client-side validation for better UX
    - Sanitize user input to prevent XSS
    - _Requirements: FR-4, NFR-3_

  - [ ] 23.2 Implement rate limiting
    - Add rate limiting middleware for API endpoints
    - Configure different limits for authenticated vs anonymous
    - Implement IP-based rate limiting
    - Add rate limit headers to responses
    - _Requirements: FR-4_

  - [ ] 23.3 Implement audit logging
    - Log all order creation and modification
    - Log all payment transactions
    - Log all authentication attempts
    - Log all configuration changes
    - _Requirements: FR-3, US-9.2_

  - [ ] 23.4 Configure HTTPS and CORS
    - Generate self-signed certificate for local network
    - Configure HTTPS redirection
    - Set up CORS policy for allowed origins
    - Implement secure cookie settings
    - _Requirements: FR-4, TC-2_

- [ ] 24. Administration Features
  - [ ] 24.1 Implement user management
    - Create user CRUD operations
    - Implement role assignment (Cashier, Waiter, Manager, Admin)
    - Add user activation/deactivation
    - Implement password reset functionality
    - _Requirements: US-9.1_

  - [ ] 24.2 Implement configuration management
    - Create feature flag management UI
    - Add printer configuration interface
    - Implement tax rate configuration
    - Add business hours configuration
    - _Requirements: US-9.2_

  - [ ] 24.3 Implement system monitoring
    - Create dashboard with active sessions
    - Display system health metrics
    - Show recent errors and warnings
    - Add performance metrics (response times, throughput)
    - _Requirements: NFR-4_

- [ ] 25. Deployment and DevOps
  - [ ] 25.1 Create deployment scripts
    - Write PowerShell script for IIS deployment
    - Create Docker Compose file for containerized deployment
    - Add database migration scripts
    - Create backup and restore scripts
    - _Requirements: TC-2, NFR-1_

  - [ ] 25.2 Configure logging and monitoring
    - Set up Serilog with file and database sinks
    - Configure log rotation and retention
    - Add structured logging for key operations
    - Implement health check endpoints
    - _Requirements: NFR-4_

  - [ ] 25.3 Create deployment documentation
    - Write installation guide for on-premises deployment
    - Document system requirements
    - Create troubleshooting guide
    - Document backup and recovery procedures
    - _Requirements: NFR-4, MR-3_


- [ ] 26. Migration and Compatibility
  - [ ] 26.1 Implement data migration utilities
    - Create script to verify database compatibility
    - Implement pending order migration from legacy system
    - Add customer data validation and cleanup
    - Create rollback procedures
    - _Requirements: MR-2, MR-4_

  - [ ] 26.2 Implement parallel operation support
    - Ensure web POS and WPF POS can coexist
    - Add conflict detection for concurrent edits
    - Implement last-write-wins with audit trail
    - Test concurrent operations from both systems
    - _Requirements: MR-1, TC-1_

  - [ ] 26.3 Create migration documentation
    - Write step-by-step migration guide
    - Document rollback procedures
    - Create training materials for staff
    - Document known issues and workarounds
    - _Requirements: MR-3_

- [ ] 27. User Training and Documentation
  - [ ] 27.1 Create user documentation
    - Write cashier user guide with screenshots
    - Create waiter tablet guide
    - Document kitchen display usage
    - Write manager reporting guide
    - _Requirements: MR-3, NFR-3_

  - [ ] 27.2 Create video tutorials
    - Record order creation walkthrough
    - Create payment processing tutorial
    - Record kitchen display usage
    - Create troubleshooting videos
    - _Requirements: MR-3_

  - [ ] 27.3 Implement in-app help
    - Add tooltips for all major features
    - Create contextual help panels
    - Implement guided tour for first-time users
    - Add FAQ section
    - _Requirements: NFR-3_

- [ ] 28. Final Testing and Validation
  - [ ] 28.1 Conduct load testing
    - Test with 10+ concurrent users
    - Simulate 1000+ orders per day
    - Test SignalR with 20+ connections
    - Measure response times under load
    - _Requirements: FR-2, NFR-2_

  - [ ] 28.2 Conduct browser compatibility testing
    - Test on Chrome (Windows, Android)
    - Test on Edge (Windows)
    - Test on Safari (iOS, macOS)
    - Test on Firefox (Windows)
    - Test on Samsung Internet (Android)
    - _Requirements: TC-4, FR-1_

  - [ ] 28.3 Conduct offline testing
    - Test order creation while offline
    - Test sync after reconnection
    - Test conflict resolution
    - Test cache behavior
    - _Requirements: US-7.1, US-7.2, NFR-1_

  - [ ] 28.4 Conduct hardware integration testing
    - Test receipt printing on all printer models
    - Test cash drawer integration
    - Test barcode scanners
    - Test on various tablet models
    - _Requirements: TC-3, US-8.1, US-8.2_

- [ ] 29. Final Checkpoint - Production Readiness
  - Verify all critical user stories are implemented
  - Ensure all tests pass (unit, integration, E2E)
  - Confirm performance meets requirements (< 2s page load, < 500ms API)
  - Validate security measures are in place
  - Ensure documentation is complete
  - Ensure all tests pass, ask the user if questions arise


- [ ] 30. Production Deployment
  - [ ] 30.1 Deploy to pilot location
    - Deploy API to on-premises server
    - Deploy Blazor client to web server
    - Configure SSL certificate
    - Set up database connection
    - Configure Redis cache
    - _Requirements: TC-2, MR-1_

  - [ ] 30.2 Configure monitoring and alerts
    - Set up application monitoring
    - Configure error alerting
    - Set up performance monitoring
    - Configure uptime monitoring
    - _Requirements: NFR-1, NFR-4_

  - [ ] 30.3 Conduct pilot testing
    - Train pilot location staff
    - Monitor system for 2 weeks
    - Collect user feedback
    - Address critical issues
    - _Requirements: MR-3_

  - [ ] 30.4 Plan full rollout
    - Create rollout schedule for remaining locations
    - Prepare training materials
    - Set up support hotline
    - Document lessons learned from pilot
    - _Requirements: MR-1, MR-3_

---

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP delivery
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation at key milestones
- Property-based tests validate universal correctness properties
- Unit and integration tests validate specific examples and edge cases
- Implementation follows clean architecture principles with clear separation of concerns
- All database operations use transactions to ensure data integrity
- SignalR provides real-time updates across all connected clients
- PWA features enable offline operation with automatic sync
- The system maintains backward compatibility with legacy WPF POS during transition

## Implementation Timeline Estimate

- **Phase 1: Infrastructure & Backend** (Tasks 1-10) - 4-6 weeks
- **Phase 2: Frontend Core** (Tasks 11-16) - 4-6 weeks
- **Phase 3: Real-Time & Offline** (Tasks 17-18) - 2-3 weeks
- **Phase 4: Hardware & Advanced Features** (Tasks 19-20) - 3-4 weeks
- **Phase 5: Testing & Optimization** (Tasks 21-23) - 3-4 weeks
- **Phase 6: Administration & Deployment** (Tasks 24-30) - 3-4 weeks

**Total Estimated Timeline**: 19-27 weeks (approximately 5-7 months)

## Dependencies

- SQL Server 2016+ with existing POS database
- .NET 8 SDK for development
- Redis server for caching
- Modern web browser (Chrome/Edge recommended)
- Network infrastructure (WiFi/Ethernet)
- SSL certificate for HTTPS (self-signed acceptable for local network)

## Success Criteria

- All critical user stories (US-1.x, US-2.x, US-3.x, US-4.x) implemented
- System handles 10+ concurrent users without performance degradation
- Page load time < 2 seconds, API response time < 500ms
- Offline mode works reliably with automatic sync
- Zero data loss during migration from legacy system
- Staff training completed within 2 weeks per location
- 99.9% uptime achieved during business hours

