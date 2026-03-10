# 🚀 Checkpoint: Initial Application Run

## Overview

This checkpoint marks the completion of the minimal viable backend and frontend setup. Both the API and Client projects are now configured and ready for an initial test run.

**Status**: ✅ Ready for Initial Run  
**Date**: 2026-02-28  
**Completed Tasks**: 11.1, 11.2, 11.3, 11.4, 2 (partial), 3, 4 (partial)

---

## What's Been Completed

### ✅ Frontend (Blazor WebAssembly Client)
- **Task 11.1**: Blazor WebAssembly configuration with MudBlazor, Fluxor, Blazored.LocalStorage
- **Task 11.2**: JWT authentication with local storage (CustomAuthenticationStateProvider)
- **Task 11.3**: SignalR client with automatic reconnection
- **Task 11.4**: Three area layouts (Identity, POS, Admin) with routing

**Key Features**:
- Three custom themes (POS, Admin, Identity)
- Dynamic theme switching based on route
- Authentication state management
- SignalR hub connection (not yet connected to backend)
- Three main pages: Login, Cashier, Dashboard

### ✅ Backend (ASP.NET Core API)
- **Task 2**: Database schema created (web schema with 5 tables)
- **Task 3**: All DTOs, models, enums, and constants created in Shared project
- **Task 4 (Partial)**: Minimal API setup with health checks and stub authentication

**Key Features**:
- Serilog logging to console and file
- EF Core with SQL Server connection
- CORS configuration for local development
- Health check endpoints (basic + database)
- Stub authentication controller (returns mock tokens)
- Swagger UI for API testing

### ✅ Database
- Web schema created on `127.0.0.1/POS` database
- 5 tables: OrderLocks, ApiAuditLog, UserSessions, FeatureFlags, SyncQueue
- All indexes and stored procedures created
- Default feature flags inserted

---

## Prerequisites

### 1. Database Setup
Ensure the database is ready:
```sql
-- Verify web schema exists
SELECT * FROM sys.schemas WHERE name = 'web';

-- Verify tables exist
SELECT TABLE_SCHEMA, TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'web';

-- Verify feature flags
SELECT * FROM web.FeatureFlags;
```

### 2. Connection String
Verify connection string in `Pos.Web.API/appsettings.json`:
```json
"ConnectionStrings": {
  "PosDatabase": "Server=127.0.0.1;Database=POS;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
}
```

---

## How to Run

### Step 1: Start the API Server

Open a terminal in the `Pos.Web` directory:

```bash
cd Pos.Web
dotnet run --project Pos.Web.API/Pos.Web.API.csproj
```

**Expected Output**:
```
[12:00:00 INF] Starting MyChair POS API
[12:00:01 INF] Now listening on: https://localhost:7001
[12:00:01 INF] Now listening on: http://localhost:5001
[12:00:01 INF] Application started. Press Ctrl+C to shut down.
```

**API Endpoints Available**:
- Root: `http://localhost:5001/` (API info)
- Health: `http://localhost:5001/health` (basic health check)
- Health DB: `http://localhost:5001/api/health/database` (database connectivity)
- Swagger: `http://localhost:5001/swagger` (API documentation)
- Login: `POST http://localhost:5001/api/auth/login` (stub authentication)

### Step 2: Start the Blazor Client

Open a **second terminal** in the `Pos.Web` directory:

```bash
cd Pos.Web
dotnet run --project Pos.Web.Client/Pos.Web.Client.csproj
```

**Expected Output**:
```
[12:00:05 INF] Now listening on: https://localhost:5000
[12:00:05 INF] Application started. Press Ctrl+C to shut down.
```

**Client URL**: `https://localhost:5000`

---

## Testing the Application

### Test 1: API Health Checks

**Using Browser**:
1. Navigate to `http://localhost:5001/`
   - Should see: `{"Application":"MyChair POS API","Version":"1.0.0","Status":"Running",...}`

2. Navigate to `http://localhost:5001/health`
   - Should see: `Healthy`

3. Navigate to `http://localhost:5001/api/health/database`
   - Should see: `{"Status":"Healthy","Database":"Connected",...}`

