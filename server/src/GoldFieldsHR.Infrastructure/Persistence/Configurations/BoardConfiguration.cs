using GoldFieldsHR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldFieldsHR.Infrastructure.Persistence.Configurations;

public class BoardConfiguration : IEntityTypeConfiguration<Board>
{
    public void Configure(EntityTypeBuilder<Board> builder)
    {
        builder.ToTable("Boards");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(b => b.Description)
            .HasMaxLength(1000);

        builder.HasOne(b => b.OwnerEmployee)
            .WithMany()
            .HasForeignKey(b => b.OwnerEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Site)
            .WithMany()
            .HasForeignKey(b => b.SiteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(b => b.OwnerEmployeeId);
    }
}
