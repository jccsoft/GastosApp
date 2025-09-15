namespace Gastos.Pwa.Shared.Services;

public class ThemeService(IJSRuntime js)
{
    private const string ThemeKey = "blazorTheme";
    private const string SetThemeFunction = "localStorage.setItem";
    private const string GetThemeFunction = "localStorage.getItem";
    private const string CheckDefaultThemeFunction = "window.matchMedia('(prefers-color-scheme: dark)').matches";

    public event Action? ThemeChanged;

    private ThemeMode _currentTheme = ThemeMode.Auto;
    private bool? _systemDarkMode;

    public ThemeMode CurrentTheme
    {
        get => _currentTheme;
        private set
        {
            if (_currentTheme != value)
            {
                _currentTheme = value;
                ThemeChanged?.Invoke();
            }
        }
    }

    public async Task<ThemeMode> GetThemeAsync()
    {
        try
        {
            var themeString = await js.InvokeAsync<string?>(GetThemeFunction, ThemeKey);
            if (Enum.TryParse<ThemeMode>(themeString, out var theme))
            {
                CurrentTheme = theme;
                return theme;
            }
        }
        catch
        {
            // Fall back to default if JS fails
        }

        CurrentTheme = ThemeMode.Auto;
        return ThemeMode.Auto;
    }

    public async Task SetThemeAsync(ThemeMode theme)
    {
        CurrentTheme = theme;
        await js.InvokeVoidAsync(SetThemeFunction, ThemeKey, theme.ToString());
    }

    public bool IsDarkMode(ThemeMode theme)
    {
        return theme switch
        {
            ThemeMode.Dark => true,
            ThemeMode.Light => false,
            ThemeMode.Auto => _systemDarkMode ?? true, // Fallback to dark mode if not detected yet
            _ => false
        };
    }

    public async Task<bool> IsSystemDarkModeAsync()
    {
        try
        {
            _systemDarkMode = await js.InvokeAsync<bool>(CheckDefaultThemeFunction);
            return _systemDarkMode.Value;
        }
        catch
        {
            // Fallback to dark mode if JS call fails
            _systemDarkMode = true;
            return true;
        }
    }

    public async Task<bool> IsDarkModeAsync(ThemeMode theme)
    {
        return theme switch
        {
            ThemeMode.Dark => true,
            ThemeMode.Light => false,
            ThemeMode.Auto => await IsSystemDarkModeAsync(),
            _ => false
        };
    }
}

public enum ThemeMode
{
    Light,
    Dark,
    Auto
}