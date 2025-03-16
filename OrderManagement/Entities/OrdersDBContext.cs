using Microsoft.EntityFrameworkCore;
using OrderManagement.Entities.Configs;
using System.Configuration;

namespace OrderManagement.Entities
{
    public class OrdersDBContext : DbContext
    {
        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<Client> Clients { get; set; }
        public virtual DbSet<OrderItem> OrderItems { get; set; }

        public OrdersDBContext() {}

        public OrdersDBContext(DbContextOptions<OrdersDBContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClientEfConfiguration).Assembly);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = System.IO.Path.Join(Environment.CurrentDirectory, "/data/ordermgmt.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}