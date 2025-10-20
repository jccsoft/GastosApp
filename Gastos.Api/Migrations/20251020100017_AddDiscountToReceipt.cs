using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gastos.Api.Migrations;

/// <inheritdoc />
public partial class AddDiscountToReceipt : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "discount",
            schema: "rc",
            table: "receipts",
            type: "numeric",
            nullable: false,
            defaultValue: 0m);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "discount",
            schema: "rc",
            table: "receipts");
    }
}
