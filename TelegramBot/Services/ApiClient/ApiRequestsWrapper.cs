using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FunAndChecks.DTO;
using TelegramBot.Models;
using TelegramBot.Services.StateStorage;

namespace TelegramBot.Services.ApiClient;

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

            session ??= new UserSession() { UserId = userId };

            session.JwtToken = newToken;
            tokenFolder.SaveUserTokenSession(session);
            response = await MakeRequest();
        }

        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Отправляет POST-запрос БЕЗ ТЕЛА (с пустым JSON {}) и автоматическим обновлением токена.
    /// Возвращает true в случае успеха.
    /// </summary>
    public async Task<bool> PostWithAuthAsync(long userId, string requestUri)
    {
        var client = GetHttpClient();
        var session = tokenFolder.GetUserTokenSession(userId);

        async Task<HttpResponseMessage> MakeRequest()
        {
            var tokenSession = tokenFolder.GetUserTokenSession(userId);

            if (tokenSession?.JwtToken == null)
            {
                // Возвращаем ошибку, чтобы запустить логику обновления токена
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenSession.JwtToken);

            // --- КЛЮЧЕВОЕ ИЗМЕНЕНИЕ ---
            // Создаем пустое JSON-тело. Это важно, т.к. многие API ожидают
            // заголовок Content-Type: application/json даже для пустых POST-запросов.
            var emptyContent = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
            return await client.PostAsync(requestUri, emptyContent);
        }

        // --- Дальнейшая логика АБСОЛЮТНО ИДЕНТИЧНА ---
        var response = await MakeRequest();

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            logger.LogInformation("Token expired for user {UserId}. Refreshing...", userId);

            var newToken = await TelegramLoginAsync(userId);
            if (newToken == null)
            {
                logger.LogError("Failed to refresh token for user {UserId}.", userId);
                return false;
            }

            session ??= new UserSession() { UserId = userId };
            session.JwtToken = newToken;
            tokenFolder.SaveUserTokenSession(session);
            response = await MakeRequest();
        }

        if (!response.IsSuccessStatusCode)
        {
            // Добавляем логирование для отладки
            var errorContent = await response.Content.ReadAsStringAsync();
            logger.LogWarning("API call to {uri} failed with status {code}. Reason: {reason}",
                requestUri, response.StatusCode, errorContent);
        }

        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Отправляет POST-запрос БЕЗ аутентификации.
    /// Возвращает true в случае успеха.
    /// </summary>
    /// <typeparam name="TRequest">Тип отправляемого DTO.</typeparam>
    protected async Task<bool> PostWithoutAuthAsync<TRequest>(string requestUri, TRequest data)
        where TRequest : class
    {
        // 1. Получаем HttpClient
        var client = GetHttpClient();

        try
        {
            // 2. Отправляем POST-запрос с данными
            //    Вся логика с токенами и заголовком Authorization удалена.
            var response = await client.PostAsJsonAsync(requestUri, data);

            // 3. Просто возвращаем, был ли ответ успешным (статус 2xx)
            if (!response.IsSuccessStatusCode)
            {
                // Добавляем логирование для отладки неудачных запросов
                var errorContent = await response.Content.ReadAsStringAsync();
                logger.LogWarning("API call to {Uri} failed with status {Code}. Reason: {Reason}",
                    requestUri, response.StatusCode, errorContent);
            }

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            // Логируем ошибки сети или подключения
            logger.LogError(ex, "An HTTP exception occurred while calling {Uri}", requestUri);
            return false;
        }
        catch (Exception ex)
        {
            // Логируем любые другие непредвиденные ошибки
            logger.LogError(ex, "An unexpected exception occurred while calling {Uri}", requestUri);
            return false;
        }
    }


    /// <summary>
    /// Отправляет POST-запрос с автоматическим обновлением токена.
    /// Возвращает десериализованный объект ответа или null в случае ошибки.
    /// </summary>
    /// <typeparam name="TRequest">Тип отправляемого DTO.</typeparam>
    /// <typeparam name="TResponse">Тип ожидаемого DTO в ответе.</typeparam>
    protected async Task<TResponse?> PostWithAuthAsync<TRequest, TResponse>(long userId, string requestUri,
        TRequest data)
        where TRequest : class
        where TResponse : class
    {
        var client = GetHttpClient();

        async Task<HttpResponseMessage> MakeRequest()
        {
            var session = tokenFolder.GetUserTokenSession(userId);
            session ??= new UserSession() { UserId = userId };

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


    /// <summary>
    /// Отправляет Put-запрос с автоматическим обновлением токена.
    /// Возвращает true в случае успеха.
    /// </summary>
    /// <typeparam name="TRequest">Тип отправляемого DTO.</typeparam>
    protected async Task<bool> PutWithAuthAsync<TRequest>(long userId, string requestUri, TRequest data)
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
            return await client.PutAsJsonAsync(requestUri, data);
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

            session ??= new UserSession() { UserId = userId };

            session.JwtToken = newToken;
            tokenFolder.SaveUserTokenSession(session);
            response = await MakeRequest();
        }

        return response.IsSuccessStatusCode;
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