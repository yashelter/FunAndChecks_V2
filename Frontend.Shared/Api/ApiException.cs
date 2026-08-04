using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Frontend.Shared.Resources;
using Microsoft.Extensions.Localization;

namespace Frontend.Shared.Api;

/// <summary>Ошибка обращения к API с человекочитаемым сообщением из ProblemDetails.</summary>
public class ApiException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}

public static class HttpResponseExtensions
{
    private sealed record ProblemDetails(string? Detail, string? Title);

    /// <summary>Бросает <see cref="ApiException"/> с сообщением из ProblemDetails, если ответ неуспешен.</summary>
    public static async Task EnsureSuccessAsync(this HttpResponseMessage response, IStringLocalizer<AppStrings> loc)
    {
        if (response.IsSuccessStatusCode)
            return;

        throw new ApiException(response.StatusCode, await ReadErrorMessageAsync(response, loc));
    }

    public static async Task<string> ReadErrorMessageAsync(this HttpResponseMessage response, IStringLocalizer<AppStrings> loc)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            if (!string.IsNullOrWhiteSpace(problem?.Detail))
                return problem!.Detail!;
            if (!string.IsNullOrWhiteSpace(problem?.Title))
                return problem!.Title!;
        }
        catch (JsonException) { /* тело не ProblemDetails */ }
        catch (NotSupportedException) { /* не JSON */ }

        return response.StatusCode switch
        {
            HttpStatusCode.TooManyRequests => loc["Error_TooManyRequests"],
            HttpStatusCode.Unauthorized => loc["Error_Unauthorized"],
            HttpStatusCode.Forbidden => loc["Error_Forbidden"],
            _ => loc["Error_RequestFailed", (int)response.StatusCode],
        };
    }
}
