using GoldFieldsHR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldFieldsHR.Infrastructure.Persistence.Configurations;

public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
{
    public void Configure(EntityTypeBuilder<Announcement> builder)
    {
        builder.ToTable("Announcements");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Body)
            .IsRequired()
            .HasMaxLength(10000);

        builder.HasOne(a => a.PostedByEmployee)
            .WithMany()
            .HasForeignKey(a => a.PostedByEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
