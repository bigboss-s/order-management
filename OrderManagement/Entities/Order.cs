
namespace OrderManagement.Entities
{
    public enum PaymentMethod
    {
        Card,
        CashOnDelivery
    }

    public enum OrderStatus
    {
        New,
        AtWarehouse,
        AwaitingShippment,
        Shipped,
        ReturnedToClient,
        Error,
        Closed
    }

    public class Order
    {
        public int Id { get; set; }
        public OrderStatus Status { get; set; }
        public double Total { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public int IdClient { get; set; }
        public string Address { get; set; }
        public DateTime? ProcessingStartTime { get; set; }
        public DateTime? ShippingDate { get; set; }
        public virtual Client Client { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}