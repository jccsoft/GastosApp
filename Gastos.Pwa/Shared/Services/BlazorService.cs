using System.Text.Json;
using static Gastos.Shared.Resources.LocalizationConstants;

namespace Gastos.Pwa.Shared.Services;

public class BlazorService(
    IDialogService dialogService,
    IJSRuntime js,
    ISnackbar snackbar,
    LocalizationService loc,
    ILogger<BlazorService> logger)
{

    #region PERSISTENCE
    public async Task<T?> RestoreDataFromLocalStorageAsync<T>(string localStorageKey)
    {
        try
        {
            var json = await js.InvokeAsync<string?>("localStorage.getItem", localStorageKey);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<T>(json);
            }
        }
        catch (Exception ex)
        {
            // Si falla la deserialización, simplemente ignoramos y continuamos
            Console.WriteLine($"Error restoring data from localStorage: {ex.Message}");
        }

        return default!;
    }

    public async Task PersistDataToLocalStorageAsync<T>(string localStorageKey, T? data)
    {
        if (data is not null)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                await js.InvokeVoidAsync("localStorage.setItem", localStorageKey, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error persisting data to localStorage: {ex.Message}");
            }
        }
    }

    public async Task ClearLocalStorageAsync(string localStorageKey)
    {
        try
        {
            await js.InvokeVoidAsync("localStorage.removeItem", localStorageKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing localStorage: {ex.Message}");
        }
    }
    #endregion


    #region ERROR MESSAGES
    public string GetResponseError<T>(Refit.ApiResponse<T> response)
    {
        logger.LogError("API Error: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);

        return response.StatusCode switch
        {
            System.Net.HttpStatusCode.BadRequest => loc.Get(RS.ErrorStatusCodeBadRequest),
            System.Net.HttpStatusCode.Conflict => loc.Get(RS.ErrorStatusCodeConflict),
            System.Net.HttpStatusCode.NotFound => loc.Get(RS.ErrorStatusCodeNotFound),
            System.Net.HttpStatusCode.Unauthorized => loc.Get(RS.ErrorStatusCodeUnauthorized),
            _ => loc.Get(RS.ErrorUnknown)
        };
    }

    public string GetResponseError(Exception ex)
    {
        logger.LogError(ex, "API Exception");

        return ex switch
        {
            TaskCanceledException => loc.Get(RS.ErrorRequestTimedOut),
            HttpRequestException httpEx when httpEx.InnerException is System.Net.Sockets.SocketException => loc.Get(RS.ErrorNetwork),
            _ => loc.Get(RS.ErrorUnknown)
        };
    }

    public void ShowResponseError<T>(Refit.ApiResponse<T> response)
    {
        snackbar.Add(GetResponseError(response), Severity.Error);
    }

    #endregion


    #region DIALOGS
    public async Task<bool> ConfirmDeletionAsync(string question)
    {
        var result = await dialogService.ShowMessageBox(
            title: "",
            message: question,
            yesText: loc.Get(RS.ActDelete),
            cancelText: loc.Get(RS.ActCancel),
            options: new DialogOptions() { MaxWidth = MaxWidth.ExtraSmall });

        return result is true;
    }

    public async Task<DialogResult?> OpenCreateReceiptDialogAsync(Guid? id, string filename)
    {
        var parameters = new DialogParameters
        {
            ["DocIntelReceiptId"] = id,
            ["Filename"] = filename
        };
        var dialog = await dialogService.ShowAsync<ReceiptCreateFromDocIntelDialog>($"{loc.Get(RS.ActAdd)} {loc.Get(RS.EntityReceipt)}", parameters);
        var result = await dialog.Result;

        return result;
    }

    public async Task<DialogResult?> OpenEditReceiptDialogAsync(Guid id)
    {
        var dialogParameters = new DialogParameters { ["ReceiptId"] = id };
        var dialog = await dialogService.ShowAsync<ReceiptEditDialog>($"{loc.Get(RS.ActEdit)} {loc.Get(RS.EntityReceipt)}", dialogParameters, new DialogOptions() { MaxWidth = MaxWidth.Small });
        var result = await dialog.Result;

        return result;
    }

    public async Task<DialogResult?> OpenEditReceiptItemDialogAsync(ReceiptItemDto? receiptItem)
    {
        var dialogParameters = new DialogParameters { ["Item"] = receiptItem };
        var dialog = await dialogService.ShowAsync<ReceiptItemEditDialog>($"{(receiptItem is null ? loc.Get(RS.ActAdd) : loc.Get(RS.ActEdit))} Item", dialogParameters, new DialogOptions() { MaxWidth = MaxWidth.Small });
        var result = await dialog.Result;

        return result;
    }

    public async Task<DialogResult?> OpenInfoReceiptDialogAsync(ReceiptDto receipt)
    {
        var dialogParameters = new DialogParameters { ["Receipt"] = receipt };
        var dialog = await dialogService.ShowAsync<ReceiptInfo>($"Info {loc.Get(RS.EntityReceipt)}", dialogParameters, new DialogOptions() { MaxWidth = MaxWidth.Small });
        var result = await dialog.Result;

        return result;
    }



    public async Task<DialogResult?> OpenAddProductDialogAsync(string initialName = "")
    {
        var dialogParameters = new DialogParameters { ["InitialName"] = initialName };
        var dialog = await dialogService.ShowAsync<ProductEditDialog>($"{loc.Get(RS.ActAdd)} {loc.Get(RS.EntityProduct)}", dialogParameters, new DialogOptions() { MaxWidth = MaxWidth.ExtraSmall });
        var result = await dialog.Result;

        return result;
    }
    public async Task<DialogResult?> OpenEditProductDialogAsync(Guid id)
    {
        var dialogParameters = new DialogParameters { ["ProductId"] = id };
        var dialog = await dialogService.ShowAsync<ProductEditDialog>($"{loc.Get(RS.ActEdit)} {loc.Get(RS.EntityProduct)}", dialogParameters, new DialogOptions() { MaxWidth = MaxWidth.ExtraSmall });
        var result = await dialog.Result;

        return result;
    }



    public async Task<DialogResult?> OpenAddStoreDialogAsync(string initialName = "")
    {
        var dialogParameters = new DialogParameters { ["InitialName"] = initialName };
        var dialog = await dialogService.ShowAsync<StoreEditDialog>($"{loc.Get(RS.ActAdd)} {loc.Get(RS.EntityStore)}", dialogParameters, new DialogOptions() { MaxWidth = MaxWidth.ExtraSmall });
        var result = await dialog.Result;

        return result;
    }

    public async Task<DialogResult?> OpenEditStoreDialogAsync(Guid id)
    {
        var dialogParameters = new DialogParameters { ["StoreId"] = id };
        var dialog = await dialogService.ShowAsync<StoreEditDialog>($"{loc.Get(RS.ActEdit)} {loc.Get(RS.EntityStore)}", dialogParameters, new DialogOptions() { MaxWidth = MaxWidth.ExtraSmall });
        var result = await dialog.Result;

        return result;
    }
    #endregion



    #region COPY TEXT
    public async Task CopyText(string textToCopy)
    {
        await js.InvokeVoidAsync("clipboardHelper.copyText",
                                 DotNetObjectReference.Create(this),
                                 nameof(CopyTextFinished),
                                 textToCopy);
    }

    [JSInvokable]
    public void CopyTextFinished(bool success)
    {
        if (success)
        {
            snackbar.Add(loc.Get(RS.TextCopiedToClipboard), Severity.Success);
        }
        else
        {
            snackbar.Add(loc.Get(RS.ErrorCopyingText), Severity.Error);
        }
    }
    #endregion



    #region OTHER HELPERS

#pragma warning disable S1104 // Fields should not have public accessibility
    public Converter<decimal?> DecimalConverter = new()
    {
        SetFunc = value => value?.ToString() ?? string.Empty,
        GetFunc = str =>
        {
            var decimalSep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            var groupSep = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
            return str is not null && decimal.TryParse(str.Replace(groupSep, decimalSep), out var val) ? val : null;
        }
    };
#pragma warning restore S1104 // Fields should not have public accessibility


    public async Task BrowserLogAsync(string message)
    {
        await js.InvokeVoidAsync("console.log", message);
    }

    public async Task SetCultureAsync(string? culture)
    {
        if (!string.IsNullOrEmpty(culture) && culture != CultureInfo.CurrentCulture.Name)
        {
            await js.InvokeVoidAsync(SetCultureFunction, culture);
        }
    }
    #endregion

}
