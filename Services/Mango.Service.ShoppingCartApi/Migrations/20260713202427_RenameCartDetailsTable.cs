using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mango.Service.ShoppingCartApi.Migrations
{
    /// <inheritdoc />
    public partial class RenameCartDetailsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cartDetails_CartHeaders_CartHeaderId",
                table: "cartDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_cartDetails",
                table: "cartDetails");

            migrationBuilder.RenameTable(
                name: "cartDetails",
                newName: "CartDetails");

            migrationBuilder.RenameIndex(
                name: "IX_cartDetails_CartHeaderId",
                table: "CartDetails",
                newName: "IX_CartDetails_CartHeaderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CartDetails",
                table: "CartDetails",
                column: "CartDetailsId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartDetails_CartHeaders_CartHeaderId",
                table: "CartDetails",
                column: "CartHeaderId",
                principalTable: "CartHeaders",
                principalColumn: "CartHeaderId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartDetails_CartHeaders_CartHeaderId",
                table: "CartDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CartDetails",
                table: "CartDetails");

            migrationBuilder.RenameTable(
                name: "CartDetails",
                newName: "cartDetails");

            migrationBuilder.RenameIndex(
                name: "IX_CartDetails_CartHeaderId",
                table: "cartDetails",
                newName: "IX_cartDetails_CartHeaderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cartDetails",
                table: "cartDetails",
                column: "CartDetailsId");

            migrationBuilder.AddForeignKey(
                name: "FK_cartDetails_CartHeaders_CartHeaderId",
                table: "cartDetails",
                column: "CartHeaderId",
                principalTable: "CartHeaders",
                principalColumn: "CartHeaderId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
