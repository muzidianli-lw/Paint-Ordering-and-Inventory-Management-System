using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintStore.API.Migrations
{
    /// <inheritdoc />
    public partial class DebugOrderItemRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderOrderItem");

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "OrderItem",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_OrderId",
                table: "OrderItem",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_Orders_OrderId",
                table: "OrderItem",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_Orders_OrderId",
                table: "OrderItem");

            migrationBuilder.DropIndex(
                name: "IX_OrderItem_OrderId",
                table: "OrderItem");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "OrderItem");

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
                name: "IX_OrderOrderItem_PaintProductId",
                table: "OrderOrderItem",
                column: "PaintProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderOrderItem_PaintProductsId",
                table: "OrderOrderItem",
                column: "PaintProductsId");
        }
    }
}
