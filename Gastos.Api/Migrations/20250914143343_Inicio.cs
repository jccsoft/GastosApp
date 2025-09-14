using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Gastos.Api.Migrations;

/// <inheritdoc />
public partial class Inicio : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "sizings",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_sizings", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "stores",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                source_name = table.Column<string>(type: "text", nullable: true),
                user_id = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_stores", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "products",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                units_pack = table.Column<int>(type: "integer", nullable: false),
                sizing_id = table.Column<int>(type: "integer", nullable: true),
                sizing_value = table.Column<decimal>(type: "numeric", nullable: true),
                user_id = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_products", x => x.id);
                table.ForeignKey(
                    name: "fk_products_sizings_sizing_id",
                    column: x => x.sizing_id,
                    principalTable: "sizings",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "receipts",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                source_id = table.Column<Guid>(type: "uuid", nullable: false),
                transaction_date_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                store_id = table.Column<Guid>(type: "uuid", nullable: true),
                user_id = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_receipts", x => x.id);
                table.ForeignKey(
                    name: "fk_receipts_stores_store_id",
                    column: x => x.store_id,
                    principalTable: "stores",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "receipt_items",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                receipt_id = table.Column<Guid>(type: "uuid", nullable: false),
                product_id = table.Column<Guid>(type: "uuid", nullable: true),
                source_description = table.Column<string>(type: "text", nullable: true),
                quantity = table.Column<decimal>(type: "numeric", nullable: false),
                amount = table.Column<decimal>(type: "numeric", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_receipt_items", x => x.id);
                table.ForeignKey(
                    name: "fk_receipt_items_products_product_id",
                    column: x => x.product_id,
                    principalTable: "products",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_receipt_items_receipts_receipt_id",
                    column: x => x.receipt_id,
                    principalTable: "receipts",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.InsertData(
            table: "sizings",
            columns: new[] { "id", "name" },
            values: new object[,]
            {
                { 1, "ml" },
                { 2, "cl" },
                { 3, "L" },
                { 4, "gr" },
                { 5, "Kg" },
                { 6, "u" }
            });

        migrationBuilder.CreateIndex(
            name: "ix_products_sizing_id",
            table: "products",
            column: "sizing_id");

        migrationBuilder.CreateIndex(
            name: "ix_products_user_id_name_units_pack_sizing_value",
            table: "products",
            columns: new[] { "user_id", "name", "units_pack", "sizing_value" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_receipt_items_product_id",
            table: "receipt_items",
            column: "product_id");

        migrationBuilder.CreateIndex(
            name: "ix_receipt_items_receipt_id",
            table: "receipt_items",
            column: "receipt_id");

        migrationBuilder.CreateIndex(
            name: "ix_receipts_store_id",
            table: "receipts",
            column: "store_id");

        migrationBuilder.CreateIndex(
            name: "ix_receipts_user_id_source_id",
            table: "receipts",
            columns: new[] { "user_id", "source_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_sizings_name",
            table: "sizings",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_stores_user_id_name",
            table: "stores",
            columns: new[] { "user_id", "name" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "receipt_items");

        migrationBuilder.DropTable(
            name: "products");

        migrationBuilder.DropTable(
            name: "receipts");

        migrationBuilder.DropTable(
            name: "sizings");

        migrationBuilder.DropTable(
            name: "stores");
    }
}
