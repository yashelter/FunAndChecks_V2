using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Frontend.Shared.Services;

/// <summary>
/// Хранит и переключает язык интерфейса, запоминая выбор в localStorage.
/// По образцу <see cref="ThemeService"/>.
/// </summary>
public class CultureService(IJSRuntime js, NavigationManager nav)
{
    private const string Key = "culture_preference";

    public string CurrentCulture { get; private set; } = "en-US";

    public event Action? OnCultureChanged;

    /// <summary>
    /// Инициализирует культуру из localStorage; если не сохранена —
    /// определяет по языку браузера (navigator.language).
    /// Вызывается из Program.cs перед RunAsync.
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            var stored = await js.InvokeAsync<string?>("localStorage.getItem", Key);
            if (stored is "en-US" or "ru-RU")
            {
                CurrentCulture = stored;
            }
            else
            {
                var lang = await js.InvokeAsync<string>("eval", "navigator.language");
                CurrentCulture = lang.StartsWith("ru", StringComparison.OrdinalIgnoreCase)
                    ? "ru-RU"
                    : "en-US";
            }
        }
        catch
        {
            CurrentCulture = "en-US";
        }

        OnCultureChanged?.Invoke();
    }

    /// <summary>
    /// Сохраняет выбранную культуру в localStorage и перезагружает страницу
    /// (forceLoad необходим для применения новой культуры в WASM-рантайме).
    /// </summary>
    public async Task SetCultureAsync(string culture)
    {
        await js.InvokeVoidAsync("localStorage.setItem", Key, culture);
        nav.NavigateTo(nav.Uri, forceLoad: true);
    }
}
