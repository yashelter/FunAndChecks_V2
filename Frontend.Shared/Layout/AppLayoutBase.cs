using Frontend.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Frontend.Shared.Layout;

/// <summary>
/// Базовый класс для AdminLayout и StudentLayout.
/// Содержит общий lifecycle: подписки на ThemeService / CultureService,
/// инициализацию темы, переключение темы, выход из аккаунта.
/// Устраняет дублирование между двумя layout'ами.
/// </summary>
public abstract class AppLayoutBase : LayoutComponentBase, IDisposable
{
    [Inject] protected ThemeService Theme { get; set; } = null!;
    [Inject] protected CultureService Culture { get; set; } = null!;
    [Inject] protected AuthService Auth { get; set; } = null!;
    [Inject] protected JwtAuthenticationStateProvider AuthState { get; set; } = null!;
    [Inject] protected NavigationManager Nav { get; set; } = null!;

    protected override void OnInitialized()
    {
        Theme.OnThemeChanged += Refresh;
        Culture.OnCultureChanged += Refresh;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await Theme.InitializeAsync();
    }

    protected Task ToggleThemeAsync() => Theme.ToggleThemeAsync();

    protected async Task LogoutAsync()
    {
        await Auth.LogoutAsync();
        AuthState.NotifyAuthenticationStateChanged();
        Nav.NavigateTo("/login");
    }

    private void Refresh() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        Theme.OnThemeChanged -= Refresh;
        Culture.OnCultureChanged -= Refresh;
    }
}
