namespace Gastos.Api.Features.Receipts;

public sealed class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
{
    public void Configure(EntityTypeBuilder<Receipt> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.UserId);

        builder.Property(r => r.TransactionDateUtc);

        builder.Property(r => r.SourceId);
        builder.HasIndex(r => new { r.UserId, r.SourceId }).IsUnique();

        builder.Property(r => r.Discount).HasDefaultValue(0);

        builder.HasOne(r => r.Store)
            .WithMany(s => s.Receipts)
            .HasForeignKey(r => r.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Items)
            .WithOne(ri => ri.Receipt)
            .HasForeignKey(ri => ri.ReceiptId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}