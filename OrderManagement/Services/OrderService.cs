using Microsoft.EntityFrameworkCore;
using OrderManagement.Entities;
using OrderManagement.Models;

namespace OrderManagement.Services
{
    public class OrderService : IOrderService
    {
        private readonly OrdersDBContext _context;

        public OrderService(OrdersDBContext context)
        {
            _context = context;
        }

        public async Task<List<OrderDTO>> GetOrdersAsync()
        {
            var orders = await _context.Orders.ToListAsync();
            List<OrderDTO> orderDTOs = [];

            foreach (var order in orders)
            {
                Dictionary<string, int> itemQuantities = [];
                
                foreach (var orderItem in order.OrderItems)
                {
                    itemQuantities.Add(orderItem.Item.Name, orderItem.ItemQuantity);
                }

                orderDTOs.Add(new OrderDTO{
                    Id = order.Id,
                    Total = order.Total,
                    PaymentMethod = order.PaymentMethod,
                    ClientName = order.Client.Name,
                    Address = order.Address,
                    itemQuantities = itemQuantities
                });
            }

            return orderDTOs;
        }

        public async Task<ResultDTO> InsertOrderAsync(int idClient, Dictionary<int, int> itemQuantities, PaymentMethod paymentMethod, string address)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await GetClientByIdAsync(idClient);

                if (address == "")
                {
                    throw new ArgumentException("Invalid shipping address.");
                }

                var newOrder = new Order
                {
                    Total = await GetNewOrderTotalAsync(itemQuantities),
                    PaymentMethod = paymentMethod,
                    IdClient = idClient,
                    Address = address
                };

                await _context.Orders.AddAsync(newOrder);
                await _context.SaveChangesAsync();

                foreach (var entry in itemQuantities)
                {
                    await _context.OrderItems.AddAsync(new OrderItem
                    {
                        ItemQuantity = entry.Value,
                        IdItem = entry.Key,
                        IdOrder = newOrder.Id
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResultDTO{
                    IsSuccess = true,
                    Message = "Order succesfully created.",
                    NewOrder = newOrder
                };

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ResultDTO{
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResultDTO> MoveOrderToWarehouse(int idOrder)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await GetOrderByIdAsync(idOrder);

                if (order.Status != OrderStatus.New)
                {
                    throw new InvalidOperationException("Order has already been moved to warehouse.");
                }

                if (order.Total > 2500 && order.PaymentMethod == PaymentMethod.CashOnDelivery)
                {
                    order.Status = OrderStatus.ReturnedToClient;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new ResultDTO{
                        IsSuccess = false,
                        Message = "Orders of value above 2500 cannot be paid by cash on delivery. Order returned to Client."
                    };
                }

                order.Status = OrderStatus.AtWarehouse;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResultDTO {
                    IsSuccess = true,
                    Message = "Order succesfully moved to warehouse."
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ResultDTO{
                    IsSuccess = false,
                    Message = ex.Message
                };
            }

        }

        public async Task<ResultDTO> ShipOrder(int idOrder){
            using var transaction = await _context.Database.BeginTransactionAsync();

            try {
                var order = await GetOrderByIdAsync(idOrder);

                if (order.Status != OrderStatus.AtWarehouse)
                {
                    throw new InvalidOperationException("Order not ready to be shipped.");
                }

                // TODO: Add 5 second delay
                order.Status = OrderStatus.Shipped;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResultDTO{
                    IsSuccess = true,
                    Message = "Order shipped succesfully"
                };
            }
            catch (Exception ex)
            {
                return new ResultDTO{
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        private async Task<double> GetNewOrderTotalAsync(Dictionary<int, int> itemQuantities)
        {
            double total = 0;

            foreach (var entry in itemQuantities)
            {
                var item = await GetItemByIdAsync(entry.Key);
                total += item.Price * entry.Value;
            }

            return Math.Round(total, 2);
        }

        private async Task<Client> GetClientByIdAsync(int idClient)
        {
            var client = await _context.Clients.FindAsync(idClient);
            if (client == null)
            {
                throw new ArgumentException("Client not found.");
            }
            return client;
        }

        private async Task<Item> GetItemByIdAsync(int idItem)
        {
            var item = await _context.Items.FindAsync(idItem);
            if (item == null)
            {
                throw new ArgumentException("Item now found.");
            }
            return item;
        }

        private async Task<Order> GetOrderByIdAsync(int idOrder)
        {
            var order = await _context.Orders.FindAsync(idOrder);
            if (order == null)
            {
                throw new ArgumentException("No order found.");
            }
            return order;
        }
    }
}