using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OrderManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Client",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ClientType = table.Column<int>(type: "INTEGER", nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Client_pk", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Price = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Item_pk", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    Total = table.Column<double>(type: "REAL", nullable: false),
                    PaymentMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    IdClient = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Order_pk", x => x.Id);
                    table.ForeignKey(
                        name: "Order_Client",
                        column: x => x.IdClient,
                        principalTable: "Client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItem",
                columns: table => new
                {
                    IdOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IdItem = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemQuantity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("OrderItem_pk", x => new { x.IdItem, x.IdOrder });
                    table.ForeignKey(
                        name: "OrderItem_Item",
                        column: x => x.IdItem,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "OrderItem_Order",
                        column: x => x.IdOrder,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Client",
                columns: new[] { "Id", "Address", "ClientType", "Name" },
                values: new object[,]
                {
                    { 1, "308 Negra Arroyo Lane, Albuquerque, NM", 1, "Mr. White" },
                    { 2, "4275 Isleta Blvd SW, Albuquerque, NM ", 0, "Los Pollos Hermoanos" }
                });

            migrationBuilder.InsertData(
                table: "Item",
                columns: new[] { "Id", "Name", "Price" },
                values: new object[,]
                {
                    { 1, "Chicken", 3.0 },
                    { 2, "Sky Blue", 40.0 }
                });

            migrationBuilder.InsertData(
                table: "Order",
                columns: new[] { "Id", "IdClient", "PaymentMethod", "Status", "Total" },
                values: new object[,]
                {
                    { 1, 2, 0, 5, 60.0 },
                    { 2, 2, 1, 5, 180.0 }
                });

            migrationBuilder.InsertData(
                table: "OrderItem",
                columns: new[] { "IdItem", "IdOrder", "ItemQuantity" },
                values: new object[,]
                {
                    { 1, 1, 20 },
                    { 1, 2, 20 },
                    { 2, 2, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Order_IdClient",
                table: "Order",
                column: "IdClient");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_IdOrder",
                table: "OrderItem",
                column: "IdOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItem");

            migrationBuilder.DropTable(
                name: "Item");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropTable(
                name: "Client");
        }
    }
}
