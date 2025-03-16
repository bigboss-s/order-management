using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace OrderManagement.Entities.Configs
{
    public class ItemEfConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.HasKey(e => e.Id).HasName("Item_pk");
            builder.Property(e => e.Id).ValueGeneratedOnAdd();

            builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
            builder.Property(e => e.Price).IsRequired();

            builder.ToTable(nameof(Item));

            Item[] items = new Item[]
            {
            new Item {Id = 1, Name = "Chicken", Price = 3.00},
            new Item {Id = 2, Name = "Sky Blue", Price = 40.00}
            };

            builder.HasData(items);
        }
    }
}