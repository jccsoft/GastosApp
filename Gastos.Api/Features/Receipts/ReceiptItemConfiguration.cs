namespace Gastos.Api.Features.Receipts;

public sealed class ReceiptItemConfiguration : IEntityTypeConfiguration<ReceiptItem>
{
    public void Configure(EntityTypeBuilder<ReceiptItem> builder)
    {
        builder.HasKey(ri => ri.Id);

        builder.Property(ri => ri.Quantity);
        builder.Property(ri => ri.Amount);
        builder.Property(ri => ri.SourceDescription);

        // Use navigation properties for relationships
        builder.HasOne(ri => ri.Receipt)
            .WithMany(r => r.Items)
            .HasForeignKey(ri => ri.ReceiptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ri => ri.Product)
            .WithMany(p => p.ReceiptItems)
            .HasForeignKey(ri => ri.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