**Using PowerShell**:
```powershell
# Test root endpoint
Invoke-RestMethod -Uri "http://localhost:5001/" -Method Get

# Test health check
Invoke-RestMethod -Uri "http://localhost:5001/health" -Method Get

# Test database health
Invoke-RestMethod -Uri "http://localhost:5001/api/health/database" -Method Get
```

### Test 2: Swagger UI

1. Navigate to `http://localhost:5001/swagger`
2. You should see Swagger UI with two controllers:
   - **AuthController**: `/api/auth/login`, `/api/auth/logout`
   - **HealthController**: `/api/health`, `/api/health/database`

3. Test the login endpoint:
   - Click on `POST /api/auth/login`
   - Click "Try it out"
   - Enter request body:
     ```json
     {
       "username": "admin",
       "password": "test123",
       "rememberMe": false
     }
     ```
   - Click "Execute"
   - Should receive mock token response

### Test 3: Blazor Client

1. Navigate to `https://localhost:5000`
2. You should see the home page with "Welcome to MyChair POS"
3. Click "Login" in the navigation menu
4. You should see the login page with:
   - Centered card layout
   - Gradient background (identity theme)
   - Username and password fields
   - "Sign In" button

**Note**: Login functionality is stubbed. The form will call the API but authentication is not fully implemented yet.

### Test 4: Theme Switching

1. Navigate to different routes and observe theme changes:
   - `/identity/login` → Identity theme (gradient background, centered card)
   - `/pos/cashier` → POS theme (SteelBlue header, Orange accents)
   - `/admin/dashboard` → Admin theme (Material Design, blue sidebar)

2. Open browser DevTools → Console
3. You should see theme switch messages:
   ```
   Theme switched to: identity-theme
   Theme switched to: pos-theme
   Theme switched to: admin-theme
   ```

### Test 5: Navigation

Test all navigation links:
- **Home** (`/`) → Home page
- **Login** (`/identity/login`) → Login page
- **Cashier** (`/pos/cashier`) → Requires authentication (should redirect to login)
- **Dashboard** (`/admin/dashboard`) → Requires authentication (should redirect to login)

---

## Expected Behavior

### ✅ What Should Work
1. **API starts successfully** on ports 5001 (HTTP) and 7001 (HTTPS)
2. **Client starts successfully** on port 5000
3. **Health checks return "Healthy"** status
4. **Database connectivity check succeeds**
5. **Swagger UI loads** and displays API endpoints
6. **Client loads** with proper theming
7. **Navigation works** between pages
8. **Theme switching works** based on route
9. **Stub login endpoint** returns mock token

### ⚠️ What Won't Work Yet (Expected)
1. **Real authentication** - Login returns mock tokens, no JWT validation
2. **Authorization** - Protected routes don't enforce authentication yet
3. **SignalR connection** - Client tries to connect but hub not implemented
4. **Data operations** - No repositories or services implemented
5. **Legacy data access** - No entity mappings for dbo schema tables
6. **Redis caching** - Not configured yet
7. **Real-time updates** - SignalR hub not implemented

---

## Troubleshooting

### Issue: API fails to start

**Error**: `Unable to connect to database`

**Solution**:
1. Verify SQL Server is running
2. Check connection string in `appsettings.json`
3. Test connection manually:
   ```powershell
   sqlcmd -S 127.0.0.1 -d POS -E -Q "SELECT 1"
   ```

### Issue: Client fails to connect to API

**Error**: `Failed to fetch` or CORS error in browser console

**Solution**:
1. Verify API is running on `http://localhost:5001`
2. Check CORS configuration in `appsettings.json`:
   ```json
   "Cors": {
     "AllowedOrigins": [
       "https://localhost:5001",
       "http://localhost:5000"
     ]
   }
   ```
3. Verify client API URL in `wwwroot/appsettings.json`:
   ```json
   {
     "ApiBaseUrl": "http://localhost:5001"
   }
   ```

### Issue: Port already in use

**Error**: `Address already in use`

