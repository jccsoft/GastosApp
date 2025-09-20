namespace Gastos.Pwa.Shared.Services;

public interface INetworkStatusService
{
    event Action<bool> NetworkStatusChanged;
    Task<bool> IsOnlineAsync();
    ValueTask DisposeAsync();
}

public class NetworkStatusService(IJSRuntime jsRuntime) : INetworkStatusService, IAsyncDisposable
{
    private IJSObjectReference? _module;
    private DotNetObjectReference<NetworkStatusService>? _dotNetRef;

    public event Action<bool>? NetworkStatusChanged;

    public async Task InitializeAsync()
    {
        _module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/network-status.js");
        _dotNetRef = DotNetObjectReference.Create(this);
        await _module.InvokeVoidAsync("initialize", _dotNetRef);
    }

    public async Task<bool> IsOnlineAsync()
    {
        if (_module is null)
            await InitializeAsync();

        return await _module!.InvokeAsync<bool>("isOnline");
    }

    [JSInvokable]
    public void OnNetworkStatusChanged(bool isOnline)
    {
        NetworkStatusChanged?.Invoke(isOnline);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "This class does not have a finalizer")]
    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
        _dotNetRef?.Dispose();
    }
}