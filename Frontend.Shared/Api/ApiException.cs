using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Frontend.Shared.Resources;
using Microsoft.Extensions.Localization;

namespace Frontend.Shared.Api;

/// <summary>Ошибка обращения к API с человекочитаемым сообщением из ProblemDetails.</summary>
public class ApiException(HttpStatusCode statusCode, string message, Dictionary<string, string[]>? validationErrors = null) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public Dictionary<string, string[]> ValidationErrors { get; } = validationErrors ?? new();
}

public static class HttpResponseExtensions
{
    private sealed record ProblemDetails(string? Detail, string? Title, Dictionary<string, string[]>? Errors);

    /// <summary>Бросает <see cref="ApiException"/> с сообщением из ProblemDetails, если ответ неуспешен.</summary>
    public static async Task EnsureSuccessAsync(this HttpResponseMessage response, IStringLocalizer<AppStrings> loc)
    {
        if (response.IsSuccessStatusCode)
            return;

        var (message, errors) = await ReadErrorAsync(response, loc);
        throw new ApiException(response.StatusCode, message, errors);
    }

    public static async Task<string> ReadErrorMessageAsync(this HttpResponseMessage response, IStringLocalizer<AppStrings> loc)
    {
        var (message, _) = await ReadErrorAsync(response, loc);
        return message;
    }

    private static async Task<(string Message, Dictionary<string, string[]> Errors)> ReadErrorAsync(
        HttpResponseMessage response, IStringLocalizer<AppStrings> loc)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            var errors = problem?.Errors ?? new Dictionary<string, string[]>();

            if (!string.IsNullOrWhiteSpace(problem?.Detail))
                return (problem!.Detail!, errors);
            if (!string.IsNullOrWhiteSpace(problem?.Title))
                return (problem!.Title!, errors);

            return (GetDefaultMessage(response.StatusCode, loc), errors);
        }
        catch (JsonException) { /* тело не ProblemDetails */ }
        catch (NotSupportedException) { /* не JSON */ }

        return (GetDefaultMessage(response.StatusCode, loc), new Dictionary<string, string[]>());
    }

    private static string GetDefaultMessage(HttpStatusCode statusCode, IStringLocalizer<AppStrings> loc) => statusCode switch
    {
        HttpStatusCode.TooManyRequests => loc["Error_TooManyRequests"],
        HttpStatusCode.Unauthorized => loc["Error_Unauthorized"],
        HttpStatusCode.Forbidden => loc["Error_Forbidden"],
        _ => loc["Error_RequestFailed", (int)statusCode],
    };
}
