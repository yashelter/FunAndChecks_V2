namespace AdminUI.Services;

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;


public class AuthHeaderHandler : DelegatingHandler
{
    private readonly IServiceProvider _serviceProvider;

    // Инжектируем IServiceProvider вместо AuthService
    public AuthHeaderHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Получаем AuthService прямо здесь, перед отправкой запроса.
        // Используем CreateScope для корректной работы с Scoped сервисами.
        await using var scope = _serviceProvider.CreateAsyncScope();
        var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
        
        var token = await authService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}