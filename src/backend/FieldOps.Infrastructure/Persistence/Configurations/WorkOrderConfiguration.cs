using FieldOps.Domain.Customers;
using FieldOps.Domain.Identity;
using FieldOps.Domain.Tenants;
using FieldOps.Domain.WorkOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FieldOps.Infrastructure.Persistence.Configurations;

internal sealed class WorkOrderConfiguration
    : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(
        EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("work_orders");

        builder.HasKey(workOrder =>
            workOrder.Id);

        builder.HasAlternateKey(workOrder =>
            new
            {
                workOrder.TenantId,
                workOrder.Id
            });

        builder.Property(workOrder =>
                workOrder.Reference)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(workOrder =>
                workOrder.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(workOrder =>
                workOrder.Description)
            .HasMaxLength(4000);

        builder.Property(workOrder =>
                workOrder.Priority)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(workOrder =>
                workOrder.Status)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(workOrder =>
                workOrder.CompletionSummary)
            .HasMaxLength(2000);

        builder.Property(workOrder =>
                workOrder.ClientReopenReason)
            .HasMaxLength(1000);

        builder.Property(workOrder =>
                workOrder.Version)
            .IsConcurrencyToken();

        builder.HasIndex(workOrder => new
        {
            workOrder.TenantId,
            workOrder.Reference
        }).IsUnique();

        builder.HasIndex(workOrder => new
        {
            workOrder.TenantId,
            workOrder.AssignedTechnicianId,
            workOrder.Status
        });

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(workOrder =>
                workOrder.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(workOrder => new
            {
                workOrder.TenantId,
                workOrder.CustomerId
            })
            .HasPrincipalKey(customer => new
            {
                customer.TenantId,
                customer.Id
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UserAccount>()
            .WithMany()
            .HasForeignKey(workOrder => new
            {
                workOrder.TenantId,
                workOrder.AssignedTechnicianId
            })
            .HasPrincipalKey(user => new
            {
                user.TenantId,
                user.Id
            })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
