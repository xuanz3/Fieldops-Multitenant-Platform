using FieldOps.Domain.Attachments;
using FieldOps.Domain.Identity;
using FieldOps.Domain.Tenants;
using FieldOps.Domain.WorkOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FieldOps.Infrastructure.Persistence.Configurations;

internal sealed class WorkOrderAttachmentConfiguration
    : IEntityTypeConfiguration<WorkOrderAttachment>
{
    public void Configure(
        EntityTypeBuilder<WorkOrderAttachment> builder)
    {
        builder.ToTable(
            "work_order_attachments");

        builder.HasKey(attachment =>
            attachment.Id);

        builder.Property(attachment =>
                attachment.FileName)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(attachment =>
                attachment.ContentType)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(attachment =>
                attachment.Content)
            .IsRequired();

        builder.Property(attachment =>
                attachment.SizeBytes)
            .IsRequired();

        builder.Property(attachment =>
                attachment.Sha256)
            .HasMaxLength(64)
            .IsFixedLength()
            .IsRequired();

        builder.Property(attachment =>
                attachment.UploadedByDisplayName)
            .HasMaxLength(120)
            .IsRequired();

        builder.HasIndex(attachment => new
        {
            attachment.TenantId,
            attachment.WorkOrderId,
            attachment.UploadedAt
        });

        builder.HasIndex(attachment => new
        {
            attachment.TenantId,
            attachment.Sha256
        });

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(attachment =>
                attachment.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<WorkOrder>()
            .WithMany()
            .HasForeignKey(attachment => new
            {
                attachment.TenantId,
                attachment.WorkOrderId
            })
            .HasPrincipalKey(workOrder => new
            {
                workOrder.TenantId,
                workOrder.Id
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UserAccount>()
            .WithMany()
            .HasForeignKey(attachment => new
            {
                attachment.TenantId,
                attachment.UploadedByUserId
            })
            .HasPrincipalKey(user => new
            {
                user.TenantId,
                user.Id
            })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
