using OrderManagement.Entities;
using OrderManagement.Models;

namespace OrderManagement.Services
{
    public interface IOrderService
    {
        public Task<List<OrderDTO>> GetOrdersAsync();
        public Task<ResultDTO> InsertOrderAsync(int idClient, Dictionary<int, int> itemQuantities, PaymentMethod paymentMethod, string address);
        public Task<ResultDTO> MoveOrderToWarehouseAsync(int idOrder);
        public Task<ResultDTO> ShipOrderAsync(int idOrder);
        public Task<ResultDTO> InsertClientAsync(string name, ClientType clientType, string address);
        public Task<ResultDTO> InsertItemAsync(string name, double price);
    }
}