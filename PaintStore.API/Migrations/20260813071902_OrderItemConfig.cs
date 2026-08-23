using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintStore.API.Migrations
{
    /// <inheritdoc />
    public partial class OrderItemConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderPaintProduct");

            migrationBuilder.CreateTable(
                name: "OrderItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaintProductId = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItem_PaintProducts_PaintProductId",
                        column: x => x.PaintProductId,
                        principalTable: "PaintProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderOrderItem",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    PaintProductsId = table.Column<int>(type: "int", nullable: false),
                    PaintProductId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderOrderItem", x => new { x.OrderId, x.PaintProductsId });
                    table.ForeignKey(
                        name: "FK_OrderOrderItem_OrderItem_PaintProductsId",
                        column: x => x.PaintProductsId,
                        principalTable: "OrderItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderOrderItem_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderOrderItem_PaintProducts_PaintProductId",
                        column: x => x.PaintProductId,
                        principalTable: "PaintProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_PaintProductId",
                table: "OrderItem",
                column: "PaintProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderOrderItem_PaintProductId",
                table: "OrderOrderItem",
                column: "PaintProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderOrderItem_PaintProductsId",
                table: "OrderOrderItem",
                column: "PaintProductsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderOrderItem");

            migrationBuilder.DropTable(
                name: "OrderItem");

            migrationBuilder.CreateTable(
                name: "OrderPaintProduct",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    PaintProductsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderPaintProduct", x => new { x.OrderId, x.PaintProductsId });
                    table.ForeignKey(
                        name: "FK_OrderPaintProduct_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderPaintProduct_PaintProducts_PaintProductsId",
                        column: x => x.PaintProductsId,
                        principalTable: "PaintProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderPaintProduct_PaintProductsId",
                table: "OrderPaintProduct",
                column: "PaintProductsId");
        }
    }
}
