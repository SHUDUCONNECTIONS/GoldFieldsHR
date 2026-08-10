using GoldFieldsHR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldFieldsHR.Infrastructure.Persistence.Configurations;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("Attachments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.EntityType)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.StoredFileName)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(a => a.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(a => a.UploadedByEmployee)
            .WithMany()
            .HasForeignKey(a => a.UploadedByEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.EntityType, a.EntityId });
    }
}
