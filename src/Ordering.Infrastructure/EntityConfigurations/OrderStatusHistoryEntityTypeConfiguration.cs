using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eShop.Ordering.Infrastructure.EntityConfigurations;

/// <summary>
/// EF Core configuration for OrderStatusHistory entity.
/// Defines table structure and relationships for order status timeline tracking.
/// </summary>
class OrderStatusHistoryEntityTypeConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> configuration)
    {
        configuration.ToTable("order_status_history");

        configuration.HasKey(h => h.Id);

        configuration.Property(h => h.Id)
            .UseHiLo("orderstatushistoryseq");

        configuration.Property(h => h.OrderId)
            .IsRequired()
            .HasColumnName("order_id");

        configuration.Property(h => h.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasColumnName("status");

        configuration.Property(h => h.StatusChangedOnUtc)
            .IsRequired()
            .HasColumnName("status_changed_on_utc");

        configuration.Property(h => h.Reason)
            .HasMaxLength(500)
            .HasColumnName("reason");

        // Foreign key to orders table
        configuration.HasOne<Order>()
            .WithMany()
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_order_status_history_order");

        // Index for efficient querying by OrderId and for ordering by timestamp
        configuration.HasIndex(h => new { h.OrderId, h.StatusChangedOnUtc })
            .HasDatabaseName("ix_order_status_history_order_date");
    }
}
