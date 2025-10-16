namespace Gastos.Api.Features.Products;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.UserId);

        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();

        builder.HasIndex(p => new { p.UserId, p.Name, p.UnitsPack, p.SizingId, p.SizingValue }).IsUnique();

        builder.Property(p => p.ImageUrl).HasMaxLength(2048);

        builder.Property(p => p.UnitsPack).IsRequired(true);
        builder.Property(p => p.SizingId).IsRequired(false);
        builder.Property(p => p.SizingValue).IsRequired(false);

        builder.HasOne(p => p.Sizing)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.SizingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.ReceiptItems)
            .WithOne(ri => ri.Product)
            .HasForeignKey(ri => ri.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
