using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OrderManagement.Entities;
using OrderManagement.Models;
using OrderManagement.Services;
using Xunit;

namespace OrderManagement.Tests
{
    public class OrderServiceTests
    {
        private readonly DbContextOptions<OrdersDBContext> _options;

        public OrderServiceTests()
        {
            _options = new DbContextOptionsBuilder<OrdersDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
        }

        [Fact]
        public async Task GetOrdersAsync_ReturnsCorrectDTOs()
        {
            using (var context = new OrdersDBContext(_options))
            {
                var client = new Client { Name = "Test Client", ClientType = ClientType.Company, Address = "Test Client Address" };
                context.Clients.Add(client);
                var item = new Item { Name = "Item1", Price = 10.0 };
                context.Items.Add(item);
                
                var order = new Order
                {
                    IdClient = 1,
                    Address = "Test Address",
                    Total = 20.0,
                    PaymentMethod = PaymentMethod.Card
                };
                
                order.OrderItems.Add(new OrderItem
                {
                    IdItem = 1,
                    IdOrder = 1,
                    ItemQuantity = 2
                });
                
                context.Orders.Add(order);
                await context.SaveChangesAsync();
            }

            using (var context = new OrdersDBContext(_options))
            {
                var service = new OrderService(context);

                var result = await service.GetOrdersAsync();

                Assert.Single(result);
                var dto = result.First();
                Assert.Equal("Test Client", dto.ClientName);
                Assert.Equal(20.0, dto.Total);
                
                Assert.True(dto.itemQuantities.ContainsKey("Item1"));
                Assert.Equal(2, dto.itemQuantities["Item1"]);
            }
        }

        [Fact]
        public async Task InsertOrderAsync_ValidData_CreatesOrder()
        {
            using (var context = new OrdersDBContext(_options))
            {
                var client = new Client { Name = "Test Client", Address = "Test Client Address", ClientType = ClientType.Individual };
                var item = new Item { Name = "Test Item", Price = 30};
                context.Clients.Add(client);
                context.Items.Add(item);
                await context.SaveChangesAsync();
            }

            using (var context = new OrdersDBContext(_options))
            {
                var service = new OrderService(context);
                var itemQuantities = new Dictionary<int, int> { { 1, 3 } };

                var result = await service.InsertOrderAsync(1, itemQuantities, PaymentMethod.Card, "Address");

                Assert.True(result.IsSuccess);
                var order = await context.Orders.FirstOrDefaultAsync();
                Assert.Equal(90, order?.Total);
            }
        }

        [Fact]
        public async Task MoveOrderToWarehouse_CashOnDeliveryOverLimit_ReturnsToClient()
        {
            using (var context = new OrdersDBContext(_options))
            {
                context.Orders.Add(new Order
                {
                    IdClient = 1,
                    Address = "Test Order Address",
                    Total = 3000,
                    PaymentMethod = PaymentMethod.CashOnDelivery,
                    Status = OrderStatus.New
                });
                context.Orders.Add(new Order
                {
                    IdClient = 1,
                    Address = "Test Order Address",
                    Total = 900,
                    PaymentMethod = PaymentMethod.CashOnDelivery,
                    Status = OrderStatus.New
                });
                await context.SaveChangesAsync();
            }

            using (var context = new OrdersDBContext(_options))
            {
                var service = new OrderService(context);

                var result1 = await service.MoveOrderToWarehouseAsync(1);
                var result2 = await service.MoveOrderToWarehouseAsync(2);

                var updatedOrder1 = await context.Orders.FindAsync(1);
                Assert.Equal(OrderStatus.ReturnedToClient, updatedOrder1?.Status);
                Assert.Contains("Order returned to Client", result1.Message);

                var updatedOrder2 = await context.Orders.FindAsync(2);
                Assert.NotEqual(OrderStatus.ReturnedToClient, updatedOrder2?.Status);
                Assert.DoesNotContain("Order returned to Client", result2.Message);
            }
        }

        [Fact]
        public async Task ShipOrder_ValidStatus_ShipsOrder()
        {
            using (var context = new OrdersDBContext(_options))
            {
                var order = new Order
                {
                    IdClient = 1,
                    Address = "Test Order Address",
                    Total = 3000,
                    PaymentMethod = PaymentMethod.CashOnDelivery,
                    Status = OrderStatus.AtWarehouse
                };
                context.Orders.Add(order);
                await context.SaveChangesAsync();
            }

            using (var context = new OrdersDBContext(_options))
            {
                var service = new OrderService(context);

                var result = await service.ShipOrderAsync(1);

                Assert.True(result.IsSuccess);
                var shippedOrder = await context.Orders.FindAsync(1);
                Assert.Equal(OrderStatus.Shipped, shippedOrder?.Status);
            }
        }

        [Fact]
        public async Task AddOrder_AddressCheck()
        {
            using (var context = new OrdersDBContext(_options))
            {
                var client = new Client
                {
                    Id = 1,
                    Name = "Test Client",
                    Address = "Test Address"
                };
                context.Clients.Add(client);

                var item = new Item { Name = "Test Item", Price = 30};
                context.Items.Add(item);

                await context.SaveChangesAsync();
            }

            using (var context = new OrdersDBContext(_options))
            {
                var service = new OrderService(context);

                var result1 = await service.InsertOrderAsync(
                    1,
                    new Dictionary<int, int>{
                        {1, 1}
                    },
                    PaymentMethod.Card,
                    ""
                );

                var result2 = await service.InsertOrderAsync(
                    1,
                    new Dictionary<int, int>{
                        {1, 1}
                    },
                    PaymentMethod.Card,
                    "Test address not empty"
                );

                Assert.False(result1.IsSuccess);
                Assert.True(result2.IsSuccess);
            }
        }
    }
}