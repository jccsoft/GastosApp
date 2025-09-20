using static Gastos.Shared.Resources.LocalizationConstants;

namespace Gastos.Pwa.Shared.Services;

public class BlazorService(
    IDialogService dialogService,
    IJSRuntime js,
    ISnackbar Snackbar,
    LocalizationService Loc)
{
    public async Task<bool> ConfirmDeletionAsync(string question)
    {
        var result = await dialogService.ShowMessageBox(
            title: "",
            message: question,
            yesText: Loc.Get(RS.ActDelete),
            cancelText: Loc.Get(RS.ActCancel),
            options: new DialogOptions() { MaxWidth = MaxWidth.ExtraSmall });

        return result is true;
    }

    public string GetResponseError<T>(Refit.ApiResponse<T> response)
    {
        return response.StatusCode switch
        {
            System.Net.HttpStatusCode.BadRequest => Loc.Get(RS.ErrorStatusCodeBadRequest),
            System.Net.HttpStatusCode.Conflict => Loc.Get(RS.ErrorStatusCodeConflict),
            System.Net.HttpStatusCode.NotFound => Loc.Get(RS.ErrorStatusCodeNotFound),
            System.Net.HttpStatusCode.Unauthorized => Loc.Get(RS.ErrorStatusCodeUnauthorized),
            _ => Loc.Get(RS.ErrorUnknown)
        };
    }

    public void ShowResponseError<T>(Refit.ApiResponse<T> response)
    {
        Snackbar.Add(GetResponseError(response), Severity.Error);
    }

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

    #region DIALOGS
    public async Task<DialogResult?> OpenCreateReceiptDialogAsync(Guid? id, string filename)
    {
        var parameters = new DialogParameters
        {
            ["DocIntelReceiptId"] = id,
            ["Filename"] = filename
        };
        var dialog = await dialogService.ShowAsync<ReceiptCreateFromDocIntelDialog>($"{Loc.Get(RS.ActCreate)} {Loc.Get(RS.EntityReceipt)}", parameters);
        var result = await dialog.Result;

        return result;
    }

    public async Task<DialogResult?> OpenEditReceiptDialogAsync(Guid id)
    {
        var dialogParameters = new DialogParameters { ["ReceiptId"] = id };
        var dialog = await dialogService.ShowAsync<ReceiptEditDialog>($"{Loc.Get(RS.ActEdit)} {Loc.Get(RS.EntityReceipt)}", dialogParameters, new DialogOptions() { MaxWidth = MaxWidth.Medium });
        var result = await dialog.Result;

        return result;
    }

    public async Task<DialogResult?> OpenEditReceiptItemDialogAsync(ReceiptItemDto receiptItem)
    {
        var dialogParameters = new DialogParameters { ["Item"] = receiptItem };
        var dialog = await dialogService.ShowAsync<ReceiptItemEditDialog>($"{Loc.Get(RS.ActEdit)} Item", dialogParameters, new DialogOptions() { MaxWidth = MaxWidth.Small });
        var result = await dialog.Result;

        return result;
    }

    public async Task<DialogResult?> OpenInfoReceiptDialogAsync(ReceiptDto receipt)
    {
        var dialogParameters = new DialogParameters { ["Receipt"] = receipt };
        var dialog = await dialogService.ShowAsync<ReceiptInfo>($"Info {Loc.Get(RS.EntityReceipt)}", dialogParameters, new DialogOptions() { MaxWidth = MaxWidth.Small });
        var result = await dialog.Result;

        return result;
    }



    public async Task<DialogResult?> OpenAddProductDialogAsync(string initialName = "")
    {
        var dialogParameters = new DialogParameters { ["InitialName"] = initialName };
        var dialog = await dialogService.ShowAsync<ProductEditDialog>($"{Loc.Get(RS.ActCreate)} {Loc.Get(RS.EntityProduct)}", dialogParameters, new DialogOptions() { MaxWidth = MaxWidth.ExtraSmall });
        var result = await dialog.Result;

        return result;
    }
    public async Task<DialogResult?> OpenEditProductDialogAsync(Guid id)
    {
        var dialogParameters = new DialogParameters { ["ProductId"] = id };
        var dialog = await dialogService.ShowAsync<ProductEditDialog>($"{Loc.Get(RS.ActEdit)} {Loc.Get(RS.EntityProduct)}", dialogParameters, new DialogOptions() { MaxWidth = MaxWidth.ExtraSmall });
        var result = await dialog.Result;

        return result;
    }



    public async Task<DialogResult?> OpenAddStoreDialogAsync(string initialName = "")
    {
        var dialogParameters = new DialogParameters { ["InitialName"] = initialName };
        var dialog = await dialogService.ShowAsync<StoreEditDialog>($"{Loc.Get(RS.ActCreate)} {Loc.Get(RS.EntityStore)}", dialogParameters, new DialogOptions() { MaxWidth = MaxWidth.ExtraSmall });
        var result = await dialog.Result;

        return result;
    }

    public async Task<DialogResult?> OpenEditStoreDialogAsync(Guid id)
    {
        var dialogParameters = new DialogParameters { ["StoreId"] = id };
        var dialog = await dialogService.ShowAsync<StoreEditDialog>($"{Loc.Get(RS.ActEdit)} {Loc.Get(RS.EntityStore)}", dialogParameters, new DialogOptions() { MaxWidth = MaxWidth.ExtraSmall });
        var result = await dialog.Result;

        return result;
    }
    #endregion


    #region COPY TEXT
    public async Task CopyText(string textToCopy)
    {
        await js.InvokeVoidAsync("clipboardCopy.copyText",
                                 DotNetObjectReference.Create(this),
                                 nameof(CopyTextFinished),
                                 textToCopy);
    }

    [JSInvokable]
    public void CopyTextFinished(bool success)
    {
        if (success)
        {
            Snackbar.Add(Loc.Get(RS.TextCopiedToClipboard), Severity.Success);
        }
        else
        {
            Snackbar.Add(Loc.Get(RS.ErrorCopyingText), Severity.Error);
        }
    }
    #endregion

}
