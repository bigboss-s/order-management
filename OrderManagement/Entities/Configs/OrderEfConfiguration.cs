using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace OrderManagement.Entities.Configs
{
    public class OrderEfConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(e => e.Id).HasName("Order_pk");
            builder.Property(e => e.Id).ValueGeneratedOnAdd();

            builder.Property(e => e.Status).IsRequired().HasDefaultValue(OrderStatus.New);
            builder.Property(e => e.Address).IsRequired().HasMaxLength(100);
            builder.Property(e => e.Total).IsRequired();
            builder.Property(e => e.PaymentMethod).IsRequired();
            builder.Property(e => e.IdClient).IsRequired();

            builder.HasOne(e => e.Client)
                .WithMany(e => e.Orders)
                .HasForeignKey(e => e.IdClient)
                .HasConstraintName("Order_Client")
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable(nameof(Order));

            Order[] orders = new Order[]
            {
            new Order { Id = 1, Address = "769 Birch Street, TX", Status = OrderStatus.Closed, Total = 60, PaymentMethod = PaymentMethod.Card, IdClient = 2 },
            new Order { Id = 2, Address = "4588 Kenwood Place, FL", Status = OrderStatus.Closed, Total = 180, PaymentMethod = PaymentMethod.CashOnDelivery, IdClient = 2}
            };

            builder.HasData(orders);
        }
    }
}