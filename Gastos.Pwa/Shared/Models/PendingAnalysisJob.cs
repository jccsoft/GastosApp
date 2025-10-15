using DocIntel.Contracts.Responses.Common;

namespace Gastos.Pwa.Shared.Models;

public sealed class PendingAnalysisJob
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = "";
    public int Status { get; set; }
    public Guid? DocumentId { get; set; }
    public string[] Errors { get; set; } = [];

    public bool IsReceiptAlreadyCreated { get; set; } = false;
    public string ReceiptMerchant { get; set; } = "";
    public DateTime? ReceiptTransactionDate { get; set; }

    public bool CanCreateReceipt => DocumentId is not null && Status == (int)AnalysisJobStatusResponse.Successful && !IsReceiptAlreadyCreated;
    public bool CheckPending
    {
        get =>
        Status == (int)AnalysisJobStatusResponse.Pending ||
        Status == (int)AnalysisJobStatusResponse.InProgress ||
        (Status == (int)AnalysisJobStatusResponse.Successful && (DocumentId is null || string.IsNullOrEmpty(ReceiptMerchant)));
    }
}
