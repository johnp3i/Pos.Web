# Technical Design Document: Web-Based POS System

## Executive Summary

This document provides the technical architecture and design for the new web-based MyChair POS system. The design follows a hybrid approach, building a modern web-based system while maintaining compatibility with the existing WPF POS during a transition period.

### Architecture Overview
- **Frontend**: Blazor WebAssembly (Pure MudBlazor) - Progressive Web App (PWA)
- **Backend**: ASP.NET Core 8 Web API with SignalR
- **Database**: Existing SQL Server (shared with legacy system)
- **Deployment**: On-premises (Kestrel/IIS) with optional cloud sync
- **Communication**: RESTful API + SignalR for real-time updates
- **UI Framework**: MudBlazor 6.x (Material Design components)
- **Theming**: Hybrid approach - Legacy layout for POS, Modern dashboard for Admin

### Key Design Principles
1. **API-First**: Backend exposes RESTful APIs for all operations
2. **Real-Time**: SignalR for live updates across stations
3. **Offline-First**: PWA with service workers for offline capability
4. **Backward Compatible**: Shares database with legacy WPF POS
5. **Scalable**: Microservices-ready architecture
6. **Testable**: Dependency injection and clean architecture

---

## System Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Client Layer (PWA)                      │
│  ┌──────────┬──────────┬──────────┬──────────┬──────────┐  │
│  │ Cashier  │ Waiter   │ Kitchen  │ Manager  │ Admin    │  │
│  │ Station  │ Tablet   │ Display  │ Dashboard│ Panel    │  │
│  └──────────┴──────────┴──────────┴──────────┴──────────┘  │
│         ↓ HTTPS/WSS                                         │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│                   API Gateway / Load Balancer               │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│                  Application Layer (ASP.NET Core)           │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              Web API Controllers                      │  │
│  │  Orders │ Products │ Customers │ Payments │ Reports  │  │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              SignalR Hubs                            │  │
│  │  Kitchen │ OrderStatus │ OrderLock │ ServerCommand   │  │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              Business Logic Layer                     │  │
│  │  Services │ Validators │ Mappers │ Handlers          │  │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              Data Access Layer                        │  │
│  │  Repositories │ Unit of Work │ EF Core DbContext     │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                     │
│  ┌──────────┬──────────┬──────────┬──────────┬──────────┐  │
│  │ SQL      │ Redis    │ RabbitMQ │ File     │ Logging  │  │
│  │ Server   │ Cache    │ Queue    │ Storage  │ (Serilog)│  │
│  └──────────┴──────────┴──────────┴──────────┴──────────┘  │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│                    External Systems                         │
│  ┌──────────┬──────────┬──────────┬──────────┬──────────┐  │
│  │ Legacy   │ Print    │ Payment  │ Delivery │ Analytics│  │
│  │ WPF POS  │ Server   │ Gateway  │ APIs     │ Platform │  │
│  └──────────┴──────────┴──────────┴──────────┴──────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### Component Interaction Flow

```
User Action (PWA)
    ↓
API Request (HTTPS)
    ↓
API Controller
    ↓
Business Service
    ↓
Repository
    ↓
Database (SQL Server)
    ↓
SignalR Hub (broadcast update)
    ↓
All Connected Clients (real-time update)
```

---

## Technology Stack

### Frontend Stack

#### Blazor WebAssembly
```csharp
Frontend Technologies:
├── Blazor WebAssembly .NET 8 (C# in browser)
├── MudBlazor 6.x (Material Design components)
├── Fluxor (Flux/Redux state management)
├── Blazored.LocalStorage (offline storage)
├── SignalR Client (real-time communication)
├── PWA support (built-in)
└── Blazor.Extensions.Logging (client-side logging)
```

**Rationale**:
- **Single Language**: C# for both frontend and backend eliminates context switching
- **Type Safety**: Strong typing throughout the entire stack
- **Code Reuse**: Share DTOs, validators, and business logic between client and server
- **Familiar Syntax**: Razor syntax similar to MVC views, easy transition for MVC developers
- **Tooling**: Full Visual Studio/Rider support with IntelliSense and debugging
- **Performance**: WebAssembly provides near-native performance
- **Team Efficiency**: Smaller team can maintain full stack with single language
- **PWA Support**: Built-in service worker and offline capabilities

