namespace Gastos.Api.Features.Sizings;

public sealed class SizingConfiguration : IEntityTypeConfiguration<Sizing>
{
    public void Configure(EntityTypeBuilder<Sizing> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).HasMaxLength(10).IsRequired();
        builder.HasIndex(s => s.Name).IsUnique();

        builder.HasMany(s => s.Products)
            .WithOne(p => p.Sizing)
            .HasForeignKey(p => p.SizingId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed the allowed units
        builder.HasData(
            new Sizing { Id = 1, Name = "ml" },
            new Sizing { Id = 2, Name = "cl" },
            new Sizing { Id = 3, Name = "L" },
            new Sizing { Id = 4, Name = "gr" },
            new Sizing { Id = 5, Name = "Kg" },
            new Sizing { Id = 6, Name = "u" }
        );
    }
}
