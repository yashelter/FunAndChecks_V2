namespace AdminUI.Services;

using Microsoft.JSInterop;
using MudBlazor;

using Microsoft.JSInterop;


public class ThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private const string ThemePreferenceKey = "theme_preference";

    public bool IsDarkMode { get; set; }
    public event Action OnThemeChanged;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var preference = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", ThemePreferenceKey);
            IsDarkMode = !string.IsNullOrEmpty(preference) && bool.Parse(preference);
        }
        catch { IsDarkMode = false; }
        
        // Уведомляем подписчиков о первоначальной теме
        OnThemeChanged?.Invoke();
    }

    public async Task ToggleThemeAsync()
    {
        IsDarkMode = !IsDarkMode;
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", ThemePreferenceKey, IsDarkMode.ToString());
        OnThemeChanged?.Invoke();
    }
}