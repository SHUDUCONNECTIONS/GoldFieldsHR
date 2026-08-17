using GoldFieldsHR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldFieldsHR.Infrastructure.Persistence.Configurations;

public class PostedScheduleDocumentConfiguration : IEntityTypeConfiguration<PostedScheduleDocument>
{
    public void Configure(EntityTypeBuilder<PostedScheduleDocument> builder)
    {
        builder.ToTable("PostedScheduleDocuments");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(d => d.PostedByEmployee)
            .WithMany()
            .HasForeignKey(d => d.PostedByEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
