using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AdminBot.Models;
using FunAndChecks.DTO;

namespace AdminBot.Services.ApiClient;

public class ApiRequestsWrapper(
    IConfiguration configuration,
    ILogger<ApiRequestsWrapper> logger,
    ITokenService tokenFolder,
    Func<HttpClient> getHttpClient)
{
    protected Func<HttpClient> GetHttpClient = getHttpClient;
    
    /// <summary>
    /// Отправляет POST-запрос с автоматическим обновлением токена.
    /// Возвращает true в случае успеха.
    /// </summary>
    /// <typeparam name="TRequest">Тип отправляемого DTO.</typeparam>
    protected async Task<bool> PostWithAuthAsync<TRequest>(long userId, string requestUri, TRequest data)
        where TRequest : class
    {
        var client = GetHttpClient();
        var session = tokenFolder.GetUserTokenSession(userId);
        
        async Task<HttpResponseMessage> MakeRequest()
        {
            var tokenSession = tokenFolder.GetUserTokenSession(userId);

            if (tokenSession?.JwtToken == null)
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenSession.JwtToken);
            return await client.PostAsJsonAsync(requestUri, data);
        }

        var response = await MakeRequest();

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            logger.LogInformation("Token expired for user {UserId}. Refreshing...", userId);

            var newToken = await TelegramLoginAsync(userId);
            if (newToken == null)
            {
                logger.LogError("Failed to refresh token for user {UserId}.", userId);
                return false;
            }

            session ??= new UserSession() {UserId = userId};
            
            session.JwtToken = newToken;
            tokenFolder.SaveUserTokenSession(session);
            response = await MakeRequest();
        }
        return response.IsSuccessStatusCode;
    }


    /// <summary>
    /// Отправляет POST-запрос с автоматическим обновлением токена.
    /// Возвращает десериализованный объект ответа или null в случае ошибки.
    /// </summary>
    /// <typeparam name="TRequest">Тип отправляемого DTO.</typeparam>
    /// <typeparam name="TResponse">Тип ожидаемого DTO в ответе.</typeparam>
    protected async Task<TResponse?> PostWithAuthAsync<TRequest, TResponse>(long userId, string requestUri, TRequest data)
        where TRequest : class
        where TResponse : class
    {
        var client = GetHttpClient();

        async Task<HttpResponseMessage> MakeRequest()
        {
            var session = tokenFolder.GetUserTokenSession(userId);
            session ??= new UserSession() {UserId = userId};
            
            if (session.JwtToken == null)
            {
                var newToken = await TelegramLoginAsync(userId);
                if (newToken == null)
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
                tokenFolder.SaveUserTokenSession(new UserSession { UserId = userId, JwtToken = newToken });
            }
            
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.JwtToken);
            return await client.PostAsJsonAsync(requestUri, data);
        }

        var response = await MakeRequest();

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            logger.LogInformation("Token expired for user {UserId}. Refreshing...", userId);
            var newToken = await TelegramLoginAsync(userId);
            if (newToken == null)
            {
                logger.LogError("Failed to refresh token for user {UserId}.", userId);
                return null;
            }

            tokenFolder.SaveUserTokenSession(new UserSession { UserId = userId, JwtToken = newToken });
            response = await MakeRequest();
        }

        if (response.IsSuccessStatusCode)
        {
            try
            {
                return await response.Content.ReadFromJsonAsync<TResponse>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to deserialize response content to type {Type}", typeof(TResponse).Name);
                return null;
            }
        }

        logger.LogWarning("POST request to {Uri} failed with status code {Code}", requestUri, response.StatusCode);
        return null;
    }

    protected async Task<T?> GetWithAuthAsync<T>(long userId, string requestUri) where T : class
    {
        var client = GetHttpClient();

        async Task<HttpResponseMessage> MakeRequest()
        {
            var session = tokenFolder.GetUserTokenSession(userId);
            if (session?.JwtToken == null)
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.JwtToken);
            return await client.GetAsync(requestUri);
        }

        var response = await MakeRequest();

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            logger.LogInformation("Token expired for user {UserId}. Refreshing...", userId);

            var newToken = await TelegramLoginAsync(userId);
            if (newToken == null)
            {
                logger.LogError("Failed to refresh token for user {UserId}.", userId);
                return null;
            }

            tokenFolder.SaveUserTokenSession(new UserSession { UserId = userId, JwtToken = newToken });
            response = await MakeRequest();
        }

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<T>();
        }

        return null;
    }
    
    protected async Task<T?> GetAsync<T>(string requestUri) where T : class
    {
        var client = GetHttpClient();
        try
        {
            var response = await client.GetFromJsonAsync<T>(requestUri);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred in method {MethodName}",
                System.Reflection.MethodBase.GetCurrentMethod()?.Name);
            return null;
        }
    }
    
    public async Task<string?> TelegramLoginAsync(long telegramId)
    {
        var client = GetHttpClient();
        var secret = configuration["ApiConfiguration:SharedSecret"];
        if (secret is null)
        {
            logger.LogError("ApiConfiguration:SharedSecret is missing.");
            return null;
        }
        
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
            logger.LogError(ex, "An exception occurred during Telegram login.");
            return null;
        }
    }


    private static string GenerateAuthHash(long telegramId, string secret)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret));
        var computedHashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(telegramId.ToString()));
        return Convert.ToBase64String(computedHashBytes);
    }
}