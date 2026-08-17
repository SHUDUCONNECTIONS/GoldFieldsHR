using GoldFieldsHR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldFieldsHR.Infrastructure.Persistence.Configurations;

public class KpiTemplateConfiguration : IEntityTypeConfiguration<KpiTemplate>
{
    public void Configure(EntityTypeBuilder<KpiTemplate> builder)
    {
        builder.ToTable("KpiTemplates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Designation)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasMany(t => t.Categories)
            .WithOne(c => c.KpiTemplate)
            .HasForeignKey(c => c.KpiTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
