using System.Text;
using AspNetCoreRateLimit;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pos.Web.API.BackgroundServices;
using Pos.Web.API.Hubs;
using Pos.Web.API.Middleware;
using Pos.Web.Infrastructure.Data;
using Pos.Web.Infrastructure.Entities;
using Pos.Web.Infrastructure.Repositories;
using Pos.Web.Infrastructure.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace Pos.Web.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Build configuration first to access connection strings
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Configure Serilog with structured logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("Application", "MyChair.POS.API")
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/pos-api-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.MSSqlServer(
                connectionString: configuration.GetConnectionString("WebPosMembership"),
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = "ApplicationLogs",
                    SchemaName = "dbo",
                    AutoCreateSqlTable = true,
                    BatchPostingLimit = 50,
                    BatchPeriod = TimeSpan.FromSeconds(5)
                },
                restrictedToMinimumLevel: LogEventLevel.Information,
                columnOptions: new ColumnOptions
                {
                    AdditionalColumns = new[]
                    {
                        new SqlColumn { ColumnName = "CorrelationId", DataType = System.Data.SqlDbType.NVarChar, DataLength = 50, AllowNull = true },
                        new SqlColumn { ColumnName = "UserId", DataType = System.Data.SqlDbType.NVarChar, DataLength = 450, AllowNull = true },
                        new SqlColumn { ColumnName = "UserName", DataType = System.Data.SqlDbType.NVarChar, DataLength = 256, AllowNull = true },
                        new SqlColumn { ColumnName = "IpAddress", DataType = System.Data.SqlDbType.NVarChar, DataLength = 45, AllowNull = true }
                    }
                })
            .CreateLogger();

        try
        {
            Log.Information("Starting MyChair POS API");
            
            var builder = WebApplication.CreateBuilder(args);

            // Add Serilog
            builder.Host.UseSerilog();

            // Add services to the container
            ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();

            // Initialize database and seed roles
            using (var scope = app.Services.CreateScope())
            {
                await DbInitializer.InitializeAsync(scope.ServiceProvider);
            }

            // Configure the HTTP request pipeline
            ConfigureMiddleware(app);

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Database - POS Database
        services.AddDbContext<PosDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("PosDatabase"),
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null
                )
            )
        );

        // Database - WebPosMembership Database
        services.AddDbContext<WebPosMembershipDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("WebPosMembership"),
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null
                )
            )
        );

        // ASP.NET Core Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 4;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = false;
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<WebPosMembershipDbContext>()
        .AddDefaultTokenProviders();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // Unit of Work
        services.AddScoped<Pos.Web.Infrastructure.UnitOfWork.IUnitOfWork, Pos.Web.Infrastructure.UnitOfWork.UnitOfWork>();

        // Redis Cache - Optional in development
        services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
        {
            try
            {
                var redisConnectionString = configuration.GetConnectionString("Redis") 
                    ?? configuration["Redis:ConnectionString"] 
                    ?? "localhost:6379";
                var configOptions = StackExchange.Redis.ConfigurationOptions.Parse(redisConnectionString);
                configOptions.AbortOnConnectFail = false; // Don't fail if Redis is unavailable
                configOptions.ConnectTimeout = 2000; // 2 second timeout
                configOptions.SyncTimeout = 2000;
                
                var multiplexer = StackExchange.Redis.ConnectionMultiplexer.Connect(configOptions);
                Log.Information("Redis connection established successfully");
                return multiplexer;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Redis connection failed - caching will be disabled. This is normal in development without Redis installed.");
                // Return a null multiplexer - the cache service will handle this gracefully
                return null!;
            }
        });
        services.AddSingleton<Pos.Web.Infrastructure.Services.ICacheService>(sp =>
        {
            var redis = sp.GetService<StackExchange.Redis.IConnectionMultiplexer>();
            var logger = sp.GetRequiredService<ILogger<Pos.Web.Infrastructure.Services.RedisCacheService>>();
            var keyPrefix = configuration["Redis:InstanceName"] ?? "pos:";
            
            if (redis == null || !redis.IsConnected)
            {
                Log.Warning("Redis not available - using in-memory cache fallback");
                return new Pos.Web.Infrastructure.Services.InMemoryCacheService(logger);
            }
            
            return new Pos.Web.Infrastructure.Services.RedisCacheService(
                redis, 
                logger, 
                keyPrefix, 
                TimeSpan.FromHours(1));
        });

        // Feature Flags
        services.AddMemoryCache();
        services.AddScoped<Pos.Web.Infrastructure.Services.IFeatureFlagService, Pos.Web.Infrastructure.Services.FeatureFlagService>();

        // API Audit Logging
        services.AddScoped<Pos.Web.Infrastructure.Services.IApiAuditLogService, Pos.Web.Infrastructure.Services.ApiAuditLogService>();

        // Business Services
        services.AddScoped<Pos.Web.Infrastructure.Services.IOrderService, Pos.Web.Infrastructure.Services.OrderService>();
        services.AddScoped<Pos.Web.Infrastructure.Services.IProductService, Pos.Web.Infrastructure.Services.ProductService>();
        services.AddScoped<Pos.Web.Infrastructure.Services.ICustomerService, Pos.Web.Infrastructure.Services.CustomerService>();
        services.AddScoped<Pos.Web.Infrastructure.Services.IOrderLockService, Pos.Web.Infrastructure.Services.OrderLockService>();
        services.AddScoped<Pos.Web.Infrastructure.Services.IPaymentService, Pos.Web.Infrastructure.Services.PaymentService>();
        // TODO: Re-enable when KitchenService.cs is properly implemented
        // services.AddScoped<Pos.Web.Infrastructure.Services.IKitchenService, Pos.Web.Infrastructure.Services.KitchenService>();

        // Authentication & Membership Services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenManager, RefreshTokenManager>();
        services.AddScoped<ISessionManager, SessionManager>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IAuditLoggingService, AuditLoggingService>();
        services.AddScoped<IUserMigrationService, UserMigrationService>();
        services.AddScoped<IPasswordHistoryService, PasswordHistoryService>();

        // Rate Limiting
        services.AddMemoryCache();
        services.Configure<AspNetCoreRateLimit.IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.Configure<AspNetCoreRateLimit.IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));
        services.AddInMemoryRateLimiting();
        services.AddSingleton<AspNetCoreRateLimit.IRateLimitConfiguration, AspNetCoreRateLimit.RateLimitConfiguration>();

        // Background Services
        services.AddHostedService<SessionCleanupService>();
        services.AddHostedService<AuditLogArchivalService>();

        // JWT Authentication
        var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
            ?? configuration["Jwt:SecretKey"] 
            ?? throw new InvalidOperationException("JWT SecretKey not configured. Set JWT_SECRET_KEY environment variable or Jwt:SecretKey in configuration.");
        
        // Validate secret key length (minimum 32 characters for 256-bit security)
        if (jwtSecretKey.Length < 32)
        {
            throw new InvalidOperationException("JWT SecretKey must be at least 32 characters (256 bits) for security. Current length: " + jwtSecretKey.Length);
        }
        
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "MyChairPOS.API";
        var jwtAudience = configuration["Jwt:Audience"] ?? "MyChairPOS.Client";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            // Configure JWT for SignalR
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }
                    
                    return Task.CompletedTask;
                }
            };
        });

        // Authorization policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Manager", "Admin"));
            options.AddPolicy("CashierOrAbove", policy => policy.RequireRole("Cashier", "Waiter", "Manager", "Admin"));
        });

        // CORS - Configure with explicit policy name
        services.AddCors(options =>
        {
            options.AddPolicy("AllowBlazorClient", policy =>
            {
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                    ?? new[] { "http://localhost:5055" };
                
                Log.Information("CORS configured with origins: {Origins}", string.Join(", ", allowedOrigins));
                
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials()
                      .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
            });
        });

        // Controllers
        services.AddControllers();
        
        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<Program>();
        
        // Response Compression
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
            options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
        });
        
        // SignalR
        services.AddSignalR(options =>
        {
            // Enable detailed errors only in development (will be set by environment)
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            options.HandshakeTimeout = TimeSpan.FromSeconds(15);
            options.MaximumReceiveMessageSize = 128 * 1024; // 128 KB
        });
        
        // HSTS Configuration (HTTP Strict Transport Security)
        services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(365); // 1 year
        });
        
        // API Explorer for Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Health Checks
        services.AddHealthChecks()
            .AddSqlServer(
                configuration.GetConnectionString("PosDatabase") ?? "",
                name: "database",
                timeout: TimeSpan.FromSeconds(3)
            );
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        // Correlation ID middleware (must be early to track all requests)
        app.UseCorrelationId();

        // Global exception handler (must be early to catch all exceptions)
        app.UseGlobalExceptionHandler();

        // Development-specific middleware
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // CORS must come before HTTPS redirection to handle preflight requests
        app.UseCors("AllowBlazorClient");

        // Rate Limiting (after CORS, before authentication)
        app.UseIpRateLimiting();

        // Response Compression
        app.UseResponseCompression();

        // HTTPS redirection - only enforce in production
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
            app.UseHsts();
        }
        
        // Log CORS configuration
        Log.Information("CORS middleware configured and active");

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Session Activity Middleware (after authentication)
        app.UseSessionActivity();

        // Map controllers
        app.MapControllers();

        // Map SignalR hubs
        app.MapHub<KitchenHub>("/hubs/kitchen");
        app.MapHub<OrderLockHub>("/hubs/orderlock");
        app.MapHub<ServerCommandHub>("/hubs/servercommand");

        // Health check endpoint
        app.MapHealthChecks("/health");

        // Root endpoint
        app.MapGet("/", () => new
        {
            Application = "MyChair POS API",
            Version = "1.0.0",
            Status = "Running",
            Timestamp = DateTime.UtcNow
        });
    }
}
