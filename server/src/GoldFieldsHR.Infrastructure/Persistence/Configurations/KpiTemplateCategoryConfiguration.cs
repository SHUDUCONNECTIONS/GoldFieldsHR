using GoldFieldsHR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldFieldsHR.Infrastructure.Persistence.Configurations;

public class KpiTemplateCategoryConfiguration : IEntityTypeConfiguration<KpiTemplateCategory>
{
    public void Configure(EntityTypeBuilder<KpiTemplateCategory> builder)
    {
        builder.ToTable("KpiTemplateCategories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasMany(c => c.Items)
            .WithOne(i => i.KpiTemplateCategory)
            .HasForeignKey(i => i.KpiTemplateCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.KpiTemplateId);
    }
}
