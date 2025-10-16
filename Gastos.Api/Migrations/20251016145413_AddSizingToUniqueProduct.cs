using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gastos.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSizingToUniqueProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_products_user_id_name_units_pack_sizing_value",
                schema: "rc",
                table: "products");

            migrationBuilder.CreateIndex(
                name: "ix_products_user_id_name_units_pack_sizing_id_sizing_value",
                schema: "rc",
                table: "products",
                columns: new[] { "user_id", "name", "units_pack", "sizing_id", "sizing_value" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_products_user_id_name_units_pack_sizing_id_sizing_value",
                schema: "rc",
                table: "products");

            migrationBuilder.CreateIndex(
                name: "ix_products_user_id_name_units_pack_sizing_value",
                schema: "rc",
                table: "products",
                columns: new[] { "user_id", "name", "units_pack", "sizing_value" },
                unique: true);
        }
    }
}
