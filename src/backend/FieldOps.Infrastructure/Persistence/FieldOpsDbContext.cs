using FieldOps.Application.Auditing;
using FieldOps.Application.Tenancy;
using FieldOps.Domain.Attachments;
using FieldOps.Domain.Auditing;
using FieldOps.Domain.Customers;
using FieldOps.Domain.Identity;
using FieldOps.Domain.Tenants;
using FieldOps.Domain.WorkOrders;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Infrastructure.Persistence;

public sealed class FieldOpsDbContext : DbContext
{
    private readonly ITenantContext
        _tenantContext;

    private readonly IAuditActorContext?
        _auditActorContext;

    public FieldOpsDbContext(
        DbContextOptions<FieldOpsDbContext> options,
        ITenantContext tenantContext,
        IAuditActorContext? auditActorContext = null)
        : base(options)
    {
        _tenantContext = tenantContext;
        _auditActorContext =
            auditActorContext;
    }

    public DbSet<Tenant> Tenants =>
        Set<Tenant>();

    public DbSet<Customer> Customers =>
        Set<Customer>();

    public DbSet<UserAccount> UserAccounts =>
        Set<UserAccount>();

    public DbSet<WorkOrder> WorkOrders =>
        Set<WorkOrder>();

    public DbSet<WorkOrderAttachment>
        WorkOrderAttachments =>
            Set<WorkOrderAttachment>();

    public DbSet<AuditEvent> AuditEvents =>
        Set<AuditEvent>();

    private Guid? ActiveTenantId =>
        _tenantContext.TenantId;

    public override int SaveChanges() =>
        SaveChanges(
            acceptAllChangesOnSuccess: true);

    public override int SaveChanges(
        bool acceptAllChangesOnSuccess)
    {
        AppendAuditEventsAsync(
                CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        return base.SaveChanges(
            acceptAllChangesOnSuccess);
    }

    public override Task<int>
        SaveChangesAsync(
            CancellationToken cancellationToken =
                default) =>
            SaveChangesAsync(
                acceptAllChangesOnSuccess:
                    true,
                cancellationToken);

    public override async Task<int>
        SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken =
                default)
    {
        await AppendAuditEventsAsync(
            cancellationToken);

        return await base.SaveChangesAsync(
            acceptAllChangesOnSuccess,
            cancellationToken);
    }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder
            .ApplyConfigurationsFromAssembly(
                typeof(FieldOpsDbContext)
                    .Assembly);

        modelBuilder.Entity<Customer>()
            .HasQueryFilter(customer =>
                ActiveTenantId ==
                customer.TenantId);

        modelBuilder.Entity<WorkOrder>()
            .HasQueryFilter(workOrder =>
                ActiveTenantId ==
                workOrder.TenantId);

        modelBuilder.Entity<UserAccount>()
            .HasQueryFilter(user =>
                ActiveTenantId ==
                user.TenantId);

        modelBuilder
            .Entity<WorkOrderAttachment>()
            .HasQueryFilter(attachment =>
                ActiveTenantId ==
                attachment.TenantId);

