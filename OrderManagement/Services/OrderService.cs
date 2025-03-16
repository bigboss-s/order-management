using OrderManagement.Entities;
using OrderManagement.Repositories;

namespace OrderManagement.Services
{
    public class OrderService : IOrderService
    {
        private IOrderRepository _repository;

        public OrderService(IOrderRepository repository){
            _repository = repository;
        }

        public async Task<List<Order>> GetOrdersAsync()
        {
            return await _repository.GetOrdersAsync();
        }

    }
}