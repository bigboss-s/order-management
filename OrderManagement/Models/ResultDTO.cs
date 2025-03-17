using OrderManagement.Entities;

namespace OrderManagement.Models;

public class ResultDTO
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public Order NewOrder { get; set; }
    public OrderDTO OrderDTO { get; set; }
}