        modelBuilder.Entity<AuditEvent>()
            .HasQueryFilter(auditEvent =>
                ActiveTenantId ==
                auditEvent.TenantId);
    }

    private async Task AppendAuditEventsAsync(
        CancellationToken cancellationToken)
    {
        var descriptors =
            CaptureAuditDescriptors();

        if (descriptors.Count == 0)
        {
            return;
        }

        foreach (var tenantGroup in
                 descriptors
                     .GroupBy(item =>
                         item.TenantId))
        {
            var latest =
                await AuditEvents
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(auditEvent =>
                        auditEvent.TenantId ==
                        tenantGroup.Key)
                    .OrderByDescending(auditEvent =>
                        auditEvent.Sequence)
                    .Select(auditEvent => new
                    {
                        auditEvent.Sequence,
                        auditEvent.EventHash
                    })
                    .FirstOrDefaultAsync(
                        cancellationToken);

            var sequence =
                (latest?.Sequence ?? 0) + 1;
            var previousHash =
                latest?.EventHash ??
                AuditEvent.GenesisHash;

            foreach (var descriptor in
                     tenantGroup)
            {
                var auditEvent =
                    new AuditEvent(
                        descriptor.TenantId,
                        sequence,
                        descriptor.Action,
                        descriptor.EntityType,
                        descriptor.EntityId,
                        descriptor.Summary,
                        _auditActorContext?
                            .DisplayName ??
                        "System",
                        _auditActorContext?
                            .Role ??
                        "System",
                        previousHash,
                        _auditActorContext?
                            .UserId,
                        descriptor.WorkOrderId);

                AuditEvents.Add(
                    auditEvent);

                previousHash =
                    auditEvent.EventHash;
                sequence++;
            }
        }
    }

    private List<AuditDescriptor>
        CaptureAuditDescriptors()
    {
        var descriptors =
            new List<AuditDescriptor>();

        foreach (var entry in
                 ChangeTracker.Entries())
        {
            switch (entry.Entity)
            {
                case Customer customer
                    when entry.State ==
                        EntityState.Added:
                    descriptors.Add(
                        new AuditDescriptor(
                            customer.TenantId,
                            "CustomerCreated",
                            "Customer",
                            customer.Id,
                            null,
                            $"Customer {customer.Reference} was created."));
                    break;

                case Customer customer
                    when entry.State ==
                        EntityState.Modified:
                    var clientChanged =
                        entry.Property(
                                nameof(
                                    Customer
                                        .ClientUserId))
                            .IsModified;

                    descriptors.Add(
                        new AuditDescriptor(
                            customer.TenantId,
                            clientChanged
                                ? "CustomerClientLinked"
                                : "CustomerUpdated",
                            "Customer",
                            customer.Id,
                            null,
                            clientChanged
                                ? $"Client ownership changed for customer {customer.Reference}."
                                : $"Customer {customer.Reference} was updated."));
                    break;

                case WorkOrder workOrder
                    when entry.State ==
                        EntityState.Added:
                    descriptors.Add(
                        new AuditDescriptor(
                            workOrder.TenantId,
                            "WorkOrderCreated",
                            "WorkOrder",
                            workOrder.Id,
                            workOrder.Id,
                            $"Work order {workOrder.Reference} was created."));
                    break;

                case WorkOrder workOrder
                    when entry.State ==
                        EntityState.Modified:
                    descriptors.Add(
                        new AuditDescriptor(
                            workOrder.TenantId,
                            WorkOrderAction(
                                workOrder,
                                entry.Property(
                                    nameof(
                                        WorkOrder
                                            .Status))
                                    .IsModified),
                            "WorkOrder",
                            workOrder.Id,
                            workOrder.Id,
                            $"Work order {workOrder.Reference} changed to {workOrder.Status}."));
                    break;

                case WorkOrderAttachment
                    attachment
                    when entry.State ==
                        EntityState.Added:
                    descriptors.Add(
                        new AuditDescriptor(
                            attachment.TenantId,
                            "AttachmentUploaded",
                            "WorkOrderAttachment",
                            attachment.Id,
                            attachment.WorkOrderId,
                            $"Attachment {attachment.FileName} was uploaded with SHA-256 {attachment.Sha256[..12]}."));
                    break;
            }
        }

        return descriptors;
    }

    private static string WorkOrderAction(
        WorkOrder workOrder,
        bool statusChanged)
    {
        if (!statusChanged)
        {
            return "WorkOrderUpdated";
        }

        return workOrder.Status switch
        {
            WorkOrderStatus.Assigned =>
                "WorkOrderAssigned",
            WorkOrderStatus.InProgress =>
                "WorkOrderStarted",
            WorkOrderStatus
                .AwaitingClientApproval =>
                    "WorkOrderSubmitted",
            WorkOrderStatus.Completed =>
                "WorkOrderApproved",
            WorkOrderStatus.Reopened =>
                "WorkOrderReopened",
            WorkOrderStatus.Cancelled =>
                "WorkOrderCancelled",
            _ =>
                "WorkOrderUpdated"
        };
    }

    private sealed record AuditDescriptor(
        Guid TenantId,
        string Action,
        string EntityType,
        Guid EntityId,
        Guid? WorkOrderId,
        string Summary);
}
