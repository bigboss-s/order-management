using Microsoft.EntityFrameworkCore;
using OrderManagement.Entities;

namespace OrderManagement.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrdersDBContext _context;

        public OrderRepository(OrdersDBContext context){
            _context = context;
        }

        public async Task<List<Order>> GetOrdersAsync()
        {
            return await _context.Orders.ToListAsync();
        }

    }
}