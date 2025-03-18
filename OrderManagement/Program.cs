namespace OrderManagement;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Entities;
using OrderManagement.Services;
using OrderManagement.UI;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var dbPath = Path.Join(Environment.CurrentDirectory, "/data/ordermgmt.db");

        var services = new ServiceCollection()
            .AddDbContext<OrdersDBContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"))
            .AddScoped<IOrderService, OrderService>()
            .AddSingleton<OrderManagementUI>()
            .BuildServiceProvider();

        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrdersDBContext>();
        await context.Database.EnsureCreatedAsync();

        var ui = services.GetRequiredService<OrderManagementUI>();
        await ui.Run();
    }
}