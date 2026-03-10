using Blazored.LocalStorage;
using Fluxor;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Polly;
using Polly.Extensions.Http;
using Pos.Web.Client.Services.Api;
using Pos.Web.Client.Services.Authentication;
using Pos.Web.Client.Services.ServerCommand;
using Pos.Web.Client.Services.SignalR;

namespace Pos.Web.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // Configure API base address
            var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;

            // Configure Polly retry policy for transient HTTP errors
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError() // Handles 5xx and 408 errors
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // Handle 429
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff: 2, 4, 8 seconds
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"Retry {retryAttempt} after {timespan.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                    });

            // Configure HttpClient with retry policy for API clients
            builder.Services.AddHttpClient<IOrderApiClient, OrderApiClient>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(retryPolicy);

            builder.Services.AddHttpClient<ICustomerApiClient, CustomerApiClient>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(retryPolicy);

            builder.Services.AddHttpClient<IProductApiClient, ProductApiClient>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(retryPolicy);

            builder.Services.AddHttpClient<IKitchenApiClient, KitchenApiClient>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(retryPolicy);

            builder.Services.AddHttpClient<IPaymentApiClient, PaymentApiClient>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(retryPolicy);

            // Configure default HttpClient for other services
            builder.Services.AddScoped(sp => new HttpClient 
            { 
                BaseAddress = new Uri(apiBaseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            });

            // Add MudBlazor services
            builder.Services.AddMudServices();

            // Add Fluxor state management
            builder.Services.AddFluxor(options =>
            {
                options.ScanAssemblies(typeof(Program).Assembly);
                // Note: Redux DevTools requires Fluxor.Blazor.Web.ReduxDevTools package
                // Install it separately if you want to use Redux DevTools for debugging
            });

            // Add Blazored LocalStorage for offline support
            builder.Services.AddBlazoredLocalStorage();

            // Add Authentication services
            builder.Services.AddScoped<CustomAuthenticationStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
                provider.GetRequiredService<CustomAuthenticationStateProvider>());
            
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
            
            builder.Services.AddAuthorizationCore();

            // Add SignalR services
            builder.Services.AddScoped<ISignalRService, SignalRService>();
            builder.Services.AddScoped<IKitchenHubService, KitchenHubService>();
            builder.Services.AddScoped<IServerCommandService, ServerCommandService>();

            await builder.Build().RunAsync();
        }
    }
}
