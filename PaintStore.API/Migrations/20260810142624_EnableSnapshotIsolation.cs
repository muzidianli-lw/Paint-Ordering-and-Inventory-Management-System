using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintStore.API.Migrations
{
    /// <inheritdoc />
    public partial class EnableSnapshotIsolation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql( 
            """
            ALTER DATABASE CURRENT SET ALLOW_SNAPSHOT_ISOLATION ON;
            """,
            suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