**Why Blazor Over React**:
- No need to learn React, JSX, or TypeScript
- Leverage existing C# and Razor knowledge
- Reuse validation logic from backend (FluentValidation)
- Better integration with ASP.NET Core ecosystem
- Smaller learning curve for MVC developers
- Single language (C#) across entire stack

**Why Pure MudBlazor**:
- Rich component library (tables, dialogs, forms, charts)
- Built specifically for Blazor (no JavaScript interop needed)
- Material Design principles (modern, clean, accessible)
- Active community and long-term support
- Highly customizable theming system
- Consistent component API across entire application

### Backend Stack

```csharp
Backend Technologies:
├── ASP.NET Core 8 (web framework)
├── Entity Framework Core 8 (ORM)
├── SignalR (real-time communication)
├── MediatR (CQRS pattern)
├── FluentValidation (validation)
├── AutoMapper (object mapping)
├── Serilog (structured logging)
├── Hangfire (background jobs)
├── Polly (resilience)
└── xUnit (testing)
```

### Infrastructure Stack

```
Infrastructure:
├── SQL Server 2019+ (database)
├── Redis (caching, session)
├── RabbitMQ (message queue)
├── IIS / Kestrel (web server)
├── Docker (optional containerization)
└── Nginx (reverse proxy, optional)
```

---

## Database Design

### Shared Database Strategy

The new web-based POS system shares the existing database with the legacy WPF POS. To maintain clear separation and avoid conflicts, all new web POS tables are created in a dedicated `web` schema.

#### Schema Organization

```
┌─────────────────────────────────────────────────────────┐
│              SQL Server Database                        │
│                                                         │
│  ┌───────────────────────────────────────────────────┐ │
│  │  dbo Schema (Existing - Legacy WPF POS)           │ │
│  │  ─────────────────────────────────────────────    │ │
│  │  • Invoices                                       │ │
│  │  • InvoiceItems                                   │ │
│  │  • PendingInvoices                                │ │
│  │  • PendingInvoiceItems                            │ │
│  │  • Customers                                      │ │
│  │  • CustomerAddresses                              │ │
│  │  • Categories                                     │ │
│  │  • CategoryItems (Products)                       │ │
│  │  • Users                                          │ │
│  │  • ServerCommandsHistory                          │ │
│  │  • ServerCommandsTypes                            │ │
│  │  • ServingTypes                                   │ │
│  │  • PaymentMethods                                 │ │
│  │  • ... (50+ existing tables)                      │ │
│  └───────────────────────────────────────────────────┘ │
│                                                         │
│  ┌───────────────────────────────────────────────────┐ │
│  │  web Schema (New - Web POS Only)                  │ │
│  │  ─────────────────────────────────────────────    │ │
│  │  • web.OrderLocks                                 │ │
│  │  • web.ApiAuditLog                                │ │
│  │  • web.UserSessions                               │ │
│  │  • web.FeatureFlags                               │ │
│  │  • web.SyncQueue                                  │ │
│  └───────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
                    ↓                    ↓
        ┌─────────────────┐  ┌─────────────────────┐
        │  Legacy WPF POS │  │  New Web-Based POS  │
        │  (dbo schema)   │  │  (dbo + web schema) │
        └─────────────────┘  └─────────────────────┘
```

#### Schema Separation Benefits

1. **Clear Ownership**: `dbo` schema for legacy, `web` schema for new system
2. **No Conflicts**: Prevents naming collisions with existing tables
3. **Easy Identification**: Instantly recognize web POS tables
4. **Simplified Permissions**: Grant schema-level permissions for web application
5. **Future Migration**: Easy to identify and migrate web-specific tables
6. **Rollback Safety**: Can drop entire `web` schema without affecting legacy system

### Schema Creation

```sql
-- Create web schema for new web POS tables
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'web')
BEGIN
    EXEC('CREATE SCHEMA web AUTHORIZATION dbo');
    PRINT 'Schema [web] created successfully';
END
GO
```

### New Tables (web Schema)

#### web.OrderLocks Table

Manages concurrent editing locks to prevent conflicts when multiple users edit the same order.

```sql
CREATE TABLE web.OrderLocks (
    ID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT NOT NULL,
    OrderType VARCHAR(20) NOT NULL, -- 'Invoice' or 'PendingInvoice'
    LockedBy INT NOT NULL, -- UserID from dbo.Users
    LockedByUserName NVARCHAR(100) NOT NULL,
    LockedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt DATETIME2 NOT NULL,
    SessionID VARCHAR(100) NOT NULL,
    DeviceInfo NVARCHAR(200) NULL,
    CONSTRAINT FK_OrderLocks_User FOREIGN KEY (LockedBy) REFERENCES dbo.Users(ID) ON DELETE CASCADE
);

CREATE NONCLUSTERED INDEX IX_OrderLocks_OrderID 
    ON web.OrderLocks(OrderID, OrderType) 
    INCLUDE (LockedBy, ExpiresAt);

CREATE NONCLUSTERED INDEX IX_OrderLocks_ExpiresAt 
    ON web.OrderLocks(ExpiresAt) 
    WHERE ExpiresAt > GETUTCDATE();

CREATE NONCLUSTERED INDEX IX_OrderLocks_SessionID 
    ON web.OrderLocks(SessionID);

GO
```

#### web.ApiAuditLog Table

Tracks all API operations for security, compliance, and debugging.

```sql
CREATE TABLE web.ApiAuditLog (
    ID BIGINT PRIMARY KEY IDENTITY(1,1),
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UserID INT NULL,
    UserName NVARCHAR(100) NULL,
    Action VARCHAR(100) NOT NULL, -- 'Create', 'Update', 'Delete', 'Read'
    EntityType VARCHAR(50) NOT NULL, -- 'Order', 'Customer', 'Product', etc.
    EntityID INT NULL,
    Changes NVARCHAR(MAX) NULL, -- JSON format: {"field": {"old": "value", "new": "value"}}
    IPAddress VARCHAR(50) NULL,
    UserAgent NVARCHAR(500) NULL,
    RequestPath NVARCHAR(500) NULL,
    HttpMethod VARCHAR(10) NULL, -- 'GET', 'POST', 'PUT', 'DELETE'
    StatusCode INT NULL, -- HTTP status code
    Duration INT NULL, -- Request duration in milliseconds
    ErrorMessage NVARCHAR(MAX) NULL,
    CONSTRAINT FK_ApiAuditLog_User FOREIGN KEY (UserID) REFERENCES dbo.Users(ID) ON DELETE SET NULL
);

CREATE NONCLUSTERED INDEX IX_ApiAuditLog_Timestamp 
    ON web.ApiAuditLog(Timestamp DESC);

CREATE NONCLUSTERED INDEX IX_ApiAuditLog_UserID 
    ON web.ApiAuditLog(UserID) 
    INCLUDE (Timestamp, Action, EntityType);

CREATE NONCLUSTERED INDEX IX_ApiAuditLog_EntityType 
    ON web.ApiAuditLog(EntityType, EntityID) 
    INCLUDE (Timestamp, Action, UserID);

GO
```

#### web.UserSessions Table

Manages active user sessions for the web application.

```sql
CREATE TABLE web.UserSessions (
    ID INT PRIMARY KEY IDENTITY(1,1),
    SessionID VARCHAR(100) NOT NULL UNIQUE,
    UserID INT NOT NULL,
    UserName NVARCHAR(100) NOT NULL,
    DeviceType VARCHAR(50) NOT NULL, -- 'Desktop', 'Tablet', 'Mobile'
    DeviceName NVARCHAR(200) NULL,
    BrowserName VARCHAR(50) NULL,
    BrowserVersion VARCHAR(20) NULL,
    IPAddress VARCHAR(50) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastActivityAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt DATETIME2 NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    RefreshToken NVARCHAR(500) NULL,
    CONSTRAINT FK_UserSessions_User FOREIGN KEY (UserID) REFERENCES dbo.Users(ID) ON DELETE CASCADE
);

CREATE UNIQUE NONCLUSTERED INDEX IX_UserSessions_SessionID 
    ON web.UserSessions(SessionID);

CREATE NONCLUSTERED INDEX IX_UserSessions_UserID 
    ON web.UserSessions(UserID) 
    INCLUDE (IsActive, ExpiresAt);

CREATE NONCLUSTERED INDEX IX_UserSessions_ExpiresAt 
    ON web.UserSessions(ExpiresAt) 
    WHERE IsActive = 1;

GO
```

#### web.FeatureFlags Table

Manages feature toggles for gradual rollout and A/B testing.

```sql
CREATE TABLE web.FeatureFlags (
    ID INT PRIMARY KEY IDENTITY(1,1),
    FeatureName VARCHAR(100) NOT NULL UNIQUE,
    DisplayName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsEnabled BIT NOT NULL DEFAULT 0,
    EnabledForUserIDs NVARCHAR(MAX) NULL, -- JSON array of user IDs
    EnabledForRoles NVARCHAR(MAX) NULL, -- JSON array of role names
    EnabledPercentage INT NOT NULL DEFAULT 0, -- 0-100, for gradual rollout
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy INT NULL,
    UpdatedBy INT NULL,
    CONSTRAINT FK_FeatureFlags_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(ID) ON DELETE SET NULL,
    CONSTRAINT FK_FeatureFlags_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users(ID) ON DELETE NO ACTION,
    CONSTRAINT CHK_FeatureFlags_Percentage CHECK (EnabledPercentage BETWEEN 0 AND 100)
);

CREATE NONCLUSTERED INDEX IX_FeatureFlags_IsEnabled 
    ON web.FeatureFlags(IsEnabled) 
    INCLUDE (FeatureName, EnabledPercentage);

GO
```

#### web.SyncQueue Table

Manages offline sync queue for PWA offline support.

```sql
CREATE TABLE web.SyncQueue (
    ID BIGINT PRIMARY KEY IDENTITY(1,1),
    QueuedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UserID INT NOT NULL,
    SessionID VARCHAR(100) NOT NULL,
    EntityType VARCHAR(50) NOT NULL, -- 'Order', 'Customer', etc.
    Operation VARCHAR(20) NOT NULL, -- 'Create', 'Update', 'Delete'
    EntityData NVARCHAR(MAX) NOT NULL, -- JSON payload
    Status VARCHAR(20) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Processing', 'Completed', 'Failed'
    RetryCount INT NOT NULL DEFAULT 0,
    MaxRetries INT NOT NULL DEFAULT 3,
    LastError NVARCHAR(MAX) NULL,
    ProcessedAt DATETIME2 NULL,
    CONSTRAINT FK_SyncQueue_User FOREIGN KEY (UserID) REFERENCES dbo.Users(ID) ON DELETE CASCADE,
    CONSTRAINT CHK_SyncQueue_Status CHECK (Status IN ('Pending', 'Processing', 'Completed', 'Failed'))
);

CREATE NONCLUSTERED INDEX IX_SyncQueue_Status 
    ON web.SyncQueue(Status, QueuedAt) 
    WHERE Status IN ('Pending', 'Processing');

CREATE NONCLUSTERED INDEX IX_SyncQueue_UserID 
    ON web.SyncQueue(UserID) 
    INCLUDE (Status, QueuedAt);

GO
```

### Database Maintenance Scripts

#### Cleanup Expired Locks

```sql
-- Stored procedure to clean up expired locks
CREATE OR ALTER PROCEDURE web.CleanupExpiredLocks
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM web.OrderLocks
    WHERE ExpiresAt < GETUTCDATE();
    
    SELECT @@ROWCOUNT AS DeletedLocks;
END
GO

-- Schedule this to run every 5 minutes
```

#### Cleanup Old Audit Logs

```sql
-- Stored procedure to archive and clean old audit logs
CREATE OR ALTER PROCEDURE web.CleanupOldAuditLogs
    @RetentionDays INT = 90
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CutoffDate DATETIME2 = DATEADD(DAY, -@RetentionDays, GETUTCDATE());
    
    DELETE FROM web.ApiAuditLog
    WHERE Timestamp < @CutoffDate;
    
    SELECT @@ROWCOUNT AS DeletedRecords;
END
GO

-- Schedule this to run daily
```

#### Cleanup Expired Sessions

```sql
-- Stored procedure to clean up expired sessions
CREATE OR ALTER PROCEDURE web.CleanupExpiredSessions
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE web.UserSessions
    SET IsActive = 0
    WHERE ExpiresAt < GETUTCDATE() AND IsActive = 1;
    
    SELECT @@ROWCOUNT AS ExpiredSessions;
END
GO

-- Schedule this to run every hour
```

### Permissions Setup

```sql
-- Create application user for web POS
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'WebPosAppUser')
BEGIN
    CREATE USER WebPosAppUser WITHOUT LOGIN;
END
GO

-- Grant permissions on dbo schema (read-only for most tables)
GRANT SELECT ON SCHEMA::dbo TO WebPosAppUser;
GRANT INSERT, UPDATE ON dbo.Invoices TO WebPosAppUser;
GRANT INSERT, UPDATE ON dbo.InvoiceItems TO WebPosAppUser;
GRANT INSERT, UPDATE ON dbo.PendingInvoices TO WebPosAppUser;
GRANT INSERT, UPDATE ON dbo.PendingInvoiceItems TO WebPosAppUser;
GRANT INSERT, UPDATE ON dbo.Customers TO WebPosAppUser;
GRANT INSERT, UPDATE ON dbo.CustomerAddresses TO WebPosAppUser;
GRANT INSERT ON dbo.ServerCommandsHistory TO WebPosAppUser;

-- Grant full permissions on web schema
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::web TO WebPosAppUser;
GRANT EXECUTE ON SCHEMA::web TO WebPosAppUser;

GO
```

### Migration Strategy

#### Phase 1: Schema Creation (Day 1)
1. Create `web` schema
2. Deploy all `web` schema tables
3. Deploy maintenance stored procedures
4. Set up scheduled jobs for cleanup

#### Phase 2: Data Seeding (Day 1)
1. Seed `web.FeatureFlags` with initial features
2. No data migration needed (new tables start empty)

#### Phase 3: Application Deployment (Day 2+)
1. Deploy web API with connection to both schemas
2. Web app reads from `dbo` schema (existing data)
3. Web app writes to `web` schema (new functionality)

#### Phase 4: Monitoring (Ongoing)
1. Monitor `web.ApiAuditLog` for errors
2. Monitor `web.OrderLocks` for deadlocks
3. Monitor `web.SyncQueue` for failed syncs

---

## Browser Compatibility

### Overview

The web-based POS system is built with Blazor WebAssembly and requires browsers that support WebAssembly and modern web standards. This section outlines browser compatibility requirements for desktop, tablet, and mobile devices.

### Desktop Browser Support

| Browser | Minimum Version | Recommended Version | WebAssembly Support | PWA Support | Notes |
|---------|----------------|---------------------|---------------------|-------------|-------|
| **Google Chrome** | 57+ | Latest | ✅ Full | ✅ Full | Best performance and PWA support |
| **Microsoft Edge** | 79+ (Chromium) | Latest | ✅ Full | ✅ Full | Excellent performance, recommended for Windows |
| **Mozilla Firefox** | 52+ | Latest | ✅ Full | ✅ Full | Good performance and standards compliance |
| **Safari** | 11+ | Latest | ✅ Full | ⚠️ Limited | PWA support limited on macOS |
| **Opera** | 44+ | Latest | ✅ Full | ✅ Full | Chromium-based, full compatibility |
| **Brave** | Any | Latest | ✅ Full | ✅ Full | Privacy-focused, Chromium-based |

### Mobile & Tablet Browser Support

| Browser | Platform | Minimum Version | WebAssembly Support | PWA Support | Notes |
|---------|----------|----------------|---------------------|-------------|-------|
| **Chrome Mobile** | Android | 57+ | ✅ Full | ✅ Full | Best mobile experience, recommended |
| **Safari Mobile** | iOS/iPadOS | 11+ | ✅ Full | ⚠️ Limited | PWA install limited, home screen add works |
| **Samsung Internet** | Android | 7.2+ | ✅ Full | ✅ Full | Excellent performance on Samsung devices |
| **Firefox Mobile** | Android | 52+ | ✅ Full | ✅ Full | Good alternative to Chrome |
| **Edge Mobile** | Android/iOS | 79+ | ✅ Full | ✅ Full | Chromium-based, full compatibility |

### Required Browser Features

The following features are **mandatory** for the application to function:

- ✅ **WebAssembly 1.0** - Core runtime for Blazor WebAssembly
- ✅ **JavaScript ES6+** - Modern JavaScript features
- ✅ **WebSocket** - Required for SignalR real-time communication
- ✅ **IndexedDB** - Client-side database for offline storage
- ✅ **Service Workers** - PWA and offline functionality
- ✅ **Local Storage** - Session and preference storage
- ✅ **Fetch API** - HTTP requests
- ✅ **Promises and async/await** - Asynchronous operations

### Optional but Recommended Features

These features enhance the user experience but are not required:

- ⭐ **Web Notifications API** - Order alerts and notifications
- ⭐ **Vibration API** - Haptic feedback on mobile devices
- ⭐ **Geolocation API** - Delivery tracking and location services
- ⭐ **Camera API** - Barcode scanning functionality
- ⭐ **Web Share API** - Share receipts and orders

### PWA Installation Support

**Full PWA Support** (install to home screen/desktop with app-like experience):
- ✅ Chrome (Desktop & Mobile)
- ✅ Microsoft Edge (Desktop & Mobile)
- ✅ Samsung Internet
- ✅ Opera (Desktop & Mobile)
- ✅ Brave

**Limited PWA Support** (add to home screen with limitations):
- ⚠️ Safari (iOS/iPadOS/macOS) - 50MB storage limit, service worker restrictions
- ⚠️ Firefox (Desktop) - Requires manual configuration

**No PWA Support**:
- ❌ Internet Explorer (all versions) - Not supported at all

### Recommended Browser Configuration

#### For POS Stations (Desktop/Laptop)

**Recommended Browser**: Chrome or Edge (latest)

**Configuration**:
- Kiosk Mode: Enabled (F11 fullscreen)
- Auto-updates: Enabled
- Cache: 500MB minimum
- Cookies: Enabled
- JavaScript: Enabled
- Pop-ups: Allowed for pos.local domain
- Notifications: Enabled

**Chrome Kiosk Mode**:
```bash
chrome.exe --kiosk "https://pos.local" --disable-pinch --overscroll-history-navigation=0
```

**Edge Kiosk Mode**:
```bash
msedge.exe --kiosk "https://pos.local" --edge-kiosk-type=fullscreen
```

#### For Waiter Tablets

**Recommended Browser**: Chrome (Android) or Safari (iOS)

**Configuration**:
- PWA: Installed to home screen
- Notifications: Enabled
- Location: Enabled (for delivery)
- Camera: Enabled (for scanning)
- Screen timeout: Extended or disabled
- Auto-rotate: Enabled

**Installation Instructions**:

**Android (Chrome)**:
1. Open https://pos.local in Chrome
2. Tap menu (⋮) → "Install app" or "Add to Home screen"
3. Confirm installation
4. Launch from home screen

**iOS (Safari)**:
1. Open https://pos.local in Safari
2. Tap Share button (□↑)
3. Tap "Add to Home Screen"
4. Confirm and launch from home screen

#### For Kitchen Display

**Recommended Browser**: Chrome or Edge (latest)

**Configuration**:
- Fullscreen: Enabled
- Auto-refresh: Disabled (SignalR handles updates)
- Sound: Enabled (for order alerts)
- Screen timeout: Disabled
- Power saving: Disabled
- Notifications: Enabled

### Performance Requirements

#### Optimal Performance

**Recommended for POS Stations**:
- Chrome 90+ on Windows 10/11
- Edge 90+ on Windows 10/11
- Chrome 90+ on Android 10+
- Safari 14+ on iOS 14+

#### Minimum Acceptable Performance

- Chrome 70+ on Windows 7+
- Firefox 70+ on Windows 7+
- Safari 12+ on iOS 12+
- Chrome 70+ on Android 7+

#### Hardware Recommendations

| Device Type | RAM | Storage | Network |
|-------------|-----|---------|---------|
| **Desktop/Laptop** | 4GB min, 8GB recommended | 500MB cache | 5 Mbps min, 10+ Mbps recommended |
| **Tablet** | 2GB min, 4GB recommended | 250MB cache | 5 Mbps min, 10+ Mbps recommended |
| **Mobile** | 2GB min, 3GB recommended | 100MB cache | 3 Mbps min, 5+ Mbps recommended |

### Known Limitations

| Browser | Limitation | Impact | Workaround |
|---------|-----------|--------|------------|
| **Safari (iOS)** | PWA limited to 50MB storage | Offline data limited | Implement aggressive cache cleanup |
| **Safari (iOS)** | Service worker restrictions | Limited offline capability | Fallback to online-only mode |
| **Safari (all)** | No Web Bluetooth | Cannot use Bluetooth scanners | Use camera-based or USB scanners |
| **Firefox** | Slower WebAssembly startup | Initial load slower | Show loading indicator, optimize bundle size |
| **Older Android** | Limited memory | App may crash on low-end devices | Implement memory-efficient rendering |
| **Safari (iOS)** | No install prompt | Users must manually add to home screen | Provide clear instructions |

### Unsupported Browsers

**Completely Unsupported**:
- ❌ **Internet Explorer** (all versions)
  - Action: Redirect to download page for modern browser
  - Message: "Internet Explorer is not supported. Please use Chrome, Edge, Firefox, or Safari."

- ❌ **Opera Mini**
  - Action: Show warning, limited functionality
  - Message: "Opera Mini has limited support. For best experience, use Opera or Chrome."

- ❌ **Browsers with JavaScript disabled**
  - Action: Show error page
  - Message: "JavaScript is required. Please enable JavaScript in your browser settings."

### Browser Detection

The application includes a browser detection service to provide appropriate fallbacks and warnings for unsupported browsers. The service checks for:
- WebAssembly support
- PWA capabilities
- Service worker support
- Required APIs (WebSocket, IndexedDB, etc.)

### Testing Matrix

**Primary Testing Targets** (must test before each release):
1. Chrome Latest (Windows 10/11)
2. Edge Latest (Windows 10/11)
3. Chrome Latest (Android 10+)
4. Safari Latest (iOS 14+)
5. Samsung Internet Latest (Android)

**Secondary Testing Targets** (should test periodically):
1. Firefox Latest (Windows)
2. Safari Latest (macOS)
3. Chrome Latest (iOS)
4. Firefox Latest (Android)
5. Edge Latest (Android)

### Support Policy

**Fully Supported**:
- Latest version of Chrome, Edge, Firefox, Safari
- Previous major version of each browser
- Latest version of mobile browsers

**Limited Support**:
- Browsers older than 2 major versions
- Beta/preview versions of browsers
- Less common browsers (Brave, Opera, etc.)

**No Support**:
- Internet Explorer (any version)
- Browsers with JavaScript disabled
- Heavily modified or outdated browsers

For detailed browser compatibility information, troubleshooting, and configuration guides, see the separate [Browser Compatibility Guide](browser-compatibility.md).

---

## Application Structure & Areas

### Overview

The application is organized into three distinct areas, each with its own layout, theming, and authorization requirements. This structure mirrors the MVC Areas concept but uses Blazor's routing and layout system.

### Area Organization

```
Pos.Web.Client/
├── Pages/
│   ├── Identity/          (@page "/identity/*")
│   │   ├── Login.razor
│   │   ├── Register.razor
│   │   └── ForgotPassword.razor
│   │
│   ├── POS/               (@page "/pos/*")
│   │   ├── Cashier.razor
│   │   ├── Waiter.razor
│   │   ├── Kitchen.razor
│   │   └── PendingOrders.razor
│   │
│   └── Admin/             (@page "/admin/*")
│       ├── Dashboard.razor
│       ├── Users/
│       ├── Products/
│       ├── Reports/
│       └── Settings/
│
├── Shared/
│   ├── Layouts/
│   │   ├── IdentityLayout.razor    (Minimal, centered)
│   │   ├── POSLayout.razor          (Legacy layout, fullscreen)
│   │   └── AdminLayout.razor        (Modern dashboard)
│   │
│   └── Components/
│       ├── Identity/
│       ├── POS/
│       └── Admin/
│
└── wwwroot/
    ├── css/
    │   ├── pos-theme.css            (Legacy colors: SteelBlue/Orange)
    │   ├── admin-theme.css          (Modern dashboard theme)
    │   └── identity-theme.css       (Minimal login theme)
    └── images/
```

### Area 1: Identity

**Purpose**: Authentication and user management  
**Route Prefix**: `/identity/*`  
**Layout**: `IdentityLayout.razor`  
**Authorization**: None (public access)  
**Theme**: Minimal, centered design

**Pages**:
- `/identity/login` - User login
- `/identity/register` - New user registration (if enabled)
- `/identity/forgot-password` - Password recovery
- `/identity/reset-password` - Password reset

**Design Characteristics**:
- Centered card layout
- Minimal distractions
- Focus on authentication form
- Responsive for all devices

### Area 2: POS Client

**Purpose**: Point of sale operations (cashier, waiter, kitchen)  
**Route Prefix**: `/pos/*`  
**Layout**: `POSLayout.razor` (matches legacy MainWindow.xaml)  
**Authorization**: Roles: Cashier, Waiter, Kitchen, Manager  
**Theme**: Legacy layout with SteelBlue/Orange colors

**Pages**:
- `/pos/cashier` - Main cashier interface (3-column layout)
- `/pos/waiter` - Tablet-optimized waiter interface
- `/pos/kitchen` - Kitchen display system
- `/pos/pending` - Pending orders management
- `/pos/history` - Order history and search

**Layout Structure** (Preserved from Legacy):
```
┌─────────────────────────────────────────────────────────────┐
│  Header: History | Discount | [Logo] | Pending | Settings  │
├──────────────┬──────────┬─────────────────────────────────┤
│              │          │                                 │
│   Product    │  Center  │    Shopping Cart                │
│   Catalog    │  Panel   │    & Customer Info              │
│   (35%)      │  (18%)   │    (40%)                        │
│              │          │                                 │
│   Search     │  Quick   │    Cart Items                   │
│   Categories │  Actions │    Totals                       │
│   Products   │          │    Customer                     │
│              │          │                                 │
├──────────────┴──────────┴─────────────────────────────────┤
│  Footer: Clear | Checkout | Save Pending | New Order      │
└─────────────────────────────────────────────────────────────┘
```

**Design Rationale**:
- **Preserve legacy layout**: Staff familiarity, zero training on layout
- **Muscle memory**: Buttons in same positions, same workflow
- **Proven design**: 10+ years of real-world usage
- **Touch-optimized**: Already designed for touch screens
- **Information density**: Shows everything needed without scrolling

**Color Scheme** (Match Legacy):
- Primary: SteelBlue (#4682B4)
- Accent: Orange (#FFA500)
- Success: Green (#4CAF50)
- Error: Crimson (#DC143C)
- Warning: Gold (#FFD700)

### Area 3: Admin

**Purpose**: Back-office management and reporting  
**Route Prefix**: `/admin/*`  
**Layout**: `AdminLayout.razor` (modern dashboard)  
**Authorization**: Roles: Manager, Admin  
**Theme**: Modern Material Design with optional Metronic CSS

**Pages**:
- `/admin/dashboard` - Sales overview, charts, KPIs
- `/admin/users` - User management (CRUD)
- `/admin/products` - Product catalog management
- `/admin/categories` - Category management
- `/admin/customers` - Customer database
- `/admin/reports/sales` - Sales reports
- `/admin/reports/inventory` - Inventory reports
- `/admin/reports/performance` - Performance analytics
- `/admin/settings/general` - General settings
- `/admin/settings/printers` - Printer configuration
- `/admin/settings/taxes` - Tax rates and service charges

**Layout Structure**:
```
┌─────────────────────────────────────────────────────────────┐
│  Header: Logo | Navigation | User Menu                      │
├──────────┬──────────────────────────────────────────────────┤
│          │                                                  │
│ Sidebar  │  Main Content Area                               │
│          │                                                  │
│ - Dash   │  ┌────────────────────────────────────────────┐ │
│ - Users  │  │  Page Title                                │ │
│ - Prod   │  ├────────────────────────────────────────────┤ │
│ - Rpts   │  │                                            │ │
│ - Sets   │  │  Content (tables, forms, charts)           │ │
│          │  │                                            │ │
│          │  └────────────────────────────────────────────┘ │
└──────────┴──────────────────────────────────────────────────┘
```

**Design Characteristics**:
- Modern Material Design
- Responsive sidebar (collapsible on mobile)
- Data-rich tables with sorting, filtering, pagination
- Interactive charts and visualizations
- Professional color scheme

### Authorization Matrix

| Area | Route | Roles | Description |
|------|-------|-------|-------------|
| **Identity** | `/identity/*` | None | Public access for login/register |
| **POS** | `/pos/cashier` | Cashier, Manager | Main POS interface |
| **POS** | `/pos/waiter` | Waiter, Manager | Tablet waiter interface |
| **POS** | `/pos/kitchen` | Kitchen, Manager | Kitchen display |
| **POS** | `/pos/pending` | Cashier, Waiter, Manager | Pending orders |
| **POS** | `/pos/history` | Cashier, Manager | Order history |
| **Admin** | `/admin/dashboard` | Manager, Admin | Dashboard overview |
| **Admin** | `/admin/users` | Admin | User management |
| **Admin** | `/admin/products` | Manager, Admin | Product management |
| **Admin** | `/admin/reports/*` | Manager, Admin | All reports |
| **Admin** | `/admin/settings/*` | Admin | System settings |

### Routing Configuration

**App.razor**
```razor
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(App).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
                <NotAuthorized>
                    @if (context.User.Identity?.IsAuthenticated == true)
                    {
                        <p>You don't have permission to access this page.</p>
                        <MudButton Href="/pos/cashier" Color="Color.Primary">Go to POS</MudButton>
                    }
                    else
                    {
                        <RedirectToLogin />
                    }
                </NotAuthorized>
            </AuthorizeRouteView>
        </Found>
        <NotFound>
            <LayoutView Layout="@typeof(MainLayout)">
                <MudContainer Class="mt-8">
                    <MudText Typo="Typo.h4">Page Not Found</MudText>
                    <MudText>Sorry, there's nothing at this address.</MudText>
                    <MudButton Href="/" Color="Color.Primary" Class="mt-4">Go Home</MudButton>
                </MudContainer>
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>
```

### Theming Strategy

#### 1. POS Theme (Legacy Colors)

**wwwroot/css/pos-theme.css**
```css
/* Match legacy WPF POS colors */
:root {
    --mud-palette-primary: #4682B4;        /* SteelBlue */
    --mud-palette-secondary: #FFA500;      /* Orange */
    --mud-palette-success: #4CAF50;        /* Green */
    --mud-palette-error: #DC143C;          /* Crimson */
    --mud-palette-warning: #FFD700;        /* Gold */
    --mud-palette-info: #1976D2;           /* Blue */
}

/* POS-specific component overrides */
.mud-button-root {
    text-transform: none;
    font-weight: 600;
    border-radius: 8px;
}

.pos-product-card {
    transition: transform 0.2s, box-shadow 0.2s;
    cursor: pointer;
}

.pos-product-card:hover {
    transform: scale(1.05);
    box-shadow: 0 4px 20px rgba(0,0,0,0.2);
}

.pos-cart-item {
    border-bottom: 1px solid #e0e0e0;
    padding: 8px 0;
}
```

#### 2. Admin Theme (Modern Dashboard)

**wwwroot/css/admin-theme.css**
```css
/* Modern Material Design for admin */
:root {
    --mud-palette-primary: #1976D2;        /* Material Blue */
    --mud-palette-secondary: #424242;      /* Material Gray */
    --mud-palette-success: #4CAF50;        /* Material Green */
    --mud-palette-error: #F44336;          /* Material Red */
    --mud-palette-warning: #FF9800;        /* Material Orange */
    --mud-palette-info: #2196F3;           /* Material Light Blue */
}

/* Optional: Import Metronic CSS for additional styling */
/* @import 'metronic/css/style.bundle.css'; */

/* Admin-specific overrides */
.admin-sidebar {
    background: linear-gradient(180deg, #1976D2 0%, #1565C0 100%);
}

.admin-card {
    border-radius: 12px;
    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.admin-stat-card {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
}
```

#### 3. Dynamic Theme Loading

**wwwroot/index.html**
```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>MyChair POS</title>
    <base href="/" />
    
    <!-- MudBlazor CSS (always loaded) -->
    <link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
    
    <!-- Dynamic theme based on route -->
    <link id="theme-css" href="css/pos-theme.css" rel="stylesheet" />
    
    <!-- PWA manifest -->
    <link rel="manifest" href="manifest.json" />
    <link rel="apple-touch-icon" sizes="512x512" href="icon-512.png" />
</head>
<body>
    <div id="app">
        <div class="loading-container">
            <div class="loading-spinner"></div>
            <p>Loading MyChair POS...</p>
        </div>
    </div>
    
    <!-- MudBlazor JS -->
    <script src="_content/MudBlazor/MudBlazor.min.js"></script>
    
    <!-- Blazor WebAssembly -->
    <script src="_framework/blazor.webassembly.js"></script>
    
    <!-- Theme switcher -->
    <script>
        // Switch theme based on route
        function updateTheme() {
            const path = window.location.pathname;
            const themeLink = document.getElementById('theme-css');
            
            if (path.startsWith('/admin')) {
                themeLink.href = 'css/admin-theme.css';
            } else if (path.startsWith('/identity')) {
                themeLink.href = 'css/identity-theme.css';
            } else {
                themeLink.href = 'css/pos-theme.css';
            }
        }
        
        // Update theme on navigation
        window.addEventListener('popstate', updateTheme);
        updateTheme();
    </script>
</body>
</html>
```

### Navigation Between Areas

**Shared/Components/AreaSwitcher.razor**
```razor
<MudMenu Icon="@Icons.Material.Filled.Apps" Color="Color.Inherit">
    <MudMenuItem Href="/pos/cashier" Icon="@Icons.Material.Filled.PointOfSale">
        POS
    </MudMenuItem>
    <MudMenuItem Href="/admin/dashboard" Icon="@Icons.Material.Filled.Dashboard">
        Admin
    </MudMenuItem>
    <MudDivider />
    <MudMenuItem OnClick="Logout" Icon="@Icons.Material.Filled.Logout">
        Logout
    </MudMenuItem>
</MudMenu>

@code {
    [Inject] private NavigationManager Navigation { get; set; }
    [Inject] private IAuthService AuthService { get; set; }
    
    private async Task Logout()
    {
        await AuthService.LogoutAsync();
        Navigation.NavigateTo("/identity/login");
    }
}
```

### Benefits of This Structure

1. **Clear Separation**: Each area has distinct purpose, layout, and theme
2. **Role-Based Access**: Authorization enforced at route level
3. **Familiar Patterns**: Similar to MVC Areas for easy understanding
4. **Flexible Theming**: Different look and feel per area
5. **Maintainable**: Organized folder structure, easy to navigate
6. **Scalable**: Easy to add new areas or pages within existing areas

---

