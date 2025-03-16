using OrderManagement.Entities;

namespace OrderManagement.Repositories
{
    public interface IOrderRepository
    {
        public Task<List<Order>> GetOrdersAsync();
    }
}