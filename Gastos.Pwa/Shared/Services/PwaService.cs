namespace Gastos.Pwa.Shared.Services;

public class PwaService(IJSRuntime jsRuntime)
{
    public async Task<bool> IsInstalledAsync()
    {
        try
        {
            return await jsRuntime.InvokeAsync<bool>("pwaHelper.isInstalled");
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsStandaloneAsync()
    {
        try
        {
            return await jsRuntime.InvokeAsync<bool>("pwaHelper.isStandalone");
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CanInstallAsync()
    {
        try
        {
            return await jsRuntime.InvokeAsync<bool>("pwaHelper.canInstall");
        }
        catch
        {
            return false;
        }
    }
}