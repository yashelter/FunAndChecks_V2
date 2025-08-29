using FunAndChecks.DTO;
using TelegramBot.Models;

namespace TelegramBot.Services;

using System.Net.Http.Headers;
using System.Net.Http.Json;


public class ApiClient : IApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiClient> _logger;
    private readonly BotStateService _botStateService;

    public ApiClient(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ApiClient> logger,  BotStateService botStateService)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _botStateService = botStateService;
    }
    
    private async Task<T?> GetWithAuthAsync<T>(long userId, string requestUri) where T : class
    {
        var client = _httpClientFactory.CreateClient("ApiV1");

        async Task<HttpResponseMessage> MakeRequest()
        {
            var session = _botStateService.GetUserSession(userId);
            if (session?.JwtToken == null)
            {
                throw new InvalidOperationException("User is not logged in.");
            }
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.JwtToken);
            return await client.GetAsync(requestUri);
        }

        var response = await MakeRequest();

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogInformation("Token expired for user {userId}. Refreshing...", userId);
        
            var newToken = await TelegramLoginAsync(userId);
            if (newToken == null)
            {
                _logger.LogError("Failed to refresh token for user {userId}.", userId);
                return null;
            } 

            _botStateService.SaveUserSession(new UserSession { UserId = userId, JwtToken = newToken });
            response = await MakeRequest();
        }

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<T>();
        }

        return null;
    }
    
    public async Task<UserInfoDto?> GetMyInfoAsync(long userId)
    {
        return await GetWithAuthAsync<UserInfoDto>(userId, "/api/users/me");
    }

    
    public async Task<string?> LoginAsync(string username, string password)
    {
        var client = _httpClientFactory.CreateClient("ApiV1");
        var loginDto = new LoginUserDto(username, password);

        try
        {
            var response = await client.PostAsJsonAsync("/api/auth/login", loginDto);
            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                return authResponse?.Token;
            }
            else
            {
                _logger.LogWarning("Login failed. Status code: {StatusCode}", response.StatusCode);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while trying to log in.");
            return null;
        }
    }

    public async Task<List<GroupDto>?> GetAllGroups()
    {
        var client = _httpClientFactory.CreateClient("ApiV1");

        try
        {
            var groups = await client.GetFromJsonAsync<List<GroupDto>>("/api/public/groups");
            return groups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred in method {MethodName}", 
                System.Reflection.MethodBase.GetCurrentMethod()?.Name);
            return null;
        }
    }

    
    public async Task<string?> TelegramLoginAsync(long telegramId)
    {
        var client = _httpClientFactory.CreateClient("ApiV1");
        var secret = _configuration["ApiConfiguration:SharedSecret"];
        var hash = GenerateAuthHash(telegramId, secret);
        var dto = new TelegramLoginDto(telegramId, hash);

        try
        {
            var response = await client.PostAsJsonAsync("/api/auth/telegram-login", dto);
            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                return authResponse?.Token;
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred during Telegram login.");
            return null;
        }
    }
    
    
    public async Task<string?> RegisterAsync
    (
        string firstName, 
        string lastName, 
        string telegramUsername,
        string email,
        string password,    
        int groupId,
        long telegramUserId
        )
    {
        var client = _httpClientFactory.CreateClient("ApiV1");
        var secret = _configuration["ApiConfiguration:SharedSecret"];
        var hash = GenerateAuthHash(telegramUserId, secret);
        var dto = new RegisterUserDto(firstName, lastName,  email,  password, groupId,
            telegramUsername, telegramUserId, null);

        try
        {
            var response = await client.PostAsJsonAsync("/api/auth/telegram-login", dto);
            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                return authResponse?.Token;
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred during Telegram login.");
            return null;
        }
    }
    
    public async Task<(bool IsSuccess, string? ErrorMessage)> RegisterUserAsync(RegisterUserDto registrationDto)
    {
        var client = _httpClientFactory.CreateClient("ApiV1");

        try
        {
            var response = await client.PostAsJsonAsync("/api/auth/register", registrationDto);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Registration failed. Status code: {StatusCode}, Response: {ErrorContent}", 
                    response.StatusCode, errorContent);
                return (false, errorContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred during user registration.");
            return (false, "Произошла непредвиденная ошибка при подключении к сервису.");
        }
    }

    public async Task<bool> LinkTelegramAccountAsync(long telegramId, string userToken)
    {
        var client = _httpClientFactory.CreateClient("ApiV1");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var response = await client.PutAsJsonAsync("/api/users/me/link-telegram", new { TelegramId = telegramId });

        return response.IsSuccessStatusCode;
    }

    private string GenerateAuthHash(long telegramId, string secret)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret));
        var computedHashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(telegramId.ToString()));
        return Convert.ToBase64String(computedHashBytes);
    }
}