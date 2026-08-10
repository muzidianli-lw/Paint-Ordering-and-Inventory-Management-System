using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintStore.API.Migrations
{
    /// <inheritdoc />
    public partial class PaintProductDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaintProducts_Orders_OrderId",
                table: "PaintProducts");

            migrationBuilder.DropIndex(
                name: "IX_PaintProducts_OrderId",
                table: "PaintProducts");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "PaintProducts");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderPaintProduct");

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "PaintProducts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaintProducts_OrderId",
                table: "PaintProducts",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaintProducts_Orders_OrderId",
                table: "PaintProducts",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");
        }
    }
}