**Solution**:
1. Change ports in `launchSettings.json` (both API and Client)
2. Update CORS configuration to match new ports
3. Update client `appsettings.json` with new API port

### Issue: Database health check fails

**Error**: `Cannot connect to database`

**Solution**:
1. Verify database exists:
   ```sql
   SELECT name FROM sys.databases WHERE name = 'POS';
   ```
2. Verify web schema exists:
   ```sql
   USE POS;
   SELECT * FROM sys.schemas WHERE name = 'web';
   ```
3. Run database setup script if needed:
   ```bash
   sqlcmd -S 127.0.0.1 -d POS -E -i database-scripts.sql
   ```

---

## Log Files

### API Logs
Location: `Pos.Web/Pos.Web.API/logs/pos-api-YYYYMMDD.txt`

**What to look for**:
- `[INF] Starting MyChair POS API` - Startup message
- `[INF] Now listening on:` - Port bindings
- `[ERR]` - Any errors during startup or runtime

### Client Logs
Location: Browser DevTools → Console

**What to look for**:
- `Theme switched to:` - Theme switching messages
- `SignalR connection` - SignalR connection attempts (will fail, expected)
- `Failed to fetch` - API connection issues (investigate if present)

---

## Next Steps

After verifying the initial run works, the next tasks to implement are:

### Immediate (Task 4 - Complete)
1. **Implement real JWT authentication**
   - Create `JwtTokenService` for token generation/validation
   - Update `AuthController` to use real authentication
   - Add JWT middleware to API pipeline

2. **Create User entity and repository**
   - Map to legacy `dbo.Users` table
   - Create `IUserRepository` interface
   - Implement `UserRepository` with EF Core

3. **Implement authentication service**
   - Validate credentials against database
   - Generate real JWT tokens
   - Handle refresh tokens

### Short-term (Tasks 5-7)
4. **Create domain entities** (Task 5)
   - Map legacy dbo schema tables
   - Create entity configurations
   - Add DbSets to PosDbContext

5. **Implement repositories** (Task 6)
   - Create repository interfaces
   - Implement repositories with EF Core
   - Add unit of work pattern

6. **Implement services** (Task 7)
   - Create service interfaces
   - Implement business logic
   - Add validation and error handling

### Medium-term (Tasks 8-10)
7. **Implement SignalR hubs** (Task 8)
8. **Add caching with Redis** (Task 9)
9. **Implement API endpoints** (Task 10)

---

## Success Criteria

This checkpoint is successful if:

- ✅ API starts without errors
- ✅ Client starts without errors
- ✅ Health checks return "Healthy"
- ✅ Database connectivity check succeeds
- ✅ Swagger UI loads and displays endpoints
- ✅ Client loads with proper theming
- ✅ Navigation works between pages
- ✅ Theme switching works based on route
- ✅ Stub login endpoint returns mock response
- ✅ No critical errors in logs

---

## Summary

**What we have**: A minimal viable application with:
- Working API with health checks and stub authentication
- Working Blazor client with three themed areas
- Database schema ready for data operations
- All DTOs and models defined
- Proper logging and error handling infrastructure

**What we need**: Real implementation of:
- JWT authentication and authorization
- Entity mappings and repositories
- Business logic services
- SignalR real-time communication
- API endpoints for CRUD operations

**Status**: ✅ Ready to proceed with full implementation

---

## Quick Reference

### API Ports
- HTTP: `http://localhost:5001`
- HTTPS: `https://localhost:7001`

### Client Port
- HTTPS: `https://localhost:5000`

### Key Endpoints
- API Root: `http://localhost:5001/`
- Health: `http://localhost:5001/health`
- Swagger: `http://localhost:5001/swagger`
- Login: `POST http://localhost:5001/api/auth/login`

### Key Files
- API Configuration: `Pos.Web.API/appsettings.json`
- Client Configuration: `Pos.Web.Client/wwwroot/appsettings.json`
- Database Script: `Pos.Web/database-scripts.sql`
- API Logs: `Pos.Web.API/logs/pos-api-*.txt`

---

**Ready to test? Run both projects and verify all success criteria are met!** 🚀
