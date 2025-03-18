using Microsoft.EntityFrameworkCore;
using OrderManagement.Entities;
using OrderManagement.Services;
using OrderManagement.UI;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var dbPath = Path.Join(Environment.CurrentDirectory, "/data/ordermgmt.db");

        var options = new DbContextOptionsBuilder<OrdersDBContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        using var context = new OrdersDBContext(options);
        await context.Database.EnsureCreatedAsync();

        var orderService = new OrderService(context);
        var ui = new OrderManagementUI(orderService);
        ui.Run();
    }
}