using GoldFieldsHR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldFieldsHR.Infrastructure.Persistence.Configurations;

public class KpiTemplateItemConfiguration : IEntityTypeConfiguration<KpiTemplateItem>
{
    public void Configure(EntityTypeBuilder<KpiTemplateItem> builder)
    {
        builder.ToTable("KpiTemplateItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.SubGroupLabel)
            .HasMaxLength(100);

        builder.HasIndex(i => i.KpiTemplateCategoryId);
    }
}
