using FieldOps.Domain.Customers;
using FieldOps.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FieldOps.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(customer => customer.Id);
        builder.HasAlternateKey(customer => new
        {
            customer.TenantId,
            customer.Id
        });

        builder.Property(customer => customer.Reference)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(customer => customer.Name)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(customer => customer.Email)
            .HasMaxLength(254);

        builder.HasIndex(customer => new
        {
            customer.TenantId,
            customer.Reference
        }).IsUnique();

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(customer => customer.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
