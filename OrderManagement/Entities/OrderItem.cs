
namespace OrderManagement.Entities
{
    public class OrderItem
    {
        public int IdOrder { get; set; }
        public int IdItem { get; set; }
        public int ItemQuantity { get; set; }
        public virtual Item Item { get; set; }
        public virtual Order Order { get; set; }
    }
}