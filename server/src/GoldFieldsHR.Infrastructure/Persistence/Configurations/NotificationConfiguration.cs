using GoldFieldsHR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldFieldsHR.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(n => n.Link)
            .HasMaxLength(200);

        builder.HasOne(n => n.RecipientEmployee)
            .WithMany()
            .HasForeignKey(n => n.RecipientEmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => new { n.RecipientEmployeeId, n.IsRead });
    }
}
