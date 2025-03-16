using OrderManagement.Entities;

namespace OrderManagement.Services
{
    public interface IOrderService
    {
        public Task<List<Order>> GetOrdersAsync();
    }
}