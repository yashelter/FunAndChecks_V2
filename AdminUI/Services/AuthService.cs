namespace AdminUI.Services;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private const string TokenKey = "jwt_token";

    public AuthService(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var loginRequest = new { telegramUsername = username, password = password };
        var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", loginRequest);

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (authResponse?.Token == null)
        {
            return false;
        }

        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, authResponse.Token);
        return true;
    }

    public async Task LogoutAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
    }

    public async Task<string> GetTokenAsync()
    {
        return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", TokenKey);
    }
}

public class AuthResponse
{
    public string Token { get; set; }
}