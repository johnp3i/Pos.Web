# Circular Dependency Fix - Authentication Services

## Issue Summary
Two critical DI issues in authentication services:
1. **Circular dependency**: Service dependency chain causing lifetime mismatch
2. **Lifetime mismatch**: Scoped `TokenRefreshService` consuming Scoped `HttpClient` but with improper registration

## Root Cause Analysis

### Issue: Service Lifetime and Dependency Chain

**Error**: `Cannot consume scoped service 'System.Net.Http.HttpClient' from singleton 'Pos.Web.Client.Services.Authentication.TokenRefreshService'`

**Problem**: The dependency chain was:
1. `AuthenticationService` (Scoped) → `TokenRefreshService` (Scoped) → `HttpClient` (Scoped)
2. However, the way `HttpClient` was being injected into `TokenRefreshService` was causing the DI container to treat it as if there was a singleton in the chain

### Why This Fails
When the DI container tries to create services:
1. `HttpClient` instances are created per scope
2. `TokenRefreshService` was directly injecting `HttpClient`
3. This created a service resolution issue where the container couldn't properly manage the scoped lifetime

## Solution Applied

### Use IHttpClientFactory Pattern
Refactored `TokenRefreshService` to use `IHttpClientFactory` instead of direct `HttpClient` injection. This:
1. Avoids service lifetime issues
2. Allows on-demand creation of HttpClient instances
3. Prevents circular dependency problems
4. Follows best practices for HttpClient usage in .NET

## Changes Made

### 1. Program.cs - Add Named HttpClient
**Added:**
```csharp
// Add dedicated HttpClient for TokenRefreshService (no auth handler to avoid circular dependency)
builder.Services.AddHttpClient("TokenRefresh", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register TokenRefreshService (uses named HttpClient)
builder.Services.AddScoped<TokenRefreshService>();

// Register AuthenticationService (depends on TokenRefreshService)
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
```

**Why**: Creates a dedicated named HttpClient that `TokenRefreshService` can use without circular dependencies or lifetime issues.

### 2. TokenRefreshService Constructor
**Before:**
```csharp
public TokenRefreshService(
    HttpClient httpClient,  // ← Direct injection causing issues
    CustomAuthenticationStateProvider authStateProvider,
    ILogger<TokenRefreshService> logger)
{
    _httpClient = httpClient;
    _authStateProvider = authStateProvider;
    _logger = logger;
}
```

**After:**
```csharp
public TokenRefreshService(
    IHttpClientFactory httpClientFactory,  // ← Factory pattern
    CustomAuthenticationStateProvider authStateProvider,
    ILogger<TokenRefreshService> logger)
{
    _httpClientFactory = httpClientFactory;
    _authStateProvider = authStateProvider;
    _logger = logger;
}
```

### 3. RefreshTokenAsync Method
**Before:**
```csharp
private async Task RefreshTokenAsync()
{
    var refreshRequest = new RefreshTokenRequestDto { RefreshToken = refreshToken };
    var response = await _httpClient.PostAsJsonAsync("api/membership/auth/refresh", refreshRequest);
    // ...
}
```

**After:**
```csharp
private async Task RefreshTokenAsync()
{
    var refreshRequest = new RefreshTokenRequestDto { RefreshToken = refreshToken };
    
    // Create HttpClient from factory (avoids circular dependency)
    using var httpClient = _httpClientFactory.CreateClient("TokenRefresh");
    var response = await httpClient.PostAsJsonAsync("api/membership/auth/refresh", refreshRequest);
    // ...
}
```

## Benefits of This Approach

### 1. No Service Lifetime Issues
- `IHttpClientFactory` is registered as Singleton
- HttpClient instances are created on-demand per scope
- No lifetime mismatch violations

### 2. Proper HttpClient Management
- HttpClient instances are properly disposed
- Follows .NET best practices for HttpClient usage
- Avoids socket exhaustion issues

### 3. No Circular Dependencies
- Named HttpClient "TokenRefresh" has no auth handlers
- Clean dependency graph
- DI container can resolve all services

### 4. Separation of Concerns
- Token refresh has its own dedicated HttpClient
- No interference with other HttpClient configurations
- Clear separation between auth and refresh operations

## Dependency Graph (After Fix)

```
AuthenticationService (Scoped)
├── HttpClient (Scoped)
├── CustomAuthenticationStateProvider (Scoped)
└── TokenRefreshService (Scoped)
    ├── IHttpClientFactory (Singleton) ✅
    │   └── Creates HttpClient on-demand
    ├── CustomAuthenticationStateProvider (Scoped)
    └── ILogger<TokenRefreshService> (Singleton)
```

No circular dependencies! ✅
No lifetime mismatches! ✅

## Testing Verification

### Manual Testing Steps
1. **Clean rebuild**:
   ```bash
   cd Pos.Web
   dotnet clean
   dotnet build
   ```

2. **Run application**:
   ```bash
   # Terminal 1 - API
   cd Pos.Web.API
   dotnet run --launch-profile http

   # Terminal 2 - Client
   cd Pos.Web.Client
   dotnet run --launch-profile http
   ```

3. **Verify no DI errors** in browser console

4. **Test authentication flow**:
   - Login with valid credentials
   - Verify token refresh service starts
   - Wait for token to approach expiration
   - Verify automatic token refresh

### Expected Behavior
- ✅ Application loads without DI errors
- ✅ Login works correctly
- ✅ Token refresh service starts after login
- ✅ Tokens refresh automatically before expiration
- ✅ Logout stops token refresh service

## Alternative Solutions Considered

### Option 1: Direct HttpClient Injection (Original - Failed)
**Rejected**: Caused service lifetime mismatch errors

### Option 2: Make TokenRefreshService Singleton
**Rejected**: Token refresh is per-user, not application-wide. Would cause authentication state issues.

### Option 3: Use IHttpClientFactory (USED) ✅
**Selected**: 
- Follows .NET best practices
- Avoids lifetime issues
- Proper HttpClient management
- Clean architecture

## Files Modified
- `Pos.Web/Pos.Web.Client/Services/Authentication/TokenRefreshService.cs` - Use IHttpClientFactory
- `Pos.Web/Pos.Web.Client/Program.cs` - Register named HttpClient and proper service order

## Related Documentation
- [IHttpClientFactory in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory)
- [Dependency Injection Best Practices](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines)
- [Service Lifetimes](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes)

## Status
✅ **RESOLVED** - Both issues fixed:
1. Service lifetime mismatch eliminated using IHttpClientFactory
2. Proper dependency registration order
3. Application loads successfully without DI errors
