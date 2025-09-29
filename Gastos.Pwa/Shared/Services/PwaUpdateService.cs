namespace Gastos.Pwa.Shared.Services;

public class PwaUpdateService(IJSRuntime jsRuntime) : IDisposable
{
    private bool _disposed;
    private DotNetObjectReference<PwaUpdateService>? _dotNetRef;

    public event Func<Task>? UpdateAvailable;
    public event Func<Task>? UpdateInstalled;
    public event Func<Task>? UpdateReady;

    public async Task InitializeAsync()
    {
        try
        {
            _dotNetRef ??= DotNetObjectReference.Create(this);
            await jsRuntime.InvokeVoidAsync("pwaUpdater.initialize", _dotNetRef);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing PWA update service: {ex.Message}");
        }
    }

    [JSInvokable]
    public async Task OnUpdateAvailable()
    {
        if (UpdateAvailable != null)
        {
            await UpdateAvailable.Invoke();
        }
    }

    [JSInvokable]
    public async Task OnUpdateInstalled()
    {
        if (UpdateInstalled != null)
        {
            await UpdateInstalled.Invoke();
        }
    }

    [JSInvokable]
    public async Task OnUpdateReady()
    {
        if (UpdateReady != null)
        {
            await UpdateReady.Invoke();
        }
    }

    public async Task<bool> CheckForUpdatesAsync()
    {
        try
        {
            return await jsRuntime.InvokeAsync<bool>("pwaUpdater.checkForUpdates");
        }
        catch
        {
            return false;
        }
    }

    public async Task SkipWaitingAsync()
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("pwaUpdater.skipWaiting");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error skipping waiting: {ex.Message}");
        }
    }

    public async Task ReloadAppAsync()
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("pwaUpdater.reloadApp");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reloading app: {ex.Message}");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _dotNetRef?.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}