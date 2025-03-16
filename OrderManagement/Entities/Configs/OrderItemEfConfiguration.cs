using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace OrderManagement.Entities.Configs
{
    public class OrderItemEfConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.HasKey(e => new { e.IdItem, e.IdOrder }).HasName("OrderItem_pk");

            builder.Property(e => e.IdItem).IsRequired();
            builder.Property(e => e.IdOrder).IsRequired();
            builder.Property(e => e.ItemQuantity).IsRequired();

            builder.HasOne(e => e.Item)
                .WithMany(e => e.OrderItems)
                .HasForeignKey(e => e.IdItem)
                .HasConstraintName("OrderItem_Item")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Order)
                .WithMany(e => e.OrderItems)
                .HasForeignKey(e => e.IdOrder)
                .HasConstraintName("OrderItem_Order")
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable(nameof(OrderItem));

            OrderItem[] orderItems = new OrderItem[]
            {
            new OrderItem {IdItem = 1, IdOrder = 1, ItemQuantity = 20},
            new OrderItem {IdItem = 1, IdOrder = 2, ItemQuantity = 20},
            new OrderItem {IdItem = 2, IdOrder = 2, ItemQuantity = 3}
            };

            builder.HasData(orderItems);
        }
    }
}