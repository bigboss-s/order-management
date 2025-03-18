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
        public Task<Order> GetOrderByIdIncludesAsync(int idOrder);
        public Task<List<Item>> GetItemsAsync();
        public Task<List<Client>> GetClientsAsync();
        public Task<ResultDTO> DeleteOrderAsync(int idOrder);
    }
}