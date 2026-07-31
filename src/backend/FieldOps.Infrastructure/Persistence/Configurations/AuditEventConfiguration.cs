using FieldOps.Domain.Auditing;
using FieldOps.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FieldOps.Infrastructure.Persistence.Configurations;

internal sealed class AuditEventConfiguration
    : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(
        EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("audit_events");

        builder.HasKey(auditEvent =>
            auditEvent.Id);

        builder.Property(auditEvent =>
                auditEvent.Action)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(auditEvent =>
                auditEvent.EntityType)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(auditEvent =>
                auditEvent.Summary)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(auditEvent =>
                auditEvent.ActorDisplayName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(auditEvent =>
                auditEvent.ActorRole)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(auditEvent =>
                auditEvent.PreviousHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(auditEvent =>
                auditEvent.EventHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(auditEvent => new
        {
            auditEvent.TenantId,
            auditEvent.Sequence
        }).IsUnique();

        builder.HasIndex(auditEvent => new
        {
            auditEvent.TenantId,
            auditEvent.OccurredAt
        });

        builder.HasIndex(auditEvent => new
        {
            auditEvent.TenantId,
            auditEvent.WorkOrderId
        });

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(auditEvent =>
                auditEvent.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
