using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mango.Service.OrderApi.Migrations
{
    /// <inheritdoc />
    public partial class FixStripeSessionIdColumnName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StripSessionId",
                table: "OrderHeaders",
                newName: "StripeSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StripeSessionId",
                table: "OrderHeaders",
                newName: "StripSessionId");
        }
    }
}
