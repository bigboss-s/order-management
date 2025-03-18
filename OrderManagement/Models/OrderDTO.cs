using OrderManagement.Entities;

namespace OrderManagement.Models;

public class OrderDTO 
{
    public int Id { get; set; }
    public double Total { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string ClientName { get; set; }
    public string Address { get; set; }
    public OrderStatus Status { get; set; }
    public Dictionary <string, int> itemQuantities = [];
}