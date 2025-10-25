using DocIntel.Contracts.Responses;
using DocIntel.Contracts.Responses.Common;
using Gastos.Shared.Extensions;

namespace Gastos.Pwa.Shared.Models;

public sealed class PendingAnalysisJob(Guid id, string fileName, DateTime createdAtUtc, DateTime? completedAtUtc, int status, Guid? documentId, string[] errors)
{
    public Guid Id { get; init; } = id;
    public string FileName { get; init; } = fileName;
    public DateTime CreatedAtUtc { get; init; } = createdAtUtc;
    public DateTime? CompletedAtUtc { get; set; } = completedAtUtc;
    public int Status { get; set; } = status;
    public Guid? DocumentId { get; set; } = documentId;
    public string[] Errors { get; set; } = errors;

    public bool IsReceiptAlreadyCreated { get; set; } = false;
    public bool IsReceiptAlreadyPending { get; set; } = false;
    public string? ReceiptMerchant { get; set; }
    public DateTime? ReceiptTransactionDate { get; set; }

    public bool CheckPending
    {
        get =>
        Status == (int)AnalysisJobStatusResponse.Pending ||
        Status == (int)AnalysisJobStatusResponse.InProgress ||
        (Status == (int)AnalysisJobStatusResponse.Successful && (DocumentId is null || string.IsNullOrEmpty(ReceiptMerchant)));
    }

    public void UpdateJobInfo(AnalysisJobResponse analysisJobResponse)
    {
        if (analysisJobResponse is null) return;

        CompletedAtUtc = analysisJobResponse.CompletedAtUtc;
        Status = analysisJobResponse.Status;
        DocumentId = analysisJobResponse.DocumentId;
        Errors = analysisJobResponse.Errors ?? [];
    }

    public void UpdateReceiptInfo(ReceiptResponse receiptResponse)
    {
        if (receiptResponse is null) return;

        ReceiptMerchant = receiptResponse.Merchant.GetFirstWord();
        ReceiptTransactionDate = receiptResponse.TransactionDateUtc;
    }
}
