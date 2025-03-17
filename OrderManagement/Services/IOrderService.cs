using OrderManagement.Entities;
using OrderManagement.Models;

namespace OrderManagement.Services
{
    public interface IOrderService
    {
        public Task<List<OrderDTO>> GetOrdersAsync();
        public Task<ResultDTO> InsertOrderAsync(int idClient, Dictionary<int, int> itemQuantities, PaymentMethod paymentMethod, string address);
        public Task<ResultDTO> MoveOrderToWarehouse(int idOrder);
        public Task<ResultDTO> ShipOrder(int idOrder);
    }
}