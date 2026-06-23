using Blazored.LocalStorage;
using BlazorFrontend.Auth;
using BlazorFrontend.Auth;
using BlazorFrontend.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BlazorFrontend;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        // Больше никакого хаоса с динамическими портами
        var gatewayUrl = "http://localhost:5000";

        Console.WriteLine($"[DEBUG] ХАРДКОД URL ГЕЙТВЕЯ: {gatewayUrl}");

        builder.Services.AddScoped<AuthHeaderHandler>();
        builder.Services.AddBlazoredLocalStorage();
        builder.Services.AddHttpClient("gateway", client =>
        {
            client.BaseAddress = new Uri(gatewayUrl);
        }).AddHttpMessageHandler<AuthHeaderHandler>();

        builder.Services.AddScoped(sp =>
            sp.GetRequiredService<IHttpClientFactory>().CreateClient("gateway"));

        // Auth
        builder.Services.AddScoped<CustomAuthStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
            sp.GetRequiredService<CustomAuthStateProvider>());
        builder.Services.AddAuthorizationCore();
        builder.Services.AddSingleton<ToastService>();

        await builder.Build().RunAsync();
    }
}
