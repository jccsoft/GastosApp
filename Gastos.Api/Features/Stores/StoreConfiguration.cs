namespace Gastos.Api.Features.Stores;

public sealed class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.UserId);

        builder.Property(s => s.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(s => new { s.UserId, s.Name }).IsUnique();

        builder.Property(s => s.SourceName);

        builder.HasMany(s => s.Receipts)
            .WithOne(r => r.Store)
            .HasForeignKey(r => r.StoreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}