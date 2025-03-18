
namespace OrderManagement.Entities
{
    public class Item
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public double Price { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public override string ToString()
        {
            return Id + " " + Name + ", priced " + Price;
        }
    }
}