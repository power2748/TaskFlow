using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage; // <-- ИСПОЛЬЗУЕМ БИБЛИОТЕКУ ТУТ

namespace BlazorFrontend.Auth;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage; // <-- Меняем IJSRuntime на сервис библиотеки

    public CustomAuthStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Читаем через библиотеку
        var token = await _localStorage.GetItemAsync<string>("jwt");

        if (string.IsNullOrEmpty(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task SetTokenAsync(string token)
    {
        // Пишем через библиотеку! Она сама добавит нужные кавычки для корректного чтения
        await _localStorage.SetItemAsync("jwt", token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task ClearTokenAsync()
    {
        // Удаляем через библиотеку
        await _localStorage.RemoveItemAsync("jwt");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        try
        {
            var payload = jwt.Split('.')[1];
            var json = System.Text.Encoding.UTF8.GetString(
                Convert.FromBase64String(PadBase64(payload)));

            using var doc = JsonDocument.Parse(json);
            var claims = new List<Claim>();

            foreach (var element in doc.RootElement.EnumerateObject())
            {
                if (element.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in element.Value.EnumerateArray())
                    {
                        claims.Add(new Claim(element.Name, item.ToString()));
                    }
                }
                else
                {
                    claims.Add(new Claim(element.Name, element.Value.ToString()));
                }
            }
            return claims;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[JWT Error]: {ex.Message}");
            return Array.Empty<Claim>();
        }
    }

    private string PadBase64(string base64)
    {
        return base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
    }
}