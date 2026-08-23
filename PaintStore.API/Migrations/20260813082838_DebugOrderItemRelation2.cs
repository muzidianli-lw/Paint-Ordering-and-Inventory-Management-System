using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintStore.API.Migrations
{
    /// <inheritdoc />
    public partial class DebugOrderItemRelation2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_PaintProducts_PaintProductId",
                table: "OrderItem");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_PaintProducts_PaintProductId",
                table: "OrderItem",
                column: "PaintProductId",
                principalTable: "PaintProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_PaintProducts_PaintProductId",
                table: "OrderItem");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_PaintProducts_PaintProductId",
                table: "OrderItem",
                column: "PaintProductId",
                principalTable: "PaintProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
