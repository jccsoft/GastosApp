namespace Gastos.Api.Features.Sizings;

public sealed class SizingConfiguration : IEntityTypeConfiguration<Sizing>
{
    public void Configure(EntityTypeBuilder<Sizing> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).HasMaxLength(10).IsRequired();
        builder.HasIndex(s => s.Name).IsUnique();
        
        builder.Property(s => s.Proportion).HasPrecision(18, 6);

        // Relación auto-referencial
        builder.HasOne(s => s.Parent)
            .WithMany(s => s.Children)
            .HasForeignKey(s => s.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Products)
            .WithOne(p => p.Sizing)
            .HasForeignKey(p => p.SizingId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed the allowed units
        builder.HasData(
            new Sizing { Id = 1, Name = "ml", ParentId = 3, Proportion = 1000m },
            new Sizing { Id = 2, Name = "cl", ParentId = 3, Proportion = 100m },
            new Sizing { Id = 3, Name = "L", ParentId = null, Proportion = null },
            new Sizing { Id = 4, Name = "gr", ParentId = 5, Proportion = 1000m },
            new Sizing { Id = 5, Name = "Kg", ParentId = null, Proportion = null },
            new Sizing { Id = 6, Name = "u", ParentId = null, Proportion = null }
        );
    }
}
